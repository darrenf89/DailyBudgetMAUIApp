using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    public partial class CreateNewBudgetViewModel : BaseViewModel
    {

        [ObservableProperty]
        private int _budgetID;

        [ObservableProperty]
        private BudgetSettings _budgetSettings;

        [ObservableProperty]
        private Budgets _budget;

        public CreateNewBudgetViewModel()
        {

        }

    }
}
