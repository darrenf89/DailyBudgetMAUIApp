using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Maui.Controls.Shapes;



namespace DailyBudgetMAUIApp.Handlers;

public partial class BudgetCategoryChart : ContentView
{
    private readonly IRestDataService _ds;
    private readonly IPopupService _ps;

    public BudgetCategoryChart()
    {
        _ds = IPlatformApplication.Current.Services.GetService<IRestDataService>();
        _ps = IPlatformApplication.Current.Services.GetService<IPopupService>();
        IsBusy = false;
        CategoryList = new List<Categories>();
        InitializeComponent();
    }

    public static readonly BindableProperty CategoryListProperty =
    BindableProperty.Create(nameof(CategoryList), typeof(List<Categories>), typeof(BudgetCategoryChart), propertyChanged: OnCategoryListChanged, defaultBindingMode: BindingMode.TwoWay);

    public List<Categories> CategoryList
    {
        get => (List<Categories>)GetValue(CategoryListProperty);
        set => SetValue(CategoryListProperty, value);
    }

    private static async void OnCategoryListChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not BudgetCategoryChart control) return;
        if (newValue is not List<Categories> CategoryList) return;

        if (CategoryList.Count != 0)
        {
            control.IsBusy = true;
            await control.LoadCategoryChartData(CategoryList, false);
            control.IsBusy = false;
        }
        
        control.InvalidateMeasure();
    }

    public static readonly BindableProperty IsBusyProperty =
        BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(BudgetCategoryChart), propertyChanged: OnIsBusyChanged, defaultBindingMode: BindingMode.TwoWay);

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    static void OnIsBusyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (BudgetCategoryChart)bindable;
        control.InvalidateMeasure();
    }

    private async Task LoadCategoryChartData(List<Categories> CategoryList, bool IsBackButton)
    {
        await Task.Delay(1);
        List<ChartClass> categoriesChart = new List<ChartClass>();
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("White", out var White);

        CategoryLegend.Children.Clear();

        if (IsBackButton)
        {
            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 2, 0, 2),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue31b",
                    Size = 32,
                    Color = (Color)Primary,
                }
            };

            TapGestureRecognizer ImageTapGesture = new TapGestureRecognizer();
            ImageTapGesture.NumberOfTapsRequired = 1;
            ImageTapGesture.Tapped += async (s, e) =>
            {
                IsBusy = true;
                await Task.Delay(1);
                List<Categories> CategoryList = await _ds.GetAllHeaderCategoryDetailsFull(App.DefaultBudgetID);
                await LoadCategoryChartData(CategoryList, false);
                IsBusy = false;
            };

            image.GestureRecognizers.Add(ImageTapGesture);
            CategoryLegend.Children.Add(image);
        }

        int i = 0;
        decimal TotalValue = 0;
        foreach (Categories cat in CategoryList)
        {
            if ((IsBackButton && cat.IsSubCategory) || !IsBackButton)
            {
                ChartClass Value = new ChartClass
                {
                    XAxesString = cat.CategoryName,
                    YAxesDouble = (double)cat.CategorySpendPayPeriod
                };

                TotalValue += cat.CategorySpendPayPeriod;

                categoriesChart.Add(Value);

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
                    Text = cat.CategoryName,
                    TextColor = (Color)White,
                    FontSize = 16,
                    Padding = new Thickness(0, 8, 0, 8)
                };

                border.Content = label;

                TapGestureRecognizer TapGesture = new TapGestureRecognizer();

                if (cat.IsSubCategory)
                {
                    TapGesture.NumberOfTapsRequired = 1;
                    TapGesture.Tapped += async (s, e) =>
                    {
                        if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                        await Task.Delay(1000);
                        FilterModel Filters = new FilterModel
                        {
                            CategoryFilter = new List<int>
                            {
                                cat.CategoryID
                            }
                        };

                        await Shell.Current.GoToAsync($"/{nameof(ViewFilteredTransactions)}",
                            new Dictionary<string, object>
                            {
                                ["Filters"] = Filters
                            });
                        return;
                    };
                }
                else
                {
                    TapGesture.NumberOfTapsRequired = 1;
                    TapGesture.Tapped += async (s, e) =>
                    {
                        IsBusy = true;
                        await Task.Delay(1);
                        List<Categories> CategoryList = await _ds.GetHeaderCategoryDetailsFull(cat.CategoryID, App.DefaultBudgetID);
                        await LoadCategoryChartData(CategoryList, true);
                        IsBusy = false;
                    };
                }

                border.GestureRecognizers.Add(TapGesture);

                CategoryLegend.Children.Add(border);
                i++;
            }
        }

        Doughnut.ItemsSource = categoriesChart;
        Doughnut.PaletteBrushes = App.ChartBrush;

        if (TotalValue == 0)
        {
            CircularChart.IsVisible = false;
            NoTransaction.IsVisible = true;
        }
        else
        {
            CircularChart.IsVisible = true;
            NoTransaction.IsVisible = false;
        }
    }


}