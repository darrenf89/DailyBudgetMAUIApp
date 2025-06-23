using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailyIncome : Popup<Object>
{
    private readonly PopupDailyIncomeViewModel _vm;
    private readonly IProductTools _pt;

    public PopupDailyIncome(PopupDailyIncomeViewModel viewModel, IProductTools pt)
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
        decimal IncomeAmount = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entIncomeAmount);

        _vm.Income.IncomeAmount = IncomeAmount;
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

        if (_vm.Income.DateOfIncomeEvent < _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
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

    private async void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {
            await CloseAsync(_vm.Income);
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