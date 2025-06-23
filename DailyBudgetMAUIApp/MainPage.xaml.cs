using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.ViewModels;
using Plugin.LocalNotification;
using System.Globalization;
using The49.Maui.BottomSheet;
using static DailyBudgetMAUIApp.Pages.ViewAccounts;


namespace DailyBudgetMAUIApp;

public partial class MainPage : BasePage
{
    private readonly MainPageViewModel _vm;
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;
    private readonly IPopupService _ps;
    public Picker SwitchBudgetPicker { get; set; }

    public MainPage(MainPageViewModel viewModel, IRestDataService ds, IProductTools pt, IPopupService ps)
	{
        _ds = ds;
        _pt = pt;
        _ps = ps;

        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
    }

    private async Task UpdateDefaultBudget()
    {
        List<Budgets> UserBudgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "EditAccountSettings");
        App.DefaultBudget = UserBudgets[0];
        App.DefaultBudgetID = App.DefaultBudget.BudgetID;
        _vm.DefaultBudgetID = App.DefaultBudget.BudgetID;

        List<PatchDoc> UserUpdate = new List<PatchDoc>();

        PatchDoc DefaultBudgetID = new PatchDoc
        {
            op = "replace",
            path = "/DefaultBudgetID",
            value = App.DefaultBudgetID
        };

        UserUpdate.Add(DefaultBudgetID);

        PatchDoc PreviousDefaultBudgetID = new PatchDoc
        {
            op = "replace",
            path = "/PreviousDefaultBudgetID",
            value = 0
        };

        UserUpdate.Add(PreviousDefaultBudgetID);

        await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);
    }

    //protected async override void OnNavigatingFrom(NavigatingFromEventArgs args)
    //{
    //    var NavStack = Shell.Current.Navigation.NavigationStack;
    //    var Count = NavStack.Count;

    //    if (!App.IsPopupShowing) 
    //    { 
    //        App.IsPopupShowing = true;
    //        Shell.Current.ShowPopup(new PopUpPage(), options: new PopupOptions { CanBeDismissedByTappingOutsideOfPopup = false, PageOverlayColor = Color.FromArgb("#80000000") });
    //    }

    //    NavStack = Shell.Current.Navigation.NavigationStack;
    //    Count = NavStack.Count;

    //    _vm.IsMainPageBusy = false;

    //    await Task.Delay(1000);
    //    base.OnNavigatingFrom(args);
    //}

    protected async override void OnAppearing()
    {
        try
        {
            if (App.IsFamilyAccount)
            {
                await Shell.Current.GoToAsync($"//{nameof(FamilyAccountMainPage)}");
                return;
            }

            _vm.IsMainPageBusy = true;
            if (App.IsBudgetUpdated)
            {
                _vm.DefaultBudget = App.DefaultBudget;
                App.IsBudgetUpdated = false;
            }

            base.OnAppearing();

            await ProcessSnackBar();

            _vm.DefaultBudget = null;
            _vm.DefaultBudgetID = Preferences.Get(nameof(App.DefaultBudgetID), 1);
            if (_vm.DefaultBudgetID != 0)
            {
                App.DefaultBudgetID = _vm.DefaultBudgetID;
            }            

            if (App.DefaultBudget == null || App.DefaultBudget.BudgetID == 0)
            {

                var Budget = await _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full");
                App.DefaultBudget = Budget;
                if (App.DefaultBudget.Error != null)
                {
                    if (App.DefaultBudget.Error.ErrorMessage == "Budget Not found")
                    {
                        await UpdateDefaultBudget();
                    }
                }
            }
            else
            {
                if (App.SessionLastUpdate == default(DateTime))
                {
                    var Budget = await _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full");
                    App.DefaultBudget = Budget;

                    if (App.DefaultBudget.Error != null)
                    {
                        if (App.DefaultBudget.Error.ErrorMessage == "Budget Not found")
                        {
                            await UpdateDefaultBudget();
                        }
                    }
                }
                else
                {
                    if (DateTime.UtcNow.Subtract(App.SessionLastUpdate) > new TimeSpan(0, 0, 3, 0))
                    {
                        DateTime? LastUpdated = await _ds.GetBudgetLastUpdatedAsync(_vm.DefaultBudgetID);
                        if(LastUpdated is null)
                        {
                            return;
                        }
                        if (App.SessionLastUpdate < LastUpdated)
                        {

                            var Budget = await _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full");
                            App.DefaultBudget = Budget;
                            if (App.DefaultBudget.Error != null)
                            {
                                if (App.DefaultBudget.Error.ErrorMessage == "Budget Not found")
                                {
                                    await UpdateDefaultBudget();
                                }
                            }
                        }
                    }
                }
            }            

            if (App.DefaultBudgetID != 0)
            {
                BudgetSettingValues Settings = await _ds.GetBudgetSettingsValues(App.DefaultBudgetID);
                App.CurrentSettings = Settings;

                _pt.SetCultureInfo(App.CurrentSettings);
            }
            else
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-gb");
            }


            _vm.DefaultBudget = App.DefaultBudget;
            _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
            App.SessionLastUpdate = DateTime.UtcNow;

            if (App.DefaultBudget.IsCreated)
            {
                var Budget = await _pt.BudgetDailyCycle(App.DefaultBudget);
                App.DefaultBudget = Budget;
                _vm.DefaultBudget = App.DefaultBudget;
            }

            if (!App.DefaultBudget.IsCreated && !App.HasVisitedCreatePage)
            {
                App.HasVisitedCreatePage = true;

                await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={App.DefaultBudgetID}&NavigatedFrom=Budget Settings");
                return;
            }

            await LoadMainDashboardContent();
            await RegisterForWeakMessages();

            _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;

            DateTime PermissionRequestExpiry = new();
            if (Preferences.ContainsKey(nameof(PermissionRequestExpiry)))
            {
                PermissionRequestExpiry = Preferences.Get(nameof(PermissionRequestExpiry), new DateTime());
            }

            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false && DateTime.UtcNow > PermissionRequestExpiry)
            {
                bool IsNotificationEnabled = await LocalNotificationCenter.Current.RequestNotificationPermission();
                if (!IsNotificationEnabled)
                {
                    Preferences.Set(nameof(PermissionRequestExpiry), DateTime.UtcNow.AddDays(60));
                }
            }

            _vm.ReloadPage += async () =>
            {
                await LoadMainDashboardContent();
                if (App.IsPopupShowing)
                {
                    App.IsPopupShowing = false;
                    await _ps.ClosePopupAsync(Shell.Current);
                }

            };

            if(App.IsPopupShowing)
            {
                App.IsPopupShowing = false;
                await _ps.ClosePopupAsync(Shell.Current);
            }

            _vm.IsMainPageBusy = false;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "OnAppearing");
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        base.OnNavigatedFrom(args);
        
    }

    private async Task RegisterForWeakMessages()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);

        WeakReferenceMessenger.Default.Register<UpdateViewAccount>(this, async (r, m) =>
        {
            try
            {
                if(!m._isBackground)
                {
                    if (App.IsPopupShowing)
                    {
                        App.IsPopupShowing = false;
                        if (App.IsPopupShowing){App.IsPopupShowing = false;await _ps.ClosePopupAsync(Shell.Current);}
                    }
                }

                await Task.Delay(1);

                await _ds.ReCalculateBudget(App.DefaultBudgetID);
                var Budget = await _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full");
                App.DefaultBudget = Budget;
                _vm.DefaultBudget = Budget;

                if (!m._isBackground)
                {
                    await LoadMainDashboardContent(); 
                }

                if (!m._isBackground)
                {
                                if(App.IsPopupShowing)
            {
                App.IsPopupShowing = false;
                if (App.IsPopupShowing){App.IsPopupShowing = false;await _ps.ClosePopupAsync(Shell.Current);}
            }
                }
            }
            catch
            {

            }
        });
    }

    private async Task LoadMainDashboardContent()
    {
        if (Preferences.ContainsKey(nameof(_vm.IsTopStickyVisible)))
        {
            _vm.IsTopStickyVisible = Preferences.Get(nameof(_vm.IsTopStickyVisible), true);
        }
        else
        {
            _vm.IsTopStickyVisible = true;
        }

        TopStickyGrid.HeightRequest = _vm.IsTopStickyVisible ? 80 : 0;

        if (_pt.GetBudgetLocalTime(DateTime.UtcNow).Hour > 12)
        {
            _vm.Title = $"Good Afternoon {App.UserDetails.NickName}!";
        }
        else
        {
            _vm.Title = $"Good Morning {App.UserDetails.NickName}!";
        }

        _vm.IsPreviousBudget = App.UserDetails.PreviousDefaultBudgetID != 0;

        if (_vm.DefaultBudget.IsCreated)
        {
            _vm.EnvelopeStats = new EnvelopeStats(_vm.DefaultBudget.Savings);

            if (_vm.EnvelopeStats.NumberOfEnvelopes == 0)
            {
                brdYourEnvelopes.IsVisible = false;
            }
            else
            {
                brdYourEnvelopes.IsVisible = true;
            }

            _vm.Envelopes = _vm.DefaultBudget.Savings.Where(s => !s.IsRegularSaving).ToList();
            _vm.CarouselSavings = _vm.DefaultBudget.Savings.Where(s => s.IsRegularSaving).ToList();
            if (_vm.CarouselSavings.Any())
            {
                absSaving.IsVisible = true;
            }
            else
            {
                absSaving.IsVisible = false;
            }

            _vm.CarouselBills = _vm.DefaultBudget.Bills;
            if (_vm.CarouselBills.Any())
            {
                absBill.IsVisible = true;
            }
            else
            {
                absBill.IsVisible = false;
            }

            _vm.CarouselIncomes = _vm.DefaultBudget.IncomeEvents;
            if (_vm.CarouselIncomes.Any())
            {
                absIncome.IsVisible = true;
            }
            else
            {
                absIncome.IsVisible = false;
            }

            _vm.RecentTransactions.Clear();
            List<Transactions> RecentTrans = await _ds.GetRecentTransactions(_vm.DefaultBudgetID, 6, "MainPage");

            if (RecentTrans.Count() > 0)
            {
                foreach (Transactions T in RecentTrans)
                {
                    _vm.RecentTransactions.Add(T);
                }
            }

            _vm.MaxBankBalance = _vm.DefaultBudget.BankBalance.GetValueOrDefault();
            _vm.MaxBankBalance += _vm.DefaultBudget.CurrentActiveIncome;
            LeftToSpendBalanceProgress.SecondaryProgress = (double)_vm.DefaultBudget.MoneyAvailableBalance;
            LeftToSpendBalanceProgress.SetProgress((double)_vm.DefaultBudget.LeftToSpendBalance, 3000, Easing.CubicInOut);
            PlusStashSpendBalanceProgress.SecondaryProgress = (double)_vm.DefaultBudget.MoneyAvailableBalance;
            PlusStashSpendBalanceProgress.SetProgress((double)_vm.DefaultBudget.PlusStashSpendBalance, 3000, Easing.CubicInOut);

            decimal Amount = 0;

            int Days = (int)Math.Ceiling((_vm.DefaultBudget.NextIncomePayday.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);
            if (Days == 1)
            {
                _vm.FutureDailySpend = -1;
            }
            else
            {
                _vm.FutureDailySpend = (decimal)(_vm.DefaultBudget.LeftToSpendBalance.GetValueOrDefault() / (Days - 1));
            }

            _vm.CategoryList = await _ds.GetAllHeaderCategoryDetailsFull(App.DefaultBudgetID);
            var PayeeList = await _ds.GetPayeeListFull(App.DefaultBudgetID);
            _vm.Payees = PayeeList.OrderByDescending(p => p.PayeeSpendAllTime).ToList();

            _vm.IsUnreadMessage = _vm.DefaultBudget.AccountInfo.NumberUnreadMessages > 0;

            Application.Current.Resources.TryGetValue("Success", out var Success);
            Application.Current.Resources.TryGetValue("Danger", out var Danger);

            Color LabelColor;
            if (_vm.DefaultBudget.LeftToSpendDailyAmount > 0)
            {
                LabelColor = (Color)Success;
            }
            else
            {
                LabelColor = (Color)Danger;
            }

            Label label = new Label
            {
                Text = _vm.DefaultBudget.LeftToSpendDailyAmount.ToString("c", CultureInfo.CurrentCulture),
                Padding = new Thickness(0, 0, 0, 5),
                TextColor = LabelColor,
                VerticalOptions = LayoutOptions.Center,
                Margin = new Thickness(0),
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            };

            MainSFGaugeAnnotation.Content = label;
            MainSFGaugeRadialAxis.Maximum = (double)_vm.DefaultBudget.StartDayDailyAmount;
            MainSFGaugeRadialRange.EndValue = (double)_vm.DefaultBudget.LeftToSpendDailyAmount;

            _vm.NumberPendingQuickTransactions = _vm.DefaultBudget.AccountInfo.NumberPendingQuickTransactions;
            if (App.IsPremiumAccount && _vm.DefaultBudget.AccountInfo.NumberPendingQuickTransactions > 0 )
            {
                PendingTransactionBorder.IsVisible = true;
                var TransactionList = await _ds.GetPendingQuickTransactions(App.DefaultBudgetID);
                _vm.PendingQuickTransaction = TransactionList.OrderByDescending(t => t.TransactionDate).FirstOrDefault();
            }
            else
            {
                PendingTransactionBorder.IsVisible = false;
            }
        }
    }

    private async Task ProcessSnackBar()
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

        if (string.IsNullOrEmpty(_vm.SnackBar))
        {

        }
        else
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            switch (_vm.SnackBar)
            {       
                case "Budget Created":

                    text = $"Hurrrrah, you have created a budget!";
                    actionButtonText = "Undo";
                    int BudgetID = _vm.SnackID;
                    action = async () => await UndoCreateBudget(BudgetID);
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();
                    break;

                case "Transaction Updated":

                    text = $"Nice one, transaction updated!";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        source.Cancel();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    var SnackBar = Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions);
                    await SnackBar.Show(token);


                    break;

                case "Transaction Added":

                    text = $"Sweet, transaction created!";
                    actionButtonText = "Undo";
                    int TransactionID = _vm.SnackID;
                    action = async () => await UndoAddNew(TransactionID, "Transaction");
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                    break;

                case "Saving Added":

                    text = $"Congrats, you have a new saving goal";
                    actionButtonText = "Undo";
                    int SavingID = _vm.SnackID;
                    action = async () => await UndoAddNew(SavingID, "Saving");
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                    break;

                case "Bill Added":

                    text = $"Ah no not another Outgoing!";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        await source.CancelAsync();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                    break;

                case "Income Added":

                    text = $"Congrats, you have added a new income";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        await source.CancelAsync();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                    break;

                case "BudgetSettingsUpdated":

                    text = $"Budget settings successfully updated";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        await source.CancelAsync();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarInfoOptions).Show();

                    BudgetSettingValues Settings = await _ds.GetBudgetSettingsValues(App.DefaultBudgetID);
                    App.CurrentSettings = Settings;
                    _pt.SetCultureInfo(App.CurrentSettings);

                    break;
                case "BudgetDeleted":

                    text = $"Budget has been deleted";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        await source.CancelAsync();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarDangerOptions).Show();

                    break;

                default:
                    break;
            }

            _vm.SnackBar = "";
            _vm.SnackID = 0;

            _vm.DefaultBudget = await _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full");

            App.DefaultBudget = _vm.DefaultBudget;
            _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
            App.SessionLastUpdate = DateTime.UtcNow;

            await LoadMainDashboardContent();
        }
    }

    private async Task UndoAddNew(int ID, string Type)
    {
        try
        {
            if (Type == "Transaction")
            {
                Transactions Transaction = await _ds.GetTransactionFromID(ID);
                Transaction.TransactionID = 0;

                await _ds.DeleteTransaction(ID);

                await Shell.Current.GoToAsync($"{nameof(MainPage)}/{nameof(AddTransaction)}?BudgetID={_vm.DefaultBudgetID}&NavigatedFrom=ViewMainPage",
                        new Dictionary<string, object>
                        {
                            ["Transaction"] = Transaction
                        });
            }
            else if (Type == "Saving")
            {
                Savings saving = await _ds.GetSavingFromID(ID);
                saving.SavingID = 0;

                await _ds.DeleteSaving(ID);

                await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(AddSaving)}?BudgetID={_vm.DefaultBudgetID}&SavingID={-1}",
                        new Dictionary<string, object>
                        {
                            ["Saving"] = saving
                        });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "UndoAddNew");
        }

    }
    private async Task UndoCreateBudget(int BudgetID)
    {
        try
        {
            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

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
                await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Finalise Budget");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "UndoCreateBudget");
        }
    }

    private async void ShareBudget_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            ShareBudgetRequest SBR = new ShareBudgetRequest
            {
                SharedBudgetID = _vm.DefaultBudgetID,
                IsVerified = false,
                SharedByUserEmail = App.UserDetails.Email,
                RequestInitiated = DateTime.UtcNow
            };

            ShareBudget page = new ShareBudget(SBR, _ds, _pt);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent(),
                new FullscreenDetent()

            };

            page.HasBackdrop = true;
            page.CornerRadius = 30;

            App.CurrentBottomSheet = page;
            await page.ShowAsync();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "ShareBudget_Tapped");
        }
    }

    private void ViewShareBudget_Tapped(object sender, TappedEventArgs e)
    {


    }

    private async void VerifyBudgetShare_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopUpOTPViewModel.UserID)] = _vm.DefaultBudget.AccountInfo.BudgetShareRequestID,
                [nameof(PopUpOTPViewModel.OTPType)] = "ShareBudget"
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            IPopupResult<object> popupResult = await _ps.ShowPopupAsync<PopUpOTP, object>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes,
                cancellationToken: CancellationToken.None
            );

            if ((string)popupResult.Result.ToString() != "User Closed")
            {
                if (popupResult.Result is ShareBudgetRequest BudgetRequest)
                {
                    bool DefaultBudgetYesNo = await Application.Current.Windows[0].Page.DisplayAlert($"Update Default Budget ", $"CONGRATS!! You have shared a budget with {BudgetRequest.SharedByUserEmail}, do you want to make this budget your default Budget?", "Yes, continue", "No Thanks!");

                    if (DefaultBudgetYesNo)
                    {
                        await _pt.ChangeDefaultBudget(App.UserDetails.UserID, BudgetRequest.SharedBudgetID, true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Page TopPage = Shell.Current.Navigation.NavigationStack[Shell.Current.Navigation.NavigationStack.Count - 1];
            if (TopPage.GetType() != typeof(NoNetworkAccess) && TopPage.GetType() != typeof(ErrorPage))
            {
                await _pt.HandleException(ex, "MainPage", "VerifyBudgetShare_Tapped");
            }
        }
    }

    private void NoMoneyLeft_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void YourTransactionsOption_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            TransactionOptionsBottomSheet page = new TransactionOptionsBottomSheet(_ds, _pt, _ps);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "YourTransactionsOption_Tapped");
        }
    }


    private async void BudgetMoreOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            BudgetOptionsBottomSheet page = new BudgetOptionsBottomSheet(_vm.DefaultBudget, _pt,_ds, _ps);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "BudgetMoreOptions_Tapped");
        }
    }

    private async void EnvelopeSavingsMoreOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            EnvelopeOptionsBottomSheet page = new EnvelopeOptionsBottomSheet(_ds, _pt, _ps);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "EnvelopeSavingsMoreOptions_Tapped");
        }

    }

    private async Task RefreshView_Refreshing(object sender, EventArgs e)
    {
        await LoadMainDashboardContent();
    }

    private async void ViewFamilyAccountsManage_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.FamilyAccountsManage)}");

        }
        catch (Exception ex)
        {
           await _pt.HandleException(ex, "MainPage", "ViewFamilyAccountsManage_Tapped");
        }

    }

    private async void ExtraIncoDetails_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewIncomes)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "ExtraIncoDetails_Tapped");
        }
    }

    private async void ExtraBillInfo_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewBills)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "ExtraBillInfo_Tapped");
        }

    }

    private async void ExtraSavingInfo_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewSavings)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "ExtraSavingInfo_Tapped");
        }

    }

    private async void SeeMoreTransactions_Tapped(object sender, TappedEventArgs e)
    {
        try
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
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "SeeMoreTransactions_Tapped");
        }
    }

    private async void EditTransaction_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            bool EditTransaction = await Application.Current.Windows[0].Page.DisplayAlert($"Are your sure?", $"Are you sure you want to Edit this transaction?", "Yes, continue", "No Thanks!");
            if (EditTransaction)
            {
                Transactions T = (Transactions)e.Parameter;
                await Shell.Current.GoToAsync($"{nameof(MainPage)}/{nameof(AddTransaction)}?BudgetID={App.DefaultBudgetID}&TransactionID={T.TransactionID}&NavigatedFrom=ViewMainPage",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = T
                    });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "EditTransaction_Tapped");
        }
    }

    private async void DeleteTransaction_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            bool DeleteTransaction = await Application.Current.Windows[0].Page.DisplayAlert($"Are your sure?", $"Are you sure you want to Delete this transaction?", "Yes", "No Thanks!");
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


                _vm.DefaultBudget = await _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full");

                App.DefaultBudget = _vm.DefaultBudget;
                _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "DeleteTransaction_Tapped");
        }
    }

    private async void QuickTransaction_Tapped(object sender, TappedEventArgs e)
    {
        //if (!brdTransactionAmount.IsVisible)
        //{
        //    var a1 = brdTransaction.RotateTo(90, 250, Easing.CubicOut);
        //    var a2 = brdTransactionAmount.FadeTo(1, 250, Easing.CubicOut);
        //    var a3 = btnTransactionAmount.FadeTo(1, 250, Easing.CubicOut);

        //    await Task.WhenAll(a1, a2, a3);

        //    Shadow shadow = new Shadow
        //    {
        //        Opacity = (float)0.95,
        //        Radius = 15
        //    };

        //    brdTransactionAmount.Shadow = shadow;
        //    brdTransactionAmount.IsVisible = true;
        //    brdTransactionAmount.IsVisible = false;
        //    brdTransactionAmount.IsVisible = true;
        //}
        //else
        //{
            
        //    entTransactionAmount.IsEnabled = false;
        //    entTransactionAmount.IsEnabled = true;

        //    var a1 = brdTransaction.RotateTo(0, 250, Easing.CubicOut);
        //    var a2 = brdTransactionAmount.FadeTo(0, 250, Easing.CubicOut);
        //    var a3 = btnTransactionAmount.FadeTo(0, 250, Easing.CubicOut);

        //    await Task.WhenAll(a1, a2, a3);
        //    brdTransactionAmount.IsVisible = false;
        //    brdTransactionAmount.IsVisible = true;
        //    brdTransactionAmount.IsVisible = false;
        //}      
    }

    private async void btnTransactionAmount_Clicked(object sender, EventArgs e)
    {
        try
        {
            Transactions T = new Transactions
            {
                TransactionAmount = _vm.TransactionAmount,
                IsSpendFromSavings = false,
                SavingID = null,
                SavingName = null,
                TransactionDate = DateTime.UtcNow,
                WhenAdded = DateTime.UtcNow,
                IsIncome = false,
                Category = null,
                Payee = null,
                Notes = null,
                CategoryID = null,
                IsTransacted = true,
                SavingsSpendType = null,
                EventType = "Transaction"
            };

            T = await _ds.SaveNewTransaction(T, App.DefaultBudget.BudgetID);
            QuickTransaction_Tapped(null, null);      

            _vm.SnackBar = "Transaction Added";
            _vm.SnackID = T.TransactionID;

            await ProcessSnackBar();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "btnTransactionAmount_Clicked");
        }

    }

    private async void CoverOverspend_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopupMoveBalanceViewModel.Budget)] = App.DefaultBudget,
                [nameof(PopupMoveBalanceViewModel.Type)] = "Budget",
                [nameof(PopupMoveBalanceViewModel.Id)] = 0,
                [nameof(PopupMoveBalanceViewModel.IsCoverOverSpend)] = true
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            IPopupResult<string> popupResult = await _ps.ShowPopupAsync<PopupMoveBalance, string>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes,
                cancellationToken: CancellationToken.None
            );

            await Task.Delay(1);
            if (popupResult.Result.ToString() == "OK")
            {
                var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
                App.DefaultBudget = Budget;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "CoverOverspend_Tapped");
        }
    }

    private async void CategoryOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            CategoryOptionsBottomSheet page = new CategoryOptionsBottomSheet(_ds, _pt, _ps);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "CategoryOptions_Tapped");
        }
    }

    private async void PayeeOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            PayeeOptionsBottomSheet page = new PayeeOptionsBottomSheet(_ds, _pt, _ps);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "PayeeOptions_Tapped");
        }
    }

    private void YourBudgetCalendarOptions_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void SwitchBudgetMain_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            SwitchBudgetPicker = await _pt.SwitchBudget("Dashboard");
            MainVSLView.Children.Add(SwitchBudgetPicker);
            SwitchBudgetPicker.Focus();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "SwitchBudgetMain_Tapped");
        }
    }

    private async void Upload_Clicked(object sender, EventArgs e)
    {
        try
        {
            FileResult UploadFile = await MediaPicker.PickPhotoAsync();

            if (UploadFile is null) return;

            if (await _pt.GetFileLengthAsync(UploadFile) < 3000000)
            {
                await _ds.UploadUserProfilePicture((App.IsFamilyAccount ? App.FamilyUserDetails.UniqueUserID : App.UserDetails.UniqueUserID), UploadFile);
            }
            else
            {

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "Upload_Clicked");
        }

    }

    private async void QuickTransaction_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            _pt.FormatEntryNumber(sender, e, entQuickTransaction);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "QuickTransaction_TextChanged");
        }

    }

    async void OnQuickTransactionSwiped(object sender, SwipedEventArgs e)
    {
        try
        {
            double targetX = e.Direction == SwipeDirection.Left ? -400 : 400;

            await Task.WhenAll(
                QuickTransactionBorder.FadeTo(0.3, 250, Easing.CubicOut),
                QuickTransactionBorder.TranslateTo(targetX, 0, 500, Easing.CubicOut),
                UpdatePendingTransactions()
            );

            await Task.Delay(500);
            QuickTransactionBorder.Opacity = 1;
            QuickTransactionBorder.TranslationX = 0;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "OnQuickTransactionSwiped");
        }

    }

    async Task UpdatePendingTransactions()
    {
        _vm.NumberPendingQuickTransactions = _vm.NumberPendingQuickTransactions - 1;
        var Transaction = await _ds.GetTransactionFromID(_vm.PendingQuickTransaction.TransactionID);
        Transaction.IsQuickTransaction = false;
        await _ds.UpdateTransaction(Transaction);        

        if (App.IsPremiumAccount && _vm.NumberPendingQuickTransactions > 0)
        {
            PendingTransactionBorder.IsVisible = true;
            var TransactionList = await _ds.GetPendingQuickTransactions(App.DefaultBudgetID);
            _vm.PendingQuickTransaction = TransactionList.OrderByDescending(t => t.TransactionDate).FirstOrDefault();
        }
        else
        {
            PendingTransactionBorder.IsVisible = false;
        }        
    }

    async void TopSticky_Swiped(object sender, SwipedEventArgs e)
    {
        try
        {
            if(e.Direction == SwipeDirection.Up)
            {
                await Task.WhenAll(
                    TopSticky.FadeTo(0, 500, Easing.CubicIn),
                    TopSticky.TranslateTo(0, -TopSticky.Height, 500, Easing.CubicIn)                    
                );

                TopStickyGrid.HeightRequest = 0;
                _vm.IsTopStickyVisible = false;

                if (Preferences.ContainsKey(nameof(_vm.IsTopStickyVisible)))
                {
                    Preferences.Remove(nameof(_vm.IsTopStickyVisible));
                }

                Preferences.Set(nameof(_vm.IsTopStickyVisible), _vm.IsTopStickyVisible);

            }
            else if(e.Direction == SwipeDirection.Down)
            {
                TopStickyGrid.HeightRequest = 80;
                _vm.IsTopStickyVisible = true;

                await Task.WhenAll(
                    TopSticky.TranslateTo(0, 0, 500, Easing.CubicOut),
                    TopSticky.FadeTo(1, 500, Easing.CubicOut)
                );

                if (Preferences.ContainsKey(nameof(_vm.IsTopStickyVisible)))
                {
                    Preferences.Remove(nameof(_vm.IsTopStickyVisible));
                }

                Preferences.Set(nameof(_vm.IsTopStickyVisible), _vm.IsTopStickyVisible);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "TopSticky_Swiped");
        }
    }
}

