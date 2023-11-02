using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using CommunityToolkit.Maui.ApplicationModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Internals;


namespace DailyBudgetMAUIApp.Pages;

public partial class CreateNewBudget : ContentPage
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

    async protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {

        try
        {
            _pt.SetCultureInfo(App.CurrentSettings);
            base.OnNavigatedTo(args);
            if (_vm.BudgetID == 0)
            {
                _vm.BudgetID = _ds.CreateNewBudget(App.UserDetails.Email).Result.BudgetID;


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
                if(_vm.Budget == null || _vm.NavigatedFrom != null)
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
                    _vm.BudgetSettings = _ds.GetBudgetSettings(_vm.BudgetID).Result;
                }

            }

            if (_vm.Budget.BudgetName == "" || _vm.Budget.BudgetName == null)
            {

                try
                {
                    string Description = "Every budget needs a name, let us know how you'd like your budget to be known so we can use this to identify it for you in the future.";
                    string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                    var popup = new PopUpPageSingleInput("Budget Name", Description, DescriptionSub, "Enter a budget name!", _vm.Budget.BudgetName, new PopUpPageSingleInputViewModel());
                    var result = await Application.Current.MainPage.ShowPopupAsync(popup);

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
                catch (Exception ex)
                {
                    ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "LoadingPopupBudgetName").Result;
                    await Shell.Current.GoToAsync(nameof(ErrorPage),
                        new Dictionary<string, object>
                        {
                            ["Error"] = Error
                        });
                }

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
            }

            pckrSymbolPlacement.SelectedIndex = _vm.SelectedCurrencyPlacement.Id - 1;            
            pckrDateFormat.SelectedIndex = _vm.SelectedDateFormats.Id - 1;
            pckrNumberFormat.SelectedIndex = _vm.SelectedNumberFormats.Id - 1;


            double BankBalance = (double?)_vm.Budget.BankBalance ?? 0;
            entBankBalance.Text = BankBalance.ToString("c", CultureInfo.CurrentCulture);
            double PayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
            entPayAmount.Text = PayAmount.ToString("c", CultureInfo.CurrentCulture);

            dtpckPayDay.Date = _vm.Budget.NextIncomePayday ?? default;
            dtpckPayDay.MinimumDate = DateTime.Now;

            UpdateSelectedOption(_vm.Budget.PaydayType);

            if(_vm.Budget.Bills.Count == 0)
            {
                vslOutgoingList.IsVisible = false;
            }

            if(_vm.Budget.Savings.Count == 0)
            {
                vslSavingList.IsVisible = false;
            }

            if(_vm.Budget.IncomeEvents.Count == 0)
            {
                vslIncomeList.IsVisible = false;
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($" --> {ex.Message}");
            ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "Constructor").Result;
            await Shell.Current.GoToAsync(nameof(ErrorPage),
                new Dictionary<string, object>
                {
                    ["Error"] = Error
                });
        }

    }
    private async void UpdateStageDisplay()
    {
        ClearBudgetValidator();

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Gray300", out var Gray300);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);

        bvStage1.Color = (_vm.Stage == "Budget Settings" || _vm.Stage == "Budget Details" || _vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;
        bvStage2.Color = (_vm.Stage == "Budget Details" || _vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;
        bvStage3.Color = (_vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;
        bvStage4.Color = (_vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;
        bvStage5.Color = (_vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;

        SettingsDetails.IsVisible = (_vm.Stage == "Budget Settings");
        BudgetDetails.IsVisible = (_vm.Stage == "Budget Details");
        BillDetails.IsVisible = (_vm.Stage == "Budget Outgoings");
        SavingsDetails.IsVisible = (_vm.Stage == "Budget Savings");
        IncomeDetails.IsVisible = (_vm.Stage == "Budget Extra Income");

        lblSettingsHeader.TextColor = (_vm.Stage == "Budget Settings") ? (Color)Primary : (Color)Tertiary;
        lblBudgetHeader.TextColor = (_vm.Stage == "Budget Details") ? (Color)Primary : (Color)Tertiary;
        lblBillsHeader.TextColor = (_vm.Stage == "Budget Outgoings") ? (Color)Primary : (Color)Tertiary;
        lblSavingsHeader.TextColor = (_vm.Stage == "Budget Savings") ? (Color)Primary : (Color)Tertiary;
        lblIncomesHeader.TextColor = (_vm.Stage == "Budget Extra Income") ? (Color)Primary : (Color)Tertiary;

        if (_vm.Stage == "Budget Settings")
        {
            await MainScrollView.ScrollToAsync(0, 95, true);
        }
        else if (_vm.Stage == "Budget Details")
        {
            await MainScrollView.ScrollToAsync(0, 155, true);
        }
        else if (_vm.Stage == "Budget Outgoings")
        {
            UpdateBillsYesNo("");
            await MainScrollView.ScrollToAsync(0, 215, true);
        }
        else if (_vm.Stage == "Budget Savings")
        {
            UpdateSavingsYesNo("");
            await MainScrollView.ScrollToAsync(0, 275, true);
        }
        else if (_vm.Stage == "Budget Extra Income")
        {
            await MainScrollView.ScrollToAsync(0, 335, true);
        }
    }

    private void GoToStageSettings_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Settings";
        UpdateStageDisplay();
        //await MainScrollView.ScrollToAsync(0, 95, true);
    }

    private void GoToStageBudget_Tapped(object sender, TappedEventArgs e)
    {
        double BankBalance = (double?)_vm.Budget.BankBalance ?? 0;
        entBankBalance.Text = BankBalance.ToString("c", CultureInfo.CurrentCulture);

        double PayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
        entPayAmount.Text = PayAmount.ToString("c", CultureInfo.CurrentCulture);

        _vm.Stage = "Budget Details";
        UpdateStageDisplay();
        //await MainScrollView.ScrollToAsync(0, 155, true);
    }

    private void GoToStageBills_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Outgoings";
        UpdateStageDisplay();
        //await MainScrollView.ScrollToAsync(0, 215, true);
    }

    private void GoToStageSavings_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Savings";
        UpdateStageDisplay();
        //await MainScrollView.ScrollToAsync(0, 275, true);

    }

    private void GoToStageIncomes_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Extra Income";
        UpdateStageDisplay();
        //await MainScrollView.ScrollToAsync(0, 335, true);

    }

    private void ChangeSelectedCurrency_Tapped(object sender, TappedEventArgs e)
    {

        _vm.SearchVisible = true;
        _vm.CurrencySearchResults = _ds.GetCurrencySymbols("").Result;
        CurrencySearch.Text = "";
    }

    private void ContinueSettingsButton_Clicked(object sender, EventArgs e)
    {
        double BankBalance = (double?)_vm.Budget.BankBalance ?? 0;
        entBankBalance.Text = BankBalance.ToString("c", CultureInfo.CurrentCulture);
        
        double PayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
        entPayAmount.Text = PayAmount.ToString("c", CultureInfo.CurrentCulture);

        dtpckPayDay.Date = _vm.Budget.NextIncomePayday ?? default;

        _vm.Stage = "Budget Details";
        UpdateStageDisplay();

        _vm.SaveStage("Budget Settings");

        //await MainScrollView.ScrollToAsync(0, 155, true);
    }

    private void BackBudgetDetailsButton_Clicked(object sender, EventArgs e)
    {
        _vm.Stage = "Budget Settings";
        UpdateStageDisplay();

        //await MainScrollView.ScrollToAsync(0, 95, true);
    }

    private void ContinueBudgetDetailsButton_Clicked(object sender, EventArgs e)
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

            _vm.SaveStage("Budget Details");

            UpdateStageDisplay();

            //await MainScrollView.ScrollToAsync(0, 215, true);
        }

    }

    private bool ValidateBudgetDetails()
    {
        bool IsValid = true;

        if (dtpckPayDay.Date <= DateTime.Now.Date)
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
        _vm.Stage = "Budget Details";
        UpdateStageDisplay();

        //await MainScrollView.ScrollToAsync(0, 155, true);
    }

    private void ContinueBudgetBillsButton_Clicked(object sender, EventArgs e)
    {
        if(ValidateBudgetOutgoings())
        {
            _vm.Stage = "Budget Savings";
            UpdateStageDisplay();

            _vm.SaveStage("Budget Outgoings");

            //await MainScrollView.ScrollToAsync(0, 275, true);
        }

    }

    private void BackBudgetSavingsButton_Clicked(object sender, EventArgs e)
    {
        _vm.Stage = "Budget Outgoings";
        UpdateStageDisplay();

        //await MainScrollView.ScrollToAsync(0, 155, true);
    }

    private void ContinueBudgetSavingsButton_Clicked(object sender, EventArgs e)
    {
        if (ValidateBudgetSavings())
        {
            _vm.Stage = "Budget Extra Income";
            UpdateStageDisplay();

            _vm.SaveStage("Budget Outgoings");

            //await MainScrollView.ScrollToAsync(0, 275, true);
        }

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
        await Shell.Current.GoToAsync($"{nameof(AddBill)}?BudgetID={_vm.BudgetID}&BillID={0}");
    }

    private async void AddSavingsNewBudget_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(AddSaving)}?BudgetID={_vm.BudgetID}&SavingID={0}");
    }

    private async void BankBalanceInfo(object sender, EventArgs e)
    {
        List<string> SubTitle = new List<string>{
            "",
            "",
            ""
        };

        List<string> Info = new List<string>{
            "The amount of money you currently have available is known in the app as your BankBalance. If all your money was in one place, it would be the amount of money you would see when you open your banking app. Fortunately though we don't care where all your money is, you can have it in multiple places in real life we use just one number to make it easier to manage.",
            "When you are creating your budget it is advisable to figure out exaxtly how much money you have to your name and use this figure, however you don't have to .. if you know better use a different figure. Whatever you input will be used to work out how much you have to spend daily until your next pay day.",
            "It is also worth knowing that your BankBalance is not always what you have to spend, you have to take into account savings, bills and any other income!, We will use other terms along with Bank Balance to describe your budgets state - MaB (Money available Balance) & LtSB (Left to Spend Balance)"
        };

        var popup = new PopupInfo("Bank Balance", SubTitle, Info);
        var result = await Application.Current.MainPage.ShowPopupAsync(popup);
    }

    void BankBalance_Changed(object sender, TextChangedEventArgs e)
    {

        double BankBalance = _pt.FormatCurrencyNumber(e.NewTextValue);
        entBankBalance.Text = BankBalance.ToString("c", CultureInfo.CurrentCulture);
        entBankBalance.CursorPosition = _pt.FindCurrencyCursorPosition(entBankBalance.Text);

    }
    void PayAmount_Changed(object sender, TextChangedEventArgs e)
    {
        double PayAmount = _pt.FormatCurrencyNumber(e.NewTextValue);
        entPayAmount.Text = PayAmount.ToString("c", CultureInfo.CurrentCulture);
        entPayAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entPayAmount.Text);
    }
    private async void PayDayInfo(object sender, EventArgs e)
    {
        List<string> SubTitle = new List<string>{
            "",
            "",
            ""
        };

        List<string> Info = new List<string>{
            "",
            "",
            ""
        };

        var popup = new PopupInfo("Budget PayDay", SubTitle, Info);
        var result = await Application.Current.MainPage.ShowPopupAsync(popup);
    }

    private async void PayDetailsInfo(object sender, EventArgs e)
    {
        List<string> SubTitle = new List<string>{
            "",
            "",
            ""
        };

        List<string> Info = new List<string>{
            "",
            "",
            ""
        };

        var popup = new PopupInfo("Budget PayDay", SubTitle, Info);
        var result = await Application.Current.MainPage.ShowPopupAsync(popup);
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
            vslOption2Select.BackgroundColor = (Color)Light;
            vslOption3Select.BackgroundColor = (Color)Light;
            vslOption4Select.BackgroundColor = (Color)Light;

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
            vslOption1Select.BackgroundColor = (Color)Light;
            vslOption2Select.BackgroundColor = (Color)Success;
            vslOption3Select.BackgroundColor = (Color)Light;
            vslOption4Select.BackgroundColor = (Color)Light;

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
            vslOption1Select.BackgroundColor = (Color)Light;
            vslOption2Select.BackgroundColor = (Color)Light;
            vslOption3Select.BackgroundColor = (Color)Success;
            vslOption4Select.BackgroundColor = (Color)Light;

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

            vslOption1Select.BackgroundColor = (Color)Light;
            vslOption2Select.BackgroundColor = (Color)Light;
            vslOption3Select.BackgroundColor = (Color)Light;
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
            vslOption1Select.BackgroundColor = (Color)Light;
            vslOption2Select.BackgroundColor = (Color)Light;
            vslOption3Select.BackgroundColor = (Color)Light;
            vslOption4Select.BackgroundColor = (Color)Light;

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
        UpdateSelectedOption("Everynth");
    }

    private void Option2Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("WorkingDays");
    }

    private void Option3Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("OfEveryMonth");
    }

    private void Option4Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("LastOfTheMonth");
    }

    void EveryNthValue_Changed(object sender, TextChangedEventArgs e)
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

    void WorkingDaysValue_Changed(object sender, TextChangedEventArgs e)
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
    void OfEveryMonthValue_Changed(object sender, TextChangedEventArgs e)
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

    private void BillsYesSelect_Tapped(object sender, TappedEventArgs e)
    {
        UpdateBillsYesNo("Yes");
    }

    private void BillsNoSelect_Tapped(object sender, TappedEventArgs e)
    {
        UpdateBillsYesNo("No");
    }

    private void SavingsYesSelect_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSavingsYesNo("Yes");
    }

    private void SavingsNoSelect_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSavingsYesNo("No");
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
            vslBillsNoSelect.BackgroundColor = (Color)Light;

            lblBillsYes.FontAttributes = FontAttributes.Bold;
            lblBillsNo.FontAttributes = FontAttributes.None;

            lblBillsYes.TextColor = (Color)White;
            lblBillsNo.TextColor = (Color)Gray900;

            btnAddBillsNewBudget.IsVisible = true;

            _vm.BillsYesNoSelect = "Yes";

        }
        else if (option == "No")
        {
            vslBillsYesSelect.BackgroundColor = (Color)Light;
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
            vslBillsYesSelect.BackgroundColor = (Color)Light;
            vslBillsNoSelect.BackgroundColor = (Color)Light;

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
            vslSavingsNoSelect.BackgroundColor = (Color)Light;

            lblSavingsYes.FontAttributes = FontAttributes.Bold;
            lblSavingsNo.FontAttributes = FontAttributes.None;

            lblSavingsYes.TextColor = (Color)White;
            lblSavingsNo.TextColor = (Color)Gray900;

            btnAddSavingsNewBudget.IsVisible = true;

            _vm.SavingsYesNoSelect = "Yes";

        }
        else if (option == "No")
        {
            vslSavingsYesSelect.BackgroundColor = (Color)Light;
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
            vslSavingsYesSelect.BackgroundColor = (Color)Light;
            vslSavingsNoSelect.BackgroundColor = (Color)Light;

            lblSavingsYes.FontAttributes = FontAttributes.None;
            lblSavingsNo.FontAttributes = FontAttributes.None;

            lblSavingsYes.TextColor = (Color)Gray900;
            lblSavingsNo.TextColor = (Color)Gray900;

            btnAddSavingsNewBudget.IsVisible = false;

            _vm.SavingsYesNoSelect = "";
        }
    }

    private async void DeleteBudgetOutgoings_Tapped(object sender, TappedEventArgs e)
    {

        var Bill = (Bills)e.Parameter;

        bool result = await DisplayAlert("Bill", "Are you sure you want to delete your Outgoing " + Bill.BillName.ToString(), "Yes, continue", "Cancel");
        if (result)
        {
            string Result = _ds.DeleteBill(Bill.BillID).Result;
            if(Result == "OK")
            {
                _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
            }            
        }
    }

    private async void EditBudgetOutgoings_Tapped(object sender, TappedEventArgs e)
    {

        var Bill = (Bills)e.Parameter;

        await Shell.Current.GoToAsync($"{nameof(AddBill)}?BudgetID={_vm.BudgetID}&BillID={Bill.BillID}");
    }

    private void OutgoingViewCell_Appearing(object sender, EventArgs e)
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

    private async void EditBudgetSavings_Tapped(object sender, TappedEventArgs e)
    {
        var Saving = (Savings)e.Parameter;

        await Shell.Current.GoToAsync($"{nameof(AddSaving)}?BudgetID={_vm.BudgetID}&SavingID={Saving.SavingID}");
    }

    private async void DeleteBudgetSavings_Tapped(object sender, TappedEventArgs e)
    {
        var Saving = (Savings)e.Parameter;

        bool result = await DisplayAlert("Savings", "Are you sure you want to delete your Saving " + Saving.SavingsName.ToString(), "Yes, continue", "Cancel");
        if (result)
        {
            string Result = _ds.DeleteSaving(Saving.SavingID).Result;
            if (Result == "OK")
            {
                _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
            }
        }
    }

    private void SavingsViewCell_Appearing(object sender, EventArgs e)
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

}
