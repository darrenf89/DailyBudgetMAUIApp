using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailyBill : Popup<Object>
{
    private readonly PopupDailyBillViewModel _vm;
    private readonly IProductTools _pt;

    public PopupDailyBill(PopupDailyBillViewModel viewModel, IProductTools pt)
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
        if (_vm.IsAcceptOnly)
        {
            _vm.OriginalDate = _vm.Bill.BillDueDate.GetValueOrDefault();
            _vm.OriginalAmount = _vm.Bill.BillAmount.GetValueOrDefault();

            hslBillAmount.IsVisible = true;
            hslTargetDate.IsVisible = true;

            double BillAmount = (double?)_vm.Bill.BillAmount ?? 0;
            lblBillAmount.Text = BillAmount.ToString("c", CultureInfo.CurrentCulture);

            string GoalDate = _vm.Bill.BillDueDate.GetValueOrDefault().ToShortDateString();
            lblTargetDate.Text = GoalDate;

            btnUpdate.Text = "No, Go Back";
            lblTitle.Text = "Pay Bill Now!";
        }
        else
        {
            _vm.OriginalDate = _vm.Bill.BillDueDate.GetValueOrDefault();
            _vm.OriginalAmount = _vm.Bill.BillAmount.GetValueOrDefault();

            hslBillAmount.IsVisible = true;
            hslTargetDate.IsVisible = true;

            double BillAmount = (double?)_vm.Bill.BillAmount ?? 0;
            lblBillAmount.Text = BillAmount.ToString("c", CultureInfo.CurrentCulture);

            string GoalDate = _vm.Bill.BillDueDate.GetValueOrDefault().ToShortDateString();
            lblTargetDate.Text = GoalDate;
        }
    }

    void BillAmount_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            decimal BillAmount = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entBillAmount);

            _vm.Bill.BillAmount = BillAmount;
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "PopupDailyBill", "BillAmount_Changed");
        }
    }

    private async void Close_Saving(object sender, EventArgs e)
    {
        await CloseAsync("OK");
    }

    private async void DeleteYes_Saving(object sender, EventArgs e)
    {
        await CloseAsync("Delete");
    }

    private void DeleteNo_Saving(object sender, EventArgs e)
    {
        try
        {
            grdFirstBtns.IsVisible = false;
            grdUpdateBtns.IsVisible = true;
            if (_vm.IsAcceptOnly) 
            {
                btnDelete.IsVisible = false;
            }

            grdDeleteBtns.IsVisible = false;

            vslDeleteBill.IsVisible = false;
            vslSavingComplete.IsVisible = true;
 
            lblBillAmount.IsVisible = false;
            lblTargetDate.IsVisible = false;

            entBillAmount.IsVisible = true;
            entTargetDate.IsVisible = true;

            double BillAmount = (double?)_vm.Bill.BillAmount ?? 0;
            entBillAmount.Text = BillAmount.ToString("c", CultureInfo.CurrentCulture);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "PopupDailyBill", "DeleteNo_Saving");
        }


    }

    private void Delete_Saving(object sender, EventArgs e)
    {
        try
        {
            grdFirstBtns.IsVisible = false;
            grdUpdateBtns.IsVisible = false;
            grdDeleteBtns.IsVisible = true;

            vslDeleteBill.IsVisible = true;
            vslSavingComplete.IsVisible = false;
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "PopupDailyBill", "Delete_Saving");
        }
    }

    private async void Update_Saving(object sender, EventArgs e)
    {
        try
        {

            if(_vm.IsAcceptOnly)
            {
                await CloseAsync("Cancel");
            }
            else
            {
                grdFirstBtns.IsVisible = false;
                grdUpdateBtns.IsVisible = true;
                if (_vm.IsAcceptOnly)
                {
                    btnDelete.IsVisible = false;
                }
                grdDeleteBtns.IsVisible = false;


                lblBillAmount.IsVisible = false;
                lblTargetDate.IsVisible = false;

                entBillAmount.IsVisible = true;
                entTargetDate.IsVisible = true;

                double BillAmount = (double?)_vm.Bill.BillAmount ?? 0;
                entBillAmount.Text = BillAmount.ToString("c", CultureInfo.CurrentCulture);
            }

        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "PopupDailyBill", "ChangeSelectedProfilePic");
        }


    }

    private bool ValidatePage()
    {
        bool IsValid = true;

        if(_vm.Bill.BillAmount == 0)
        {
            IsValid = false;
            validatorBillAmount.IsVisible = true;
        }
        else
        {
            validatorBillAmount.IsVisible = false;
        }

        if (_vm.Bill.BillDueDate < _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
        {
            IsValid = false;
            validatorBillDate.IsVisible = true;
        }
        else
        {
            validatorBillDate.IsVisible = false;
        }

        return IsValid;
    }

    private async void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        try
        {
            if (ValidatePage())
            {

                int DaysToSavingDate = (int)Math.Ceiling((_vm.Bill.BillDueDate.GetValueOrDefault().Date - DateTime.Today.Date).TotalDays);
                decimal? AmountOutstanding = _vm.Bill.BillAmount - _vm.Bill.BillCurrentBalance;

                if (DaysToSavingDate != 0)
                {
                    _vm.Bill.RegularBillValue = AmountOutstanding / DaysToSavingDate;
                }
                else
                {
                    _vm.Bill.RegularBillValue = AmountOutstanding;
                }            

                await CloseAsync(_vm.Bill);
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "PopupDailyBill", "ChangeSelectedProfilePic");
        }
    }

    private void Reset_Saving(object sender, EventArgs e)
    {
        try
        {
            grdFirstBtns.IsVisible = true;
            grdUpdateBtns.IsVisible = false;
            grdDeleteBtns.IsVisible = false;

            _vm.Bill.BillAmount = _vm.OriginalAmount;
            _vm.Bill.BillDueDate = _vm.OriginalDate;

            lblBillAmount.IsVisible = true;
            lblTargetDate.IsVisible = true;

            entBillAmount.IsVisible = false;
            entTargetDate.IsVisible = false;

            double BillAmount = (double?)_vm.Bill.BillAmount ?? 0;
            lblBillAmount.Text = BillAmount.ToString("c", CultureInfo.CurrentCulture);

            string GoalDate = _vm.Bill.BillDueDate.GetValueOrDefault().ToShortDateString();
            lblTargetDate.Text = GoalDate;
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "PopupDailyBill", "ChangeSelectedProfilePic");
        }


    }
}