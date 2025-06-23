using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;

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
        private readonly IPopupService _ps;

        [ObservableProperty]
        public partial int BudgetID { get; set; }

        [ObservableProperty]
        public partial int TransactionID { get; set; }

        [ObservableProperty]
        public partial Transactions? Transaction { get; set; } = null;

        [ObservableProperty]
        public partial bool IsPageValid { get; set; }

        [ObservableProperty]
        public partial bool IsFutureDatedTransaction { get; set; }

        [ObservableProperty]
        public partial bool IsPayee { get; set; }

        [ObservableProperty]
        public partial bool IsAccount { get; set; }

        [ObservableProperty]
        public partial bool IsAppearing { get; set; }

        [ObservableProperty]
        public partial bool IsMultipleAccounts { get; set; }

        [ObservableProperty]
        public partial bool IsSpendCategory { get; set; }

        [ObservableProperty]
        public partial bool IsNote { get; set; }

        [ObservableProperty]
        public partial string NavigatedFrom { get; set; }

        [ObservableProperty]
        public partial string RedirectTo { get; set; }

        [ObservableProperty]
        public partial string? DefaultAccountName { get; set; }

        [ObservableProperty]
        public partial List<BankAccounts> BankAccounts { get; set; }

        [ObservableProperty]
        public partial BankAccounts? SelectedBankAccount { get; set; }




        public AddTransactionViewModel(IProductTools pt, IRestDataService ds, IPopupService ps)
        {
            _pt = pt;
            _ds = ds;
            _ps = ps;

            Title = "Add a New Transaction";            
        }

        [RelayCommand]
        public async Task BackButton()
        {
            try
            {
                if (string.Equals(RedirectTo, "ViewTransactions",StringComparison.OrdinalIgnoreCase))
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Shell.Current.GoToAsync($"///{nameof(ViewTransactions)}");
                }
                else if (string.Equals(RedirectTo, "ViewSavings", StringComparison.OrdinalIgnoreCase))
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Shell.Current.GoToAsync($"///{nameof(ViewSavings)}");
                }
                else if (string.Equals(RedirectTo, "ViewEnvelopes", StringComparison.OrdinalIgnoreCase))
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Shell.Current.GoToAsync($"///{nameof(ViewEnvelopes)}");
                }
                else if (string.Equals(RedirectTo, "ViewMainPage", StringComparison.OrdinalIgnoreCase))
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                }
                else if (string.Equals(RedirectTo, "ViewAccounts", StringComparison.OrdinalIgnoreCase))
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
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
