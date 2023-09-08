using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Diagnostics;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class MainPageViewModel : BaseViewModel  
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;
        public MainPageViewModel(IRestDataService ds, IProductTools pt)
        {
            Title = "Home Page!";
            _ds = ds;
            _pt = pt;

            DefaultBudgetID = Preferences.Get(nameof(App.DefaultBudgetID),1);

            if(!Preferences.ContainsKey(nameof(App.DefaultBudget)))
            {
                DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID).Result;
                IsBudgetUpdate = true;
            }      
            else
            {
                if (!Preferences.ContainsKey(nameof(App.SessionLastUpdate)))
                {
                    Preferences.Remove(nameof(App.DefaultBudget));

                    DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID).Result;
                    IsBudgetUpdate = true;
                }
                else
                {
                    if(DateTime.UtcNow.Subtract(App.SessionLastUpdate) > new TimeSpan(0,0,3,0))
                    {
                        DateTime LastUpdate = _ds.GetBudgetLastUpdatedAsync(DefaultBudgetID).Result;

                        if (App.SessionLastUpdate < LastUpdate)
                        {
                            Preferences.Remove(nameof(App.DefaultBudget));
                            Preferences.Remove(nameof(App.SessionLastUpdate));

                            DefaultBudget = _ds.GetBudgetDetailsAsync(DefaultBudgetID).Result;
                            IsBudgetUpdate = true;
                        }
                    }
                }
            }

        }

        [ObservableProperty]
        private int _defaultBudgetID;

        [ObservableProperty]
        private Budgets _defaultBudget;

        [ObservableProperty]
        private bool _isBudgetUpdate;

    }
}
