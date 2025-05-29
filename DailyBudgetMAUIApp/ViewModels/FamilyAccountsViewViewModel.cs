using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Collections.ObjectModel;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(FamilyAccountID), nameof(FamilyAccountID))]

    public partial class FamilyAccountsViewViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        public BorderlessPicker switchBudgetPicker;
        [ObservableProperty]
        public List<FamilyUserAccount> familyUserAccounts = new List<FamilyUserAccount>();
        [ObservableProperty]
        public FamilyUserAccount familyUserAccount;
        [ObservableProperty]
        public Budgets budget;
        [ObservableProperty]
        public int familyAccountID;

        [ObservableProperty]
        private List<Savings> carouselSavings;
        [ObservableProperty]
        private List<Savings> carouselEnvelopes;
        [ObservableProperty]
        private List<Bills> carouselBills;
        [ObservableProperty]
        private List<IncomeEvents> carouselIncomes;
        [ObservableProperty]
        private ObservableCollection<Transactions> recentTransactions = new ObservableCollection<Transactions>();

        [ObservableProperty]
        private bool isBillVisible;
        [ObservableProperty]
        private bool isSavingVisible;
        [ObservableProperty]
        private bool isEnvelopeVisible;
        [ObservableProperty]
        private bool isIncomeVisible;
        [ObservableProperty]
        private bool isTransactionVisible;
        [ObservableProperty]
        private bool isBudgetVisible = true;

        [ObservableProperty]
        private double recentTransactionsHeight = 452;
        [ObservableProperty]
        private double borderWidth;
        [ObservableProperty]
        private double progressBarWidthRequest;
        [ObservableProperty]
        private double signOutButtonWidth;
        [ObservableProperty]
        private double screenWidth;

        [ObservableProperty]
        private decimal maxBankBalance = 0;

        public FamilyAccountsViewViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Family Account Details";
            borderWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 20;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            ProgressBarWidthRequest = ScreenWidth - 85;
        }

        public async Task OnLoad()
        {
            var Accounts = await _ds.GetUserFamilyAccounts(App.UserDetails.UserID);
            FamilyUserAccounts = Accounts.Where(a => a.IsActive).ToList();

            if(FamilyUserAccounts.Count == 0)
            {
                IsBudgetVisible = false;
                return;
            }

            if (FamilyAccountID == 0)
            {
                Budget = await _ds.GetBudgetDetailsAsync(FamilyUserAccounts[0].BudgetID, "Full");
                FamilyUserAccount = FamilyUserAccounts[0];
                FamilyAccountID = FamilyUserAccounts[0].UserID;
            }
            else
            {
                FamilyUserAccount = FamilyUserAccounts.FirstOrDefault(a => a.UserID == FamilyAccountID);
                Budget = await _ds.GetBudgetDetailsAsync(FamilyUserAccount.BudgetID, "Full");
            }

            Application.Current.Resources.TryGetValue("White", out var White);
            Application.Current.Resources.TryGetValue("PrimaryDark", out var PrimaryDark);
            Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

            BorderlessPicker picker = new BorderlessPicker
            {
                Title = "Select a user",
                ItemsSource = FamilyUserAccounts,
                TitleColor = (Color)Gray900,
                BackgroundColor = (Color)White,
                TextColor = (Color)PrimaryDark,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 16
            };

            picker.ItemDisplayBinding = new Binding("NickName", BindingMode.Default);
            picker.SelectedIndexChanged += async (s, e) =>
            {
                IsPageBusy = true;
                await Task.Delay(1);

                var picker = (Picker)s;
                var SelectedAccount = (FamilyUserAccount)picker.SelectedItem;
                FamilyUserAccount = SelectedAccount;

                Budget = await _ds.GetBudgetDetailsAsync(SelectedAccount.BudgetID, "Full");
                FamilyAccountID = SelectedAccount.UserID;
                await LoadBudgetDetails();

                IsPageBusy = false;
            };

            for (int i = 0; i < FamilyUserAccounts.Count; i++)
            {
                if (FamilyUserAccounts[i].UserID == FamilyAccountID)
                {
                    picker.SelectedItem = FamilyUserAccounts[i];
                    FamilyUserAccount = FamilyUserAccounts[i];
                }
            }

            SwitchBudgetPicker = picker;

            await LoadBudgetDetails();
        }

        private async Task LoadBudgetDetails()
        {
            CarouselEnvelopes = Budget.Savings.Where(s => !s.IsRegularSaving).ToList();
            IsEnvelopeVisible = CarouselEnvelopes.Any() && !FamilyUserAccount.IsBudgetHidden;

            CarouselSavings = Budget.Savings.Where(s => s.IsRegularSaving).ToList();
            IsSavingVisible = CarouselSavings.Any() && !FamilyUserAccount.IsBudgetHidden;

            CarouselBills = Budget.Bills;
            IsBillVisible = CarouselBills.Any() && !FamilyUserAccount.IsBudgetHidden;

            CarouselIncomes = Budget.IncomeEvents;
            IsIncomeVisible = CarouselIncomes.Any() && !FamilyUserAccount.IsBudgetHidden;

            IsTransactionVisible = !FamilyUserAccount.IsBudgetHidden;

            RecentTransactions.Clear();
            List<Transactions> RecentTrans = await _ds.GetRecentTransactions(Budget.BudgetID, 6, "MainPage");            

            if (RecentTrans.Count() > 0)
            {
                foreach (Transactions T in RecentTrans)
                {
                    RecentTransactions.Add(T);
                }
            }

            RecentTransactionsHeight = (RecentTransactions.Count() * 60) + 22;

            MaxBankBalance = Budget.BankBalance.GetValueOrDefault();
            MaxBankBalance += Budget.CurrentActiveIncome;
        }

        [RelayCommand]
        async Task ViewAllTransactions()
        {
            try
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
                await Task.Delay(1);
                FilterModel Filters = new FilterModel
                {

                };

                await Shell.Current.GoToAsync($"/{nameof(ViewFilteredTransactions)}?BudgetID={FamilyUserAccount.BudgetID}",
                    new Dictionary<string, object>
                    {
                        ["Filters"] = Filters
                    });
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountView", "ViewAllTransactions");
            }
        }

        [RelayCommand]
        public async Task BackButton()
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

                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountView", "BackButton");
            }
        }
    }
}
