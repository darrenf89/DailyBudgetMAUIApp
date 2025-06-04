using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Plugin.Maui.AppRating;
using Syncfusion.Maui.Carousel;
using Syncfusion.Maui.Scheduler;
using System.Collections.ObjectModel;
using System.Globalization;
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
        private bool isMainPageBusy = false;
        [ObservableProperty]
        private Budgets  defaultBudget = new Budgets();
        [ObservableProperty]
        private List<Savings> carouselSavings;
        [ObservableProperty]
        private List<Savings> envelopes;
        [ObservableProperty]
        private List<Bills> carouselBills;
        [ObservableProperty]
        private List<IncomeEvents> carouselIncomes;
        [ObservableProperty]
        private List<Categories> categoryList;
        [ObservableProperty]
        private Transactions pendingQuickTransaction;

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
        private List<Payees>  payees = new List<Payees>();
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
        private bool isTopStickyVisible;
        [ObservableProperty]
        private List<Budgets> quickTransactionBudgets = new List<Budgets>();
        [ObservableProperty]
        private Budgets selectedQuickTransactionBudget;        
        [ObservableProperty]
        private string quickTransactionAmount;
        [ObservableProperty]
        private int numberPendingQuickTransactions;
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
                    EventType = "Transaction",
                    IsQuickTransaction = true
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
        async Task NavigateViewSupports()
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
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
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"//{nameof(ViewBudgets)}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "GoToBudgetSettings");
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
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Task.Delay(1);
                await Shell.Current.GoToAsync($"/{nameof(DailyBudgetMAUIApp.Pages.CreateNewBudget)}?BudgetID={DefaultBudgetID}&NavigatedFrom=Budget Settings");
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
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Task.Delay(1);

                if (Preferences.ContainsKey(nameof(App.UserDetails)))
                {
                    Preferences.Remove(nameof(App.UserDetails));
                }

                if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                {
                    Preferences.Remove(nameof(App.DefaultBudgetID));
                }

                if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
                {
                    Preferences.Remove(nameof(App.IsFamilyAccount));
                }

                App.DefaultBudgetID = 0;
                App.DefaultBudget = null;

                Application.Current!.MainPage = new AppShell();
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
                DefaultBudget = await _ds.GetBudgetDetailsAsync(DefaultBudgetID, "Full");

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
                var UserDetails = await _ds.GetUserDetailsAsync(App.UserDetails.Email);
                int PreviousBudgetID = UserDetails.PreviousDefaultBudgetID;
                await _pt.ChangeDefaultBudget(App.UserDetails.UserID, PreviousBudgetID, true);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "SwapBudget");
            }
        }


        [RelayCommand]
        async Task SpendEnvelope(object obj)
        {
            try
            {
                if (obj is not Savings Saving)
                {
                    return;
                }

                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    await Task.Delay(1);
                }

                string SpendType = "EnvelopeSaving";
                Transactions T = new Transactions
                {
                    IsSpendFromSavings = true,
                    SavingID = Saving.SavingID,
                    SavingName = Saving.SavingsName,
                    SavingsSpendType = SpendType,
                    EventType = "Envelope",
                    TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow)
                };

                await Shell.Current.GoToAsync($"/{nameof(AddTransaction)}?BudgetID={App.DefaultBudget.BudgetID}&NavigatedFrom=ViewMainPage&TransactionID=0",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = T
                    });
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "SpendEnvelope");
            }
        }

        [RelayCommand]
        async Task EditEnvelope(object obj)
        {
            try
            {
                if(obj is not Savings Saving)
                {
                    return;
                }

                bool result = await Shell.Current.DisplayAlert($"Edit {Saving.SavingsName}?", $"Are you sure you want to edit {Saving.SavingsName}?", "Yes", "Cancel");

                if (result)
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                        await Task.Delay(1);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}//{nameof(AddSaving)}?BudgetID={DefaultBudget.BudgetID}&SavingID={Saving.SavingID}&NavigatedFrom=MainPage");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "EditEnvelope");
            }
        }

        [RelayCommand]
        async Task AddNewEnvelope()
        {
            try
            {
                bool result = await Shell.Current.DisplayAlert($"Add new envelope?", $"Are you sure you want to add a new envelope?", "Yes", "Cancel");

                if (result)
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                        await Task.Delay(1);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(AddSaving)}?SavingType=Envelope");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "AddNewEnvelope");
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
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Task.Delay(500);

                await _ds.ReCalculateBudget(App.DefaultBudgetID);
                var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
                App.DefaultBudget = Budget;
                DefaultBudget = App.DefaultBudget;
                ReloadPage?.Invoke();
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "RecalculateBudget");
            }
        }

        [RelayCommand]
        async Task EditQuickTransaction(Transactions T)
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Task.Delay(1);

                await Shell.Current.GoToAsync($"{nameof(MainPage)}/{nameof(AddTransaction)}?BudgetID={App.DefaultBudgetID}&TransactionID={T.TransactionID}&NavigatedFrom=ViewMainPage",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = T
                    });
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MainPage", "EditQuickTransaction");
            }
        }
        
    }
}
