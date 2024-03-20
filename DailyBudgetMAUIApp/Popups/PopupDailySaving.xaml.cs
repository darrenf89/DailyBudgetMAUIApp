using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailySaving : Popup
{
    private readonly PopupDailySavingViewModel _vm;
    private readonly IProductTools _pt;

    public PopupDailySaving(Savings Saving, PopupDailySavingViewModel viewModel, IProductTools pt)
	{
        InitializeComponent();

        viewModel.Saving = Saving;

        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;

        _vm.OriginalDate = _vm.Saving.GoalDate.GetValueOrDefault();
        _vm.OriginalTarget = _vm.Saving.SavingsGoal.GetValueOrDefault();
        _vm.OriginalDaily = _vm.Saving.RegularSavingValue.GetValueOrDefault();

        if (_vm.Saving.SavingsType == "TargetAmount")
        {
            hslTargetAmount.IsVisible = true;
            hslDailyAmount.IsVisible = true;

            double RegularSavingValue = (double?)_vm.Saving.RegularSavingValue ?? 0;
            lblSavingAmount.Text = RegularSavingValue.ToString("c", CultureInfo.CurrentCulture);

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            lblSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);
        }
        else if (_vm.Saving.SavingsType == "TargetDate")
        {
            hslTargetAmount.IsVisible = true;
            hslTargetDate.IsVisible = true;

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            lblSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);

            string GoalDate = _vm.Saving.GoalDate.GetValueOrDefault().ToShortDateString();
            lblTargetDate.Text = GoalDate;
        }
    }

    void SavingTarget_Changed(object sender, TextChangedEventArgs e)
    {
        decimal SavingTarget = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);
        entSavingTarget.CursorPosition = _pt.FindCurrencyCursorPosition(entSavingTarget.Text);

        _vm.Saving.SavingsGoal = SavingTarget;
    }

    void SavingAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal SavingValue = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entSavingAmount.Text = SavingValue.ToString("c", CultureInfo.CurrentCulture);
        entSavingAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entSavingAmount.Text);

        _vm.Saving.RegularSavingValue = SavingValue;
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

        vslDeleteSaving.IsVisible = false;
        vslSavingComplete.IsVisible = true;

        if (_vm.Saving.SavingsType == "TargetAmount")
        {
            lblSavingAmount.IsVisible = false;
            lblSavingTarget.IsVisible = false;

            entSavingAmount.IsVisible = true;
            entSavingTarget.IsVisible = true;

            double RegularSavingValue = (double?)_vm.Saving.RegularSavingValue ?? 0;
            entSavingAmount.Text = RegularSavingValue.ToString("c", CultureInfo.CurrentCulture);

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);

        }
        else if (_vm.Saving.SavingsType == "TargetDate")
        {
            lblSavingTarget.IsVisible = false;
            lblTargetDate.IsVisible = false;

            entSavingTarget.IsVisible = true;
            entTargetDate.IsVisible = true;

            entTargetDate.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date;

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);

        }
    }

    private void Delete_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = false;
        grdDeleteBtns.IsVisible = true;

        vslDeleteSaving.IsVisible = true;
        vslSavingComplete.IsVisible = false;
    }

    private void Update_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = true;
        grdDeleteBtns.IsVisible = false;

        if (_vm.Saving.SavingsType == "TargetAmount")
        {
            lblSavingAmount.IsVisible = false;
            lblSavingTarget.IsVisible = false;

            entSavingAmount.IsVisible = true;
            entSavingTarget.IsVisible = true;

            double RegularSavingValue = (double?)_vm.Saving.RegularSavingValue ?? 0;
            entSavingAmount.Text = RegularSavingValue.ToString("c", CultureInfo.CurrentCulture);

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);

        }
        else if (_vm.Saving.SavingsType == "TargetDate")
        {
            lblSavingTarget.IsVisible = false;
            lblTargetDate.IsVisible = false;

            entSavingTarget.IsVisible = true;
            entTargetDate.IsVisible = true;

            entTargetDate.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date;

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);

        }
    }

    private bool ValidatePage()
    {
        bool IsValid = true;

        if(_vm.Saving.SavingsGoal < _vm.OriginalTarget)
        {
            IsValid = false;
            validatorSavingsGoal.IsVisible = true;
        }
        else
        {
            validatorSavingsGoal.IsVisible = false;
        }

        if (_vm.Saving.RegularSavingValue == 0)
        {
            IsValid = false;
            validatorSavingsDailyAmount.IsVisible = true;
        }
        else
        {
            validatorSavingsDailyAmount.IsVisible = false;
        }

        if (_vm.Saving.GoalDate < _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
        {
            IsValid = false;
            validatorSavingsDate.IsVisible = true;
        }
        else
        {
            validatorSavingsDate.IsVisible = false;
        }

        return IsValid;
    }

    private void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {
            if (_vm.Saving.SavingsType == "TargetAmount")
            {
                decimal? BalanceLeft = _vm.Saving.SavingsGoal - (_vm.Saving.CurrentBalance ?? 0);
                int NumberOfDays = (int)Math.Ceiling(BalanceLeft / _vm.Saving.RegularSavingValue ?? 0);

                DateTime Today = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date;
                _vm.Saving.GoalDate = Today.AddDays(NumberOfDays);
            }
            else if (_vm.Saving.SavingsType == "TargetDate")
            {
                int DaysToSavingDate = (int)Math.Ceiling((_vm.Saving.GoalDate.GetValueOrDefault().Date - DateTime.Today.Date).TotalDays);
                decimal? AmountOutstanding = _vm.Saving.SavingsGoal - _vm.Saving.CurrentBalance;

                if (DaysToSavingDate != 0)
                {
                    _vm.Saving.RegularSavingValue = AmountOutstanding / DaysToSavingDate;
                }
                else
                {
                    _vm.Saving.RegularSavingValue = AmountOutstanding;
                }
            }

            this.Close(_vm.Saving);
        }
    }

    private void Reset_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = true;
        grdUpdateBtns.IsVisible = false;
        grdDeleteBtns.IsVisible = false;

        _vm.Saving.SavingsGoal = _vm.OriginalTarget;
        _vm.Saving.RegularSavingValue = _vm.OriginalDaily;
        _vm.Saving.GoalDate = _vm.OriginalDate;

        if (_vm.Saving.SavingsType == "TargetAmount")
        {
            lblSavingAmount.IsVisible = true;
            lblSavingTarget.IsVisible = true;

            entSavingAmount.IsVisible = false;
            entSavingTarget.IsVisible = false;

            double RegularSavingValue = (double?)_vm.Saving.RegularSavingValue ?? 0;
            lblSavingAmount.Text = RegularSavingValue.ToString("c", CultureInfo.CurrentCulture);

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            lblSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);
        }
        else if (_vm.Saving.SavingsType == "TargetDate")
        {
            lblSavingTarget.IsVisible = true;
            lblTargetDate.IsVisible = true;

            entSavingTarget.IsVisible = false;
            entTargetDate.IsVisible = false;

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            lblSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);

            string GoalDate = _vm.Saving.GoalDate.GetValueOrDefault().ToShortDateString();
            lblTargetDate.Text = GoalDate;
        }

    }
}