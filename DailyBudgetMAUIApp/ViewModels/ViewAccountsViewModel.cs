using DailyBudgetMAUIApp.DataServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Pages;
using System.Globalization;
using The49.Maui.BottomSheet;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewAccountsViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;
        private readonly IModalPopupService _ps;

        [ObservableProperty]
        public partial List<BankAccounts> BankAccounts { get; set; } = new List<BankAccounts>();

        [ObservableProperty]
        public partial double SignOutButtonWidth { get; set; }

        [ObservableProperty]
        public partial string BankBalance { get; set; }

        [ObservableProperty]
        public partial int AccountCount { get; set; }


        public ViewAccountsViewModel(IProductTools pt, IRestDataService ds, IModalPopupService ps)
        {
            _ds = ds;
            _pt = pt;
            _ps = ps;

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            SignOutButtonWidth = ScreenWidth - 60;

        }

        public async Task GetBankAccountDetails() 
        { 
            BankAccounts = await _ds.GetBankAccounts(App.DefaultBudgetID);
            AccountCount = BankAccounts.Count();
            BankBalance = App.DefaultBudget.BankBalance.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
        }

        public async Task HandleException(Exception ex, string Page, string Method)
        {
            await _pt.HandleException(ex, Page, Method);
        }

        [RelayCommand]
        public async Task AddNewAccount()
        {
            try
            {
                MultipleAccountsBottomSheet page = new MultipleAccountsBottomSheet(_pt, _ds, _ps);

                page.Detents = new DetentsCollection()
                {
                    new ContentDetent(),
                    new FullscreenDetent()
                };

                page.HasBackdrop = true;
                page.CornerRadius = 0;

                App.CurrentBottomSheet = page;

                await page.ShowAsync();
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewAccounts", "AddNewAccount");
            }
        }

        [RelayCommand]
        public async Task SpendFromAccount(object parameter)
        {
            try
            {
                BankAccounts Account = (BankAccounts)parameter;
                bool result = await Shell.Current.DisplayAlert($"Spend money?", $"Are you sure you want to take money out of {Account.BankAccountName}?", "Yes", "Cancel");

                if (result)
                {
                    Transactions T = new Transactions
                    {
                        TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow),
                        AccountID = Account.ID
                    };

                    await Shell.Current.GoToAsync($"/{nameof(AddTransaction)}?BudgetID={App.DefaultBudget.BudgetID}&NavigatedFrom=ViewAccounts&TransactionID=0",
                        new Dictionary<string, object>
                        {
                            ["Transaction"] = T
                        });
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewAccounts", "SpendFromAccount");
            }
        }

        [RelayCommand]
        public async Task DeleteAccount(object parameter)
        {
            try
            {
                BankAccounts Account = (BankAccounts)parameter;
                bool result = await Shell.Current.DisplayAlert($"Delete Account?", $"Are you sure you want to delete this account. Any money in it will be removed from your budget.", "Yes", "Cancel");

                if (result)
                {
                    if (Account.IsDefaultAccount)
                    {
                        await Shell.Current.DisplayAlert($"Can't delete this account", $"You can't delete your default account, turn off multiple accounts or change your default budget!", "Ok");
                        return;
                    }
                    
                    decimal Balance = Account.AccountBankBalance.GetValueOrDefault();
                    await Task.Delay(1);

                    List<PatchDoc> BudgetUpdate = new List<PatchDoc>();
                    PatchDoc BankBalancePatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/BankBalance",
                        value = App.DefaultBudget.BankBalance - Balance
                    };

                    BudgetUpdate.Add(BankBalancePatch);
                    await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);

                    await _ds.DeleteBankAccount(Account.ID);
                    await GetBankAccountDetails();
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewAccounts", "DeleteAccount");
            }
        }

        [RelayCommand]
        public async Task MoveBalance(object parameter)
        {
            try
            {
                if(this.BankAccounts.Count() < 2)
                {
                    await Shell.Current.DisplayAlert($"Only one account", $"You can't move balances unless you have more than one account, add another account to move balances.", "Ok");
                    return;
                }

                BankAccounts Account = (BankAccounts)parameter;

                var queryAttributes = new Dictionary<string, object>
                {
                    [nameof(PopupMoveAccountBalanceViewModel.ToAccount)] = Account,
                    [nameof(PopupMoveAccountBalanceViewModel.FromAccounts)] = this.BankAccounts
                };

                var popupOptions = new PopupOptions
                {
                    CanBeDismissedByTappingOutsideOfPopup = false,
                    PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
                };

                IPopupResult<string> popupResult = await _ps.PopupService.ShowPopupAsync<PopupMoveAccountBalance, string>(
                    Shell.Current,
                    options: popupOptions,
                    shellParameters: queryAttributes,
                    cancellationToken: CancellationToken.None
                );

                await Task.Delay(100);
                if (popupResult.Result == "OK")
                {
                    await GetBankAccountDetails();
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewAccounts", "MoveBalance");
            }
        }
        [RelayCommand]
        public async Task EditAccount(object parameter)
        {
            try
            {
                BankAccounts Account = (BankAccounts)parameter;
                MultipleAccountsBottomSheet page = new MultipleAccountsBottomSheet(_pt, _ds, _ps);

                page.Detents = new DetentsCollection()
                {
                    new ContentDetent(),
                    new FullscreenDetent()
                };

                page.HasBackdrop = true;
                page.CornerRadius = 0;

                App.CurrentBottomSheet = page;

                await page.ShowAsync();
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewAccounts", "EditAccount");
            }
        }
    }
}
