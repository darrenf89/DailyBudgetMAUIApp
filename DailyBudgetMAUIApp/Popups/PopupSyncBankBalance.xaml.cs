using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupSyncBankBalance : Popup
{
    private readonly PopupSyncBankBalanceViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public PopupSyncBankBalance(Budgets Budget, PopupSyncBankBalanceViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        InitializeComponent();

        viewModel.Budget = Budget;        

        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        _vm.OriginalAmount = _vm.Budget.BankBalance.GetValueOrDefault();
        _vm.Amount = _vm.Budget.BankBalance.GetValueOrDefault();

        hslPayDayAmount.IsVisible = true;

        double PayDayAmount = (double?)_vm.Amount ?? 0;
        entPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);   
    }

    void PayDayAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal PayDayAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);
        int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
        if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entPayDayAmount.CursorPosition > position)
        {
            entPayDayAmount.CursorPosition = entPayDayAmount.Text.Length;
        }

        _vm.Amount = PayDayAmount;
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

    private void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {
            List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

            PatchDoc BankBalance = new PatchDoc
            {
                op = "replace",
                path = "/BankBalance",
                value = _vm.Amount
            };

            BudgetUpdate.Add(BankBalance);

            _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);

            App.DefaultBudget.BankBalance = _vm.Amount;
            this.Close(_vm.Budget);
        }
    }

    private void CancelUpdate_Saving(object sender, EventArgs e)
    {
        this.Close("Closed");
    }

    private void Reset_Saving(object sender, EventArgs e)
    {
        grdUpdateBtns.IsVisible = true;

        double PayDayAmount = (double?)_vm.OriginalAmount ?? 0;
        entPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);

    }
}