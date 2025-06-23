using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupEditNextPayInfo : Popup<Object>
{
    private readonly PopupEditNextPayInfoViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public PopupEditNextPayInfo(PopupEditNextPayInfoViewModel viewModel, IProductTools pt, IRestDataService ds)
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
        _vm.Date = _vm.Budget.NextIncomePayday.GetValueOrDefault();
        _vm.Amount = _vm.Budget.PaydayAmount.GetValueOrDefault();

        hslPayDayAmount.IsVisible = true;
        hslNextIncomePayday.IsVisible = true;

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
            _pt.HandleException(ex, "PopupEditNextPayInfo", "PayDayAmount_Changed");
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
        try
        {
            if (ValidatePage())
            {
                List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                PatchDoc PayDayAmount = new PatchDoc
                {
                    op = "replace",
                    path = "/PayDayAmount",
                    value = _vm.Amount
                };

                BudgetUpdate.Add(PayDayAmount);

                PatchDoc NextIncomePayday = new PatchDoc
                {
                    op = "replace",
                    path = "/NextIncomePayday",
                    value = _vm.Date.Date
                };

                BudgetUpdate.Add(NextIncomePayday);

                await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);

                App.DefaultBudget.NextIncomePayday = _vm.Date.Date;
                App.DefaultBudget.PaydayAmount = _vm.Amount;
                await CloseAsync(_vm.Budget);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "PopupEditNextPayInfo", "AcceptUpdate_Saving");
        }
    }

    private async void CancelUpdate_Saving(object sender, EventArgs e)
    {
        await CloseAsync("Closed");
    }

    private void Reset_Saving(object sender, EventArgs e)
    {
        try
        {
            grdUpdateBtns.IsVisible = true;

            _vm.Budget.PaydayAmount = _vm.OriginalAmount;
            _vm.Budget.NextIncomePayday = _vm.OriginalDate;

            double PayDayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
            entPayDayAmount.Text = PayDayAmount.ToString("c", CultureInfo.CurrentCulture);

            entNextIncomePayday.Date = _vm.Budget.NextIncomePayday.GetValueOrDefault();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "PopupEditNextPayInfo", "Reset_Saving");
        }
    }
}