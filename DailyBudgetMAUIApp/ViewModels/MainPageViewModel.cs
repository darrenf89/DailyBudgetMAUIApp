using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Alerts;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Globalization;


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
        private Budgets _defaultBudget;

        [ObservableProperty]
        private bool _isBudgetCreated;
        [ObservableProperty]
        private bool _isRefreshing;
        [ObservableProperty]
        public string _snackBar = "";
        [ObservableProperty]
        public int _snackID = 0;

        public MainPageViewModel(IRestDataService ds, IProductTools pt)
        {
            _ds = ds;
            _pt = pt;
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
            DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID, "Limited").Result;

            App.DefaultBudget = DefaultBudget;
            IsBudgetCreated = App.DefaultBudget.IsCreated;
            App.SessionLastUpdate = DateTime.UtcNow;

            BudgetSettingValues Settings = _ds.GetBudgetSettingsValues(App.DefaultBudgetID).Result;
            App.CurrentSettings = Settings;

            IsRefreshing = false;

        }

    }
}
