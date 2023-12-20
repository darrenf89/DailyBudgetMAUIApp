using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Globalization;
using System.Transactions;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(TransactionID), nameof(TransactionID))]
    [QueryProperty(nameof(Transaction), nameof(Transaction))]
    public partial class AddTransactionViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private int _transactionID;
        [ObservableProperty]
        private Transactions? _transaction = null;
        [ObservableProperty]
        private bool _isPageValid;
        [ObservableProperty]
        private bool _isFutureDatedTransaction;
        [ObservableProperty]
        private bool _isPayee;
        [ObservableProperty]
        private bool _isSpendCategory;
        [ObservableProperty]
        private bool _isNote;

        public AddTransactionViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Add a New Transaction";            
        }


    }
}
