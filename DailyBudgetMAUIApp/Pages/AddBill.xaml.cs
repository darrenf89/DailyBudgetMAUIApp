using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Popups;
using CommunityToolkit.Maui.Views;
using System.Globalization;
using System.Text.RegularExpressions;


namespace DailyBudgetMAUIApp.Pages;

public partial class AddBill : ContentPage
{
    private readonly AddBillViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    public AddBill(AddBillViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
		InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        dtpckBillDueDate.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).AddDays(1);
    }

    async protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {

        base.OnNavigatedFrom(args);
        _vm.NavigatedFrom = "";
    }

    async protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {

        base.OnNavigatedTo(args);
    }

    async protected override void OnAppearing()
    {
        try
        {

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
            else if (string.Equals(_vm.NavigatedFrom, "CreateNewBudget", StringComparison.OrdinalIgnoreCase) || string.Equals(_vm.NavigatedFrom, "ViewBills", StringComparison.OrdinalIgnoreCase))
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
                    _vm.Bill = _ds.GetBillFromID(_vm.BillID).Result;
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

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddBill", "OnAppearing");
        }
    }

    private void LoadExistingBill()
    {
        SelectBillType.IsVisible = false;
        if (_vm.Bill.IsRecuring)
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

            decimal AmountDue = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
            entAmountDue.Text = AmountDue.ToString("c", CultureInfo.CurrentCulture);
            int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
            if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entAmountDue.CursorPosition > position)
            {
                entAmountDue.CursorPosition = entAmountDue.Text.Length;
            }
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

            decimal CurrentSaved = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
            entCurrentSaved.Text = CurrentSaved.ToString("c", CultureInfo.CurrentCulture);
            int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
            if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entCurrentSaved.CursorPosition > position)
            {
                entCurrentSaved.CursorPosition = entCurrentSaved.Text.Length;
            }
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
            _pt.HandleException(ex, "AddBill", "Option1Select_Tapped");
        }
    }

    private void Option2Select_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            UpdateSelectedOption("OfEveryMonth");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddBill", "Option2Select_Tapped");
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

            lblOption1.FontAttributes = FontAttributes.Bold;
            lblOption2.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)White;
            lblOption2.TextColor = (Color)Gray900;;

            vslOption1.IsVisible = true;
            vslOption2.IsVisible = false;

            pckrEverynthDuration.SelectedItem = _vm.Bill.BillDuration ?? "days";
            entEverynthValue.Text = _vm.Bill.BillValue.ToString() ?? "1";
            
            _vm.Bill.BillType = "Everynth";
            _vm.BillTypeText = "Everynth";

        }
        else if (option == "OfEveryMonth")
        {
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = (Color)Success;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.Bold;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)White;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = true;

            entOfEveryMonthValue.Text = _vm.Bill.BillValue.ToString() ?? "1";
            
            _vm.Bill.BillType = "OfEveryMonth";
            _vm.BillTypeText = "OfEveryMonth";
        }
        else
        {
            vslOption1Select.BackgroundColor = Color.FromArgb("#00FFFFFF");
            vslOption2Select.BackgroundColor = Color.FromArgb("#00FFFFFF");

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = false;
            
            if(_vm.Bill != null)
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
        else if (_vm.Bill.BillType == "OfEveryMonth")
        {
            _vm.Bill.BillDuration = null;
            _vm.Bill.BillValue = Convert.ToInt32(entOfEveryMonthValue.Text);
        }
        else
        {
            _vm.Bill.BillDuration = null;
            _vm.Bill.BillValue = null;
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
            _pt.HandleException(ex, "AddBill", "OfEveryMonthValue_Changed");
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
            _pt.HandleException(ex, "AddBill", "EveryNthValue_Changed");
        }
    }
    private async Task<string> ChangeBillName()
    {

        string Description = "Every outgoing needs a name, we will refer to it by the name you give it and will make it easier to identify!";
        string DescriptionSub = "Call it something useful or call it something silly up to you really!";
        var popup = new PopUpPageSingleInput("Outgoing Name", Description, DescriptionSub, "Enter an outgoing name!", _vm.Bill.BillName, new PopUpPageSingleInputViewModel());
        var result = await Application.Current.MainPage.ShowPopupAsync(popup);

        if (result != null || (string)result != "")
        {
            _vm.Bill.BillName = (string)result;
        }

        return (string)result;
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
            await Shell.Current.GoToAsync($"/{nameof(SelectPayeePage)}?BudgetID={_vm.BudgetID}&PageType=Bill",
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
        entAmountDue.IsEnabled = false;
        entAmountDue.IsEnabled = true;
        entCurrentSaved.IsEnabled = false;
        entCurrentSaved.IsEnabled = true;
        entEverynthValue.IsEnabled = false;
        entEverynthValue.IsEnabled = true;
        entOfEveryMonthValue.IsEnabled = false;
        entOfEveryMonthValue.IsEnabled = true;
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

            await Shell.Current.GoToAsync($"/{nameof(SelectCategoryPage)}?BudgetID={_vm.BudgetID}&PageType=Bill",
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
}