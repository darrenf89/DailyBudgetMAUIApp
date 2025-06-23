using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;


namespace DailyBudgetMAUIApp.Handlers;

public partial class BudgetPayeeChart : ContentView
{
    private readonly IRestDataService _ds;
    public int CurrentPayeeOffset = 0;

    public BudgetPayeeChart()
    {
        _ds = IPlatformApplication.Current.Services.GetService<IRestDataService>();
        IsBusy = false;
        PayeeList = new List<Payees>();
        InitializeComponent();
    }

    public static readonly BindableProperty PayeeListProperty =
    BindableProperty.Create(nameof(PayeeList), typeof(List<Payees>), typeof(BudgetPayeeChart), propertyChanged: OnPayeeListChanged, defaultBindingMode: BindingMode.TwoWay);

    public List<Payees> PayeeList
    {
        get => (List<Payees>)GetValue(PayeeListProperty);
        set => SetValue(PayeeListProperty, value);
    }

    private static async void OnPayeeListChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not BudgetPayeeChart control) return;
        if (newValue is not List<Payees> PayeeList) return;

        if (PayeeList.Count != 0)
        {
            control.IsBusy = true;
            await control.LoadPayeeChartData(PayeeList);
            control.IsBusy = false;
        }
        
        control.InvalidateMeasure();
    }

    public static readonly BindableProperty IsBusyProperty =
        BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(BudgetPayeeChart), propertyChanged: OnIsBusyChanged, defaultBindingMode: BindingMode.TwoWay);

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    static void OnIsBusyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (BudgetPayeeChart)bindable;
        control.InvalidateMeasure();
    }

    private async Task LoadPayeeChartData(List<Payees> PayeeList)
    {
        await Task.Delay(1);
        List<ChartClass> payeesChart = new List<ChartClass>();
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);

        PayeeLegend.Children.Clear();
        PreviousNextPayee.Children.Clear();

        int MaxIndex = CurrentPayeeOffset + 8 >= PayeeList.Count() ? PayeeList.Count() - CurrentPayeeOffset : 8;

        List<Payees> Payees = PayeeList.GetRange(CurrentPayeeOffset, MaxIndex);

        int i = 0;
        decimal TotalValue = 0;
        foreach (Payees payee in Payees)
        {
            ChartClass Value = new ChartClass
            {
                XAxesString = payee.Payee,
                YAxesDouble = (double)payee.PayeeSpendPayPeriod
            };

            TotalValue += payee.PayeeSpendPayPeriod;

            payeesChart.Add(Value);

            Border border = new Border
            {
                BackgroundColor = App.ChartColor[i],
                Stroke = (Brush)PrimaryBrush,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(4)
                },
                Margin = new Thickness(0, 2, 10, 2),
                Padding = new Thickness(10, 0, 0, 0)
            };

            Label label = new Label
            {
                Text = payee.Payee,
                TextColor = (Color)White,
                FontSize = 16,
                Padding = new Thickness(0, 8, 0, 8)
            };

            border.Content = label;

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();

            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += async (s, e) =>
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
                await Task.Delay(1000);
                FilterModel Filters = new FilterModel
                {
                    PayeeFilter = new List<string>
                        {
                            payee.Payee
                        }
                };

                await Shell.Current.GoToAsync($"/{nameof(ViewFilteredTransactions)}",
                    new Dictionary<string, object>
                    {
                        ["Filters"] = Filters
                    });
                return;
            };

            border.GestureRecognizers.Add(TapGesture);
            PayeeLegend.Children.Add(border);

            i++;
        }

        Doughnut.ItemsSource = payeesChart;
        Doughnut.PaletteBrushes = App.ChartBrush;

        if (TotalValue == 0)
        {
            CircularChart.IsVisible = false;
            NoTransactions.IsVisible = true;
        }
        else
        {
            CircularChart.IsVisible = true;
            NoTransactions.IsVisible = false;
        }

        if (CurrentPayeeOffset >= 8)
        {
            HorizontalStackLayout HSLPrevious = new HorizontalStackLayout
            {
                Padding = new Thickness(10, 0, 0, 5)
            };

            Image PreviousImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 0, 10, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5c4",
                    Size = 25,
                    Color = (Color)Tertiary,
                }
            };

            TapGestureRecognizer PreviousImageTapGesture = new TapGestureRecognizer();
            PreviousImageTapGesture.NumberOfTapsRequired = 1;
            PreviousImageTapGesture.Tapped += async (s, e) =>
            {
                IsBusy = true;
                await Task.Delay(1);
                CurrentPayeeOffset -= 8;
                await LoadPayeeChartData(PayeeList);
                IsBusy = false;
            };

            HSLPrevious.GestureRecognizers.Add(PreviousImageTapGesture);
            HSLPrevious.Children.Add(PreviousImage);

            Label PreviousLabel = new Label
            {
                Text = "Previous",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                TextColor = (Color)Tertiary,
                FontSize = 18,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                FontAttributes = FontAttributes.Bold
            };

            HSLPrevious.Children.Add(PreviousLabel);
            PreviousNextPayee.Add(HSLPrevious, 0, 0);
        }

        if (CurrentPayeeOffset + 8 < PayeeList.Count())
        {
            HorizontalStackLayout HSLNext = new HorizontalStackLayout
            {
                Padding = new Thickness(0, 0, 0, 5),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };

            Image NextImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(10, 0, 10, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5c8",
                    Size = 25,
                    Color = (Color)Tertiary,
                }
            };

            TapGestureRecognizer NextImageTapGesture = new TapGestureRecognizer();
            NextImageTapGesture.NumberOfTapsRequired = 1;
            NextImageTapGesture.Tapped += async (s, e) =>
            {
                IsBusy = true;
                await Task.Delay(1);
                CurrentPayeeOffset += 8;
                await LoadPayeeChartData(PayeeList);
                IsBusy = false;
            };

            HSLNext.GestureRecognizers.Add(NextImageTapGesture);


            Label NextLabel = new Label
            {
                Text = "Next",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                TextColor = (Color)Tertiary,
                FontSize = 18,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                FontAttributes = FontAttributes.Bold
            };

            HSLNext.Children.Add(NextLabel);
            HSLNext.Children.Add(NextImage);

            PreviousNextPayee.Add(HSLNext, 1, 0);
        }
    }


}