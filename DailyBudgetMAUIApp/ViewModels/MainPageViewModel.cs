using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Popups;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Syncfusion.Maui.Carousel;
using System.Collections.ObjectModel;
using Syncfusion.Maui.Scheduler;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(SnackBar), nameof(SnackBar))]
    [QueryProperty(nameof(SnackID), nameof(SnackID))]
    public partial class MainPageViewModel : BaseViewModel  
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;

        [ObservableProperty]
        private int  defaultBudgetID;

        [ObservableProperty]
        private Budgets  defaultBudget = new Budgets();

        [ObservableProperty]
        private bool  isBudgetCreated;
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
        private ObservableCollection<SchedulerAppointment>  eventList = new ObservableCollection<SchedulerAppointment>();

        public MainPageViewModel(IRestDataService ds, IProductTools pt)
        {
            _ds = ds;
            _pt = pt;
            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            ProgressBarWidthRequest = ScreenWidth - 85;
            SignOutButtonWidth = ScreenWidth - 30;
            QuickTransactionWidth = ScreenWidth - 180;
            QuickTransactionInputWidth = QuickTransactionWidth - 70;
            ProgressBarCarWidthRequest = ScreenWidth - 115;

            ChartBrushes = App.ChartBrush;
        }

        [RelayCommand]
        private async void GoToAccountSettings(object obj)
        {

        }

        [RelayCommand]
        private async void GoToBudgetSettings(object obj)
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.MainPage.ShowPopup(PopUp);
            }

            await Task.Delay(500);

            EditBudgetSettings page = new EditBudgetSettings(new EditBudgetSettingsViewModel(new ProductTools(new RestDataService()), new RestDataService()));

            await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
        }

        [RelayCommand]
        async void NavigateCreateNewBudget()
        {
            var page = new LoadingPage();
            await Application.Current.MainPage.Navigation.PushModalAsync(page);

            await Application.Current.MainPage.Navigation.PopModalAsync();
            await Shell.Current.GoToAsync($"{nameof(CreateNewBudget)}?BudgetID={DefaultBudgetID}&NavigatedFrom=Budget Settings");
        }


        [RelayCommand]
        async void SignOut()
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

        [RelayCommand]
        async void RefreshPage()
        {
            DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID, "Full").Result;

            App.DefaultBudget = DefaultBudget;
            IsBudgetCreated = App.DefaultBudget.IsCreated;
            App.SessionLastUpdate = DateTime.UtcNow;

            BudgetSettingValues Settings = _ds.GetBudgetSettingsValues(App.DefaultBudgetID).Result;
            App.CurrentSettings = Settings;

            EnvelopeStats = new EnvelopeStats(DefaultBudget.Savings);

            IsRefreshing = false;

        }

    }
}
