using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Globalization;
using The49.Maui.BottomSheet;
using Syncfusion.Maui.Carousel;
using DailyBudgetMAUIApp.Converters;
using Syncfusion.Maui.ProgressBar;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Extensions;


namespace DailyBudgetMAUIApp;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _vm;
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public MainPage(MainPageViewModel viewModel, IRestDataService ds, IProductTools pt)
	{
        _ds = ds;
        _pt = pt;

        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
 
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;

        ProcessSnackBar();

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    protected async override void OnAppearing()
    {

        base.OnAppearing();

        _vm.DefaultBudgetID = Preferences.Get(nameof(App.DefaultBudgetID), 1);
        if (_vm.DefaultBudgetID != 0)
        {
            App.DefaultBudgetID = _vm.DefaultBudgetID;
        }

        if (App.DefaultBudgetID != 0)
        {
            if (App.CurrentSettings == null || App.CurrentSettings.IsUpdatedFlag)
            {
                BudgetSettingValues Settings = _ds.GetBudgetSettingsValues(App.DefaultBudgetID).Result;
                App.CurrentSettings = Settings;
            }

            _pt.SetCultureInfo(App.CurrentSettings);
        }
        else
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-gb");
        }

        if (App.DefaultBudget == null || App.DefaultBudget.BudgetID == 0)
        {
            _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

            App.DefaultBudget = _vm.DefaultBudget;
            _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
            App.SessionLastUpdate = DateTime.UtcNow;
        }
        else
        {
            if (App.SessionLastUpdate == default(DateTime))
            {

                _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

                App.DefaultBudget = _vm.DefaultBudget;
                _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;

            }
            else
            {
                if (DateTime.UtcNow.Subtract(App.SessionLastUpdate) > new TimeSpan(0, 0, 3, 0))
                {
                    DateTime LastUpdated = _ds.GetBudgetLastUpdatedAsync(_vm.DefaultBudgetID).Result;

                    if (App.SessionLastUpdate < LastUpdated)
                    {
                        _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;
                        App.DefaultBudget = _vm.DefaultBudget;
                        _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                        App.SessionLastUpdate = DateTime.UtcNow;
                    }
                }
            }
        }

        if(App.DefaultBudget.IsCreated)
        {
            App.DefaultBudget = await _pt.BudgetDailyCycle(App.DefaultBudget);
            _vm.DefaultBudget = App.DefaultBudget;
        }

        if (!App.DefaultBudget.IsCreated && !App.HasVisitedCreatePage)
        {
            App.HasVisitedCreatePage = true;

            await Shell.Current.GoToAsync($"{nameof(CreateNewBudget)}?BudgetID={App.DefaultBudgetID}&NavigatedFrom=Budget Settings");
        }

        await LoadMainDashboardContent();
    }

    private async Task LoadMainDashboardContent()
    {
        if (_pt.GetBudgetLocalTime(DateTime.UtcNow).Hour > 12)
        {
            _vm.Title = $"Good afternoon {App.UserDetails.NickName}!";
        }
        else
        {
            _vm.Title = $"Good morning {App.UserDetails.NickName}!";
        }

        _vm.EnvelopeStats = new EnvelopeStats(_vm.DefaultBudget.Savings);

        if (_vm.EnvelopeStats.NumberOfEnvelopes == 0)
        {
            brdYourEnvelopes.IsVisible = false;
        }
        else
        {
            brdYourEnvelopes.IsVisible = true;
        }

        SavingCarousel.Children.Clear();
        SavingCarouselIdent.Children.Clear();
        if (_vm.DefaultBudget.Savings.Where(s => s.IsRegularSaving).ToList().Count() != 0)
        {
            brdSavingCarousel.IsVisible = true;
            _vm.SavingCarousel = await CreateSavingCarousel();
            SavingCarousel.Children.Add(_vm.SavingCarousel);
        }
        else
        {
            brdSavingCarousel.IsVisible = false;
        }

        BillCarousel.Children.Clear();
        BillCarouselIdent.Children.Clear();
        if (_vm.DefaultBudget.Bills.Count() != 0)
        {
            brdBillCarousel.IsVisible = true;
            _vm.BillCarousel = await CreateBillCarousel();
            BillCarousel.Children.Add(_vm.BillCarousel);
        }
        else
        {
            brdBillCarousel.IsVisible = false;
        }

        IncomeCarousel.Children.Clear();
        IncomeCarouselIdent.Children.Clear();
        if (_vm.DefaultBudget.IncomeEvents.Any())
        {
            brdIncomeCarousel.IsVisible = true;
            _vm.IncomeCarousel = await CreateIncomeCarousel();
            IncomeCarousel.Children.Add(_vm.IncomeCarousel);
        }
        else
        {
            brdIncomeCarousel.IsVisible = false;
        }

        List<Transactions> RecentTrans = await _ds.GetRecentTransactions(_vm.DefaultBudgetID, 6, "MainPage");
        foreach(Transactions T in RecentTrans)
        {
            _vm.RecentTransactions.Add(T);
        }
        
    }

    private async void ProcessSnackBar()
    {
        string text;
        string actionButtonText;
        Action action;
        TimeSpan duration;

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Warning", out var Warning);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Danger", out var Danger);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("White", out var White);

        var snackbarSuccessOptions = new SnackbarOptions
        {
            BackgroundColor = (Color)Success,
            TextColor = (Color)White,
            ActionButtonTextColor = (Color)White,
            CornerRadius = new CornerRadius(2),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(22),
            CharacterSpacing = 0.1
        };

        var snackbarInfoOptions = new SnackbarOptions
        {
            BackgroundColor = (Color)Info,
            TextColor = (Color)White,
            ActionButtonTextColor = (Color)White,
            CornerRadius = new CornerRadius(2),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(22),
            CharacterSpacing = 0.1
        };

        var snackbarWarningOptions = new SnackbarOptions
        {
            BackgroundColor = (Color)Warning,
            TextColor = (Color)White,
            ActionButtonTextColor = (Color)White,
            CornerRadius = new CornerRadius(2),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(22),
            CharacterSpacing = 0.1
        };

        var snackbarDangerOptions = new SnackbarOptions
        {
            BackgroundColor = (Color)Danger,
            TextColor = (Color)White,
            ActionButtonTextColor = (Color)White,
            CornerRadius = new CornerRadius(2),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(22),
            CharacterSpacing = 0.1
        };

        if (_vm.SnackBar == null || _vm.SnackBar == "")
        {

        }
        else
        {
            if(_vm.SnackBar == "Budget Created")
            {
                text=$"Hurrrrah, you have created a budget!";
                actionButtonText="Undo";
                int BudgetID = _vm.SnackID;
                action = async () => await UndoCreateBudget(BudgetID);
                duration = TimeSpan.FromSeconds(10);

                await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();
            }

            if(_vm.SnackBar == "Transaction Updated")
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                text = $"Nice one, transaction updated!";
                actionButtonText = "Ok";
                action = async() =>
                {
                    source.Cancel();
                };
                duration = TimeSpan.FromSeconds(10);

                var SnackBar = Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions);
                await SnackBar.Show(token);

                _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

                App.DefaultBudget = _vm.DefaultBudget;
                _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;
            }


            if (_vm.SnackBar == "Transaction Added")
            {
                text = $"Sweet, transaction created!";
                actionButtonText = "Undo";
                int TransactionID = _vm.SnackID;
                action = async () => await UndoAddTransaction(TransactionID);
                duration = TimeSpan.FromSeconds(10);

                await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

                App.DefaultBudget = _vm.DefaultBudget;
                _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;
            }

            _vm.SnackBar = "";
            _vm.SnackID = 0;
        }
    }
    private async Task UndoAddTransaction(int TransactionID)
    {
        Transactions Transaction = await _ds.GetTransactionFromID(TransactionID);
        Transaction.TransactionID = 0;

        await _ds.DeleteTransaction(TransactionID);

        await Shell.Current.GoToAsync($"{nameof(AddTransaction)}?BudgetID={_vm.DefaultBudgetID}",
                new Dictionary<string, object>
                {
                    ["Transaction"] = Transaction
                });
    }
    private async Task UndoCreateBudget(int BudgetID)
    {
        var popup = new PopUpPage();
        Application.Current.MainPage.ShowPopup(popup);

        List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

        PatchDoc BudgetStage = new PatchDoc
        {
            op = "replace",
            path = "/IsCreated",
            value = false
        };

        BudgetUpdate.Add(BudgetStage);
        var PatchBudgetResult = await _ds.PatchBudget(BudgetID, BudgetUpdate);

        if(PatchBudgetResult == "OK")
        {
            await Shell.Current.GoToAsync($"{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Finalise Budget");
        }

    }

    private void ShareBudget_Tapped(object sender, TappedEventArgs e)
    {
        ShareBudgetRequest SBR = new ShareBudgetRequest
        {
            SharedBudgetID = _vm.DefaultBudgetID,
            IsVerified = false,
            SharedByUserEmail = App.UserDetails.Email
        };

        ShareBudget page = new ShareBudget(SBR, new RestDataService());

        page.Detents = new DetentsCollection()
        {
            new ContentDetent(),
            new FullscreenDetent()

        };

        page.HasBackdrop = true;
        page.CornerRadius = 30;

        App.CurrentBottomSheet = page;
        page.ShowAsync();

    }

    private async void VerifyBudgetShare_Tapped(object sender, TappedEventArgs e)
    {
        var popup = new PopUpOTP(_vm.DefaultBudget.AccountInfo.BudgetShareRequestID, new PopUpOTPViewModel(new RestDataService()), "ShareBudget", new ProductTools(new RestDataService()), new RestDataService());
        var result = await Application.Current.MainPage.ShowPopupAsync(popup);

        if ((string)result.ToString() != "User Closed")
        {
            ShareBudgetRequest BudgetRequest = (ShareBudgetRequest)result;

            bool DefaultBudgetYesNo = await Application.Current.MainPage.DisplayAlert($"Update Default Budget ", $"CONGRATS!! You have shared a budget with {BudgetRequest.SharedByUserEmail}, do you want to make this budget your default Budget?", "Yes, continue", "No Thanks!");

            if (DefaultBudgetYesNo)
            {
                await _pt.ChangeDefaultBudget(App.UserDetails.UserID, BudgetRequest.SharedBudgetID, true);
            }
        }
    }

    private void NoMoneyLeft_Tapped(object sender, TappedEventArgs e)
    {

    }
    private void YourTransactionsOption_Tapped(object sender, TappedEventArgs e)
    {
        TransactionOptionsBottomSheet page = new TransactionOptionsBottomSheet();

        page.Detents = new DetentsCollection()
        {
            new ContentDetent()
        };

        page.HasBackdrop = true;
        page.CornerRadius = 0;

        App.CurrentBottomSheet = page;

        page.ShowAsync();
    }


    private void BudgetMoreOptions_Tapped(object sender, TappedEventArgs e)
    {
        BudgetOptionsBottomSheet page = new BudgetOptionsBottomSheet(_vm.DefaultBudget, new ProductTools(new RestDataService()),new RestDataService());

        page.Detents = new DetentsCollection()
        {
            new ContentDetent()
        };

        page.HasBackdrop = true;
        page.CornerRadius = 0;

        App.CurrentBottomSheet = page;

        page.ShowAsync();
    }

    private void EnvelopeSavingsMoreOptions_Tapped(object sender, TappedEventArgs e)
    {
        EnvelopeOptionsBottomSheet page = new EnvelopeOptionsBottomSheet();

        page.Detents = new DetentsCollection()
        {
            new ContentDetent()
        };

        page.HasBackdrop = true;
        page.CornerRadius = 0;

        App.CurrentBottomSheet = page;

        page.ShowAsync();
    }

    private async Task<SfCarousel> CreateSavingCarousel()
    {
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
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2))},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2))}
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

            Image ClickImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 5, 5, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue25d",
                    Size = 40,
                    Color = (Color)Primary,
                }
            };

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();
            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += (s, e) =>
            {
                SavingsMoreOptions_Tapped(s, e);
            };

            ClickImage.GestureRecognizers.Add(TapGesture);

            //grid.AddWithSpan(ClickImage, 0, 2, 2, 1);


            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10, 15, 0, 0),
                //Source = ImageSource.FromFile("saving.svg"),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue2eb",
                    Size = 45,
                    Color = (Color)Primary,
                },
                Aspect = Aspect.AspectFill,
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
            lblTitle.SetBinding(Label.TextProperty, "SavingsName");
            grid.AddWithSpan(lblTitle, 0, 1, 1, 1);

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
                WidthRequest = _vm.ProgressBarCarWidthRequest,
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
            WidthRequest = _vm.SignOutButtonWidth + 10,
            ItemWidth = (int)Math.Ceiling(_vm.SignOutButtonWidth) - 10,
            ItemHeight = 250,
            ItemSpacing = 20
        };

        sc.ItemTemplate = dt;
        sc.ItemsSource = _vm.DefaultBudget.Savings.Where(s => s.IsRegularSaving).Take(7).ToList();


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

    private async Task<SfCarousel> CreateBillCarousel()
    {
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
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2))},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2))}
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
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)}
                }
                
            };

            Image ClickImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 5, 5, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue25d",
                    Size = 40,
                    Color = (Color)Primary,                    
                }
            };

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();
            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += (s, e) =>
            {
                SavingsMoreOptions_Tapped(s,e);
            };
            
            ClickImage.GestureRecognizers.Add(TapGesture);

            //grid.AddWithSpan(ClickImage, 0, 2, 2, 1);


            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10,15,0,0),
                //Source = ImageSource.FromFile("saving.svg"),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\uef6e",
                    Size = 45,
                    Color = (Color)Primary,                    
                },
                Aspect = Aspect.AspectFill,
                WidthRequest = 28
            };
            grid.AddWithSpan(image,0,0,6,1);

            Label lblTitle = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 21,
                Padding = new Thickness(10,5,0,0),
                TextColor = (Color)Primary,
                Margin = new Thickness(0)
            };
            lblTitle.SetBinding(Label.TextProperty, "BillName");
            grid.AddWithSpan(lblTitle, 0, 1, 1, 1);

            Label lblSavingType = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 0),
                TextColor = (Color)Tertiary,
                Margin = new Thickness(0),
                CharacterSpacing = 0
            };
            lblSavingType.SetBinding(Label.TextProperty, "IsRecuring", BindingMode.Default,new BillTypeConverter());
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
            lblCurrentBalance.SetBinding(Label.TextProperty, "BillCurrentBalance", BindingMode.Default, new DecimalToCurrencyString());
            grid.Add(lblCurrentBalance, 1, 2);

            Label lblBalance = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 10),
                TextColor = (Color)Gray400,
                CharacterSpacing = 0,
                Text = "Current Balance",
                Margin = new Thickness(0)
            };
            grid.AddWithSpan(lblBalance, 3, 1,1,2);

            SfLinearProgressBar ProgressBar = new SfLinearProgressBar
            {
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = _vm.ProgressBarCarWidthRequest,
                TrackFill = (Color)Gray100,
                ProgressFill = (Color)Success,
                TrackHeight = 10,
                TrackCornerRadius = 5,
                ProgressCornerRadius = 5,
                ProgressHeight = 10,
                Margin = new Thickness(10,0,10,5),
                Minimum = 0
            };
            ProgressBar.SetBinding(SfLinearProgressBar.MaximumProperty, "BillAmount");
            ProgressBar.SetBinding(SfLinearProgressBar.ProgressProperty, "BillCurrentBalance");
            grid.AddWithSpan(ProgressBar, 4, 1, 1, 2);

            Label lblSavingGoalText = new Label
            {
                FontSize = 12,
                Padding = new Thickness(10, 0, 10, 0),
                TextColor = (Color)Tertiary,
                CharacterSpacing = 0,
                HorizontalTextAlignment = TextAlignment.End,
                Text = "Outgoing Amount Due"
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
            SavingProgressBarMaxString.SetBinding(Label.TextProperty, "BillAmount", BindingMode.Default, new DecimalToCurrencyString());
            grid.Add(SavingProgressBarMaxString, 2, 6);

            BoxView bv = new BoxView
            {
                Color = (Color)Gray100,
                HeightRequest = 2,
                Margin = new Thickness(10, 5, 10, 10)
            };
            grid.AddWithSpan(bv, 7, 1,1,2);

            HorizontalStackLayout hsl1 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelOne = new Label
            {
                Text = "Due Date | ",
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
            labelTwo.SetBinding(Label.TextProperty, "BillDueDate", BindingMode.Default, new BillDueDate());

            hsl1.Children.Add(labelTwo);

            grid.AddWithSpan(hsl1, 9, 1, 1, 2);

            HorizontalStackLayout hsl2 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelThree = new Label
            {
                Text = "Daily Amount Put Away | ",
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
            labelFour.SetBinding(Label.TextProperty, "RegularBillValue", BindingMode.Default, new DecimalToCurrencyString());

            hsl2.Children.Add(labelFour);            
            grid.AddWithSpan(hsl2, 10, 1, 1, 2);

            HorizontalStackLayout hsl3 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelFive= new Label
            {
                Text = "Recurring Details | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl3.Children.Add(labelFive);

            Label labelSix = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
            };
            labelSix.SetBinding(Label.TextProperty, ".", BindingMode.Default, new RecurringBillDetails());

            hsl3.Children.Add(labelSix);
            grid.AddWithSpan(hsl3, 8, 1, 1, 2);

            hsl3.SetBinding(HorizontalStackLayout.IsVisibleProperty, "IsRecuring");

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
            WidthRequest = _vm.SignOutButtonWidth + 10,
            ItemWidth = (int)Math.Ceiling(_vm.SignOutButtonWidth) - 10,
            ItemHeight = 270,
            ItemSpacing = 20
        };

        sc.ItemTemplate = dt;
        sc.ItemsSource = _vm.DefaultBudget.Bills;


        if (sc.ItemsSource.Any())
        {
            if((sc.ItemsSource.Count() % 2) == 0)
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2) - 1;
            }
            else
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2);
            }            
        }

        sc.SwipeStarted += async (s, e)  =>
        {
            await BillCarouselSwipeStarted(s,e);
        };

        sc.SwipeEnded += async (s, e) =>
        {
            await BillCarouselSwipeEnded(s,e);
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

            if(i == sc.SelectedIndex)
            {
                button.BackgroundColor = (Color)PrimaryLight;
            }
            else
            {
                button.BackgroundColor = (Color)Gray100;
            }

            BillCarouselIdent.Children.Add(button);            

        }

        return sc;
    }

    private async Task<SfCarousel> CreateIncomeCarousel()
    {
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
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2))},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2))}
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
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)}

                }

            };

            Image ClickImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 5, 5, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue25d",
                    Size = 40,
                    Color = (Color)Primary,
                }
            };

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();
            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += (s, e) =>
            {
                SavingsMoreOptions_Tapped(s, e);
            };

            ClickImage.GestureRecognizers.Add(TapGesture);

            //grid.AddWithSpan(ClickImage, 0, 2, 2, 1);


            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10, 15, 0, 0),
                //Source = ImageSource.FromFile("saving.svg"),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue8e5",
                    Size = 45,
                    Color = (Color)Primary,
                },
                Aspect = Aspect.AspectFill,
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
            lblTitle.SetBinding(Label.TextProperty, "IncomeName");
            grid.AddWithSpan(lblTitle, 0, 1, 1, 1);

            Label lblSavingType = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 0),
                TextColor = (Color)Tertiary,
                Margin = new Thickness(0),
                CharacterSpacing = 0
            };
            lblSavingType.SetBinding(Label.TextProperty, ".", BindingMode.Default, new IncomeTypeConverter());
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
            lblCurrentBalance.SetBinding(Label.TextProperty, "IncomeAmount", BindingMode.Default, new DecimalToCurrencyString());
            grid.Add(lblCurrentBalance, 1, 2);

            Label lblBalance = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 10),
                TextColor = (Color)Gray400,
                CharacterSpacing = 0,
                Text = "Extra Income Amount",
                Margin = new Thickness(0)
            };
            grid.AddWithSpan(lblBalance, 3, 1, 1, 2);


            BoxView bv = new BoxView
            {
                Color = (Color)Gray100,
                HeightRequest = 2,
                Margin = new Thickness(10, 5, 10, 10)
            };
            grid.AddWithSpan(bv, 4, 1, 1, 2);

            HorizontalStackLayout hsl1 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelOne = new Label
            {
                Text = "Due Date | ",
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
            labelTwo.SetBinding(Label.TextProperty, "DateOfIncomeEvent", BindingMode.Default, new BillDueDate());

            hsl1.Children.Add(labelTwo);

            grid.AddWithSpan(hsl1, 5, 1, 1, 2);

            HorizontalStackLayout hsl2 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelThree = new Label
            {
                Text = "Number Of Days To Income | ",
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
            labelFour.SetBinding(Label.TextProperty, "DateOfIncomeEvent", BindingMode.Default, new DateToNumberOfDays());

            hsl2.Children.Add(labelFour);
            grid.AddWithSpan(hsl2, 6, 1, 1, 2);

            HorizontalStackLayout hsl3 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelFive = new Label
            {
                Text = "Recurring Details | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl3.Children.Add(labelFive);

            Label labelSix = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
            };
            labelSix.SetBinding(Label.TextProperty, ".", BindingMode.Default, new RecurringIncomeDetails());

            hsl3.Children.Add(labelSix);
            grid.AddWithSpan(hsl3, 7, 1, 1, 2);

            hsl3.SetBinding(HorizontalStackLayout.IsVisibleProperty, "IsRecurringIncome");

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
            WidthRequest = _vm.SignOutButtonWidth + 10,
            ItemWidth = (int)Math.Ceiling(_vm.SignOutButtonWidth) - 10,
            ItemHeight = 215,
            ItemSpacing = 20
        };

        sc.ItemTemplate = dt;
        sc.ItemsSource = _vm.DefaultBudget.IncomeEvents;


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
            await IncomeCarouselSwipeStarted(s, e);
        };

        sc.SwipeEnded += async (s, e) =>
        {
            await IncomeCarouselSwipeEnded(s, e);
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

            IncomeCarouselIdent.Children.Add(button);

        }

        return sc;
    }

    private async Task IncomeCarouselSwipeStarted(object Sender, Syncfusion.Maui.Core.Carousel.SwipeStartedEventArgs Event)
    {
        Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

        var Carousel = (SfCarousel)Sender;
        var Elements = IncomeCarouselIdent.GetVisualTreeDescendants();

        int Index = (Carousel.SelectedIndex * 2) + 1;

        Border button = (Border)Elements[Index];
        await button.BackgroundColorTo((Color)Gray100, 16, 500, Easing.CubicIn);

    }
    private async Task IncomeCarouselSwipeEnded(object Sender, EventArgs Event)
    {
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);

        var Carousel = (SfCarousel)Sender;
        var Elements = IncomeCarouselIdent.GetVisualTreeDescendants();

        int Index = (Carousel.SelectedIndex * 2) + 1;

        Border button = (Border)Elements[Index];
        await button.BackgroundColorTo((Color)PrimaryLight, 16, 500, Easing.CubicIn);
    }

    private async Task CarouselSwipeStarted(object Sender, Syncfusion.Maui.Core.Carousel.SwipeStartedEventArgs Event)
    {
        Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

        var Carousel = (SfCarousel)Sender;
        var Elements = SavingCarouselIdent.GetVisualTreeDescendants();

        int Index = (Carousel.SelectedIndex * 2) + 1;

        Border button = (Border)Elements[Index];
        await button.BackgroundColorTo((Color)Gray100,16,500,Easing.CubicIn);

    }

    private async Task CarouselSwipeEnded(object Sender, EventArgs Event)
    {
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);

        var Carousel = (SfCarousel)Sender;
        var Elements = SavingCarouselIdent.GetVisualTreeDescendants();

        int Index = (Carousel.SelectedIndex * 2) + 1;

        Border button = (Border)Elements[Index];
        await button.BackgroundColorTo((Color)PrimaryLight, 16, 500, Easing.CubicIn);
    }
    private async Task BillCarouselSwipeStarted(object Sender, Syncfusion.Maui.Core.Carousel.SwipeStartedEventArgs Event)
    {
        Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

        var Carousel = (SfCarousel)Sender;
        var Elements = BillCarouselIdent.GetVisualTreeDescendants();

        int Index = (Carousel.SelectedIndex * 2) + 1;

        Border button = (Border)Elements[Index];
        await button.BackgroundColorTo((Color)Gray100, 16, 500, Easing.CubicIn);

    }

    private async Task BillCarouselSwipeEnded(object Sender, EventArgs Event)
    {
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);

        var Carousel = (SfCarousel)Sender;
        var Elements = BillCarouselIdent.GetVisualTreeDescendants();

        int Index = (Carousel.SelectedIndex * 2) + 1;

        Border button = (Border)Elements[Index];
        await button.BackgroundColorTo((Color)PrimaryLight, 16, 500, Easing.CubicIn);
    }
    private void RefreshView_Refreshing(object sender, EventArgs e)
    {
        LoadMainDashboardContent();
    }

    private void SavingsMoreOptions_Tapped(object sender, TappedEventArgs e)
    {
        EnvelopeOptionsBottomSheet page = new EnvelopeOptionsBottomSheet();

        page.Detents = new DetentsCollection()
        {
            new ContentDetent()
        };

        page.HasBackdrop = true;
        page.CornerRadius = 0;

        App.CurrentBottomSheet = page;

        page.ShowAsync();
    }

    private void ExtraInfoDetails_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void ExtraBillInfo_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void ExtraSavingInfo_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void SeeMoreTransactions_Tapped(object sender, TappedEventArgs e)
    {

        if (!vslRecentTransactions.IsVisible)
        {           
            vslRecentTransactions.IsVisible = true;
            fisRecentTransactionHideShow.Glyph = "\ue5cf";
            vslRecentTransactions.HeightRequest = 0;

            var animation = new Animation(v => vslRecentTransactions.HeightRequest = v, 0, _vm.RecentTransactionsHeight);
            animation.Commit(this, "ShowRecentTran", 16, 100, Easing.CubicIn);

            await MainScrollView.ScrollToAsync(vslRecentTransactions, ScrollToPosition.Start, true);
        }
        else
        {           
            var animation = new Animation(v => vslRecentTransactions.HeightRequest = v, _vm.RecentTransactionsHeight, 0);
            animation.Commit(this, "HideRecentTran", 16, 1000, Easing.CubicIn, async (v, c) =>
            {
                fisRecentTransactionHideShow.Glyph = "\ue5ce";
                vslRecentTransactions.IsVisible = false;
                await MainScrollView.ScrollToAsync(brdYourTransactions,ScrollToPosition.Start, true);
                
            });
        }
    }

    private async void EditTransaction_Tapped(object sender, TappedEventArgs e)
    {
        bool EditTransaction = await Application.Current.MainPage.DisplayAlert($"Are your sure?", $"Are you sure you want to Edit this transaction?", "Yes, continue", "No Thanks!");
        if (EditTransaction)
        {
            Transactions transaction = (Transactions)e.Parameter;
            await Shell.Current.GoToAsync($"{nameof(AddTransaction)}?BudgetID={_vm.DefaultBudgetID}&TransactionID={transaction.TransactionID}",
                new Dictionary<string, object>
                {
                    ["Transaction"] = transaction
                });
        }
    }

    private async void DeleteTransaction_Tapped(object sender, TappedEventArgs e)
    {
        bool DeleteTransaction = await Application.Current.MainPage.DisplayAlert($"Are your sure?", $"Are you sure you want to Delete this transaction?", "Yes", "No Thanks!");
        if (DeleteTransaction)
        {
            Transactions transaction = (Transactions)e.Parameter;
            await _ds.DeleteTransaction(transaction.TransactionID);

            _vm.RecentTransactions.Clear();

            List<Transactions> RecentTrans = await _ds.GetRecentTransactions(_vm.DefaultBudgetID, 6, "MainPage");
            foreach (Transactions T in RecentTrans)
            {
                _vm.RecentTransactions.Add(T);
            }

            LVTransactions.RefreshItem();
            LVTransactions.RefreshView();
        }
    }
}

