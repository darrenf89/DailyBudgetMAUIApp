using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;


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
        public MainPageViewModel(IRestDataService ds, IProductTools pt)
        {
            var popup = new PopUpPage();
            Application.Current.MainPage.ShowPopup(popup);
            
            Title = "Home Page!";
            _ds = ds;
            _pt = pt;

            DefaultBudgetID = Preferences.Get(nameof(App.DefaultBudgetID),1);

            if(App.DefaultBudget == null)
            {
                DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID).Result;

                App.DefaultBudget = DefaultBudget;
                App.SessionLastUpdate = DateTime.UtcNow;
            }      
            else
            {
                if (App.SessionLastUpdate == default(DateTime))
                {

                    DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID).Result;

                    App.DefaultBudget = DefaultBudget;
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
                            App.SessionLastUpdate = DateTime.UtcNow;
                        }
                    }
                }
            }

            popup.Close();

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
