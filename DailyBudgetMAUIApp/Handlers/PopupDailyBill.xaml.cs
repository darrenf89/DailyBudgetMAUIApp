using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailyBill : Popup
{
    private readonly PopupDailyBillViewModel _vm;
    private readonly IProductTools _pt;

    public PopupDailyBill(Bills Bill, PopupDailyBillViewModel viewModel, IProductTools pt)
	{
        InitializeComponent();

        viewModel.Bill = Bill;

        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;

        _vm.OriginalDate = _vm.Bill.BillDueDate.GetValueOrDefault();
        _vm.OriginalAmount = _vm.Bill.BillAmount.GetValueOrDefault();

        hslBillAmount.IsVisible = true;
        hslTargetDate.IsVisible = true;

        double BillAmount = (double?)_vm.Bill.BillAmount ?? 0;
        lblBillAmount.Text = BillAmount.ToString("c", CultureInfo.CurrentCulture);

        string GoalDate = _vm.Bill.BillDueDate.GetValueOrDefault().ToShortDateString();
        lblTargetDate.Text = GoalDate;
   
    }

    void BillAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal BillAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entBillAmount.Text = BillAmount.ToString("c", CultureInfo.CurrentCulture);
        entBillAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entBillAmount.Text);

        _vm.Bill.BillAmount = BillAmount;
    }

    private void Close_Saving(object sender, EventArgs e)
    {
        this.Close("OK");
    }

    private void DeleteYes_Saving(object sender, EventArgs e)
    {
        this.Close("Delete");
    }

    private void DeleteNo_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = true;
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

    private void Delete_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = false;
        grdDeleteBtns.IsVisible = true;

        vslDeleteBill.IsVisible = true;
        vslSavingComplete.IsVisible = false;
    }

    private void Update_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = true;
        grdDeleteBtns.IsVisible = false;


        lblBillAmount.IsVisible = false;
        lblTargetDate.IsVisible = false;

        entBillAmount.IsVisible = true;
        entTargetDate.IsVisible = true;

        double BillAmount = (double?)_vm.Bill.BillAmount ?? 0;
        entBillAmount.Text = BillAmount.ToString("c", CultureInfo.CurrentCulture);

        
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

    private void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {

            int DaysToSavingDate = (_vm.Bill.BillDueDate.GetValueOrDefault().Date - DateTime.Today.Date).Days;
            decimal? AmountOutstanding = _vm.Bill.BillAmount - _vm.Bill.BillCurrentBalance;

            if (DaysToSavingDate != 0)
            {
                _vm.Bill.RegularBillValue = AmountOutstanding / DaysToSavingDate;
            }
            else
            {
                _vm.Bill.RegularBillValue = AmountOutstanding;
            }            

            this.Close(_vm.Bill);
        }
    }

    private void Reset_Saving(object sender, EventArgs e)
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
}