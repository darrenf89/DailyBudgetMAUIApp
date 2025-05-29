using CommunityToolkit.Maui.Extensions;
using DailyBudgetMAUIApp.Converters;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Carousel;
using Syncfusion.Maui.ProgressBar;

namespace DailyBudgetMAUIApp.Handlers;

public partial class SavingsCarousel : ContentView
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;
    private readonly double SignOutButtonWidth;
    private readonly double ProgressBarCarWidthRequest;

    public SavingsCarousel()
    {
        IsBusy = false;
        Savings = new List<Savings>();
        InitializeComponent();
        _pt = IPlatformApplication.Current.Services.GetService<IProductTools>();
        _ds = IPlatformApplication.Current.Services.GetService<IRestDataService>();

        double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        SignOutButtonWidth = ScreenWidth - 30;
        ProgressBarCarWidthRequest = ScreenWidth - 115;
    }

    public static readonly BindableProperty SavingsProperty =
    BindableProperty.Create(nameof(Savings), typeof(List<Savings>), typeof(SavingsCarousel), propertyChanged: OnSavingsChanged, defaultBindingMode: BindingMode.TwoWay);

    public List<Savings> Savings
    {
        get => (List<Savings>)GetValue(SavingsProperty);
        set => SetValue(SavingsProperty, value);
    }

    private static async void OnSavingsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not SavingsCarousel control) return;
        if (newValue is not List<Savings> SavingList) return;

        if (SavingList.Count != 0)
        {
            control.IsBusy = true;
            control.SavingCarousel?.Children.Clear();
            control.SavingCarouselIdent?.Children.Clear();
            var carousel = await control.CreateSavingCarousel(SavingList);
            control.SavingCarousel?.Children.Add(carousel);
            control.IsBusy = false;
        }
        
        control.InvalidateMeasure();
    }

    public static readonly BindableProperty IsBusyProperty =
        BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(SavingsCarousel), propertyChanged: OnIsBusyChanged, defaultBindingMode: BindingMode.TwoWay);

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    static void OnIsBusyChanged(BindableObject bindable, object oldValue, object newValue)
    {

        var control = (SavingsCarousel)bindable;
        control.InvalidateMeasure();
    }

    public async Task<SfCarousel> CreateSavingCarousel(List<Savings> SavingList)
    {
        await Task.Delay(1);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("Gray100", out var Gray100);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Info", out var Info);

        DataTemplate dt = new DataTemplate(() =>
        {
            Border border = new Border
            {
                Stroke = (Brush)PrimaryBrush,
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(4)
                },
                BackgroundColor = (Color)White
            };

            Grid grid = new Grid
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width = new GridLength(45)},
                    new ColumnDefinition{Width = new GridLength(((SignOutButtonWidth - 65)/2)-50)},
                    new ColumnDefinition{Width = new GridLength(((SignOutButtonWidth - 65)/2)+50)}
                },
                RowDefinitions =
                {
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)}
                }

            };

            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10, 15, 0, 0),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue2eb",
                    Size = 45,
                    Color = (Color)Primary,
                },
                WidthRequest = 28
            };
            grid.AddWithSpan(image, 0, 0, 6, 1);

            Label lblTitle = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 21,
                Padding = new Thickness(10, 5, 0, 0),
                TextColor = (Color)Primary,
                Margin = new Thickness(0)
            };
            lblTitle.SetBinding(Label.TextProperty, ".", BindingMode.Default, new SavingsNameToText());
            grid.AddWithSpan(lblTitle, 0, 1, 1, 2);

            Label lblSavingType = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 0),
                TextColor = (Color)Tertiary,
                Margin = new Thickness(0),
                CharacterSpacing = 0
            };
            lblSavingType.SetBinding(Label.TextProperty, "SavingsType", BindingMode.Default, new SavingTypeConverter());
            grid.AddWithSpan(lblSavingType, 1, 1, 1, 2);

            Label lblCurrentBalance = new Label
            {
                FontSize = 16,
                Padding = new Thickness(10, 10, 0, 0),
                TextColor = (Color)Gray900,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0)
            };
            lblCurrentBalance.SetBinding(Label.TextProperty, "CurrentBalance", BindingMode.Default, new DecimalToCurrencyString());
            grid.Add(lblCurrentBalance, 1, 2);

            Label lblBalance = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 10),
                TextColor = (Color)Gray400,
                CharacterSpacing = 0,
                Text = "Current Saving Balance",
                Margin = new Thickness(0)
            };
            grid.AddWithSpan(lblBalance, 3, 1, 1, 2);

            SfLinearProgressBar ProgressBar = new SfLinearProgressBar
            {
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = ProgressBarCarWidthRequest,
                TrackFill = (Color)Gray100,
                ProgressFill = (Color)Success,
                TrackHeight = 10,
                TrackCornerRadius = 5,
                ProgressCornerRadius = 5,
                ProgressHeight = 10,
                Margin = new Thickness(10, 0, 10, 5),
                Minimum = 0
            };
            ProgressBar.SetBinding(SfLinearProgressBar.MaximumProperty, ".", BindingMode.Default, new SavingProgressBarMax());
            ProgressBar.SetBinding(SfLinearProgressBar.ProgressProperty, "CurrentBalance");
            grid.AddWithSpan(ProgressBar, 4, 1, 1, 2);

            Label lblSavingGoalText = new Label
            {
                FontSize = 12,
                Padding = new Thickness(10, 0, 10, 0),
                TextColor = (Color)Tertiary,
                CharacterSpacing = 0,
                HorizontalTextAlignment = TextAlignment.End,
                Text = "Saving Goal"
            };
            grid.AddWithSpan(lblSavingGoalText, 5, 2, 1, 1);

            Label SavingProgressBarMaxString = new Label
            {
                FontSize = 12,
                Padding = new Thickness(10, 0, 10, 5),
                TextColor = (Color)Gray900,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.End
            };
            SavingProgressBarMaxString.SetBinding(Label.TextProperty, ".", BindingMode.Default, new SavingProgressBarMaxString());
            grid.Add(SavingProgressBarMaxString, 2, 6);

            BoxView bv = new BoxView
            {
                Color = (Color)Gray100,
                HeightRequest = 2,
                Margin = new Thickness(10, 5, 10, 10)
            };
            grid.AddWithSpan(bv, 7, 1, 1, 2);

            HorizontalStackLayout hsl1 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelOne = new Label
            {
                Text = "Daily Saving Amount | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl1.Children.Add(labelOne);

            Label labelTwo = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,

            };
            labelTwo.SetBinding(Label.TextProperty, ".", BindingMode.Default, new RegularSavingValueString());

            hsl1.Children.Add(labelTwo);
            grid.AddWithSpan(hsl1, 8, 1, 1, 2);

            HorizontalStackLayout hsl2 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelThree = new Label
            {
                Text = "Saving Goal Date | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl2.Children.Add(labelThree);

            Label labelFour = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
            };
            labelFour.SetBinding(Label.TextProperty, ".", BindingMode.Default, new SavingGoalDateString());

            hsl2.Children.Add(labelFour);
            grid.AddWithSpan(hsl2, 9, 1, 1, 2);


            border.Content = grid;

            return border;
        });

        SfCarousel sc = new SfCarousel
        {
            ScaleOffset = (float)0.9,
            RotationAngle = 20,
            Duration = 1000,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = SignOutButtonWidth + 10,
            ItemWidth = (int)Math.Ceiling(SignOutButtonWidth) - 10,
            ItemHeight = 250,
            ItemSpacing = 20
        };

        sc.ItemTemplate = dt;
        sc.ItemsSource = SavingList;


        if (sc.ItemsSource.Any())
        {
            if ((sc.ItemsSource.Count() % 2) == 0)
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2) - 1;
            }
            else
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2);
            }
        }

        sc.SwipeStarted += async (s, e) =>
        {
            await CarouselSwipeStarted(s, e);
        };

        sc.SwipeEnded += async (s, e) =>
        {
            await CarouselSwipeEnded(s, e);
        };

        for (int i = 0; i < sc.ItemsSource.Count(); i++)
        {
            Border button = new Border
            {
                HeightRequest = 10,
                WidthRequest = 10,
                Margin = new Thickness(2, 0, 2, 0),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5)
                }
            };

            if (i == sc.SelectedIndex)
            {
                button.BackgroundColor = (Color)PrimaryLight;
            }
            else
            {
                button.BackgroundColor = (Color)Gray100;
            }

            SavingCarouselIdent.Children.Add(button);


        }

        return sc;
    }

    private async Task CarouselSwipeStarted(object Sender, Syncfusion.Maui.Core.Carousel.SwipeStartedEventArgs Event)
    {
        try
        {
            Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

            var Carousel = (SfCarousel)Sender;
            var Elements = SavingCarouselIdent.GetVisualTreeDescendants();

            int Index = (Carousel.SelectedIndex * 2) + 1;

            Border button = (Border)Elements[Index];
            await button.BackgroundColorTo((Color)Gray100, 16, 500, Easing.CubicIn);

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "CarouselSwipeStarted");
        }

    }

    private async Task CarouselSwipeEnded(object Sender, EventArgs Event)
    {
        try
        {
            Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);

            var Carousel = (SfCarousel)Sender;
            var Elements = SavingCarouselIdent.GetVisualTreeDescendants();

            int Index = (Carousel.SelectedIndex * 2) + 1;

            Border button = (Border)Elements[Index];
            await button.BackgroundColorTo((Color)PrimaryLight, 16, 500, Easing.CubicIn);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "CarouselSwipeEnded");
        }
    }
}