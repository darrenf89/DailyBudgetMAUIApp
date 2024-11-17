using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
        private int  budgetID;
        [ObservableProperty]
        private int  transactionID;
        [ObservableProperty]
        private Transactions?  transaction = null;
        [ObservableProperty]
        private bool  isPageValid;
        [ObservableProperty]
        private bool  isFutureDatedTransaction;
        [ObservableProperty]
        private bool  isPayee;
        [ObservableProperty]
        private bool  isAccount;
        [ObservableProperty]
        private bool isMultipleAccounts;
        [ObservableProperty]
        private bool  isSpendCategory;
        [ObservableProperty]
        private bool  isNote;
        [ObservableProperty]
        private string  navigatedFrom;
        [ObservableProperty]
        private string redirectTo;
        [ObservableProperty]
        private string? defaultAccountName;
        [ObservableProperty]
        private List<BankAccounts> bankAccounts;
        [ObservableProperty]
        private BankAccounts? selectedBankAccount;



        public AddTransactionViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Add a New Transaction";            
        }

        [RelayCommand]
        public async void BackButton()
        {
            try
            {
                if (string.Equals(RedirectTo, "ViewTransactions",StringComparison.OrdinalIgnoreCase))
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(ViewTransactions)}");
                }
                else if (string.Equals(RedirectTo, "ViewSavings", StringComparison.OrdinalIgnoreCase))
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(ViewSavings)}");
                }
                else if (string.Equals(RedirectTo, "ViewEnvelopes", StringComparison.OrdinalIgnoreCase))
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(ViewEnvelopes)}");
                }
                else if (string.Equals(RedirectTo, "ViewMainPage", StringComparison.OrdinalIgnoreCase))
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                }
                else if (string.Equals(RedirectTo, "ViewAccounts", StringComparison.OrdinalIgnoreCase))
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"..");
                }
                else
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "AddTransaction", "BackButton");
            }
        }

    }
}
