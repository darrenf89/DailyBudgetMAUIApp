using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using IeuanWalker.Maui.Switch;
using IeuanWalker.Maui.Switch.Events;
using IeuanWalker.Maui.Switch.Helpers;
using System.Globalization;

namespace DailyBudgetMAUIApp.Pages;

public partial class AddSaving : BasePage
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
       
	}

    async protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            dtpckGoalDate.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).AddDays(1);

            if (_vm.BudgetID == 0)
            {
                _vm.BudgetID = App.DefaultBudgetID;
            }

            _vm.BudgetNextPayDate = _ds.GetBudgetNextIncomePayDayAsync(_vm.BudgetID).Result;
            _vm.BudgetDaysToNextPay = (int)Math.Ceiling((_vm.BudgetNextPayDate.Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);
            _vm.BudgetDaysBetweenPay = _ds.GetBudgetDaysBetweenPayDay(_vm.BudgetID).Result;

            if (_vm.SavingID == 0)
            {
                _vm.Saving = new Savings();
                _vm.Title = "Add a New Saving";
                btnAddSaving.IsVisible = true;

                if (_vm.NavigatedFrom == "ViewSavings")
                {
                    _vm.SavingType = "Regular";
                }
                else if (_vm.NavigatedFrom == "ViewEnvelopes")
                {
                    _vm.SavingType = "Envelope";
                }

                if (_vm.SavingType == "Envelope")
                {
                    _vm.SavingRecurringText = "Envelope";
                    _vm.Saving.IsRegularSaving = false;

                    UpdateDisplaySelection("Envelope");
                }
                else if (_vm.SavingType == "Regular")
                {
                    _vm.SavingRecurringText = "Ongoing";
                    _vm.Saving.IsRegularSaving = true;
                    btnOngoingSaving_Clicked(new Object(), new EventArgs());
                }
            }
            else
            {
                if (_vm.SavingID != -1)
                {
                    _vm.Saving = _ds.GetSavingFromID(_vm.SavingID).Result;
                    _vm.Title = $"Update Saving {_vm.Saving.SavingsName}";
                    btnUpdateSaving.IsVisible = true;
                }
                else
                {
                    _vm.Title = "Add a New Saving";
                    btnAddSaving.IsVisible = true;
                }            

                if (_vm.Saving.IsRegularSaving)
                {
                    _vm.SavingRecurringText = "Ongoing";

                    btnOngoingSaving_Clicked(new Object(), new EventArgs());

                    if (_vm.Saving.SavingsType == "TargetDate")
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
                    _vm.Saving.IsRegularSaving = false;

                    UpdateDisplaySelection("Envelope");
                }
            }

            double CalcAmount = 0;
            entCalculateAmount.Text = CalcAmount.ToString("c", CultureInfo.CurrentCulture);

            double SavingTarget = (double?)_vm.Saving.SavingsGoal ?? 0;
            entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);

            double CurrentBalance = (double?)_vm.Saving.CurrentBalance ?? 0;
            entCurrentBalance.Text = CurrentBalance.ToString("c", CultureInfo.CurrentCulture);

            double RegularValue = 0;

            if (_vm.Saving.DdlSavingsPeriod == "PerPayPeriod")
            {
                RegularValue = (double?)_vm.Saving.PeriodSavingValue ?? 0;

            }
            else if (_vm.Saving.DdlSavingsPeriod == "PerDay")
            {
                RegularValue = (double?)_vm.Saving.RegularSavingValue ?? 0;
            }
        
            entSavingAmount.Text = RegularValue.ToString("c", CultureInfo.CurrentCulture);

            if(_vm.NavigatedFrom=="ViewSavings" && _vm.SavingID != 0)
            {
                vslOption1Select.IsEnabled = false;
                vslOption2Select.IsEnabled = false;
                vslOption3Select.IsEnabled = false;

                UpdateSelectedOptionDisabled(_vm.Saving.SavingsType);          
            }

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddSaving", "OnAppearing");
        }
    }

    private async Task<string> ChangeSavingsName()
    {
        string Description = "Every savings needs a name, we will refer to it by the name you give it and this will make it easier to identify!";
        string DescriptionSub = "Call it something useful or call it something silly up to you really!";
        var popup = new PopUpPageSingleInput("Saving Name", Description, DescriptionSub, "Enter an Saving name!", _vm.Saving.SavingsName, new PopUpPageSingleInputViewModel());
        var result = await Application.Current.MainPage.ShowPopupAsync(popup);

        if (result != null || (string)result != "")
        {
            _vm.Saving.SavingsName = (string)result;
        }

        return (string)result;
    }

    private async void SaveSaving_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.Saving.SavingsName == "" || _vm.Saving.SavingsName == null)
            {
                string status = await ChangeSavingsName();
            }

            if (ValidateSavingDetails())
            {
                if(_vm.SavingID == 0)
                {
                    _vm.AddSaving();
                }
                else
                {
                    _vm.UpdateSaving();
                }
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddSaving", "SaveSaving_Clicked");
        }
    }

    private async void ResetSaving_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool result = await DisplayAlert("Savings Reset", "Are you sure you want to Reset " + _vm.Saving.SavingsName, "Yes, continue", "Cancel");
            if (result)
            {
                UpdateDisplaySelection("");
                UpdateSelectedOption("");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddSaving", "ResetSaving_Clicked");
        }
    }

    private void btnOngoingSaving_Clicked(object sender, EventArgs e)
    {
        try
        {
            ClearAllValidators();

            _vm.SavingRecurringText = "Ongoing";
            _vm.Saving.IsRegularSaving = true;

            lblSelectedSavingTitle.Text = "Select a type of saving";
            lblSelectedSavingParaOne.Text = "There are 3 types of regular savings depending on what you need";
            lblSelectedSavingParaTwo.Text = "Click on the option and play about to see what you want to add!";

            SelectSavingType.IsVisible = false;
            SavingTypeSelected.IsVisible = true;
            vslSavingRecurringTypeSelected.IsVisible = true;
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "btnOngoingSaving_Clicked");
        }
    }
    private void btnEnvelopeSaving_Clicked(object sender, EventArgs e)
    {
        try
        {
            ClearAllValidators();

            pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[0];

            _vm.SavingRecurringText = "Envelope";
            _vm.Saving.IsRegularSaving = false;        

            UpdateDisplaySelection("Envelope");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "btnEnvelopeSaving_Clicked");
        }

    }

    private void Option1Select_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSelectedOption("TargetDate");
            pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[1];
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "Option1Select_Tapped");
        }
    }

    private void Option2Select_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSelectedOption("SavingsBuilder");
            pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[1];
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "Option2Select_Tapped");
        }
    }

    private void Option3Select_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSelectedOption("TargetAmount");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "Option3Select_Tapped");
        }
    }

    private void UpdateSelectedOptionDisabled(string option)
    {
        Application.Current.Resources.TryGetValue("Secondary", out var Secondary);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "TargetDate")
        {
            vslOption1Select.BackgroundColor = (Color)Secondary;
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblOption1.FontAttributes = FontAttributes.Bold;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)White;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)Gray900;
        }
        else if (option == "SavingsBuilder")
        {
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = (Color)Secondary;
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.Bold;
            lblOption3.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)White;
            lblOption3.TextColor = (Color)Gray900;
        }
        else if (option == "TargetAmount")
        {
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption3Select.BackgroundColor = (Color)Secondary;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.Bold;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)White;
        }

    }

    private void UpdateSelectedOption(string option)
    {
        ClearAllValidators();

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "TargetDate")
        {
            vslOption1Select.BackgroundColor = (Color)Success;
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

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
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = (Color)Success;
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

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
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
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
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

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
        ClearAllValidators();

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
            SavingGoalDateLabel.Text = "Saving Goal Date";
            vslSavingAmount.IsVisible = true;            

            _vm.Saving.IsDailySaving = true;
            _vm.Saving.DdlSavingsPeriod = "PerDay";
            chbxIsAutoComplete.IsChecked = false;

            hslIsAutoComplete.IsVisible = false;
            hslCanExceedGoal.IsVisible = true;

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
            VSLCalculator.IsVisible = false;

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
            SavingGoalDateLabel.Text = "Saving Goal Date";
            vslSavingAmount.IsVisible = true;

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

            _vm.Saving.GoalDate = null;
            _vm.Saving.SavingsGoal = 0;
            chbxIsAutoComplete.IsChecked = false;
            chbxCanExceedGoal.IsChecked = false;
            _vm.Saving.IsDailySaving = true;
            _vm.Saving.DdlSavingsPeriod = "PerDay";
            VSLCalculator.IsVisible = true;



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
            SavingGoalDateLabel.Text = "Saving Goal Date";
            vslSavingAmount.IsVisible = true;
            
            _vm.Saving.IsDailySaving = true; 

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
            VSLCalculator.IsVisible = false;
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
            vslGoalDate.IsVisible = true;
            SavingGoalDateLabel.Text = "Next Stash Date";
            _vm.Saving.GoalDate = _vm.BudgetNextPayDate;

            vslSavingAmount.IsVisible = true;

            hslIsAutoComplete.IsVisible = false;
            hslCanExceedGoal.IsVisible = false;

            entSavingTarget.IsEnabled = true;
            entCurrentBalance.IsEnabled = true;
            dtpckGoalDate.IsEnabled = false;
            entSavingAmount.IsEnabled = true;
            pckrSavingPeriod.IsEnabled = false;

            brdSavingTarget.IsEnabled = true;
            brdCurrentBalance.IsEnabled = true;
            brdGoalDate.IsEnabled = false;
            brdSavingAmount.IsEnabled = true;
            brdSavingPeriod.IsEnabled = false;

            _vm.Saving.GoalDate = _vm.BudgetNextPayDate;
            _vm.Saving.SavingsGoal = 0;
            chbxIsAutoComplete.IsChecked = false;
            chbxCanExceedGoal.IsChecked = true;
            _vm.Saving.IsDailySaving = false;
            _vm.Saving.DdlSavingsPeriod = "PerPayPeriod";
            VSLCalculator.IsVisible = false;

            swhIsTopUp_Toggled(null, null);
        }
        else
        {
            SelectSavingType.IsVisible = true;
            SavingTypeSelected.IsVisible = false;
            brdSavingDetails.IsVisible = false;
            vslSavingRecurringTypeSelected.IsVisible = false;
            SavingGoalDateLabel.Text = "Saving Goal Date";

            _vm.SavingRecurringText = "";
            _vm.SavingTypeText = "";

        }
    }

    static void CustomSwitch_SwitchPanUpdate(CustomSwitch customSwitch, SwitchPanUpdatedEventArgs e)
    {

        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);

        //Switch Color Animation
        Color fromSwitchColor = e.IsToggled ? (Color)Primary : (Color)Gray400;
        Color toSwitchColor = e.IsToggled ? (Color)Gray400 : (Color)Primary;

        //BackGroundColor Animation
        Color fromColor = e.IsToggled ? (Color)Tertiary : (Color)PrimaryLight;
        Color toColor = e.IsToggled ? (Color)PrimaryLight : (Color)Tertiary;

        double t = e.Percentage * 0.01;

        customSwitch.KnobBackgroundColor = ColorAnimationUtil.ColorAnimation(fromSwitchColor, toSwitchColor, t);
        customSwitch.BackgroundColor = ColorAnimationUtil.ColorAnimation(fromColor, toColor, t);

    }

    void SavingTarget_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            decimal SavingTarget = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
            entSavingTarget.Text = SavingTarget.ToString("c", CultureInfo.CurrentCulture);
            int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
            if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entSavingTarget.CursorPosition > position)
            {
                entSavingTarget.CursorPosition = entSavingTarget.Text.Length;
            }
            _vm.Saving.SavingsGoal = SavingTarget;

            RecalculateValues("entSavingTarget");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "SavingTarget_Changed");
        }
    }
    void CurrentBalance_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            decimal CurrentBalance = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
            entCurrentBalance.Text = CurrentBalance.ToString("c", CultureInfo.CurrentCulture);
            int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
            if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entCurrentBalance.CursorPosition > position)
            {
                entCurrentBalance.CursorPosition = entCurrentBalance.Text.Length;
            }
            _vm.Saving.CurrentBalance = CurrentBalance;

            RecalculateValues("entCurrentBalance");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "CurrentBalance_Changed");
        }
    }

    private void dtpckGoalDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        try
        {
            RecalculateValues("dtpckGoalDate");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "dtpckGoalDate_DateSelected");
        }
    }

    void SavingAmount_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            decimal SavingValue = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
            entSavingAmount.Text = SavingValue.ToString("c", CultureInfo.CurrentCulture);
            int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
            if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entSavingAmount.CursorPosition > position)
            {
                entSavingAmount.CursorPosition = entSavingAmount.Text.Length;
            }

            if (!_vm.Saving.IsRegularSaving)
            {
                if(!(_vm.NavigatedFrom == "ViewSavings" || _vm.NavigatedFrom == "ViewEnvelopes"))
                {
                    //entCurrentBalance.Text = SavingValue.ToString("c", CultureInfo.CurrentCulture);
                    //_vm.Saving.CurrentBalance = SavingValue;
                }
            }

            if(_vm.Saving.DdlSavingsPeriod == "PerPayPeriod")
            {
                _vm.Saving.PeriodSavingValue = SavingValue;
                _vm.Saving.RegularSavingValue = SavingValue / _vm.BudgetDaysBetweenPay;
            }
            else if (_vm.Saving.DdlSavingsPeriod == "PerDay")
            {
                _vm.Saving.PeriodSavingValue = SavingValue * _vm.BudgetDaysBetweenPay;
                _vm.Saving.RegularSavingValue = SavingValue;
            }

            RecalculateValues("entSavingAmount");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "SavingAmount_Changed");
        }
    }
    void CalculateAmount_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            decimal CalculateAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
            entCalculateAmount.Text = CalculateAmount.ToString("c", CultureInfo.CurrentCulture);
            if(_vm.ShowCalculator)
            {
                int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
                if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entCalculateAmount.CursorPosition > position)
                {
                    entCalculateAmount.CursorPosition = entCalculateAmount.Text.Length;
                }
            }
        

            string SelectedDuration = (string)pckrEverynthDuration.SelectedItem ?? "Week";
            decimal DailyAmount = 0;

            if (SelectedDuration == "Week")
            {
                DailyAmount = CalculateAmount / 7;
            }
            else if(SelectedDuration == "Fortnight")
            {
                DailyAmount = CalculateAmount / 14;
            }        
            else if(SelectedDuration == "Pay")
            {
                DailyAmount = CalculateAmount / _vm.BudgetDaysBetweenPay;
            }
            else if(SelectedDuration == "Month")
            {
                DailyAmount = CalculateAmount / 30;
            }
            else if(SelectedDuration == "Year")
            {
                DailyAmount = CalculateAmount / 365;
            }

            if(_vm.ShowCalculator)
            {
                entSavingAmount.Text = DailyAmount.ToString("c", CultureInfo.CurrentCulture);
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "CalculateAmount_Changed");
        }

    }

    private void pckrSavingPeriod_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            PickerClass SavingPeriodClass = (PickerClass)pckrSavingPeriod.SelectedItem;
            _vm.Saving.DdlSavingsPeriod = SavingPeriodClass.Key;

            decimal SavingValue = (decimal)_pt.FormatCurrencyNumber(entSavingAmount.Text);

            if (_vm.Saving.DdlSavingsPeriod == "PerPayPeriod")
            {
                _vm.Saving.PeriodSavingValue = SavingValue;
                _vm.Saving.RegularSavingValue = SavingValue / _vm.BudgetDaysBetweenPay;
                _vm.Saving.IsDailySaving = false;
            }
            else if (_vm.Saving.DdlSavingsPeriod == "PerDay")
            {
                _vm.Saving.PeriodSavingValue = SavingValue * _vm.BudgetDaysBetweenPay;
                _vm.Saving.RegularSavingValue = SavingValue;
                _vm.Saving.IsDailySaving = true;
            }

            RecalculateValues("pckrSavingPeriod");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "pckrSavingPeriod_SelectedIndexChanged");
        }

    }

    private async void btnUpdateSaving_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.Saving.SavingsName == "" || _vm.Saving.SavingsName == null)
            {
                string status = await ChangeSavingsName();
            }

            if(ValidateSavingDetails())
            {
                _vm.UpdateSaving();
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddSaving", "btnUpdateSaving_Clicked");
        }
    }

    private async void btnAddSaving_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.Saving.SavingsName == "" || _vm.Saving.SavingsName == null)
            {
                string status = await ChangeSavingsName();
            }

            if (ValidateSavingDetails())
            {
                _vm.AddSaving();
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddSaving", "btnAddSaving_Clicked");
        }
    }
    private void RecalculateValues(string sender)
    {
        ClearAllValidators();

        if (_vm.Saving.SavingsType == "TargetDate")
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

    private bool ValidateSavingDetails()
    {
        bool IsValid = true;

        if (_vm.Saving.SavingsName == "" || _vm.Saving.SavingsName == null)
        {
            IsValid = false;
            validatorSavingsName.IsVisible = true;
        }
        else
        {
            validatorSavingsName.IsVisible = false;
        }

        if(_vm.SavingRecurringText == "")
        {
            IsValid = false;
            validatorSavingRecurring.IsVisible = true;
        }
        else
        {
            validatorSavingRecurring.IsVisible = false;
        }
        
        if(_vm.SavingRecurringText == "Ongoing")
        {
            if (_vm.SavingTypeText == "")
            {
                IsValid = false;
                validatorSavingType.IsVisible = true;
            }
            else
            {
                validatorSavingType.IsVisible = false;
            }

            if (_vm.SavingTypeText == "TargetDate")
            {
                if (_vm.Saving.SavingsGoal == null || _vm.Saving.SavingsGoal == 0)
                {
                    IsValid = false;
                    validatorSavingTarget.IsVisible = true;
                }
                else
                {
                    validatorSavingTarget.IsVisible = false;
                }

                if(_vm.Saving.CurrentBalance > _vm.Saving.SavingsGoal)
                {
                    IsValid = false;
                    validatorCurrentBalance.IsVisible = true;
                }
                else
                {
                    validatorCurrentBalance.IsVisible = false;
                }

                if (_pt.GetBudgetLocalTime(DateTime.UtcNow).Date > _vm.Saving.GoalDate)
                {
                    IsValid = false;
                    validatorGoalDate.IsVisible = true;
                }
                else
                {
                    validatorGoalDate.IsVisible = false;
                }

                if (_vm.Saving.PeriodSavingValue == 0 || _vm.Saving.RegularSavingValue == 0)
                {
                    IsValid = false;
                    validatorSavingAmount.IsVisible = true;
                }
                else
                {
                    validatorSavingAmount.IsVisible = false;
                }
            }
            else if (_vm.SavingTypeText == "SavingsBuilder")
            {
                if (_vm.Saving.PeriodSavingValue == 0 || _vm.Saving.RegularSavingValue == 0)
                {
                    IsValid = false;
                    validatorSavingAmount.IsVisible = true;
                }
                else
                {
                    validatorSavingAmount.IsVisible = false;
                }
            }
            else if (_vm.SavingTypeText == "TargetAmount")
            {
                if (_vm.Saving.SavingsGoal == null || _vm.Saving.SavingsGoal == 0)
                {
                    IsValid = false;
                    validatorSavingTarget.IsVisible = true;
                }
                else
                {
                    validatorSavingTarget.IsVisible = false;
                }

                if (_vm.Saving.CurrentBalance > _vm.Saving.SavingsGoal)
                {
                    IsValid = false;
                    validatorCurrentBalance.IsVisible = true;
                }
                else
                {
                    validatorCurrentBalance.IsVisible = false;
                }

                if (_pt.GetBudgetLocalTime(DateTime.UtcNow).Date > _vm.Saving.GoalDate)
                {
                    IsValid = false;
                    validatorGoalDate.IsVisible = true;
                }
                else
                {
                    validatorGoalDate.IsVisible = false;
                }

                if (_vm.Saving.PeriodSavingValue == 0 || _vm.Saving.RegularSavingValue == 0)
                {
                    IsValid = false;
                    validatorSavingAmount.IsVisible = true;
                }
                else
                {
                    validatorSavingAmount.IsVisible = false;
                }
            }
        }
        else
        {
            if(!_vm.Saving.IsTopUp)
            {
                if (_vm.Saving.PeriodSavingValue == 0 || _vm.Saving.RegularSavingValue == 0)
                {
                    IsValid = false;
                    validatorSavingAmount.IsVisible = true;
                }
                else
                {
                    validatorSavingAmount.IsVisible = false;
                }
            }

        }

        _vm.IsPageValid = IsValid;
        return IsValid;
    }

    private void ClearAllValidators()
    {
        validatorSavingsName.IsVisible = false;
        validatorSavingRecurring.IsVisible = false;
        validatorSavingType.IsVisible = false;
        validatorSavingAmount.IsVisible = false;
        validatorGoalDate.IsVisible = false;
        validatorCurrentBalance.IsVisible = false;
        validatorSavingTarget.IsVisible = false;
    }

    private void pckrSavingPeriod_Loaded(object sender, EventArgs e)
    {
        try
        {
            if (_vm.SavingID == 0)
            {
                if (_vm.SavingType == "Envelope")
                {
                    pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[0];
                }
                else
                {
                    pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[1];
                }            
            }
            else
            {
                if (_vm.Saving.DdlSavingsPeriod == "PerPayPeriod")
                {
                    pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[0];
                }
                else if (_vm.Saving.DdlSavingsPeriod == "PerDay")
                {
                    pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[1];
                }
                else
                {
                    if (_vm.Saving.IsRegularSaving)
                    {
                        pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[1];
                    }
                    else
                    {
                        pckrSavingPeriod.SelectedItem = _vm.DropDownSavingPeriod[0];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "pckrSavingPeriod_Loaded");
        }
    }

    private void pckrEverynthDuration_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(entCalculateAmount.Text))
            {
                decimal CalculateAmount = (decimal)_pt.FormatCurrencyNumber(entCalculateAmount.Text);

                if (CalculateAmount > 0)
                {
                    string SelectedDuration = (string)pckrEverynthDuration.SelectedItem ?? "Week";
                    decimal DailyAmount = 0;

                    if (SelectedDuration == "Week")
                    {
                        DailyAmount = CalculateAmount / 7;
                    }
                    else if (SelectedDuration == "Fortnight")
                    {
                        DailyAmount = CalculateAmount / 14;
                    }
                    else if (SelectedDuration == "Pay")
                    {
                        DailyAmount = CalculateAmount / _vm.BudgetDaysBetweenPay;
                    }
                    else if (SelectedDuration == "Month")
                    {
                        DailyAmount = CalculateAmount / 30;
                    }
                    else if (SelectedDuration == "Year")
                    {
                        DailyAmount = CalculateAmount / 365;
                    }

                    entSavingAmount.Text = DailyAmount.ToString("c", CultureInfo.CurrentCulture);
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "pckrEverynthDuration_SelectedIndexChanged");
        }
    }

    private void swhIsTopUp_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            if (_vm.Saving.IsTopUp) 
            {
                _vm.IsTopUpParaText = "By topping up your saving we will add the saving amount to your balance every period. This way you can keep building up a stash for bigger less frequent spends. This also allows you to add to your stash as you go on and this amount will remain in your stash.";
                _vm.IsTopUpLabelText = "Top up";
            }
            else
            {
                _vm.IsTopUpParaText = "By replenishing the stash, every pay period we will reset the envelopes balance to the saving amount. Any balance not spent by the end of the period will effectively be added back to your budget for you to spend!";
                _vm.IsTopUpLabelText = "Replenish";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddSaving", "swhIsTopUp_Toggled");
        }

    }
}