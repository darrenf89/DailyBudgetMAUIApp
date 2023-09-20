using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Globalization;


namespace DailyBudgetMAUIApp.ViewModels
{
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

        public MainPageViewModel(IRestDataService ds, IProductTools pt)
        {

            if (DateTime.Now.Hour > 12)
            {
                Title = $"Good afternoon {App.UserDetails.NickName}!";
            }
            else
            {
                Title = $"Good morning {App.UserDetails.NickName}!";
            }

            _ds = ds;
            _pt = pt;

            DefaultBudgetID = Preferences.Get(nameof(App.DefaultBudgetID),1);

            if (App.DefaultBudgetID != 0)
            {
                if (App.CurrentSettings == null)
                {
                    BudgetSettingValues Settings = _ds.GetBudgetSettings(App.DefaultBudgetID).Result;
                    App.CurrentSettings = Settings;
                }

                CultureInfo CultureSetting = new CultureInfo("en-gb");

                CultureSetting.NumberFormat.CurrencySymbol = App.CurrentSettings.CurrencySymbol;
                CultureSetting.NumberFormat.CurrencyDecimalSeparator = App.CurrentSettings.CurrencyDecimalSeparator;
                CultureSetting.NumberFormat.CurrencyGroupSeparator = App.CurrentSettings.CurrencyGroupSeparator;
                CultureSetting.NumberFormat.CurrencyDecimalDigits = App.CurrentSettings.CurrencyDecimalDigits;
                CultureSetting.NumberFormat.CurrencyPositivePattern = App.CurrentSettings.CurrencyPositivePattern;
                CultureSetting.DateTimeFormat.ShortDatePattern = App.CurrentSettings.ShortDatePattern;
                CultureSetting.DateTimeFormat.DateSeparator = App.CurrentSettings.DateSeparator;

                CultureInfo.CurrentCulture = CultureSetting;
            }
            else
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-gb");
            }

            if (App.DefaultBudget == null)
            {
                DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID).Result;

                App.DefaultBudget = DefaultBudget;
                IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;
            }      
            else
            {
                if (App.SessionLastUpdate == default(DateTime))
                {

                    DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID).Result;

                    App.DefaultBudget = DefaultBudget;
                    IsBudgetCreated = App.DefaultBudget.IsCreated;
                    App.SessionLastUpdate = DateTime.UtcNow;

                }
                else
                {
                    if(DateTime.UtcNow.Subtract(App.SessionLastUpdate) > new TimeSpan(0,0,3,0))
                    {
                        DateTime LastUpdate = _ds.GetBudgetLastUpdatedAsync(DefaultBudgetID).Result;

                        if (App.SessionLastUpdate < LastUpdate)
                        {
                            DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID).Result;
                            App.DefaultBudget = DefaultBudget;
                            IsBudgetCreated = App.DefaultBudget.IsCreated;
                            App.SessionLastUpdate = DateTime.UtcNow;
                        }
                    }
                }
            }

        }

        [ICommand]
        async void NavigateCreateNewBudget()
        {
            await Shell.Current.GoToAsync($"{nameof(CreatNewBudget)}?BudgetID={DefaultBudgetID}");
        }


        [ICommand]
        async void SignOut()
        {
            if (Preferences.ContainsKey(nameof(App.UserDetails)))
            {
                Preferences.Remove(nameof(App.UserDetails));
            }

            if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
            {
                Preferences.Remove(nameof(App.DefaultBudgetID));
            }

            await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
        }

    }
}
