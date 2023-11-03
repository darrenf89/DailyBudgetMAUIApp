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
using Microsoft.Maui.Primitives;

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

        dtpckBillDueDate.MinimumDate = DateTime.UtcNow.AddDays(1);
    }

    async protected override void OnAppearing()
    {
        if (_vm.BudgetID == 0)
        {
            _vm.BudgetID = App.DefaultBudgetID;
        }

        if (_vm.BillID == 0)
        {
            _vm.Bill = new Bills();
            _vm.Title = "Add a New Outgoing";
            btnAddBill.IsVisible = true;

        }
        else
        {
            _vm.Bill = _ds.GetBillFromID(_vm.BillID).Result;
            _vm.Title = $"Update Outgoing {_vm.Bill.BillName}";
            btnUpdateBill.IsVisible = true;

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

        double AmountDue = (double?)_vm.Bill.BillAmount ?? 0;
        entAmountDue.Text = AmountDue.ToString("c", CultureInfo.CurrentCulture);

        double CurrentSaved = (double?)_vm.Bill.BillCurrentBalance ?? 0;
        entCurrentSaved.Text = CurrentSaved.ToString("c", CultureInfo.CurrentCulture);

        double RegularValue = (double?)_vm.Bill.RegularBillValue ?? 0;
        lblRegularBillValue.Text = RegularValue.ToString("c", CultureInfo.CurrentCulture);

        base.OnAppearing();

    }

    private void btnRecurringBill_Clicked(object sender, EventArgs e)
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
    private void btnOneoffBill_Clicked(object sender, EventArgs e)
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

    void AmountDue_Changed(object sender, TextChangedEventArgs e)
    {
        ClearAllValidators();

        decimal AmountDue = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entAmountDue.Text = AmountDue.ToString("c", CultureInfo.CurrentCulture);
        entAmountDue.CursorPosition = _pt.FindCurrencyCursorPosition(entAmountDue.Text);
        _vm.Bill.BillAmount = AmountDue;

        lblRegularBillValue.Text = _vm.CalculateRegularBillValue();
    }

    void CurrentSaved_Changed(object sender, TextChangedEventArgs e)
    {
        ClearAllValidators();

        decimal CurrentSaved = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entCurrentSaved.Text = CurrentSaved.ToString("c", CultureInfo.CurrentCulture);
        entCurrentSaved.CursorPosition = _pt.FindCurrencyCursorPosition(entCurrentSaved.Text);
        _vm.Bill.BillCurrentBalance = CurrentSaved;

        lblRegularBillValue.Text = _vm.CalculateRegularBillValue();
    }

    private void dtpckBillDueDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        ClearAllValidators();

        lblRegularBillValue.Text = _vm.CalculateRegularBillValue();
    }

    private void Option1Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("Everynth");
    }

    private void Option2Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("OfEveryMonth");
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
            vslOption2Select.BackgroundColor = (Color)Light;

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
            vslOption1Select.BackgroundColor = (Color)Light;
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
            vslOption1Select.BackgroundColor = (Color)Light;
            vslOption2Select.BackgroundColor = (Color)Light;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = false;
            
            _vm.Bill.BillType = null;
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
    private async void AddBill_Clicked(object sender, EventArgs e)
    {
        if (_vm.Bill.BillName == "" || _vm.Bill.BillName == null)
        {
            _vm.ChangeBillName();
        }

        if(ValidateBillDetails())
        {
            SaveBillTypeOptions();

            _vm.AddBill();
        }
    }
    private async void UpdateBill_Clicked(object sender, EventArgs e)
    {
        if (_vm.Bill.BillName == "" || _vm.Bill.BillName == null)
        {
            _vm.ChangeBillName();
        }

        if(ValidateBillDetails())
        {
            SaveBillTypeOptions();

            _vm.UpdateBill();
        }
    }

    private async void SaveBill_Clicked(object sender, EventArgs e)
    {

        if (_vm.Bill.BillName == "" || _vm.Bill.BillName == null)
        {
            _vm.ChangeBillName();
        }

        if(ValidateBillDetails())
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

    private async void ResetBill_Clicked(object sender, EventArgs e)
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
            SaveBillTypeOptions();

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

        if (dtpckBillDueDate.Date <= DateTime.Now.Date)
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
}