using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Plugin.Maui.AppRating;
using Plugin.MauiMTAdmob;
using Syncfusion.Maui.Carousel;
using Syncfusion.Maui.Scheduler;
using System.Collections.ObjectModel;
using System.Globalization;
using static DailyBudgetMAUIApp.Pages.ViewAccounts;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(SnackBar), nameof(SnackBar))]
    [QueryProperty(nameof(SnackID), nameof(SnackID))]
    public partial class FamilyAccountMainPageViewModel : BaseViewModel  
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;
        private readonly IAppRating _ar;

        [ObservableProperty]
        public partial int DefaultBudgetID { get; set; }

        [ObservableProperty]
        public partial Budgets DefaultBudget { get; set; } = new Budgets();

        [ObservableProperty]
        public partial List<Savings> CarouselSavings { get; set; }

        [ObservableProperty]
        public partial List<Savings> Envelopes { get; set; }

        [ObservableProperty]
        public partial List<Bills> CarouselBills { get; set; }

        [ObservableProperty]
        public partial List<IncomeEvents> CarouselIncomes { get; set; }

        [ObservableProperty]
        public partial List<Categories> CategoryList { get; set; }

        [ObservableProperty]
        public partial Transactions PendingQuickTransaction { get; set; }

        [ObservableProperty]
        public partial bool IsBudgetCreated { get; set; }

        [ObservableProperty]
        public partial bool IsPreviousBudget { get; set; }

        [ObservableProperty]
        public partial bool IsRefreshing { get; set; }

        [ObservableProperty]
        public partial string SnackBar { get; set; }

        [ObservableProperty]
        public partial int SnackID { get; set; }

        [ObservableProperty]
        public partial double ProgressBarWidthRequest { get; set; }

        [ObservableProperty]
        public partial double ProgressBarCarWidthRequest { get; set; }

        [ObservableProperty]
        public partial EnvelopeStats EnvelopeStats { get; set; }

        [ObservableProperty]
        public partial double SignOutButtonWidth { get; set; }

        [ObservableProperty]
        public partial double ScreenWidth { get; set; }

        [ObservableProperty]
        public partial SfCarousel SavingCarousel { get; set; }

        [ObservableProperty]
        public partial SfCarousel BillCarousel { get; set; }

        [ObservableProperty]
        public partial SfCarousel IncomeCarousel { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<Transactions> RecentTransactions { get; set; } = new ObservableCollection<Transactions>();

        [ObservableProperty]
        public partial double RecentTransactionsHeight { get; set; } = 452;

        [ObservableProperty]
        public partial decimal MaxBankBalance { get; set; } = 0;

        [ObservableProperty]
        public partial decimal TransactionAmount { get; set; }

        [ObservableProperty]
        public partial double QuickTransactionWidth { get; set; }

        [ObservableProperty]
        public partial double QuickTransactionInputWidth { get; set; }

        [ObservableProperty]
        public partial decimal FutureDailySpend { get; set; }

        [ObservableProperty]
        public partial List<Payees> Payees { get; set; } = new List<Payees>();

        [ObservableProperty]
        public partial List<Brush> ChartBrushes { get; set; } = new List<Brush>();

        [ObservableProperty]
        public partial bool PayeeChartVisible { get; set; } = true;

        [ObservableProperty]
        public partial bool CategoryChartVisible { get; set; } = true;

        [ObservableProperty]
        public partial bool IsUnreadMessage { get; set; }

        [ObservableProperty]
        public partial bool IsQuickTransaction { get; set; }

        [ObservableProperty]
        public partial bool IsTopStickyVisible { get; set; }

        [ObservableProperty]
        public partial List<Budgets> QuickTransactionBudgets { get; set; } = new List<Budgets>();

        [ObservableProperty]
        public partial Budgets SelectedQuickTransactionBudget { get; set; }

        [ObservableProperty]
        public partial string QuickTransactionAmount { get; set; }

        [ObservableProperty]
        public partial string BudgetVisibilityText { get; set; }

        [ObservableProperty]
        public partial string BudgetVisibilityHeader { get; set; }

        [ObservableProperty]
        public partial int NumberPendingQuickTransactions { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<SchedulerAppointment> EventList { get; set; } = new ObservableCollection<SchedulerAppointment>();


        public delegate void ReloadPageAction();
        public event ReloadPageAction ReloadPage;

        public FamilyAccountMainPageViewModel(IRestDataService ds, IProductTools pt, IAppRating ar)
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "QuickTransaction");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "QuickTransaction");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "ViewSupports");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "GoToBudgetSettings");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "SignOut");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "RefreshPage");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "SpendEnvelope");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "EditEnvelope");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "AddNewEnvelope");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "RecalculateBudget");
            }
        }

        [RelayCommand]
        async Task UpdateBudgetVisibility()
        {
            try
            {

                bool DefaultBudgetYesNo = await Application.Current.Windows[0].Page.DisplayAlert($"{(App.FamilyUserDetails.IsBudgetHidden ? "Show full budget details?" : "Hide budget Details?")}", $"{(App.FamilyUserDetails.IsBudgetHidden ? "Are you sure you want to update your account so your parent account can see the full details?" : "Are you sure you want to update your account so you hide the detailed information of your budget from the parent account?")}", "Yes, continue", "No Thanks!");

                if (DefaultBudgetYesNo)
                {

                    App.FamilyUserDetails.IsBudgetHidden = !App.FamilyUserDetails.IsBudgetHidden;

                    List<PatchDoc> UserUpdate = new List<PatchDoc>();

                    PatchDoc IsBudgetHidden = new PatchDoc
                    {
                        op = "replace",
                        path = "/IsBudgetHidden",
                        value = App.FamilyUserDetails.IsBudgetHidden
                    };

                    UserUpdate.Add(IsBudgetHidden);

                    await _ds.PatchFamilyUserAccount(App.FamilyUserDetails.UserID, UserUpdate);

                    if (App.FamilyUserDetails.IsBudgetHidden)
                    {
                        BudgetVisibilityHeader = "Your budget is hidden";
                        BudgetVisibilityText = "Your budget's details are hidden from the parent account, click to show them.";
                    }
                    else
                    {
                        BudgetVisibilityHeader = "Careful all budget details are visible";
                        BudgetVisibilityText = "Your budget is fully visible to your parent account, click to hide it.";
                    }
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountMainPage", "UpdateBudgetVisibility");
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
                await _pt.HandleException(ex, "FamilyAccountMainPage", "EditQuickTransaction");
            }
        }        
        
    }
}
