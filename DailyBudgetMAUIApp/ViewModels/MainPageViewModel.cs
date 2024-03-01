using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Popups;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;
using Syncfusion.Maui.Carousel;
using System.Collections.ObjectModel;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(SnackBar), nameof(SnackBar))]
    [QueryProperty(nameof(SnackID), nameof(SnackID))]
    public partial class MainPageViewModel : BaseViewModel  
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;

        [ObservableProperty]
        private int _defaultBudgetID;

        [ObservableProperty]
        private Budgets _defaultBudget = new Budgets();

        [ObservableProperty]
        private bool _isBudgetCreated;
        [ObservableProperty]
        private bool _isRefreshing;
        [ObservableProperty]
        private string _snackBar;
        [ObservableProperty]
        private int _snackID;
        [ObservableProperty]
        private double _progressBarWidthRequest;
        [ObservableProperty]
        private double _progressBarCarWidthRequest;
        [ObservableProperty]
        private EnvelopeStats _envelopeStats;
        [ObservableProperty]
        private double _signOutButtonWidth;
        [ObservableProperty]
        private SfCarousel _savingCarousel;
        [ObservableProperty]
        private SfCarousel _billCarousel;
        [ObservableProperty]
        private SfCarousel _incomeCarousel;
        [ObservableProperty]
        private ObservableCollection<Transactions> _recentTransactions = new ObservableCollection<Transactions>();
        [ObservableProperty]
        private double _recentTransactionsHeight = 452;
        [ObservableProperty]
        private decimal _maxBankBalance = 0;
        [ObservableProperty]
        private decimal _transactionAmount;
        [ObservableProperty]
        private double _quickTransactionWidth;
        [ObservableProperty]
        private double _quickTransactionInputWidth;
        [ObservableProperty]
        private decimal _futureDailySpend;

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
        }        

        [ICommand]
        async void NavigateCreateNewBudget()
        {
            var page = new LoadingPage();
            await Application.Current.MainPage.Navigation.PushModalAsync(page);

            await Application.Current.MainPage.Navigation.PopModalAsync();
            await Shell.Current.GoToAsync($"{nameof(CreateNewBudget)}?BudgetID={DefaultBudgetID}&NavigatedFrom=Budget Settings");
        }


        [ICommand]
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

        [ICommand]
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
