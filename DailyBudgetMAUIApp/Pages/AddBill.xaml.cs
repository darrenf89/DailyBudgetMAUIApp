using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Popups;
using DailyBudgetMAUIApp.ViewModels;
using System.Globalization;
using System.Text.RegularExpressions;


namespace DailyBudgetMAUIApp.Pages;

public partial class AddBill : BasePage
{
    private readonly AddBillViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    private readonly IKeyboardService _ks;
    private readonly IModalPopupService _ps;
    public AddBill(AddBillViewModel viewModel, IProductTools pt, IRestDataService ds, IKeyboardService ks, IModalPopupService ps)
	{
		InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;
        _ks = ks;
        _ps = ps;

        dtpckBillDueDate.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).AddDays(1);
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {

        base.OnNavigatedFrom(args);
        _vm.NavigatedFrom = "";
        _vm.IsPageBusy = false;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (Navigation.NavigationStack.Count > 1)
        {
            Shell.SetTabBarIsVisible(this, false);
        }
    }

    async protected override void OnAppearing()
    {
        try
        {
            await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());
            base.OnAppearing();

            if (string.IsNullOrEmpty(_vm.NavigatedFrom))
            {
                _vm.Bill = null;
                _vm.BillID = 0;
                _vm.BillPayee = "";
                _vm.BillCategory = "";
                _vm.RedirectTo = "";
                _vm.BillName = "";

                SelectBillType.IsVisible = true;
                BillTypeSelected.IsVisible = false;
                lblSelectedBillTitle.Text = "";
                lblSelectedBillParaOne.Text = "";
                lblSelectedBillParaTwo.Text = "";

                brdBillDetails.IsVisible = false;
                vslBillDetails.IsVisible = false;
                brdBillTypes.IsVisible = false;

                UpdateSelectedOption("");

                _vm.BillRecurringText = "";
            }
            else if(string.Equals(_vm.NavigatedFrom, "ViewBillsNew", StringComparison.OrdinalIgnoreCase))
            {
                _vm.Bill = null;
                _vm.BillID = 0;
                _vm.RedirectTo = "ViewBills";
                _vm.BillName = "";
                _vm.BillPayee = "";
                _vm.BillCategory = "";
            }
            else if (string.Equals(_vm.NavigatedFrom, "CreateNewBudget", StringComparison.OrdinalIgnoreCase) || string.Equals(_vm.NavigatedFrom, "ViewBills", StringComparison.OrdinalIgnoreCase) || string.Equals(_vm.NavigatedFrom, "CreateNewFamilyAccount", StringComparison.OrdinalIgnoreCase))
            {
                _vm.RedirectTo = _vm.NavigatedFrom;
            }

            if (_vm.BudgetID == 0)
            {
                _vm.BudgetID = App.DefaultBudgetID;
            }

            if (_vm.BillID == 0)
            {
                if(_vm.Bill == null)
                {
                    _vm.Bill = new Bills();
                    _vm.Bill.BillPayee = "";
                    _vm.Bill.BillName = "";
                    _vm.Title = "Add a New Outgoing";
                    btnAddBill.IsVisible = true;
                }   
                else
                {
                    _vm.Title = "Add a New Outgoing";
                    btnAddBill.IsVisible = true;

                    LoadExistingBill();
                }
            }
            else
            {
                if(_vm.Bill is null)
                {
                    _vm.Bill = await _ds.GetBillFromID(_vm.BillID);
                }
                _vm.Title = $"Update Outgoing {_vm.Bill.BillName}";
                btnUpdateBill.IsVisible = true;

                LoadExistingBill();
            }

            double AmountDue = (double?)_vm.Bill.BillAmount ?? 0;
            entAmountDue.Text = AmountDue.ToString("c", CultureInfo.CurrentCulture);

            double CurrentSaved = (double?)_vm.Bill.BillCurrentBalance ?? 0;
            entCurrentSaved.Text = CurrentSaved.ToString("c", CultureInfo.CurrentCulture);

            double RegularValue = (double?)_vm.Bill.RegularBillValue ?? 0;
            lblRegularBillValue.Text = RegularValue.ToString("c", CultureInfo.CurrentCulture);  
        
            if(!string.IsNullOrEmpty(_vm.Bill.BillName))
            {
                _vm.BillName = _vm.Bill.BillName;
            }

            if (!string.IsNullOrEmpty(_vm.Bill.BillPayee))
            {
                _vm.BillPayee = _vm.Bill.BillPayee;
            }

            if (!string.IsNullOrEmpty(_vm.Bill.Category))
            {
                _vm.BillCategory = _vm.Bill.Category;
            }

            if(_vm.Bill != null)
            { 
                _vm.BillOldBalance = _vm.Bill.BillCurrentBalance;
            }

            _vm.IsMultipleAccounts = App.DefaultBudget.IsMultipleAccounts && _vm.IsPremiumAccount;
            if (_vm.IsMultipleAccounts)
            {
                _vm.BankAccounts = await _ds.GetBankAccounts(_vm.BudgetID);

                if (_vm.Bill.BillID == 0 || _vm.Bill.AccountID == 0)
                {
                    BankAccounts? B = _vm.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                    _vm.SelectedBankAccount = B;
                    _vm.Bill.AccountID = B.ID;
                }
                else
                {
                    BankAccounts? B = _vm.BankAccounts.Where(b => b.ID == _vm.Bill.AccountID.GetValueOrDefault()).FirstOrDefault();
                    if (B != null)
                    {
                        _vm.SelectedBankAccount = B;
                        _vm.Bill.AccountID = B.ID;
                    }
                    else
                    {
                        B = _vm.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                        _vm.SelectedBankAccount = B;
                        _vm.Bill.AccountID = B.ID;
                    }
                }
            }
            else
            {
                _vm.Bill.AccountID = null;
            }

            await _ps.CloseAsync<PopUpPage>();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddBill", "OnAppearing");
        }
    }

    private void LoadExistingBill()
    {
        SelectBillType.IsVisible = false;
        if (_vm.Bill.IsRecuring.GetValueOrDefault())
        {
            BillTypeSelected.IsVisible = true;
            lblSelectedBillTitle.Text = "You are adding a recurring outgoing";
            lblSelectedBillParaOne.Text = "For most of your bills! Phone, car, Netflix, the list goes on ...";
            lblSelectedBillParaTwo.Text = "Tell us how much, when the next bill is due and how often it occurs";

            brdBillDetails.IsVisible = true;
            brdBillTypes.IsVisible = true;

            _vm.BillRecurringText = "Recurring";

            UpdateSelectedOption(_vm.Bill.BillType);
            _vm.BillTypeText = _vm.Bill.BillType;
        }
        else
        {
            BillTypeSelected.IsVisible = true;
            lblSelectedBillTitle.Text = "You are adding a one off outgoing";
            lblSelectedBillParaOne.Text = "For those one off bills, owe someone money?";
            lblSelectedBillParaTwo.Text = "Tell us how much and when the bill is due";

            brdBillDetails.IsVisible = true;
            brdBillTypes.IsVisible = false;

            _vm.BillRecurringText = "OneOff";
        }
    }

    private void btnRecurringBill_Clicked(object sender, EventArgs e)
    {
        try
        {
            ClearAllValidators();

            _vm.Bill.IsRecuring = true;

            SelectBillType.IsVisible = false;
            BillTypeSelected.IsVisible = true;
            lblSelectedBillTitle.Text = "You are adding a recurring outgoing";
            lblSelectedBillParaOne.Text = "For most of your bills! Phone, car, Netflix, the list goes on ...";
            lblSelectedBillParaTwo.Text = "Tell us how much, when the next bill is due and how often it occurs";

            brdBillDetails.IsVisible = true;
            vslBillDetails.IsVisible = true;
            brdBillTypes.IsVisible = true;

            _vm.BillRecurringText = "Recurring";
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddBill", "btnRecurringBill_Clicked");
        }
    }
    private void btnOneoffBill_Clicked(object sender, EventArgs e)
    {
        try
        {
            ClearAllValidators();

            _vm.Bill.IsRecuring = false;

            SelectBillType.IsVisible = false;
            BillTypeSelected.IsVisible = true;
            lblSelectedBillTitle.Text = "You are adding a one off outgoing";
            lblSelectedBillParaOne.Text = "For those one off bills, owe someone money?";
            lblSelectedBillParaTwo.Text = "Tell us how much and when the bill is due";

            brdBillDetails.IsVisible = true;
            vslBillDetails.IsVisible = true;
            brdBillTypes.IsVisible = false;

            _vm.BillRecurringText = "OneOff";
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddBill", "btnOneoffBill_Clicked");
        }
    }

    void AmountDue_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            ClearAllValidators();

            decimal AmountDue = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entAmountDue);

            _vm.Bill.BillAmount = AmountDue;

            lblRegularBillValue.Text = _vm.CalculateRegularBillValue();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddBill", "AmountDue_Changed");
        }
    }

    void CurrentSaved_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            ClearAllValidators();

            decimal CurrentSaved = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entCurrentSaved);

            _vm.Bill.BillCurrentBalance = CurrentSaved;

            lblRegularBillValue.Text = _vm.CalculateRegularBillValue();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddBill", "OnAppearing");
        }
    }

    private void dtpckBillDueDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        try
        {
            ClearAllValidators();

            lblRegularBillValue.Text = _vm.CalculateRegularBillValue();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddBill", "dtpckBillDueDate_DateSelected");
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
    private void UpdateSelectedOption(string option)
    {

        ClearAllValidators();

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

            pckrEverynthDuration.SelectedItem = _vm.Bill.BillDuration ?? "days";
            entEverynthValue.Text = _vm.Bill.BillValue.ToString() ?? "1";

            _vm.Bill.BillType = "Everynth";
            _vm.BillTypeText = "Everynth";

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

            entWorkingDaysValue.Text = _vm.Bill.BillValue.ToString() ?? "1";

            _vm.Bill.BillType = "WorkingDays";
            _vm.BillTypeText = "WorkingDays";

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

            entOfEveryMonthValue.Text = _vm.Bill.BillValue.ToString() ?? "1";

            _vm.Bill.BillType = "OfEveryMonth";
            _vm.BillTypeText = "OfEveryMonth";
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

            pckrLastOfTheMonthDuration.SelectedItem = _vm.Bill.BillDuration ?? "Monday";

            _vm.Bill.BillType = "LastOfTheMonth";
            _vm.BillTypeText = "LastOfTheMonth";
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

            if (_vm.Bill != null)
            {
                _vm.Bill.BillType = null;
            }
            _vm.BillTypeText = "";
            entOfEveryMonthValue.Text = "";
            entEverynthValue.Text = "";
        }
    }

    private void SaveBillTypeOptions()
    {
        if (_vm.Bill.BillType == "Everynth")
        {
            _vm.Bill.BillDuration =  pckrEverynthDuration.SelectedItem.ToString();
            _vm.Bill.BillValue =  Convert.ToInt32(entEverynthValue.Text);
        }
        else if (_vm.Bill.BillType == "WorkingDays")
        {
            _vm.Bill.BillDuration = null;
            _vm.Bill.BillValue = Convert.ToInt32(entWorkingDaysValue.Text);
        }
        else if (_vm.Bill.BillType == "OfEveryMonth")
        {
            _vm.Bill.BillDuration = null;
            _vm.Bill.BillValue = Convert.ToInt32(entOfEveryMonthValue.Text);
        }
        else if (_vm.Bill.BillType == "LastOfTheMonth")
        {
            _vm.Bill.BillDuration = pckrLastOfTheMonthDuration.SelectedItem.ToString();
            _vm.Bill.BillValue = 0;
        }
        else
        {
            _vm.Bill.BillDuration = null;
            _vm.Bill.BillValue = null;
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

    private async Task<string> ChangeBillName()
    {
        var queryAttributes = new Dictionary<string, object>
        {
            [nameof(PopUpPageSingleInputViewModel.Description)] = "Every outgoing needs a name, we will refer to it by the name you give it and will make it easier to identify!",
            [nameof(PopUpPageSingleInputViewModel.DescriptionSub)] = "Call it something useful or call it something silly up to you really!",
            [nameof(PopUpPageSingleInputViewModel.InputTitle)] = "Outgoing Name",
            [nameof(PopUpPageSingleInputViewModel.Placeholder)] = "Enter an outgoing name!",
            [nameof(PopUpPageSingleInputViewModel.Input)] = _vm.Bill.BillName
        };

        var popupOptions = new PopupOptions
        {
            CanBeDismissedByTappingOutsideOfPopup = false,
            PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
        };

        IPopupResult<object> popupResult = await _ps.PopupService.ShowPopupAsync<PopUpPageSingleInput, object>(
            Shell.Current,
            options: popupOptions,
            shellParameters: queryAttributes,
            cancellationToken: CancellationToken.None
        );

        if (popupResult.Result != null || (string)popupResult.Result != "")
        {
            _vm.Bill.BillName = (string)popupResult.Result;
        }

        return (string)popupResult.Result;
    }
    private async void AddBill_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.Bill.BillName == "" || _vm.Bill.BillName == null)
            {
                string status = await ChangeBillName();
            }

            if (ValidateBillDetails())
            {
                SaveBillTypeOptions();

                _vm.AddBill();
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddBill", "AddBill_Clicked");
        }
    }
    private async void UpdateBill_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.Bill.BillName == "" || _vm.Bill.BillName == null)
            {
                string status = await ChangeBillName();
            }

            if (ValidateBillDetails())
            {
                SaveBillTypeOptions();

                _vm.UpdateBill();
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddBill", "UpdateBill_Clicked");
        }

    }

    private async void SaveBill_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.Bill.BillName == "" || _vm.Bill.BillName == null)
            {
                string status = await ChangeBillName();
            }

            if (ValidateBillDetails())
            {
                SaveBillTypeOptions();

                if(_vm.BillID == 0)
                {
                    _vm.AddBill();
                }
                else
                {
                    _vm.UpdateBill();
                }
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddBill", "SaveBill_Clicked");
        }
    }

    private async void ResetBill_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool result = await DisplayAlert("Outgoing Reset", "Are you sure you want to Reset " + _vm.Bill.BillName , "Yes, continue", "Cancel");
            if (result)
            {
                SelectBillType.IsVisible = true;
                BillTypeSelected.IsVisible = false;
                lblSelectedBillTitle.Text = "";
                lblSelectedBillParaOne.Text = "";
                lblSelectedBillParaTwo.Text = "";

                brdBillDetails.IsVisible = false;
                vslBillDetails.IsVisible = false;
                brdBillTypes.IsVisible = false;

                UpdateSelectedOption("");

                double AmountDue = (double) 0;
                entAmountDue.Text = AmountDue.ToString("c", CultureInfo.CurrentCulture);

                double CurrentSaved = (double) 0;
                entCurrentSaved.Text = CurrentSaved.ToString("c", CultureInfo.CurrentCulture);

                _vm.Bill.RegularBillValue = 0;
                _vm.Bill.BillDueDate = _vm.MinimumDate;
                _vm.BillRecurringText = "";

                _vm.Bill.BillType = "";
                _vm.BillPayee = "";
                _vm.BillCategory = "";

                SaveBillTypeOptions();

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddBill", "ResetBill_Clicked");
        }
    }
    private bool ValidateBillDetails()
    {
        bool IsValid = true;

        if (_vm.Bill.BillName == "")
        {
            IsValid = false;
            validatorBillName.IsVisible = true;
        }
        else
        {
            validatorBillName.IsVisible = false;
        }

        if (dtpckBillDueDate.Date <= _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
        {
            IsValid = false;
            validatorBillDue.IsVisible = true;
        }
        else
        {
            validatorBillDue.IsVisible = false;
        }


        if (_vm.Bill.BillAmount == 0)
        {
            IsValid = false;
            validatorBillAmount.IsVisible = true;
        }
        else
        {
            validatorBillAmount.IsVisible = false;
        }

        if(_vm.Bill.BillCurrentBalance >= _vm.Bill.BillAmount)
        {
            IsValid = false;
            validatorBillBalance.IsVisible = true;
        }
        else
        {
            validatorBillBalance.IsVisible = false;
        }



        if(_vm.BillRecurringText == "" || _vm.BillRecurringText == null)
        {
            IsValid = false;
            validatorBillRecurring.IsVisible = true;
        }
        else
        {
            validatorBillRecurring.IsVisible = false;
        }

        if(_vm.BillRecurringText == "Recurring")
        {
            if (_vm.BillTypeText == "" || _vm.BillTypeText == null)
            {
                IsValid = false;
                validatorBillType.IsVisible = true;
            }
            else
            {
                validatorBillType.IsVisible = false;
            }

            if (_vm.BillTypeText == "Everynth" && entEverynthValue.Text == "")
            {
                IsValid = false;
                validatorEveryNthDuration.IsVisible = true;
            }
            else
            {
                validatorEveryNthDuration.IsVisible = false;
            }

            if (_vm.BillTypeText == "WorkingDays" && entWorkingDaysValue.Text == "")
            {
                IsValid = false;
                validatorWorkingDayDuration.IsVisible = true;
            }
            else
            {
                validatorWorkingDayDuration.IsVisible = false;
            }

            if (_vm.BillTypeText == "OfEveryMonth" && entOfEveryMonthValue.Text == "")
            {
                IsValid = false;
                validatorOfEveryMonthDuration.IsVisible = true;
            }
            else
            {
                validatorOfEveryMonthDuration.IsVisible = false;
            }
        }

        _vm.IsPageValid = IsValid;
        return IsValid;

        
    }
    private void ClearAllValidators()
    {
        validatorOfEveryMonthDuration.IsVisible = false;
        validatorWorkingDayDuration.IsVisible = false;
        validatorEveryNthDuration.IsVisible = false;
        validatorBillType.IsVisible = false;
        validatorBillRecurring.IsVisible = false;
        validatorBillBalance.IsVisible = false;
        validatorBillAmount.IsVisible = false;
        validatorBillDue.IsVisible = false;
        validatorBillName.IsVisible = false;

        _vm.IsPageValid = true;
    }

    private async void SelectPayee_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            HideKeyBoard();

            SaveBillTypeOptions();

            if (_vm.Bill.BillPayee is null)
            {
                _vm.Bill.BillPayee = "";
            }
            await Shell.Current.GoToAsync($"/{nameof(SelectPayeePage)}?BudgetID={_vm.BudgetID}&PageType=Bill{(_vm.FamilyAccountID > 0 ? $"&FamilyAccountID={_vm.FamilyAccountID}" : "")}",
                new Dictionary<string, object>
                {
                    ["Bill"] = _vm.Bill
                });
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddBill", "SelectPayee_Tapped");
        }
    }

    private void HideKeyBoard()
    {
        _ks.HideKeyboard();
    }

    private async void SelectCategory_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

            HideKeyBoard();

            SaveBillTypeOptions();

            if (_vm.Bill.Category is null)
            {
                _vm.Bill.Category = "";
            }

            await Shell.Current.GoToAsync($"/{nameof(SelectCategoryPage)}?BudgetID={_vm.BudgetID}&PageType=Bill{(_vm.FamilyAccountID > 0 ? $"&FamilyAccountID={_vm.FamilyAccountID}" : "")}",
                new Dictionary<string, object>
                {
                    ["Bill"] = _vm.Bill
                });
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddBill", "SelectCategory_Tapped");
        }
    }

    private void entIsAccount_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            _vm.Bill.AccountID = _vm.SelectedBankAccount.ID;
        }
        catch (Exception ex)
        {

            _pt.HandleException(ex, "AddBill", "entIsAccount_SelectedIndexChanged");
        }
    }
}