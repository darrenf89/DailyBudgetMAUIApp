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
    [QueryProperty(nameof(NavigatedFrom), nameof(NavigatedFrom))]
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
        [ObservableProperty]
        private string _navigatedFrom;



        public AddTransactionViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Add a New Transaction";            
        }

        [ICommand]
        public async void BackButton()
        {
            if (NavigatedFrom == "ViewTransactions")
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"//{nameof(ViewTransactions)}");
            }
            else if (NavigatedFrom == "ViewSavings")
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"//{nameof(ViewSavings)}");
            }
            else if (NavigatedFrom == "ViewEnvelopes")
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"//{nameof(ViewEnvelopes)}");
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            }
        }
    }
}
