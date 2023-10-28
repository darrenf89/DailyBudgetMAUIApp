using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using System.Globalization;

namespace DailyBudgetMAUIApp.Pages;

public partial class AddSaving : ContentPage
{
    private readonly AddSavingViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    public AddSaving(AddSavingViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        dtpckGoalDate.MinimumDate = DateTime.UtcNow.AddDays(1);

       
	}
    async protected override void OnAppearing()
    {

        base.OnAppearing();

        if (_vm.BudgetID == 0)
        {
            _vm.BudgetID = App.DefaultBudgetID;
        }

        if (_vm.SavingID == 0)
        {
            _vm.Saving = new Savings();
            _vm.Title = "Add a New Saving";
            btnAddSaving.IsVisible = true;

        }
        else
        {
            _vm.Saving = _ds.GetSavingFromID(_vm.SavingID).Result;
            _vm.Title = $"Update Saving {_vm.Saving.SavingsName}";

            btnUpdateSaving.IsVisible = true; 

            if (_vm.Saving.IsRegularSaving)
            {
                _vm.SavingRecurringText = "Ongoing";

                btnOngoingSaving_Clicked(new Object(), new EventArgs());

                if(_vm.Saving.SavingsType == "TargetDate")
                {
                    UpdateSelectedOption("TargetDate");
                }
                else if (_vm.Saving.SavingsType == "SavingsBuilder")
                {
                    UpdateSelectedOption("SavingsBuilder");
                }
                else if (_vm.Saving.SavingsType == "TargetAmount")
                {
                    UpdateSelectedOption("TargetAmount");
                }
            }
            else
            {
                _vm.SavingRecurringText = "Envelope";
                btnEnvelopeSaving_Clicked(new Object(), new EventArgs());
            }
        }

        double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
        entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);

        double CurrentBalance = (double?)_vm.Saving.CurrentBalance ?? 0;
        entCurrentBalance.Text = CurrentBalance.ToString("c", CultureInfo.CurrentCulture);

        double RegularValue = 0;

        if (_vm.Saving.DdlSavingsPeriod == "PerPayPeriod")
        {
            RegularValue = (double?)_vm.Saving.PeriodSavingValue ?? 0;
            pckrSavingPeriod.SelectedIndex = 0;

        }
        else if (_vm.Saving.DdlSavingsPeriod == "PerDay")
        {
            RegularValue = (double?)_vm.Saving.RegularSavingValue ?? 0;
            pckrSavingPeriod.SelectedIndex = 1;
        }
        
        entSavingAmount.Text = RegularValue.ToString("c", CultureInfo.CurrentCulture);

        if (_vm.Saving.SavingsName == "" || _vm.Saving.SavingsName == null)
        {
            _vm.ChangeSavingsName();
        }

        _vm.BudgetNextPayDate = _ds.GetBudgetNextIncomePayDayAsync(_vm.BudgetID).Result;
        _vm.BudgetDaysToNextPay = (_vm.BudgetNextPayDate.Date - DateTime.Now.Date).Days;

        if (_vm.SavingID == 0)
        {
            UpdateDisplaySelection("TargetAmount");
            UpdateDisplaySelection("");
        }

    }

    private async void SaveSaving_Clicked(object sender, EventArgs e)
    {

    }

    private async void ResetSaving_Clicked(object sender, EventArgs e)
    {

    }

    private void btnOngoingSaving_Clicked(object sender, EventArgs e)
    {
        _vm.SavingRecurringText = "Ongoing";
        _vm.Saving.IsRegularSaving = true;

        lblSelectedSavingTitle.Text = "Select a type of saving";
        lblSelectedSavingParaOne.Text = "There are 3 types of regular savings depending on what you need";
        lblSelectedSavingParaTwo.Text = "Click on the option and play about to see what you want to add!";

        SelectSavingType.IsVisible = false;
        SavingTypeSelected.IsVisible = true;
        vslSavingRecurringTypeSelected.IsVisible = true;
    }
    private void btnEnvelopeSaving_Clicked(object sender, EventArgs e)
    {
        _vm.SavingRecurringText = "Envelope";
        _vm.Saving.IsRegularSaving = false;
        UpdateDisplaySelection("Envelope");
    }

    private void Option1Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("TargetDate");
    }

    private void Option2Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("SavingsBuilder");
    }

    private void Option3Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("TargetAmount");
    }

    private void UpdateSelectedOption(string option)
    {
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "TargetDate")
        {
            vslOption1Select.BackgroundColor = (Color)Success;
            vslOption2Select.BackgroundColor = (Color)White;
            vslOption3Select.BackgroundColor = (Color)White;

            lblOption1.FontAttributes = FontAttributes.Bold;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)White;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)Gray900;

            _vm.SavingTypeText = "TargetDate";
            _vm.Saving.SavingsType = "TargetDate";

            UpdateDisplaySelection("TargetDate");
            

        }
        else if (option == "SavingsBuilder")
        {
            vslOption1Select.BackgroundColor = (Color)White;
            vslOption2Select.BackgroundColor = (Color)Success;
            vslOption3Select.BackgroundColor = (Color)White;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.Bold;
            lblOption3.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)White;
            lblOption3.TextColor = (Color)Gray900;

            _vm.SavingTypeText = "SavingsBuilder";
            _vm.Saving.SavingsType = "SavingsBuilder";

            UpdateDisplaySelection("SavingsBuilder");

        }
        else if (option == "TargetAmount")
        {
            vslOption1Select.BackgroundColor = (Color)White;
            vslOption2Select.BackgroundColor = (Color)White;
            vslOption3Select.BackgroundColor = (Color)Success;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.Bold;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)White;

            _vm.SavingTypeText = "TargetAmount";
            _vm.Saving.SavingsType = "TargetAmount";

            UpdateDisplaySelection("TargetAmount");
        }
        else
        {
            vslOption1Select.BackgroundColor = (Color)White;
            vslOption2Select.BackgroundColor = (Color)White;
            vslOption3Select.BackgroundColor = (Color)White;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)Gray900;

            _vm.SavingTypeText = "";

            UpdateDisplaySelection("");
        }
    }

    private void UpdateDisplaySelection(string option)
    {
        if (option == "TargetDate")
        {           

            SelectSavingType.IsVisible = false;
            SavingTypeSelected.IsVisible = true;
            brdSavingDetails.IsVisible = true;

            lblSelectedSavingTitle.Text = "Creating a Target Date Saving";
            lblSelectedSavingParaOne.Text = "For holidays, your wedding ...";
            lblSelectedSavingParaTwo.Text = "Figure out how much you need to contribute each day to meet a goal with a deadline";
            
            vslSavingTarget.IsVisible = true;
            vslCurrentBalance.IsVisible = true;
            vslGoalDate.IsVisible = true;
            vslSavingAmount.IsVisible = true;            

            _vm.Saving.IsDailySaving = true;
            _vm.Saving.DdlSavingsPeriod = "PerDay";
            chbxIsAutoComplete.IsChecked = false;
            pckrSavingPeriod.SelectedIndex = 1;

            hslIsAutoComplete.IsVisible = true;
            hslCanExceedGoal.IsVisible = false;

            entSavingTarget.IsEnabled = true;
            entCurrentBalance.IsEnabled = true;
            dtpckGoalDate.IsEnabled = true;
            entSavingAmount.IsEnabled = false;
            pckrSavingPeriod.IsEnabled = false;

            brdSavingTarget.IsEnabled = true;
            brdCurrentBalance.IsEnabled = true;
            brdGoalDate.IsEnabled = true;
            brdSavingAmount.IsEnabled = false;
            brdSavingPeriod.IsEnabled = false;

        }
        else if (option == "SavingsBuilder")
        {
            SelectSavingType.IsVisible = false;
            SavingTypeSelected.IsVisible = true;
            brdSavingDetails.IsVisible = true;

            lblSelectedSavingTitle.Text = "Creating a Builder Savings";
            lblSelectedSavingParaOne.Text = "For saving goals without any time or target constraints";
            lblSelectedSavingParaTwo.Text = "Save the same amount each period, as much as you can afford!";
            
            vslSavingTarget.IsVisible = false;
            vslCurrentBalance.IsVisible = true;
            vslGoalDate.IsVisible = false;
            vslSavingAmount.IsVisible = true;

            hslIsAutoComplete.IsVisible = false;
            hslCanExceedGoal.IsVisible = false;

            pckrSavingPeriod.SelectedIndex = 1;

            entSavingTarget.IsEnabled = true;
            entCurrentBalance.IsEnabled = true;
            dtpckGoalDate.IsEnabled = true;
            entSavingAmount.IsEnabled = true;
            pckrSavingPeriod.IsEnabled = false;

            brdSavingTarget.IsEnabled = true;
            brdCurrentBalance.IsEnabled = true;
            brdGoalDate.IsEnabled = true;
            brdSavingAmount.IsEnabled = true;
            brdSavingPeriod.IsEnabled = false;

            _vm.Saving.GoalDate = null;
            _vm.Saving.SavingsGoal = 0;
            chbxIsAutoComplete.IsChecked = false;
            chbxCanExceedGoal.IsChecked = false;
            _vm.Saving.IsDailySaving = true;
            _vm.Saving.DdlSavingsPeriod = "PerDay";
        }
        else if (option == "TargetAmount")
        {
            SelectSavingType.IsVisible = false;
            SavingTypeSelected.IsVisible = true;
            brdSavingDetails.IsVisible = true;

            lblSelectedSavingTitle.Text = "Creating a Target Amount Saving";
            lblSelectedSavingParaOne.Text = "For emergency fund, down payment ...";
            lblSelectedSavingParaTwo.Text = "Save towards a target over time, contribute as much as you can each period";
            
            vslSavingTarget.IsVisible = true;
            vslCurrentBalance.IsVisible = true;
            vslGoalDate.IsVisible = true;
            vslSavingAmount.IsVisible = true;
            
            _vm.Saving.IsDailySaving = true;            

            if(_vm.Saving.DdlSavingsPeriod == "PerPayPeriod")
            {
                pckrSavingPeriod.SelectedIndex = 0;
            }
            else
            {
                _vm.Saving.DdlSavingsPeriod = "PerDay";
                pckrSavingPeriod.SelectedIndex = 1;
            }
            

            hslIsAutoComplete.IsVisible = true;
            hslCanExceedGoal.IsVisible = true;

            entSavingTarget.IsEnabled = true;
            entCurrentBalance.IsEnabled = true;
            dtpckGoalDate.IsEnabled = false;
            entSavingAmount.IsEnabled = true;
            pckrSavingPeriod.IsEnabled = true;

            brdSavingTarget.IsEnabled = true;
            brdCurrentBalance.IsEnabled = true;
            brdGoalDate.IsEnabled = false;
            brdSavingAmount.IsEnabled = true;
            brdSavingPeriod.IsEnabled = true;
        }
        else if(option == "Envelope")
        {
            SelectSavingType.IsVisible = false;
            SavingTypeSelected.IsVisible = true;
            brdSavingDetails.IsVisible = true;

            lblSelectedSavingTitle.Text = "Creating an Envelope Saving";
            lblSelectedSavingParaOne.Text = "Envelope Saving allows you to set aside money after you've been paid to spend during the month";
            lblSelectedSavingParaTwo.Text = "Maybe its for groceries, or money for going out! Using Envelopes will help you better control your spending!";                       

            vslSavingTarget.IsVisible = false;
            vslCurrentBalance.IsVisible = true;
            vslGoalDate.IsVisible = false;
            vslSavingAmount.IsVisible = true;

            pckrSavingPeriod.SelectedIndex = 0;

            hslIsAutoComplete.IsVisible = false;
            hslCanExceedGoal.IsVisible = false;

            entSavingTarget.IsEnabled = true;
            entCurrentBalance.IsEnabled = true;
            dtpckGoalDate.IsEnabled = true;
            entSavingAmount.IsEnabled = true;
            pckrSavingPeriod.IsEnabled = false;

            brdSavingTarget.IsEnabled = true;
            brdCurrentBalance.IsEnabled = true;
            brdGoalDate.IsEnabled = true;
            brdSavingAmount.IsEnabled = true;
            brdSavingPeriod.IsEnabled = false;

            _vm.Saving.GoalDate = _vm.BudgetNextPayDate;
            _vm.Saving.SavingsGoal = 0;
            chbxIsAutoComplete.IsChecked = false;
            chbxCanExceedGoal.IsChecked = false;
            _vm.Saving.IsDailySaving = false;
            _vm.Saving.DdlSavingsPeriod = "PerPayPeriod";
        }
        else
        {
            SelectSavingType.IsVisible = true;
            SavingTypeSelected.IsVisible = false;
            brdSavingDetails.IsVisible = false;

        }
    }

    void SavingTarget_Changed(object sender, TextChangedEventArgs e)
    {
        decimal SavingTarget = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);
        entSavingTarget.CursorPosition = _pt.FindCurrencyCursorPosition(entSavingTarget.Text);
        _vm.Saving.SavingsGoal = SavingTarget;

        RecalculateValues("entSavingTarget");
    }
    void CurrentBalance_Changed(object sender, TextChangedEventArgs e)
    {
        decimal CurrentBalance = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entCurrentBalance.Text = CurrentBalance.ToString("c", CultureInfo.CurrentCulture);
        entCurrentBalance.CursorPosition = _pt.FindCurrencyCursorPosition(entCurrentBalance.Text);
        _vm.Saving.CurrentBalance = CurrentBalance;

        RecalculateValues("entCurrentBalance");
    }

    private void dtpckGoalDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        RecalculateValues("dtpckGoalDate");
    }

    void SavingAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal SavingValue = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entSavingAmount.Text = SavingValue.ToString("c", CultureInfo.CurrentCulture);
        entSavingAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entSavingAmount.Text);

        if(_vm.Saving.DdlSavingsPeriod == "PerPayPeriod")
        {
            _vm.Saving.PeriodSavingValue = SavingValue;
            _vm.Saving.RegularSavingValue = SavingValue / _vm.BudgetDaysToNextPay;
        }
        else if (_vm.Saving.DdlSavingsPeriod == "PerDay")
        {
            _vm.Saving.PeriodSavingValue = SavingValue * _vm.BudgetDaysToNextPay;
            _vm.Saving.RegularSavingValue = SavingValue;
        }

        RecalculateValues("entSavingAmount");
    }
    private void pckrSavingPeriod_SelectedIndexChanged(object sender, EventArgs e)
    {
        PickerClass SavingPeriodClass = (PickerClass)pckrSavingPeriod.SelectedItem;
        _vm.Saving.DdlSavingsPeriod = SavingPeriodClass.Key;

        decimal SavingValue = (decimal)_pt.FormatCurrencyNumber(entSavingAmount.Text);

        if (_vm.Saving.DdlSavingsPeriod == "PerPayPeriod")
        {
            _vm.Saving.PeriodSavingValue = SavingValue;
            _vm.Saving.RegularSavingValue = SavingValue / _vm.BudgetDaysToNextPay;
            _vm.Saving.IsDailySaving = false;
        }
        else if (_vm.Saving.DdlSavingsPeriod == "PerDay")
        {
            _vm.Saving.PeriodSavingValue = SavingValue * _vm.BudgetDaysToNextPay;
            _vm.Saving.RegularSavingValue = SavingValue;
            _vm.Saving.IsDailySaving = true;
        }

        RecalculateValues("pckrSavingPeriod");
    }

    private void btnUpdateSaving_Clicked(object sender, EventArgs e)
    {

    }

    private void btnAddSaving_Clicked(object sender, EventArgs e)
    {

    }
    private void RecalculateValues(string sender)
    {

        if(_vm.Saving.SavingsType == "TargetDate")
        {
            if(sender != "entSavingAmount")
            {
                entSavingAmount.Text = _vm.CalculateSavingRegularValues();
            }            
        }
        else if(_vm.Saving.SavingsType == "TargetAmount")
        {
            if (sender != "dtpckGoalDate")
            {
                dtpckGoalDate.Date = _vm.CalculateSavingDate();
            }
        }
    }
}