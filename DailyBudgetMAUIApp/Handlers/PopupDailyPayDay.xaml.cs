using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailyPayDay : Popup
{
    private readonly PopupDailyPayDayViewModel _vm;
    private readonly IProductTools _pt;

    public PopupDailyPayDay(Budgets Budget, PopupDailyPayDayViewModel viewModel, IProductTools pt)
	{
        InitializeComponent();

        viewModel.Budget = Budget;

        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;

        _vm.OriginalDate = _vm.Budget.NextIncomePayday.GetValueOrDefault();
        _vm.OriginalAmount = _vm.Budget.PaydayAmount.GetValueOrDefault();

        hslPayDayAmount.IsVisible = true;
        hslNextIncomePayday.IsVisible = true;

        double PayDayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
        lblPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);

        string NextIncomePayday = _vm.Budget.NextIncomePayday.GetValueOrDefault().ToShortDateString();
        lblNextIncomePayday.Text = NextIncomePayday;
   
    }

    void PayDayAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal PayDayAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);
        entPayDayAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entPayDayAmount.Text);

        _vm.Budget.PaydayAmount = PayDayAmount;
    }

    private void Close_Saving(object sender, EventArgs e)
    {
        this.Close("OK");
    }

    private void Update_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = true;

        lblPayDayAmount.IsVisible = false;
        lblNextIncomePayday.IsVisible = false;

        entPayDayAmount.IsVisible = true;
        entNextIncomePayday.IsVisible = true;

        double PaydayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
        entPayDayAmount.Text = PaydayAmount.ToString("c", CultureInfo.CurrentCulture);        
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

        if (_vm.Budget.NextIncomePayday < DateTime.UtcNow.Date)
        {
            IsValid = false;
            validatorNextIncomePayday.IsVisible = true;
        }
        else
        {
            validatorNextIncomePayday.IsVisible = false;
        }

        return IsValid;
    }

    private void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {
            this.Close(_vm.Budget);
        }
    }

    private void Reset_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = true;
        grdUpdateBtns.IsVisible = false;

        _vm.Budget.PaydayAmount = _vm.OriginalAmount;
        _vm.Budget.NextIncomePayday = _vm.OriginalDate;

        lblPayDayAmount.IsVisible = true;
        lblNextIncomePayday.IsVisible = true;

        entPayDayAmount.IsVisible = false;
        entNextIncomePayday.IsVisible = false;

        double PaydayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
        lblPayDayAmount.Text = PaydayAmount.ToString("c", CultureInfo.CurrentCulture);

        string GoalDate = _vm.Budget.NextIncomePayday.GetValueOrDefault().ToShortDateString();
        lblNextIncomePayday.Text = GoalDate;        

    }
}