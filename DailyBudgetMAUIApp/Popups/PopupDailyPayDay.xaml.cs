using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.ViewModels;
using System.Globalization;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailyPayDay : Popup<Object>
{
    private readonly PopupDailyPayDayViewModel _vm;
    private readonly IProductTools _pt;

    public PopupDailyPayDay(PopupDailyPayDayViewModel viewModel, IProductTools pt)
	{
        InitializeComponent();
        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        Loaded += async (s, e) => await Load();
    }

    private async Task Load()
    {
        await Task.Delay(1);

        if (_vm.IsPayNow)
        {
            _vm.OriginalDate = _vm.Budget.NextIncomePayday.GetValueOrDefault();
            _vm.OriginalAmount = _vm.Budget.PaydayAmount.GetValueOrDefault();

            hslPayDayAmount.IsVisible = true;
            hslNextIncomePayday.IsVisible = true;

            double PayDayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
            lblPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);

            string NextIncomePayday = _vm.Budget.NextIncomePayday.GetValueOrDefault().ToShortDateString();
            lblNextIncomePayday.Text = NextIncomePayday;

            lblTitle.Text = App.IsFamilyAccount ? "Time for your allowance" : "Transact Payday now!";
            lblOne.Text = App.IsFamilyAccount ? "Nice, its time for your allowance to be paid. Check with the owner of your parent budget to make sure they have given you the money." : "No better time of the year, month or day ... than pay day";
            lblTwo.Text = App.IsFamilyAccount ? "Once you have the money, lets allocate it out see what you have left and have some fun!" : "Or do we have it wrong, is pay day not today? Or did you get paid more ... or less this time?!";
            btnUpdate.Text = "No, go back!";

            if (App.IsFamilyAccount)
            {
                btnUpdate.IsVisible = false;
                btnFinsih.Text = "OK";
            }
        }
        else
        {
            _vm.OriginalDate = _vm.Budget.NextIncomePayday.GetValueOrDefault();
            _vm.OriginalAmount = _vm.Budget.PaydayAmount.GetValueOrDefault();

            hslPayDayAmount.IsVisible = true;
            hslNextIncomePayday.IsVisible = true;

            double PayDayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
            lblPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);

            string NextIncomePayday = _vm.Budget.NextIncomePayday.GetValueOrDefault().ToShortDateString();
            lblNextIncomePayday.Text = NextIncomePayday;

            lblTitle.Text = App.IsFamilyAccount ? "Time for your allowance" : "Transact Payday now!";

            if (App.IsFamilyAccount)
            {
                btnUpdate.IsVisible = false;
                btnFinsih.Text = "OK";
            }
        }
            
    }

    void PayDayAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal PayDayAmount = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entPayDayAmount);

        _vm.Budget.PaydayAmount = PayDayAmount;
    }

    private async void Close_Saving(object sender, EventArgs e)
    {
        await CloseAsync("OK");
    }

    private async void Update_Saving(object sender, EventArgs e)
    {
        if(_vm.IsPayNow)
        {
            await CloseAsync("Cancel");
        }
        else
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

        if (_vm.Budget.NextIncomePayday < _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
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

    private async void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {
            await CloseAsync(_vm.Budget);
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