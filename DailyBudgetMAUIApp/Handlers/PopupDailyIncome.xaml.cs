using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailyIncome : Popup
{
    private readonly PopupDailyIncomeViewModel _vm;
    private readonly IProductTools _pt;

    public PopupDailyIncome(IncomeEvents Income, PopupDailyIncomeViewModel viewModel, IProductTools pt)
	{
        InitializeComponent();

        viewModel.Income = Income;

        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;

        _vm.OriginalDate = _vm.Income.DateOfIncomeEvent;
        _vm.OriginalAmount = _vm.Income.IncomeAmount;

        hslIncomeAmount.IsVisible = true;
        hslIncomeDate.IsVisible = true;

        double IncomeAmount = (double?)_vm.Income.IncomeAmount ?? 0;
        lblIncomeAmount.Text = IncomeAmount.ToString("c", CultureInfo.CurrentCulture);

        string GoalDate = _vm.Income.DateOfIncomeEvent.ToShortDateString();
        lblIncomeDate.Text = GoalDate;
   
    }

    void IncomeAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal IncomeAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entIncomeAmount.Text = IncomeAmount.ToString("c", CultureInfo.CurrentCulture);
        entIncomeAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entIncomeAmount.Text);

        _vm.Income.IncomeAmount = IncomeAmount;
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

        vslDeleteIncome.IsVisible = false;
        vslSavingComplete.IsVisible = true;
 
        lblIncomeAmount.IsVisible = false;
        lblIncomeDate.IsVisible = false;

        entIncomeAmount.IsVisible = true;
        entIncomeDate.IsVisible = true;

        double IncomeAmount = (double?)_vm.Income.IncomeAmount ?? 0;
        entIncomeAmount.Text = IncomeAmount.ToString("c", CultureInfo.CurrentCulture);

        
    }

    private void Delete_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = false;
        grdDeleteBtns.IsVisible = true;

        vslDeleteIncome.IsVisible = true;
        vslSavingComplete.IsVisible = false;
    }

    private void Update_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = true;
        grdDeleteBtns.IsVisible = false;


        lblIncomeAmount.IsVisible = false;
        lblIncomeDate.IsVisible = false;

        entIncomeAmount.IsVisible = true;
        entIncomeDate.IsVisible = true;

        double IncomeAmount = (double?)_vm.Income.IncomeAmount ?? 0;
        entIncomeAmount.Text = IncomeAmount.ToString("c", CultureInfo.CurrentCulture);

        
    }

    private bool ValidatePage()
    {
        bool IsValid = true;

        if(_vm.Income.IncomeAmount == 0)
        {
            IsValid = false;
            validatorIncomeAmount.IsVisible = true;
        }
        else
        {
            validatorIncomeAmount.IsVisible = false;
        }

        if (_vm.Income.DateOfIncomeEvent < DateTime.UtcNow.Date)
        {
            IsValid = false;
            validatorIncomeDate.IsVisible = true;
        }
        else
        {
            validatorIncomeDate.IsVisible = false;
        }

        return IsValid;
    }

    private void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {
            this.Close(_vm.Income);
        }
    }

    private void Reset_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = true;
        grdUpdateBtns.IsVisible = false;
        grdDeleteBtns.IsVisible = false;

        _vm.Income.IncomeAmount = _vm.OriginalAmount;
        _vm.Income.DateOfIncomeEvent = _vm.OriginalDate;

        lblIncomeAmount.IsVisible = true;
        lblIncomeDate.IsVisible = true;

        entIncomeAmount.IsVisible = false;
        entIncomeDate.IsVisible = false;

        double IncomeAmount = (double?)_vm.Income.IncomeAmount ?? 0;
        lblIncomeAmount.Text = IncomeAmount.ToString("c", CultureInfo.CurrentCulture);

        string GoalDate = _vm.Income.DateOfIncomeEvent.ToShortDateString();
        lblIncomeDate.Text = GoalDate;
        

    }
}