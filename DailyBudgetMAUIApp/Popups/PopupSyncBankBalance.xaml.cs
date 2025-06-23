using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupSyncBankBalance : Popup<Object>
{
    private readonly PopupSyncBankBalanceViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public PopupSyncBankBalance(Budgets Budget, PopupSyncBankBalanceViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        InitializeComponent(); 
        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        Loaded += async (s, e) => await Load(); 
    }
    private async Task Load()
    {
        await Task.Delay(1);
        _vm.OriginalAmount = _vm.Budget.BankBalance.GetValueOrDefault();
        _vm.Amount = _vm.Budget.BankBalance.GetValueOrDefault();

        hslPayDayAmount.IsVisible = true;

        double PayDayAmount = (double?)_vm.Amount ?? 0;
        entPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);
    }

    void PayDayAmount_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            decimal PayDayAmount = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entPayDayAmount);

            _vm.Amount = PayDayAmount;
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "PopupSyncBankBalance", "PayDayAmount_Changed");
        }
    }

    private bool ValidatePage()
    {
        bool IsValid = true;

        if(_vm.Budget.PaydayAmount == 0)
        {
            IsValid = false;
            validatorPayDayAmount.IsVisible = true;
        }
        else
        {
            validatorPayDayAmount.IsVisible = false;
        }

        return IsValid;
    }

    private async void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        try
        {
            if (ValidatePage())
            {
                List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                PatchDoc BankBalance = new PatchDoc
                {
                    op = "replace",
                    path = "/BankBalance",
                    value = _vm.Amount
                };

                BudgetUpdate.Add(BankBalance);

                await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);

                App.DefaultBudget.BankBalance = _vm.Amount;
                await CloseAsync(_vm.Budget);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "PopupSyncBankBalance", "AcceptUpdate_Saving");
        }
    }

    private async void CancelUpdate_Saving(object sender, EventArgs e)
    {
        await CloseAsync("Closed");
    }

    private void Reset_Saving(object sender, EventArgs e)
    {
        grdUpdateBtns.IsVisible = true;

        double PayDayAmount = (double?)_vm.OriginalAmount ?? 0;
        entPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);

    }
}