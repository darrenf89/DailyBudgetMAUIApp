using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailyAllowance : Popup<Object>
{
    private readonly PopupDailyAllowanceViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public PopupDailyAllowance(PopupDailyAllowanceViewModel viewModel, IProductTools pt, IRestDataService ds)
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
        _vm.OriginalDate = _vm.Budget.NextIncomePayday.GetValueOrDefault();
        _vm.OriginalAmount = _vm.Budget.PaydayAmount.GetValueOrDefault();

        hslPayDayAmount.IsVisible = true;
        hslNextIncomePayday.IsVisible = true;

        double PayDayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
        lblPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);

        string NextIncomePayday = _vm.Budget.NextIncomePayday.GetValueOrDefault().ToShortDateString();
        lblNextIncomePayday.Text = NextIncomePayday;

        if (_vm.Type == "Parent")
        {
            lblTitle.Text = $"Time to pay {_vm.NickName} their allowance!";

            if(_vm.Budget.NextIncomePayday.GetValueOrDefault().Date == DateTime.UtcNow.Date)
            {
                _vm.TextOne = $"{_vm.NickName} is due their allowance of {PayDayAmount.ToString("c", CultureInfo.CurrentCulture)} today, click confirm to take the money from your account now!";
            }
            else
            {
                _vm.TextOne = $"{_vm.NickName} was due their allowance of {PayDayAmount.ToString("c", CultureInfo.CurrentCulture)} on {NextIncomePayday}, click confirm to take the money from your account now!";
            }

            _vm.TextTwo = $"If this isn't the correct amount or the correct date you can update and correct the allowance now. If your are no longer paying an allowance to {_vm.NickName} and they aren't using their account you can deactivate the account!";
        }
        else if(_vm.Type == "ParentProcessed")
        {
            lblTitle.Text = $"You need to pay {_vm.NickName} their allowance!";

            if (_vm.Budget.NextIncomePayday.GetValueOrDefault().Date == DateTime.UtcNow.Date)
            {
                _vm.TextOne = $"{_vm.NickName} was paid their allowance of {PayDayAmount.ToString("c", CultureInfo.CurrentCulture)} of today.";
            }
            else
            {
                _vm.TextOne = $"{_vm.NickName} was paid their allowance of {PayDayAmount.ToString("c", CultureInfo.CurrentCulture)} on {NextIncomePayday}.";
            }

            _vm.TextTwo = $"We will add a transaction to your budget to cover the cost of the allowance. don't forget to pay {_vm.NickName} in real life!";

            btnDeactivate.IsVisible = false;
            btnUpdate.IsVisible = false;
            btnFinsih.Text = "Ok";
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

    private async void Deactivate_account(object sender, EventArgs e)
    {
        bool DefaultBudgetYesNo = await Application.Current.Windows[0].Page.DisplayAlert($"Deactivate Account?", $"Are you sure you want to deactivate this users account?", "Yes, continue", "No Thanks!");

        if (DefaultBudgetYesNo)
        {
            List<PatchDoc> UpdateUserDetails = new List<PatchDoc>();

            PatchDoc IsActive = new PatchDoc
            {
                op = "replace",
                path = "/IsActive",
                value = false
            };

            UpdateUserDetails.Add(IsActive);

            await _ds.PatchFamilyUserAccount(_vm.UserID, UpdateUserDetails);

            await CloseAsync("DEACTIVATE");
        }               
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

            List<PatchDoc> UpdateBudget = new List<PatchDoc>();

            PatchDoc PaydayAmount = new PatchDoc
            {
                op = "replace",
                path = "/PaydayAmount",
                value = _vm.Budget.PaydayAmount
            };

            UpdateBudget.Add(PaydayAmount);

            PatchDoc NextIncomePayday = new PatchDoc
            {
                op = "replace",
                path = "/NextIncomePayday",
                value = _vm.Budget.NextIncomePayday
            };

            UpdateBudget.Add(NextIncomePayday);

            await _ds.PatchBudget(_vm.Budget.BudgetID, UpdateBudget);

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