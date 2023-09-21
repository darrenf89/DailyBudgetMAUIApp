using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    public partial class CreatNewBudgetViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;

        [ObservableProperty]
        private int _budgetID;

        [ObservableProperty]
        private BudgetSettings _budgetSettings;

        [ObservableProperty]
        private Budgets _budget;

        public CreatNewBudgetViewModel(IRestDataService ds, IProductTools pt)
        {
            _ds = ds;
            _pt = pt;  

            var popup = new PopUpPage();
            _pt.ShowPopup(popup);

            try
            {

                if(BudgetID == 0)
                {
                    //TODO: Create a new budget because a previous budget hasn't been passed.
                    BudgetID = _ds.CreateNewBudget(App.UserDetails.Email).Result.BudgetID;

                    if (BudgetID != 0)
                    {
                        Budget = _ds.GetBudgetDetailsAsync(BudgetID, "Full").Result; 
                    }
                    else
                    {
                        throw new Exception("Couldn't create a new budget in the database when trying to set one up");
                    }
                }
                else
                {
                    //TODO: Get the current budgets details
                    Budget = _ds.GetBudgetDetailsAsync(BudgetID, "Full").Result;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" --> {ex.Message}");
                ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "Constructor").Result;
                popup.Close();
                Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }

            popup.Close();
        }
    }
}
