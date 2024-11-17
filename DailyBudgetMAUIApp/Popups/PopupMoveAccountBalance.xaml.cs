using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;
using System;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupMoveAccountBalance : Popup
{
    private readonly PopupMoveAccountBalanceViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    private BankAccounts ToAccount;
    private List<BankAccounts> FromAccounts;

    public PopupMoveAccountBalance(BankAccounts Account, List<BankAccounts> BankAccounts, PopupMoveAccountBalanceViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        InitializeComponent();

        ToAccount = Account;
        FromAccounts = BankAccounts;
        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        _vm.Amount = 0;

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Danger", out var Danger);
        Application.Current.Resources.TryGetValue("pillSuccess", out var pillSuccess);
        Application.Current.Resources.TryGetValue("pillDanger", out var pillDanger);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        
        _vm.FromBalanceColor = (Color)Success;
        _vm.FromBalanceStyle = (Style)pillSuccess;
        _vm.ToBalanceColor = (Color)Success;
        _vm.ToBalanceStyle = (Style)pillSuccess;

        double Amount = 0;
        entAmount.Text = Amount.ToString("c", CultureInfo.CurrentCulture);

        foreach(BankAccounts account in BankAccounts)
        {
            MoveBalanceClass MoveBalance = new MoveBalanceClass
            {
                Name = account.BankAccountName,
                Id = account.ID,
                Type = "BankAccount",
                Balance = account.AccountBankBalance.GetValueOrDefault()
            };

            _vm.MoveBalances.Add(MoveBalance);
            if(Account.ID == account.ID)
            {
                _vm.ToSelectedMoveBalance = MoveBalance;
            }
            else if(_vm.FromSelectedMoveBalance == null)
            {
                _vm.FromSelectedMoveBalance = MoveBalance;
            }
        }
        _vm.FromEnabled = true;
        _vm.ToEnabled = false;        
    }

    void Amount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal PayDayAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);
        int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
        if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entAmount.CursorPosition > position)
        {
            entAmount.CursorPosition = entAmount.Text.Length;
        }  
        _vm.Amount = PayDayAmount;
    }

    private bool ValidatePage()
    {
        bool IsValid = true;

        if (_vm.Amount == 0)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Select an amount to move between accounts";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        if (_vm.ToSelectedMoveBalance is null)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Select an account to move a balance to";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        if (_vm.FromSelectedMoveBalance is null)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Select an account to move a balance from";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        decimal FromAmount = _vm.FromSelectedMoveBalance.Balance - _vm.Amount;
        if (FromAmount < 0)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Moving balance from this account will put it into negative, cant do that!";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        if (_vm.FromSelectedMoveBalance.Name == _vm.ToSelectedMoveBalance.Name && _vm.FromSelectedMoveBalance.Type == _vm.ToSelectedMoveBalance.Type && _vm.FromSelectedMoveBalance.Id == _vm.ToSelectedMoveBalance.Id)
        {
            validator.IsVisible = true;
            lblValidator.Text = "Can't move balance from an account to the same account, seems obvious to us!";
            return false;
        }
        else
        {
            validator.IsVisible = false;
        }

        return IsValid;
    }

    private async void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        try
        {
            if (ValidatePage())
            {
                bool MoveBalance = await Application.Current.MainPage.DisplayAlert($"Move money around?", $"Are you sure you want to move {entAmount.Text} from {_vm.FromSelectedMoveBalance.Name} to {_vm.ToSelectedMoveBalance.Name}", "Yes", "No");
                if (MoveBalance)
                {
                    decimal FromAmount = _vm.FromSelectedMoveBalance.Balance - _vm.Amount;
                    int FromID = _vm.FromSelectedMoveBalance.Id;

                    BankAccounts? FromAccount = FromAccounts.Where(a=> a.ID == FromID).FirstOrDefault();
                    FromAccount.AccountBankBalance = FromAmount;
                    await _ds.UpdateBankAccounts(App.DefaultBudgetID, FromAccount);

                    decimal ToAmount = _vm.ToSelectedMoveBalance.Balance + _vm.Amount; 
                    ToAccount.AccountBankBalance = ToAmount;
                    await _ds.UpdateBankAccounts(App.DefaultBudgetID, ToAccount);
                }
                await _pt.MakeSnackBar("We have updated your bank accounts balances, get budgeting", null, null, new TimeSpan(0, 0, 10), "Success");
                this.Close("OK");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "PopupMoveAccountBalance", "ChangeSelectedProfilePic");
        }
    }

    private void CancelUpdate_Saving(object sender, EventArgs e)
    {
        this.Close("Closed");
    }

}