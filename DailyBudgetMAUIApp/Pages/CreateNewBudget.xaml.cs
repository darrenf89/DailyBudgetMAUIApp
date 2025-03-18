using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Popups;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using IeuanWalker.Maui.Switch.Events;
using IeuanWalker.Maui.Switch;
using IeuanWalker.Maui.Switch.Helpers;



namespace DailyBudgetMAUIApp.Pages;

public partial class CreateNewBudget : BasePage
{
    private readonly CreateNewBudgetViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public CreateNewBudget(CreateNewBudgetViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
		InitializeComponent(); 

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    protected async override void OnAppearing()
    {
       
        base.OnAppearing();
        try
        {
            if (App.CurrentSettings == null)
            {
                BudgetSettingValues Settings = _ds.GetBudgetSettingsValues(App.DefaultBudgetID).Result;
                App.CurrentSettings = Settings;
            }

            _pt.SetCultureInfo(App.CurrentSettings);

            
            if (_vm.BudgetID == 0)
            {
                string BudgetType = "Basic";
                if (!string.IsNullOrEmpty(App.UserDetails.SubscriptionType))
                {
                    BudgetType = App.UserDetails.SubscriptionType;
                }

                _vm.BudgetID = _ds.CreateNewBudget(App.UserDetails.Email, BudgetType).Result.BudgetID;


                if (_vm.BudgetID != 0 || _vm.NavigatedFrom != null)
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
                    _vm.BudgetSettings = _ds.GetBudgetSettings(_vm.BudgetID).Result;
                }
                else
                {
                    throw new Exception("Couldn't create a new budget in the database when trying to set one up");
                }
            }
            else
            {
                if (_vm.Budget == null || _vm.NavigatedFrom != null)
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
                    _vm.BudgetSettings = _ds.GetBudgetSettings(_vm.BudgetID).Result;
                }

            }

            if (_vm.Budget.BudgetName == "" || _vm.Budget.BudgetName == null)
            {

                string Description = "Every budget needs a name, let us know how you'd like your budget to be known so we can use this to identify it for you in the future.";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Budget Name", Description, DescriptionSub, "Enter a budget name!", _vm.Budget.BudgetName, new PopUpPageSingleInputViewModel());
                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    _vm.Budget.BudgetName = (string)result;
                }

                List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                PatchDoc BudgetName = new PatchDoc
                {
                    op = "replace",
                    path = "/BudgetName",
                    value = _vm.Budget.BudgetName
                };

                BudgetUpdate.Add(BudgetName);

                string ReturnString = _ds.PatchBudget(_vm.BudgetID, BudgetUpdate).Result;

            }

            if (_vm.NavigatedFrom != null)
            {
                _vm.Stage = _vm.NavigatedFrom;

            }

            UpdateStageDisplay();

            if (_vm.SelectedCurrencySymbol == null)
            {
                _vm.SelectedCurrencySymbol = _ds.GetCurrencySymbols(_vm.BudgetSettings.CurrencySymbol.ToString()).Result[0];
                _vm.SelectedCurrencyPlacement = _ds.GetCurrencyPlacements(_vm.BudgetSettings.CurrencyPattern.ToString()).Result[0];
                _vm.SelectedDateFormats = _ds.GetDateFormatsById(_vm.BudgetSettings.ShortDatePattern ?? 1, _vm.BudgetSettings.DateSeperator ?? 1).Result;
                _vm.SelectedNumberFormats = _ds.GetNumberFormatsById(_vm.BudgetSettings.CurrencyDecimalDigits ?? 2, _vm.BudgetSettings.CurrencyDecimalSeparator ?? 2, _vm.BudgetSettings.CurrencyGroupSeparator ?? 1).Result;
                _vm.SelectedTimeZone = _ds.GetTimeZoneById(_vm.BudgetSettings.TimeZone.GetValueOrDefault()).Result;
            }

            pckrSymbolPlacement.SelectedIndex = _vm.SelectedCurrencyPlacement.Id - 1;
            pckrDateFormat.SelectedIndex = _vm.SelectedDateFormats.Id - 1;
            pckrNumberFormat.SelectedIndex = _vm.SelectedNumberFormats.Id - 1;
            pckrTimeZone.SelectedIndex = _vm.SelectedTimeZone.TimeZoneID - 1;


            double BankBalance = (double?)_vm.Budget.BankBalance ?? 0;
            entBankBalance.Text = BankBalance.ToString("c", CultureInfo.CurrentCulture);
            double PayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
            entPayAmount.Text = PayAmount.ToString("c", CultureInfo.CurrentCulture);

            _vm.IsBorrowPay = _vm.Budget.IsBorrowPay;

            dtpckPayDay.Date = _vm.Budget.NextIncomePayday ?? default;
            dtpckPayDay.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow);

            UpdateSelectedOption(_vm.Budget.PaydayType);

            if (_vm.Budget.Bills.Count == 0)
            {
                vslOutgoingList.IsVisible = false;
            }
            else
            {
                vslOutgoingList.IsVisible = true;
            }

            if (_vm.Budget.Savings.Count == 0)
            {
                vslSavingList.IsVisible = false;
            }
            else
            {
                vslSavingList.IsVisible = true;
            }

            if (_vm.Budget.IncomeEvents.Count == 0)
            {
                vslIncomeList.IsVisible = false;
            }
            else
            {
                vslIncomeList.IsVisible = true;
            }

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "OnAppearing");
        }


    }

    private async void UpdateStageDisplay()
    {
        ClearBudgetValidator();

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Gray300", out var Gray300);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Info", out var Info);

        bvStage1.Color = (_vm.Stage == "Budget Settings" || _vm.Stage == "Budget SavingsMauiDetails" || _vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income" || _vm.Stage == "Finalise Budget") ? (Color)Success : (Color)Gray300;
        bvStage2.Color = (_vm.Stage == "Budget SavingsMauiDetails" || _vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income" || _vm.Stage == "Finalise Budget") ? (Color)Success : (Color)Gray300;
        bvStage3.Color = (_vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income" || _vm.Stage == "Finalise Budget") ? (Color)Success : (Color)Gray300;
        bvStage4.Color = (_vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income" || _vm.Stage == "Finalise Budget") ? (Color)Success : (Color)Gray300;
        bvStage5.Color = (_vm.Stage == "Budget Extra Income" || _vm.Stage == "Finalise Budget") ? (Color)Success : (Color)Gray300;

        SettingsDetails.IsVisible = (_vm.Stage == "Budget Settings");
        BudgetDetails.IsVisible = (_vm.Stage == "Budget SavingsMauiDetails");
        BillDetails.IsVisible = (_vm.Stage == "Budget Outgoings");
        SavingsDetails.IsVisible = (_vm.Stage == "Budget Savings");
        IncomeDetails.IsVisible = (_vm.Stage == "Budget Extra Income");
        FinalBudgetDetails.IsVisible = (_vm.Stage == "Finalise Budget");

        lblSettingsHeader.TextColor = (_vm.Stage == "Budget Settings") ? (Color)Primary : (Color)Tertiary;
        lblBudgetHeader.TextColor = (_vm.Stage == "Budget SavingsMauiDetails") ? (Color)Primary : (Color)Tertiary;
        lblBillsHeader.TextColor = (_vm.Stage == "Budget Outgoings") ? (Color)Primary : (Color)Tertiary;
        lblSavingsHeader.TextColor = (_vm.Stage == "Budget Savings") ? (Color)Primary : (Color)Tertiary;
        lblIncomesHeader.TextColor = (_vm.Stage == "Budget Extra Income") ? (Color)Primary : (Color)Tertiary;
        lblFinalBudgetHeader.TextColor = (_vm.Stage == "Finalise Budget") ? (Color)Primary : (Color)Tertiary;

        if (_vm.Stage == "Budget Settings")
        {
            if(_vm.Budget.Stage < 1)
            {
                _vm.Budget.Stage = 1;
            }
            await MainScrollView.ScrollToAsync(0, 95, true);
        }
        else if (_vm.Stage == "Budget SavingsMauiDetails")
        {
            if(_vm.Budget.Stage < 2)
            {
                _vm.Budget.Stage = 2;
            }
            await MainScrollView.ScrollToAsync(0, 155, true);
        }
        else if (_vm.Stage == "Budget Outgoings")
        {
            if(_vm.Budget.Stage < 3)
            {
                _vm.Budget.Stage = 3;
            }

            if(_vm.Budget.Stage > 3)
            {
                UpdateBillsYesNo("No");
            }
            else
            {
                UpdateBillsYesNo("");
            }
            
            await MainScrollView.ScrollToAsync(0, 215, true);
        }
        else if (_vm.Stage == "Budget Savings")
        {
            if(_vm.Budget.Stage < 4)
            {
                _vm.Budget.Stage = 4;
            }

            if(_vm.Budget.Stage > 4)
            {
                UpdateSavingsYesNo("No");
                UpdateBillsYesNo("No");
            }
            else
            {
                UpdateSavingsYesNo("");
            }

            await MainScrollView.ScrollToAsync(0, 275, true);
        }
        else if (_vm.Stage == "Budget Extra Income")
        {
            if(_vm.Budget.Stage < 5)
            {
                _vm.Budget.Stage = 5;
            }

            if(_vm.Budget.Stage > 5)
            {
                UpdateIncomeYesNo("No");
                UpdateSavingsYesNo("No");
                UpdateBillsYesNo("No");
            }
            else
            {
                UpdateIncomeYesNo("");
            }

            await MainScrollView.ScrollToAsync(0, 335, true);
        }
        else if (_vm.Stage == "Finalise Budget")
        {
            if(_vm.Budget.Stage < 6)
            {
                _vm.Budget.Stage = 6;
            }

            await MainScrollView.ScrollToAsync(0, 395, true);            
        }

        if (_vm.Budget.Stage > 5)
        {
            UpdateIncomeYesNo("No");
            UpdateSavingsYesNo("No");
            UpdateBillsYesNo("No");
        }
        else
        {
            UpdateIncomeYesNo("");
        }

        grdDetailsHeader.IsEnabled = _vm.Budget.Stage > 1;
        grdBillsHeader.IsEnabled = _vm.Budget.Stage > 2;
        grdSavingsHeader.IsEnabled = _vm.Budget.Stage > 3;
        grdIncomeHeader.IsEnabled = _vm.Budget.Stage > 4;
        grdFinalHeader.IsEnabled = _vm.Budget.Stage > 5;

        lblDetailsEdit.TextColor = (_vm.Budget.Stage > 1) ? (Color)Info : (Color)Gray300;
        lblBillsEdit.TextColor = (_vm.Budget.Stage > 2) ? (Color)Info : (Color)Gray300;
        lblSavingsEdit.TextColor = (_vm.Budget.Stage > 3) ? (Color)Info : (Color)Gray300;
        lblIncomeEdit.TextColor = (_vm.Budget.Stage > 4) ? (Color)Info : (Color)Gray300;
        lblFinalEdit.TextColor = (_vm.Budget.Stage > 5) ? (Color)Info : (Color)Gray300;

        lblBudgetHeader.TextColor = (_vm.Budget.Stage < 2) ? (Color)Gray300 : lblBudgetHeader.TextColor;
        lblBillsHeader.TextColor = (_vm.Budget.Stage < 3) ? (Color)Gray300 : lblBillsHeader.TextColor;
        lblSavingsHeader.TextColor = (_vm.Budget.Stage < 4) ? (Color)Gray300 : lblSavingsHeader.TextColor;
        lblIncomesHeader.TextColor = (_vm.Budget.Stage < 5) ? (Color)Gray300 : lblIncomesHeader.TextColor;
        lblFinalBudgetHeader.TextColor = (_vm.Budget.Stage < 6) ? (Color)Gray300 : lblFinalBudgetHeader.TextColor;
    }

    private async void GoToStageFinalBudget_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.Stage = "Finalise Budget";
            await _vm.SaveStage("Finalise Budget");
            UpdateStageDisplay();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "GoToStageFinalBudget_Tapped");
        }
    }
    private void GoToStageSettings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.Stage = "Budget Settings";
            UpdateStageDisplay();
            //await MainScrollView.ScrollToAsync(0, 95, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "GoToStageSettings_Tapped");
        }
    }

    private void GoToStageBudget_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            double BankBalance = (double?)_vm.Budget.BankBalance ?? 0;
            entBankBalance.Text = BankBalance.ToString("c", CultureInfo.CurrentCulture);

            double PayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
            entPayAmount.Text = PayAmount.ToString("c", CultureInfo.CurrentCulture);

            _vm.Stage = "Budget SavingsMauiDetails";
            UpdateStageDisplay();
            //await MainScrollView.ScrollToAsync(0, 155, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "GoToStageBudget_Tapped");
        }
    }

    private void GoToStageBills_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.Stage = "Budget Outgoings";
            UpdateStageDisplay();
            //await MainScrollView.ScrollToAsync(0, 215, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "GoToStageBills_Tapped");
        }
    }

    private void GoToStageSavings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.Stage = "Budget Savings";
            UpdateStageDisplay();
            //await MainScrollView.ScrollToAsync(0, 275, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "GoToStageSavings_Tapped");
        }

    }

    private void GoToStageIncomes_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.Stage = "Budget Extra Income";
            UpdateStageDisplay();
            //await MainScrollView.ScrollToAsync(0, 335, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "GoToStageIncomes_Tapped");
        }

    }

    private void ChangeSelectedCurrency_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.SearchVisible = true;
            _vm.CurrencySearchResults = _ds.GetCurrencySymbols("").Result;
            CurrencySearch.Text = "";
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "ChangeSelectedCurrency_Tapped");
        }
    }

    private async void ContinueSettingsButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            double BankBalance = (double?)_vm.Budget.BankBalance ?? 0;
            entBankBalance.Text = BankBalance.ToString("c", CultureInfo.CurrentCulture);
        
            double PayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
            entPayAmount.Text = PayAmount.ToString("c", CultureInfo.CurrentCulture);

            dtpckPayDay.Date = _vm.Budget.NextIncomePayday ?? default;

            _vm.Stage = "Budget SavingsMauiDetails";
            UpdateStageDisplay();

            await _vm.SaveStage("Budget Settings");

            //await MainScrollView.ScrollToAsync(0, 155, true);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "ContinueSettingsButton_Clicked");
        }
    }

    private void BackBudgetDetailsButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            _vm.Stage = "Budget Settings";
            UpdateStageDisplay();

            //await MainScrollView.ScrollToAsync(0, 95, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "BackBudgetDetailsButton_Clicked");
        }

    }

    private async void ContinueBudgetDetailsButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (ValidateBudgetDetails())
            {
                _vm.Stage = "Budget Outgoings";

                _vm.PayAmountText = entPayAmount.Text ?? "";
                _vm.BankBalanceText = entBankBalance.Text ?? "";
                _vm.PayDayDateValue = dtpckPayDay.Date;
                if (_vm.PayDayTypeText == "Everynth")
                {
                    _vm.EveryNthValue = entEverynthValue.Text ?? "0";
                    _vm.EveryNthDuration = pckrEverynthDuration.SelectedItem.ToString() ?? "Weeks";
                }
                else if (_vm.PayDayTypeText == "WorkingDays")
                {
                    _vm.WorkingDaysValue = entWorkingDaysValue.Text ?? "0";
                }
                else if (_vm.PayDayTypeText == "OfEveryMonth")
                {
                    _vm.OfEveryMonthValue = entOfEveryMonthValue.Text ?? "0";
                }
                else if (_vm.PayDayTypeText == "LastOfTheMonth")
                {
                    _vm.LastOfTheMonthDuration = pckrLastOfTheMonthDuration.SelectedItem.ToString() ?? "Monday";
                }

                UpdateStageDisplay();

                await _vm.SaveStage("Budget SavingsMauiDetails");

                entBankBalance.IsEnabled = false;
                entBankBalance.IsEnabled = true;
                entPayAmount.IsEnabled = false;
                entPayAmount.IsEnabled = true;
                entEverynthValue.IsEnabled = false;
                entEverynthValue.IsEnabled = true;
                entOfEveryMonthValue.IsEnabled = false;
                entOfEveryMonthValue.IsEnabled = true;
                entWorkingDaysValue.IsEnabled = false;
                entWorkingDaysValue.IsEnabled = true;

                //await MainScrollView.ScrollToAsync(0, 215, true);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "ContinueBudgetDetailsButton_Clicked");
        }

    }

    private bool ValidateBudgetDetails()
    {
        bool IsValid = true;

        if (dtpckPayDay.Date <= _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
        {
            IsValid = false;
            validatorPayDay.IsVisible = true;
        }
        else
        {
            validatorPayDay.IsVisible = false;
        }

        if(_vm.PayDayTypeText == "" || _vm.PayDayTypeText == null)
        {
            IsValid = false;
            validatorPayType.IsVisible = true;
        }
        else
        {
            validatorPayType.IsVisible = false;
        }

        if (_vm.PayDayTypeText == "Everynth" && entEverynthValue.Text == "")
        {
            IsValid = false;
            validatorEveryNthDuration.IsVisible = true;
        }
        else
        {
            validatorEveryNthDuration.IsVisible = false;
        }

        if (_vm.PayDayTypeText == "WorkingDays" && entWorkingDaysValue.Text == "")
        {
            IsValid = false;
            validatorWorkingDayDuration.IsVisible = true;
        }
        else
        {
            validatorWorkingDayDuration.IsVisible = false;
        }

        if (_vm.PayDayTypeText == "OfEveryMonth" && entOfEveryMonthValue.Text == "")
        {
            IsValid = false;
            validatorOfEveryMonthDuration.IsVisible = true;
        }
        else
        {
            validatorOfEveryMonthDuration.IsVisible = false;
        }


        return IsValid;
    }

    private void BackBudgetBillsButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            _vm.Stage = "Budget SavingsMauiDetails";
            UpdateStageDisplay();

            //await MainScrollView.ScrollToAsync(0, 155, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "BackBudgetBillsButton_Clicked");
        }
    }

    private async void ContinueBudgetBillsButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (ValidateBudgetOutgoings())
            {
                _vm.Stage = "Budget Savings";
                UpdateStageDisplay();

                await _vm.SaveStage("Budget Outgoings");

                //await MainScrollView.ScrollToAsync(0, 275, true);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "ContinueBudgetBillsButton_Clicked");
        }

    }

    private void BackBudgetSavingsButton_Clicked(object sender, EventArgs e)
    {
        try
        {

            _vm.Stage = "Budget Outgoings";
            UpdateStageDisplay();

            //await MainScrollView.ScrollToAsync(0, 155, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "BackBudgetSavingsButton_Clicked");
        }
    }

    private async void ContinueBudgetSavingsButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (ValidateBudgetSavings())
            {
                _vm.Stage = "Budget Extra Income";
                UpdateStageDisplay();

                await _vm.SaveStage("Budget Outgoings");

                //await MainScrollView.ScrollToAsync(0, 275, true);
            }

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "ContinueBudgetSavingsButton_Clicked");
        }

    }

    private async void ContinueBudgetIncomeButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (ValidateBudgetIncome())
            {
                _vm.Stage = "Finalise Budget";
                UpdateStageDisplay();

                await _vm.SaveStage("Budget Extra Income");
                await _vm.SaveStage("Finalise Budget");

                //await MainScrollView.ScrollToAsync(0, 275, true);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "ContinueBudgetIncomeButton_Clicked");
        }
    }

    private void BackBudgetIncomeButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            _vm.Stage = "Budget Savings";
            UpdateStageDisplay();

            //await MainScrollView.ScrollToAsync(0, 155, true);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "BackBudgetIncomeButton_Clicked");
        }
    }

    private async void FinaliseBudgetButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            if (ValidateFinaliseBudget())
            {
                var page = new LoadingPage();
                await Application.Current.Windows[0].Navigation.PushModalAsync(page);

                await _vm.SaveStage("Finalise Budget");
                await _vm.SaveStage("Create Budget");

                ;

                if (App.DefaultBudgetID != _vm.BudgetID)
                {
                    bool result = await Shell.Current.DisplayAlert("Change Default Budget?", "Do you want to make the newly created budget your default budget?", "Yes", "No");
                    if (result)
                    {
                    
                        await _pt.ChangeDefaultBudget(App.UserDetails.UserID, _vm.BudgetID, false);
                    }
                }

                App.SessionLastUpdate = default(DateTime);
    ;
                await Shell.Current.GoToAsync($"///{nameof(MainPage)}?SnackBar=Budget Created&SnackID={_vm.BudgetID}");

            }

            //popup.Close();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "FinaliseBudgetButton_Clicked");
        }

    }

    private void BackFinalBudgetButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            _vm.Stage = "Budget Extra Income";
            UpdateStageDisplay();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "BackFinalBudgetButton_Clicked");
        }
    }

    private bool ValidateBudgetIncome()
    {
        bool IsValid = true;

        if (_vm.IncomeYesNoSelect != "No")
        {
            if (_vm.IncomeYesNoSelect == "Yes")
            {
                MessagevalidatorIncomeYesNo.Text = "Please add an income or select no to continue!";
            }
            else
            {
                MessagevalidatorIncomeYesNo.Text = "Let us know if you want to add any incomes or not.";
            }
            IsValid = false;
            validatorIncomeYesNo.IsVisible = true;
        }
        else
        {
            validatorIncomeYesNo.IsVisible = false;
        }

        return IsValid;
    }

    private bool ValidateBudgetSavings()
    {
        bool IsValid = true;

        if (_vm.SavingsYesNoSelect != "No")
        {
            if (_vm.SavingsYesNoSelect == "Yes")
            {
                MessagevalidatorSavingsYesNo.Text = "Please add a saving or select no to continue!";
            }
            else
            {
                MessagevalidatorSavingsYesNo.Text = "Let us know if you want to add any savings or not.";
            }
            IsValid = false;
            validatorSavingsYesNo.IsVisible = true;
        }
        else
        {
            validatorSavingsYesNo.IsVisible = false;
        }

        return IsValid;
    }

    public void ClearBudgetValidator()
    {
        validatorOutgoingsYesNo.IsVisible = false;
        validatorPayDay.IsVisible = false;
        validatorPayType.IsVisible = false;
        validatorEveryNthDuration.IsVisible = false;
        validatorWorkingDayDuration.IsVisible = false;
        validatorOfEveryMonthDuration.IsVisible = false;
        validatorOutgoingsYesNo.IsVisible = false;
        validatorSavingsYesNo.IsVisible = false;
        validatorIncomeYesNo.IsVisible = false;
        validatorAcceptTerms.IsVisible = false;
    }  

    private bool ValidateFinaliseBudget()
    {
        bool IsValid = true;

        if(!chbxIsAcceptTerms.IsChecked)
        {
            validatorAcceptTerms.IsVisible = true;
            IsValid = false;
        }
        else
        {
            validatorAcceptTerms.IsVisible = false;
            if(!ValidateBudgetIncome())
            {
                _vm.Stage = "Budget Extra Income";
                IsValid = false;
            }

            if(!ValidateBudgetSavings())
            {
                _vm.Stage = "Budget Savings";
                IsValid = false;
            }

            if(!ValidateBudgetOutgoings())
            {
                _vm.Stage = "Budget Outgoings";
                IsValid = false;
            }

            if(!ValidateBudgetDetails())
            {
                _vm.Stage = "Budget SavingsMauiDetails";
                IsValid = false;
            }

            UpdateStageDisplay();

        }

        return IsValid;
    }

    private bool ValidateBudgetOutgoings()
    {
        bool IsValid = true;

        if (_vm.BillsYesNoSelect != "No")
        {
            if (_vm.BillsYesNoSelect == "Yes")
            {
                MessagevalidatorBillsYesNo.Text = "Please add an outgoing or select no to continue!";
            }
            else
            {
                MessagevalidatorBillsYesNo.Text = "Let us know if you want to add any outgoing or not.";
            }
            IsValid = false;
            validatorOutgoingsYesNo.IsVisible = true;
        }
        else
        {
            validatorOutgoingsYesNo.IsVisible = false;
        }

        return IsValid;
    }

    private async void AddBillsNewBudget_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            await Shell.Current.GoToAsync($"../{nameof(AddBill)}?BudgetID={_vm.BudgetID}&BillID={0}&NavigatedFrom=CreateNewBudget");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "AddBillsNewBudget_Clicked");
        }
    }

    private async void AddSavingsNewBudget_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            await Shell.Current.GoToAsync($"../{nameof(AddSaving)}?BudgetID={_vm.BudgetID}&SavingID={0}&NavigatedFrom=CreateNewBudget");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "AddSavingsNewBudget_Clicked");
        }
    }

    private async void AddIncomeNewBudget_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            await Shell.Current.GoToAsync($"../{nameof(AddIncome)}?BudgetID={_vm.BudgetID}&IncomeID={0}&NavigatedFrom=CreateNewBudget");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "AddIncomeNewBudget_Clicked");
        }
    }

    private async void BankBalanceInfo(object sender, EventArgs e)
    {
        try
        {
            List<string> SubTitle = new List<string>{
                "",
                "",
                ""
            };

            List<string> Info = new List<string>{
                "The amount of money you currently have available is known in the app as your BankBalance. If all your money was in one place, it would be the amount of money you would see when you open your banking app. Fortunately though we don't care where all your money is, you can have it in multiple places in real life we use just one number to make it easier to manage.",
                "When you are creating your budget it is advisable to figure out exactly how much money you have to your name and use this figure, however you don't have to .. if you know better use a different figure. Whatever you input will be used to work out how much you have to spend daily until your next pay day.",
                "It is also worth knowing that your BankBalance is not always what you have to spend, you have to take into account savings, bills and any other income!, We will use other terms along with Bank Balance to describe your budgets state - MaB (Money available Balance) & LtSB (Left to Spend Balance)"
            };

            var popup = new PopupInfo("Bank Balance", SubTitle, Info);
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "BankBalanceInfo");
        }
    }

    void BankBalance_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            _pt.FormatBorderlessEntryNumber(sender, e, entBankBalance);
        }
        catch (Exception ex)
        {
             _pt.HandleException(ex, "CreateNewBudget", "BankBalance_Changed");
        }

    }
    void PayAmount_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            _pt.FormatBorderlessEntryNumber(sender, e, entPayAmount);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "PayAmount_Changed");
        }
    }
    private async void PayDayInfo(object sender, EventArgs e)
    {
        try
        {
            List<string> SubTitle = new List<string>{
                "",
                "",
                ""
            };

            List<string> Info = new List<string>{
                "The \"When is Pay Day?\" field in our app is essential for establishing your financial starting point. By entering the exact date of your next payday—whether it's tomorrow, next week, or next month—you enable the app to accurately calculate your initial budget values. This initial input, combined with your other budget details, sets the foundation for a personalized budgeting experience. Subsequently, the app uses the pay frequency information you've provided to determine future pay dates, ensuring that your budget aligns seamlessly with your income schedule from that point onward."
            };

            var popup = new PopupInfo("When is Pay day?", SubTitle, Info);
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "PayDayInfo");
        }
    }

    private async void PayDetailsInfo(object sender, EventArgs e)
    {
        try
        {
            List<string> SubTitle = new List<string>{
                "",
                "Option 1: \"Every Nth\"",
                "Option 2: \"Nth Last Working Day of the Month\"",
                "Option 3: \"Same Day Every Month\"",
                "Option 4: \"Last Weekday of the Month\"",
                "Understanding Budget Cycles and Daily Values:",
                "",
                ""
            };

            List<string> Info = new List<string>{
                "The \"How do you get paid?\" section in our app offers four customizable options to align your budgeting cycle with your income frequency. This flexibility ensures that your budget accurately reflects your financial situation, enhancing your ability to manage expenses effectively.",
                "The \"Every Nth\" option allows you to define your pay frequency by specifying the exact number of days, weeks, or months between each payday. For example, if you select \"every 2 weeks,\" your budget cycle will span 14 days, and the app will calculate your subsequent paydays by adding 14 days to the previous one. This setting ensures that your budgeting aligns precisely with your unique income schedule, providing accurate daily budget calculations based on your specified cycle.",
                "The \"Nth Last Working Day of the Month\" option allows you to set your payday to fall on a specific weekday occurrence before the month's end. For example, selecting '2' as the number means your payday will be calculated as the second-to-last working day of the next month, considering weekends and holidays. This approach ensures that your payday aligns with your preferences while accounting for variations in month lengths and non-working days",
                "The \"Same Day Every Month\" option allows you to set your payday to occur on the same day each month, such as the 28th. This consistency simplifies budgeting by providing predictable income intervals. However, it's important to note that months vary in length, so the 28th may not always be the same day of the week. Additionally, some months have more than 28 days, so the app adjusts your payday accordingly, ensuring it falls on the specified day each month.",
                "The \"Last Weekday of the Month\" option allows you to set your payday to occur on the final weekday of each month, ensuring consistency in your budgeting cycle. For example, selecting \"Last Thursday\" means your payday will always fall on the last Thursday of every month. This feature is particularly useful for individuals whose pay schedules align with specific weekdays near the end of the month.",
                "In our app, the budget cycle directly influences how daily budget values are calculated. For instance, if you select a bi-weekly pay schedule, the app divides your total income by 14 to determine your daily budget. This method ensures that your spending limits are proportionate to your income distribution, promoting balanced and realistic budgeting.",
                "By customizing your pay frequency and budget cycle, you gain greater control over your financial planning, allowing for a budgeting experience that truly reflects your income dynamics.",
            };

            var popup = new PopupInfo("How do you get paid?", SubTitle, Info);
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "PayDetailsInfo");
        }
    }

    private void UpdateSelectedOption(string option) 
    {
        ClearBudgetValidator();

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "Everynth")
        {
            vslOption1Select.BackgroundColor = (Color)Success;
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption4Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblOption1.FontAttributes = FontAttributes.Bold;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.None;
            lblOption4.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)White;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)Gray900;
            lblOption4.TextColor = (Color)Gray900;

            vslOption1.IsVisible = true;
            vslOption2.IsVisible = false;
            vslOption3.IsVisible = false;
            vslOption4.IsVisible = false;

            pckrEverynthDuration.SelectedItem = _vm.Budget.PaydayDuration ?? "days";
            entEverynthValue.Text = _vm.Budget.PaydayValue.ToString() ?? "1";

            _vm.PayDayTypeText = "Everynth";

        }
        else if (option == "WorkingDays")
        {
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = (Color)Success;
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption4Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.Bold;
            lblOption3.FontAttributes = FontAttributes.None;
            lblOption4.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)White;
            lblOption3.TextColor = (Color)Gray900;
            lblOption4.TextColor = (Color)Gray900;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = true;
            vslOption3.IsVisible = false;
            vslOption4.IsVisible = false;

            entWorkingDaysValue.Text = _vm.Budget.PaydayValue.ToString() ?? "1";

            _vm.PayDayTypeText = "WorkingDays";
        }
        else if (option == "OfEveryMonth")
        {
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption3Select.BackgroundColor = (Color)Success;
            vslOption4Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.Bold;
            lblOption4.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)White;
            lblOption4.TextColor = (Color)Gray900;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = false;
            vslOption3.IsVisible = true;
            vslOption4.IsVisible = false;

            entOfEveryMonthValue.Text = _vm.Budget.PaydayValue.ToString() ?? "1";

            _vm.PayDayTypeText = "OfEveryMonth";
        }
        else if (option == "LastOfTheMonth")
        {

            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption4Select.BackgroundColor = (Color)Success;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.None;
            lblOption4.FontAttributes = FontAttributes.Bold;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)Gray900;
            lblOption4.TextColor = (Color)White;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = false;
            vslOption3.IsVisible = false;
            vslOption4.IsVisible = true;

            pckrLastOfTheMonthDuration.SelectedItem = _vm.Budget.PaydayDuration ?? "Monday";

            _vm.PayDayTypeText = "LastOfTheMonth";
        }
        else
        {
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption3Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption4Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;
            lblOption3.FontAttributes = FontAttributes.None;
            lblOption4.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;
            lblOption3.TextColor = (Color)Gray900;
            lblOption4.TextColor = (Color)Gray900;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = false;
            vslOption3.IsVisible = false;
            vslOption4.IsVisible = false;

            _vm.PayDayTypeText = "";
        }
    }

    private void Option1Select_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSelectedOption("Everynth");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "Option1Select_Tapped");
        }
    }

    private void Option2Select_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSelectedOption("WorkingDays");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "Option2Select_Tapped");
        }
    }

    private void Option3Select_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSelectedOption("OfEveryMonth");

        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "Option3Select_Tapped");
        }
    }

    private void Option4Select_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSelectedOption("LastOfTheMonth");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "Option4Select_Tapped");
        }
    }

    void EveryNthValue_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            Regex regex = new Regex(@"^\d+$");

            if (e.NewTextValue != null && e.NewTextValue != "")
            {
                if (!regex.IsMatch(e.NewTextValue))
                {
                    entEverynthValue.Text = e.OldTextValue;
                }
                else
                {
                    entEverynthValue.Text = e.NewTextValue;
                }
            }

        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "EveryNthValue_Changed");
        }
    }

    void WorkingDaysValue_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {

            Regex regex = new Regex(@"^\d+$");

            if (e.NewTextValue != null && e.NewTextValue != "")
            {
                if (!regex.IsMatch(e.NewTextValue))
                {
                    entWorkingDaysValue.Text = e.OldTextValue;
                }
                else
                {
                    entWorkingDaysValue.Text = e.NewTextValue;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "WorkingDaysValue_Changed");
        }
    }
    void OfEveryMonthValue_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            Regex regex = new Regex(@"^\d+$");

            if (e.NewTextValue != null && e.NewTextValue != "")
            {
                if (!regex.IsMatch(e.NewTextValue))
                {
                    entOfEveryMonthValue.Text = e.OldTextValue;
                }
                else
                {
                    entOfEveryMonthValue.Text = e.NewTextValue;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "OnAppearing");
        }
    }

    private void BillsYesSelect_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateBillsYesNo("Yes");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "BillsYesSelect_Tapped");
        }
    }

    private void BillsNoSelect_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateBillsYesNo("No");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "BillsNoSelect_Tapped");
        }        
    }

    private void SavingsYesSelect_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSavingsYesNo("Yes");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "SavingsYesSelect_Tapped");
        }        
    }

    private void SavingsNoSelect_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSavingsYesNo("No");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "SavingsNoSelect_Tapped");
        }
    }

    private void IncomeYesSelect_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateIncomeYesNo("Yes");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "SavingsNoSelect_Tapped");
        }
    }

    private void IncomeNoSelect_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateIncomeYesNo("No");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "IncomeNoSelect_Tapped");
        }
    }

    private void UpdateBillsYesNo(string option) 
    {
        ClearBudgetValidator();

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "Yes")
        {
            vslBillsYesSelect.BackgroundColor = (Color)Success;
            vslBillsNoSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblBillsYes.FontAttributes = FontAttributes.Bold;
            lblBillsNo.FontAttributes = FontAttributes.None;

            lblBillsYes.TextColor = (Color)White;
            lblBillsNo.TextColor = (Color)Gray900;

            btnAddBillsNewBudget.IsVisible = true;

            _vm.BillsYesNoSelect = "Yes";

        }
        else if (option == "No")
        {
            vslBillsYesSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslBillsNoSelect.BackgroundColor = (Color)Success;

            lblBillsYes.FontAttributes = FontAttributes.None;
            lblBillsNo.FontAttributes = FontAttributes.Bold;

            lblBillsYes.TextColor = (Color)Gray900;
            lblBillsNo.TextColor = (Color)White;

            btnAddBillsNewBudget.IsVisible = false;

            _vm.BillsYesNoSelect = "No";
        }
        else
        {
            vslBillsYesSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslBillsNoSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblBillsYes.FontAttributes = FontAttributes.None;
            lblBillsNo.FontAttributes = FontAttributes.None;

            lblBillsYes.TextColor = (Color)Gray900;
            lblBillsNo.TextColor = (Color)Gray900;

            btnAddBillsNewBudget.IsVisible = false;

            _vm.BillsYesNoSelect = "";
        }
    }

    private void UpdateSavingsYesNo(string option)
    {
        ClearBudgetValidator();

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "Yes")
        {
            vslSavingsYesSelect.BackgroundColor = (Color)Success;
            vslSavingsNoSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblSavingsYes.FontAttributes = FontAttributes.Bold;
            lblSavingsNo.FontAttributes = FontAttributes.None;

            lblSavingsYes.TextColor = (Color)White;
            lblSavingsNo.TextColor = (Color)Gray900;

            btnAddSavingsNewBudget.IsVisible = true;

            _vm.SavingsYesNoSelect = "Yes";

        }
        else if (option == "No")
        {
            vslSavingsYesSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslSavingsNoSelect.BackgroundColor = (Color)Success;

            lblSavingsYes.FontAttributes = FontAttributes.None;
            lblSavingsNo.FontAttributes = FontAttributes.Bold;

            lblSavingsYes.TextColor = (Color)Gray900;
            lblSavingsNo.TextColor = (Color)White;

            btnAddSavingsNewBudget.IsVisible = false;

            _vm.SavingsYesNoSelect = "No";
        }
        else
        {
            vslSavingsYesSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslSavingsNoSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblSavingsYes.FontAttributes = FontAttributes.None;
            lblSavingsNo.FontAttributes = FontAttributes.None;

            lblSavingsYes.TextColor = (Color)Gray900;
            lblSavingsNo.TextColor = (Color)Gray900;

            btnAddSavingsNewBudget.IsVisible = false;

            _vm.SavingsYesNoSelect = "";
        }
    }

    private void UpdateIncomeYesNo(string option)
    {
        ClearBudgetValidator();

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "Yes")
        {
            vslIncomeYesSelect.BackgroundColor = (Color)Success;
            vslIncomeNoSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblIncomeYes.FontAttributes = FontAttributes.Bold;
            lblIncomeNo.FontAttributes = FontAttributes.None;

            lblIncomeYes.TextColor = (Color)White;
            lblIncomeNo.TextColor = (Color)Gray900;

            btnAddIncomeNewBudget.IsVisible = true;

            _vm.IncomeYesNoSelect = "Yes";

        }
        else if (option == "No")
        {
            vslIncomeYesSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslIncomeNoSelect.BackgroundColor = (Color)Success;

            lblIncomeYes.FontAttributes = FontAttributes.None;
            lblIncomeNo.FontAttributes = FontAttributes.Bold;

            lblIncomeYes.TextColor = (Color)Gray900;
            lblIncomeNo.TextColor = (Color)White;

            btnAddIncomeNewBudget.IsVisible = false;

            _vm.IncomeYesNoSelect = "No";
        }
        else
        {
            vslIncomeYesSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslIncomeNoSelect.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblIncomeYes.FontAttributes = FontAttributes.None;
            lblIncomeNo.FontAttributes = FontAttributes.None;

            lblIncomeYes.TextColor = (Color)Gray900;
            lblIncomeNo.TextColor = (Color)Gray900;

            btnAddIncomeNewBudget.IsVisible = false;

            _vm.IncomeYesNoSelect = "";
        }
    }

    private async void DeleteBudgetOutgoings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Bill = (Bills)e.Parameter;

            bool result = await DisplayAlert("Bill", "Are you sure you want to delete your Outgoing " + Bill.BillName.ToString(), "Yes, continue", "Cancel");
            if (result)
            {
                string Result = _ds.DeleteBill(Bill.BillID).Result;
                if(Result == "OK")
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;

                    if (_vm.Budget.Bills.Count == 0)
                    {
                        vslOutgoingList.IsVisible = false;
                    }
                }            
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "DeleteBudgetOutgoings_Tapped");
        }
    }

    private async void EditBudgetOutgoings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            var Bill = (Bills)e.Parameter;

            await Shell.Current.GoToAsync($"../{nameof(AddBill)}?BudgetID={_vm.BudgetID}&BillID={Bill.BillID}&NavigatedFrom=CreateNewBudget");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "EditBudgetOutgoings_Tapped");
        }
    }

    private void OutgoingViewCell_Appearing(object sender, EventArgs e)
    {
        try
        {
            var vcBill = (ViewCell)sender;
            var Bill = (Bills)vcBill.BindingContext;        
            Label Header = (Label)vcBill.FindByName("lblOutgoingheader");
            Label Values = (Label)vcBill.FindByName("lblBillValues");
            Label lblBillRegValues = (Label)vcBill.FindByName("lblBillRegValues");
            Label lblBillCurrent = (Label)vcBill.FindByName("lblBillCurrent");

            lblBillRegValues.Text = String.Format("   {0:c} per day", Bill.RegularBillValue);
            Values.Text = String.Format("/{0:c} ", Bill.BillAmount, Bill.RegularBillValue);
            lblBillCurrent.Text = String.Format("{0:c}", Bill.BillCurrentBalance);

            if (Bill.BillType == "OfEveryMonth")
            {
                Header.Text = "Monthly Outgoing Added";
            }
            else if(Bill.BillType == "Everynth")
            {
                Header.Text = "Every x " + Bill.BillDuration + " Outgoing Added";
            }
            else
            {
                Header.Text = "One-off Outgoing Added";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "OutgoingViewCell_Appearing");
        }
    }

    private async void EditBudgetSavings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            var Saving = (Savings)e.Parameter;

            await Shell.Current.GoToAsync($"../{nameof(AddSaving)}?BudgetID={_vm.BudgetID}&SavingID={Saving.SavingID}&NavigatedFrom=CreateNewBudget");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "EditBudgetSavings_Tapped");
        }
    }

    private async void DeleteBudgetSavings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;

            bool result = await DisplayAlert("Savings", "Are you sure you want to delete your Saving " + Saving.SavingsName.ToString(), "Yes, continue", "Cancel");
            if (result)
            {
                string Result = _ds.DeleteSaving(Saving.SavingID).Result;
                if (Result == "OK")
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;

                    if (_vm.Budget.Savings.Count == 0)
                    {
                        vslSavingList.IsVisible = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "DeleteBudgetSavings_Tapped");
        }
    }

    private void SavingsViewCell_Appearing(object sender, EventArgs e)
    {
        try
        {
            var vcSaving = (ViewCell)sender;
            var Saving = (Savings)vcSaving.BindingContext;

            Label lblSavingsheader = (Label)vcSaving.FindByName("lblSavingsheader");
            Label lblSavingCurrent = (Label)vcSaving.FindByName("lblSavingCurrent");
            Label lblSavingTarget = (Label)vcSaving.FindByName("lblSavingTarget");
            Label lblSavingRegValues = (Label)vcSaving.FindByName("lblSavingRegValues");

            if(Saving.SavingsType == "TargetDate")
            {
                Image imgTargetDate = (Image)vcSaving.FindByName("imgTargetDate");
                imgTargetDate.IsVisible = true;

                lblSavingsheader.Text = Saving.GoalDate.GetValueOrDefault().ToString("dd MMM yy") + " Savings Goal Added";
                lblSavingCurrent.Text = String.Format("{0:c} / ", Saving.CurrentBalance);
                lblSavingTarget.Text = String.Format("{0:c}    ", Saving.SavingsGoal);
                lblSavingRegValues.Text = String.Format("{0:c}", Saving.RegularSavingValue);

                lblSavingCurrent.IsVisible = true;
                lblSavingTarget.IsVisible = true;
                lblSavingRegValues.IsVisible = true;

            }
            else if (Saving.SavingsType == "TargetAmount")
            {
                Image imgTargetAmount = (Image)vcSaving.FindByName("imgTargetAmount");
                imgTargetAmount.IsVisible = true;

                lblSavingsheader.Text = String.Format("{0:c}", Saving.SavingsGoal) + " Savings Goal Added";
                lblSavingCurrent.Text = String.Format("{0:c} / ", Saving.CurrentBalance);
                lblSavingTarget.Text = String.Format("{0:c}    ", Saving.SavingsGoal);
                if(Saving.IsDailySaving)
                {
                    lblSavingRegValues.Text = String.Format("{0:c}", Saving.RegularSavingValue);
                }
                else
                {
                    lblSavingRegValues.Text = String.Format("{0:c}", Saving.PeriodSavingValue);
                }

                lblSavingCurrent.IsVisible = true;
                lblSavingTarget.IsVisible = true;
                lblSavingRegValues.IsVisible = true;

            }
            else if (Saving.SavingsType == "SavingsBuilder")
            {
                Image imgSavingsBuilder = (Image)vcSaving.FindByName("imgSavingsBuilder");
                imgSavingsBuilder.IsVisible = true;

                lblSavingsheader.Text = "Builder Savings Goal Added";
                lblSavingCurrent.Text = String.Format("{0:c}    ", Saving.CurrentBalance);
                lblSavingRegValues.Text = String.Format("{0:c}", Saving.RegularSavingValue);

                lblSavingCurrent.IsVisible = true;
                lblSavingRegValues.IsVisible = true;
            }
            else
            {
                Image imgEnvelope = (Image)vcSaving.FindByName("imgEnvelope");
                imgEnvelope.IsVisible = true;
                lblSavingsheader.Text = String.Format("{0:c}", Saving.PeriodSavingValue) + " Pay Period Saving Added";
                lblSavingCurrent.Text = String.Format("{0:c}    ", Saving.CurrentBalance);
                lblSavingRegValues.Text = String.Format("{0:c}", Saving.PeriodSavingValue);

                lblSavingCurrent.IsVisible = true;
                lblSavingRegValues.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "SavingsViewCell_Appearing");
        }
    }

    private void IncomesViewCell_Appearing(object sender, EventArgs e)
    {
        try
        {
            var vcIncome = (ViewCell)sender;
            var Income = (IncomeEvents)vcIncome.BindingContext;

            Label lblIncomeheader = (Label)vcIncome.FindByName("lblIncomeheader");
            Label lblIncomeAmount = (Label)vcIncome.FindByName("lblIncomeAmount");
            Label lblIncomeDate = (Label)vcIncome.FindByName("lblIncomeDate");

            if (Income.RecurringIncomeType == "OfEveryMonth")
            {
                lblIncomeheader.Text = "Monthly Outgoing Added";
            }
            else if(Income.RecurringIncomeType == "Everynth")
            {
                lblIncomeheader.Text = "Every x " + Income.RecurringIncomeDuration + " Outgoing Added";
            }
            else
            {
                lblIncomeheader.Text = "One-off Income Added";
            }

            lblIncomeAmount.Text = string.Format("{0:c}     ",Income.IncomeAmount);
            lblIncomeDate.Text = Income.DateOfIncomeEvent.ToString("dd MMM yy");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "IncomesViewCell_Appearing");
        }
    }

    private async void EditBudgetIncome_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            var Income = (IncomeEvents)e.Parameter;

            await Shell.Current.GoToAsync($"../{nameof(AddIncome)}?BudgetID={_vm.BudgetID}&IncomeID={Income.IncomeEventID}&NavigatedFrom=CreateNewBudget");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "EditBudgetIncome_Tapped");
        }
    }

    private async void DeleteBudgetIncome_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Income = (IncomeEvents)e.Parameter;

            bool result = await DisplayAlert("Income", "Are you sure you want to delete your Income " + Income.IncomeName.ToString(), "Yes, continue", "Cancel");
            if (result)
            {
                string Result = _ds.DeleteIncome(Income.IncomeEventID).Result;
                if (Result == "OK")
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;

                    if (_vm.Budget.IncomeEvents.Count == 0)
                    {
                        vslIncomeList.IsVisible = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewBudget", "DeleteBudgetIncome_Tapped");
        }

    }

    private void IsAcceptTerms_CheckChanged(object sender, CheckedChangedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("Success", out var Success);
            Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
            Application.Current.Resources.TryGetValue("White", out var White);

            if(chbxIsAcceptTerms.IsChecked)
            {
                brdIsAcceptTerms.BackgroundColor = (Color)Success;
                hslIsAcceptTerms.BackgroundColor = (Color)Success;
                lblIsAcceptTerms.TextColor = (Color)White;
            }
            else
            {
                brdIsAcceptTerms.BackgroundColor = (Color)White;
                hslIsAcceptTerms.BackgroundColor = (Color)White;
                lblIsAcceptTerms.TextColor = (Color)Gray900;
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewBudget", "IsAcceptTerms_CheckChanged");
        }
    }

}
