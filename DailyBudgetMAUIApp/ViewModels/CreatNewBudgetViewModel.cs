using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    public partial class CreatNewBudgetViewModel : BaseViewModel
    {
        [ObservableProperty]
        private int _budgetID;

        [ObservableProperty]
        private BudgetSettings _budgetSettings;

        [ObservableProperty]
        private Budgets _budget;

        public CreatNewBudgetViewModel()
        {
            if(BudgetID == 0)
            {
                //TODO: Create a new budget because a previous budget hasn't been passed.
            }
            else
            {
                //TODO: Get the current budgets details
            }
        }
    }
}
