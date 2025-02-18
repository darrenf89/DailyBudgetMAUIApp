using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Popups;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.Maui.Toolkit.Carousel;
using System.Collections.ObjectModel;
using Syncfusion.Maui.Scheduler;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using Plugin.Maui.AppRating;
using Plugin.MauiMTAdmob;
using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using static DailyBudgetMAUIApp.Pages.ViewAccounts;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(SnackBar), nameof(SnackBar))]
    [QueryProperty(nameof(SnackID), nameof(SnackID))]
    public partial class MainPageViewModel : BaseViewModel  
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;
        private readonly IAppRating _ar;

        [ObservableProperty]
        private int  defaultBudgetID;

        [ObservableProperty]
        private Budgets  defaultBudget = new Budgets();

        [ObservableProperty]
        private bool  isBudgetCreated;
        [ObservableProperty]
        private bool isPreviousBudget;
        [ObservableProperty]
        private bool  isRefreshing;
        [ObservableProperty]
        private string  snackBar;
        [ObservableProperty]
        private int  snackID;
        [ObservableProperty]
        private double  progressBarWidthRequest;
        [ObservableProperty]
        private double  progressBarCarWidthRequest;
        [ObservableProperty]
        private EnvelopeStats  envelopeStats;
        [ObservableProperty]
        private double  signOutButtonWidth;
        [ObservableProperty]
        private double  screenWidth;
        [ObservableProperty]
        private SfCarousel  savingCarousel;
        [ObservableProperty]
        private SfCarousel  billCarousel;
        [ObservableProperty]
        private SfCarousel  incomeCarousel;
        [ObservableProperty]
        private ObservableCollection<Transactions>  recentTransactions = new ObservableCollection<Transactions>();
        [ObservableProperty]
        private double  recentTransactionsHeight = 452;
        [ObservableProperty]
        private decimal  maxBankBalance = 0;
        [ObservableProperty]
        private decimal  transactionAmount;
        [ObservableProperty]
        private double  quickTransactionWidth;
        [ObservableProperty]
        private double  quickTransactionInputWidth;
        [ObservableProperty]
        private decimal  futureDailySpend;
        [ObservableProperty]
        private int  currentPayeeOffset = 0;
        [ObservableProperty]
        private List<Payees>  payees = new List<Payees>();
        [ObservableProperty]
        private ObservableCollection<ChartClass>  categoriesChart = new ObservableCollection<ChartClass>();
        [ObservableProperty]
        private ObservableCollection<ChartClass>  payeesChart = new ObservableCollection<ChartClass>();
        [ObservableProperty]
        private List<Brush>  chartBrushes = new List<Brush>();
        [ObservableProperty]
        private bool  payeeChartVisible = true;
        [ObservableProperty]
        private bool  categoryChartVisible = true;
        [ObservableProperty]
        private bool isUnreadMessage;
        [ObservableProperty]
        private bool isQuickTransaction;
        [ObservableProperty]
        private List<Budgets> quickTransactionBudgets = new List<Budgets>();
        [ObservableProperty]
        private Budgets selectedQuickTransactionBudget;        
        [ObservableProperty]
        private string quickTransactionAmount;
        [ObservableProperty]
        private ObservableCollection<SchedulerAppointment>  eventList = new ObservableCollection<SchedulerAppointment>();

        public delegate void ReloadPageAction();
        public event ReloadPageAction ReloadPage;

        public MainPageViewModel(IRestDataService ds, IProductTools pt, IAppRating ar)
        {
            _ds = ds;
            _pt = pt;
            _ar = ar;

            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            ProgressBarWidthRequest = ScreenWidth - 85;
            SignOutButtonWidth = ScreenWidth - 30;
            QuickTransactionWidth = ScreenWidth - 180;
            QuickTransactionInputWidth = QuickTransactionWidth - 70;
            ProgressBarCarWidthRequest = ScreenWidth - 115;

            ChartBrushes = App.ChartBrush;
        }


        [RelayCommand]
        async Task QuickTransaction(object obj)
        {
            try
            {
                if (QuickTransactionBudgets == null || QuickTransactionBudgets.Count == 0)
                {
                    QuickTransactionBudgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "MainPage");                    
                }

                SelectedQuickTransactionBudget = QuickTransactionBudgets.Where(b => b.BudgetID == App.DefaultBudgetID).FirstOrDefault();
                QuickTransactionAmount = 0.ToString("c", CultureInfo.CurrentCulture);
                IsQuickTransaction = !IsQuickTransaction;
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "QuickTransaction");
            }
        }

        [RelayCommand]
        async Task ConfirmQuickTransaction(object obj)
        {
            try
            {
                Transactions T = new Transactions
                {
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

                await _pt.MakeSnackBar("Processing transaction", null, null, new TimeSpan(0, 0, 10), "Success");
                IsQuickTransaction = false;

                T.TransactionAmount = (decimal)_pt.FormatCurrencyNumber(QuickTransactionAmount);

                if (SelectedQuickTransactionBudget.IsMultipleAccounts) 
                {
                    List<BankAccounts> bankAccounts = await _ds.GetBankAccounts(SelectedQuickTransactionBudget.BudgetID);
                    T.AccountID = bankAccounts.Where(a=>a.IsDefaultAccount).FirstOrDefault().ID;
                }

                await _ds.SaveNewTransaction(T, SelectedQuickTransactionBudget.BudgetID);
                if(App.DefaultBudget.BudgetID == SelectedQuickTransactionBudget.BudgetID)
                {
                    WeakReferenceMessenger.Default.Send(new UpdateViewAccount(true, false));
                }                

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "QuickTransaction");
            }
        }

        [RelayCommand]
        async Task GoToAccountSettings(object obj)
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Task.Delay(500);

                EditAccountSettings page = new EditAccountSettings(new EditAccountSettingsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()));

                await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "GoToAccountSettings");
            }
        }

        [RelayCommand]
        async Task NavigateViewSupports()
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"{nameof(ViewSupports)}");

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "ViewSupports");
            }
        }

        [RelayCommand]
        async Task GoToBudgetSettings(object obj)
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Task.Delay(500);

                EditBudgetSettings page = new EditBudgetSettings(new EditBudgetSettingsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()));

                await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "GoToBudgetSettings");
            }
        }

        [RelayCommand]
        async Task GoToAccountDetails(object obj)
        {
            try
            {
                CrossMauiMTAdmob.Current.InitialiseAndShowConsentForm();

                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Task.Delay(500);

                EditAccountDetails page = new EditAccountDetails(new EditAccountDetailsViewModel(_pt, _ds), _pt, _ar);

                await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "GoToAccountDetails");
            }

        }

        [RelayCommand]
        async Task CreateNewBudget(object obj)
        {
            try
            {
                var result = await Shell.Current.DisplayPromptAsync("Create a new budget?", "Before you start creating a new budget give it a name and then let's get going!", "Ok", "Cancel");
                if (result != null)
                {

                    Budgets NewBudget = new Budgets();

                    if (!string.IsNullOrEmpty(result))
                    {
                        NewBudget = await _ds.CreateNewBudget(App.UserDetails.Email, "PremiumPlus");

                        List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                        PatchDoc BudgetName = new PatchDoc
                        {
                            op = "replace",
                            path = "/BudgetName",
                            value = result
                        };

                        BudgetUpdate.Add(BudgetName);

                        await _ds.PatchBudget(NewBudget.BudgetID, BudgetUpdate);
                        NewBudget.BudgetName = result;

                    }
                    await _pt.ChangeDefaultBudget(App.UserDetails.UserID, NewBudget.BudgetID, false);
                    App.DefaultBudgetID = NewBudget.BudgetID;
                    App.DefaultBudget = NewBudget;
                    App.HasVisitedCreatePage = true;

                    if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                    {
                        Preferences.Remove(nameof(App.DefaultBudgetID));
                    }
                    Preferences.Set(nameof(App.DefaultBudgetID), NewBudget.BudgetID);


                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(DailyBudgetMAUIApp.Pages.CreateNewBudget)}?BudgetID={NewBudget.BudgetID}&NavigatedFrom=Budget Settings");

                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "CreateNewBudget");
            }
        }

        [RelayCommand]
        async Task NavigateCreateNewBudget()
        {
            try
            {
                var page = new LoadingPage();
                await Application.Current.MainPage.Navigation.PushModalAsync(page);

                await Application.Current.MainPage.Navigation.PopModalAsync();
                await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={DefaultBudgetID}&NavigatedFrom=Budget Settings");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "NavigateCreateNewBudget");
            }
        }


        [RelayCommand]
        async Task SignOut()
        {
            try
            {
                var page = new LoadingPage();
                await Application.Current.MainPage.Navigation.PushModalAsync(page);

                if (Preferences.ContainsKey(nameof(App.UserDetails)))
                {
                    Preferences.Remove(nameof(App.UserDetails));
                }

                if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                {
                    Preferences.Remove(nameof(App.DefaultBudgetID));
                }

                App.DefaultBudgetID = 0;
                App.DefaultBudget = null;

                Application.Current!.MainPage = new AppShell();

                await Application.Current.MainPage.Navigation.PopModalAsync();
                await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "SignOut");
            }
        }

        [RelayCommand]
        async Task RefreshPage()
        {
            try
            {
                DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID, "Full").Result;

                App.DefaultBudget = DefaultBudget;
                IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;

                BudgetSettingValues Settings = await _ds.GetBudgetSettingsValues(App.DefaultBudgetID);
                App.CurrentSettings = Settings;

                EnvelopeStats = new EnvelopeStats(DefaultBudget.Savings);
                IsRefreshing = false;
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "RefreshPage");
            }
        }

        [RelayCommand]
        async Task SwapBudget()
        {
            try
            {
                int PreviousBudgetID = _ds.GetUserDetailsAsync(App.UserDetails.Email).Result.PreviousDefaultBudgetID;
                await _pt.ChangeDefaultBudget(App.UserDetails.UserID, PreviousBudgetID, true);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "SwapBudget");
            }
        }

        [RelayCommand]
        async Task RecalculateBudget()
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Task.Delay(500);

                await _ds.ReCalculateBudget(App.DefaultBudgetID);
                App.DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID, "Full").Result;
                DefaultBudget = App.DefaultBudget;
                ReloadPage?.Invoke();
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "RecalculateBudget");
            }
        }

    }
}
