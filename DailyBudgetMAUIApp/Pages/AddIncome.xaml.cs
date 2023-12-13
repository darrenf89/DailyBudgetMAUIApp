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

public partial class AddIncome : ContentPage
{
    private readonly AddIncomeViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    public AddIncome(AddIncomeViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

    }

    async protected override void OnAppearing()
    {

        if (_vm.BudgetID == 0)
        {
            _vm.BudgetID = App.DefaultBudgetID;
        }

        if (_vm.IncomeID == 0)
        {

            _vm.Title = "Add New Income";
            _vm.Income = new IncomeEvents();
            btnAddIncome.IsVisible = true;

        }
        else
        {
            _vm.Income = _ds.GetIncomeFromID(_vm.IncomeID).Result;
            btnUpdateIncome.IsVisible = true;
            _vm.Title = $"Update Income {_vm.Income.IncomeName}";
            SelectIncomeType.IsVisible = false;
            IncomeTypeSelected.IsVisible = true;

            if (_vm.Income.IsRecurringIncome)
            {
                btnRecurringIncome_Clicked(new object(), new EventArgs());
                if (_vm.Income.IsInstantActive ?? false)
                {
                    UpdateIncomeActiveYesNo("Yes");
                }
                else
                {
                    UpdateIncomeActiveYesNo("No");
                }
                UpdateSelectedOption(_vm.Income.RecurringIncomeType);

            }
            else
            {
                btnOneOffIncome_Clicked(new object(), new EventArgs());
                
                if (_vm.Income.IsInstantActive ?? false)
                {
                    UpdateIncomeActiveYesNo("Yes");
                }
                else
                {
                    UpdateIncomeActiveYesNo("No");
                }
                
            }

        }

        double IncomeAmount = (double?)_vm.Income.IncomeAmount ?? 0;
        entIncomeAmount.Text = IncomeAmount.ToString("c", CultureInfo.CurrentCulture);

        base.OnAppearing();
    }

    private async void SaveIncome_Clicked(object sender, EventArgs e)
    {
        
        if (_vm.Income.IncomeName == "" || _vm.Income.IncomeName == null)
        {
            string status = await ChangeIncomeName();
        }

        if (ValidateIncomeDetails())
        {
            SaveIncomeTypeOptions();

            if(_vm.IncomeID == 0)
            {
                _vm.AddIncome();
            }
            else
            {
                _vm.UpdateIncome();
            }
        }
    }

    async private void ResetIncome_Clicked(object sender, EventArgs e)
    {
        ClearAllValidators();

        bool result = await DisplayAlert("Income Reset", "Are you sure you want to Income " + _vm.Income.IncomeName, "Yes, continue", "Cancel");
        if (result)
        {
            SelectIncomeType.IsVisible = true;
            IncomeTypeSelected.IsVisible = false;

            lblSelectedIncomeTitle.Text = "";
            lblSelectedIncomeParaOne.Text = "";
            lblSelectedIncomeParaTwo.Text = "";

            brdSavingRecurringTypeSelected.IsVisible = false;
            vslIncomeInstantActive.IsVisible = false;
            vslIncomeDetails.IsVisible = false;
            vslIncomeType.IsVisible = false;

            _vm.IncomeTypeText = "";
            _vm.Income.IsRecurringIncome = false;

            UpdateIncomeActiveYesNo("");
            UpdateSelectedOption("");
        }
    }

    private void btnOneOffIncome_Clicked(object sender, EventArgs e)
    {
        ClearAllValidators();

        SelectIncomeType.IsVisible = false;
        IncomeTypeSelected.IsVisible = true;

        lblSelectedIncomeTitle.Text = "You are adding a one off Income";
        lblSelectedIncomeParaOne.Text = "Any income you know you are going to get but will only receive it once";
        lblSelectedIncomeParaTwo.Text = "Maybe a gift from family, sold something big ... or small, any extra money you get put it in here.";

        brdSavingRecurringTypeSelected.IsVisible = true;
        vslIncomeInstantActive.IsVisible = true;
        vslIncomeDetails.IsVisible = true;
        vslIncomeType.IsVisible = false;

        _vm.IncomeTypeText = "OneOff";
        _vm.Income.IsRecurringIncome = false;

    }

    private void btnRecurringIncome_Clicked(object sender, EventArgs e)
    {
        ClearAllValidators();

        SelectIncomeType.IsVisible = false;
        IncomeTypeSelected.IsVisible = true;

        lblSelectedIncomeTitle.Text = "You are adding a recurring Income";
        lblSelectedIncomeParaOne.Text = "Any income outside of your main pay you get on a regular and consistent way!";
        lblSelectedIncomeParaTwo.Text = "Maybe a second part time job, some cash from investments or maybe you have really generous friends?!";

        brdSavingRecurringTypeSelected.IsVisible = true;
        vslIncomeInstantActive.IsVisible = true;
        vslIncomeDetails.IsVisible = true;
        vslIncomeType.IsVisible = true;

        _vm.IncomeTypeText = "Recurring";
        _vm.Income.IsRecurringIncome = true;
    }

    private void IncomeActiveYesSelect_Tapped(object sender, TappedEventArgs e)
    {
        UpdateIncomeActiveYesNo("Yes");
    }

    private void IncomeActiveNoSelect_Tapped(object sender, TappedEventArgs e)
    {
        UpdateIncomeActiveYesNo("No");
    }

    private void UpdateIncomeActiveYesNo(string option)
    {
        ClearAllValidators();

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "Yes")
        {
            vslIncomeActiveYesSelect.BackgroundColor = (Color)Success;
            vslIncomeActiveNoSelect.BackgroundColor = (Color)White;

            lblIncomeActiveYes.FontAttributes = FontAttributes.Bold;
            lblIncomeActiveNo.FontAttributes = FontAttributes.None;

            lblIncomeActiveYes.TextColor = (Color)White;
            lblIncomeActiveNo.TextColor = (Color)Gray900;

            _vm.IncomeActiveText = "Yes";
            _vm.Income.IsInstantActive = true;
            hslInstanceActiveCaution.IsVisible = true;

            _vm.Income.IncomeActiveDate = DateTime.UtcNow;

        }
        else if (option == "No")
        {
            vslIncomeActiveYesSelect.BackgroundColor = (Color)White;
            vslIncomeActiveNoSelect.BackgroundColor = (Color)Success;

            lblIncomeActiveYes.FontAttributes = FontAttributes.None;
            lblIncomeActiveNo.FontAttributes = FontAttributes.Bold;

            lblIncomeActiveYes.TextColor = (Color)Gray900;
            lblIncomeActiveNo.TextColor = (Color)White;

            _vm.IncomeActiveText = "No";
            _vm.Income.IsInstantActive = false;
            hslInstanceActiveCaution.IsVisible = false;

            _vm.Income.IncomeActiveDate = _vm.Income.DateOfIncomeEvent;
        }
        else
        {
            vslIncomeActiveYesSelect.BackgroundColor = (Color)White;
            vslIncomeActiveNoSelect.BackgroundColor = (Color)White;

            lblIncomeActiveYes.FontAttributes = FontAttributes.None;
            lblIncomeActiveNo.FontAttributes = FontAttributes.None;

            lblIncomeActiveYes.TextColor = (Color)Gray900;
            lblIncomeActiveNo.TextColor = (Color)Gray900;

            _vm.IncomeActiveText = "";
            _vm.Income.IsInstantActive = false;
            hslInstanceActiveCaution.IsVisible = false;
        }
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
            vslOption2Select.BackgroundColor = (Color)White;

            lblOption1.FontAttributes = FontAttributes.Bold;
            lblOption2.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)White;
            lblOption2.TextColor = (Color)Gray900;

            vslOption1.IsVisible = true;
            vslOption2.IsVisible = false;

            pckrEverynthDuration.SelectedItem = _vm.Income.RecurringIncomeDuration ?? "days";
            entEverynthValue.Text = _vm.Income.RecurringIncomeValue.ToString() ?? "1";

            _vm.Income.RecurringIncomeType = "Everynth";
            _vm.RecurringIncomeTypeText = "Everynth";

        }
        else if (option == "OfEveryMonth")
        {
            vslOption1Select.BackgroundColor = (Color)White;
            vslOption2Select.BackgroundColor = (Color)Success;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.Bold;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)White;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = true;

            entOfEveryMonthValue.Text = _vm.Income.RecurringIncomeValue.ToString() ?? "1";

            _vm.Income.RecurringIncomeType = "OfEveryMonth";
            _vm.RecurringIncomeTypeText = "OfEveryMonth";
        }
        else
        {
            vslOption1Select.BackgroundColor = (Color)White;
            vslOption2Select.BackgroundColor = (Color)White;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = false;

            _vm.Income.RecurringIncomeType = null;
            _vm.RecurringIncomeTypeText = "";
            entOfEveryMonthValue.Text = "";
            entEverynthValue.Text = "";
        }
    }

    void OfEveryMonthValue_Changed(object sender, TextChangedEventArgs e)
    {
        ClearAllValidators();

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

        ClearAllValidators();

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

    private void SaveIncomeTypeOptions()
    {
        if (_vm.Income.RecurringIncomeType == "Everynth")
        {
            _vm.Income.RecurringIncomeDuration = pckrEverynthDuration.SelectedItem.ToString();
            _vm.Income.RecurringIncomeValue = Convert.ToInt32(entEverynthValue.Text);
        }
        else if (_vm.Income.RecurringIncomeType == "OfEveryMonth")
        {
            _vm.Income.RecurringIncomeDuration = null;
            _vm.Income.RecurringIncomeValue = Convert.ToInt32(entOfEveryMonthValue.Text);
        }
        else
        {
            _vm.Income.RecurringIncomeDuration = null;
            _vm.Income.RecurringIncomeValue = null;
        }
    }
    void IncomeAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal IncomeAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entIncomeAmount.Text = IncomeAmount.ToString("c", CultureInfo.CurrentCulture);
        entIncomeAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entIncomeAmount.Text);
        _vm.Income.IncomeAmount = IncomeAmount;
    }

    private async Task<string> ChangeIncomeName()
    {
        string Description = "Every income needs a name, we will refer to it by the name you give it and this will make it easier to identify!";
        string DescriptionSub = "Call it something useful or call it something silly up to you really!";
        var popup = new PopUpPageSingleInput("Income Name", Description, DescriptionSub, "Enter an Income name!", _vm.Income.IncomeName, new PopUpPageSingleInputViewModel());
        var result = await Application.Current.MainPage.ShowPopupAsync(popup);

        if (result != null || (string)result != "")
        {
            _vm.Income.IncomeName = (string)result;
        }

        return (string)result;
    }

    private async void AddIncome_Clicked(object sender, EventArgs e)
    {
        if (_vm.Income.IncomeName == "" || _vm.Income.IncomeName == null)
        {
            string status = await ChangeIncomeName();
        }

        if(ValidateIncomeDetails())
        {
            SaveIncomeTypeOptions();

            _vm.AddIncome();
        }
        
    }
    private async void UpdateIncome_Clicked(object sender, EventArgs e)
    {
        if (_vm.Income.IncomeName == "" || _vm.Income.IncomeName == null)
        {
            string status = await ChangeIncomeName();
        }

        if (ValidateIncomeDetails())
        {
            SaveIncomeTypeOptions();

            _vm.UpdateIncome();
        }
    }

    private void dtpckIncomeDate_DateSelected(object sender, DateChangedEventArgs e)
    {
        if(_vm.Income.IsInstantActive ?? false)
        {
            _vm.Income.IncomeActiveDate = DateTime.UtcNow;
        }
        else
        {
            _vm.Income.IncomeActiveDate = _vm.Income.DateOfIncomeEvent;
        }
    }

    private bool ValidateIncomeDetails()
    {
        bool IsValid = true;

        if (_vm.Income.IncomeName == "")
        {
            IsValid = false;
            validatorIncomeName.IsVisible = true;
        }
        else
        {
            validatorIncomeName.IsVisible = false;
        }
        
        if (dtpckIncomeDate.Date <= _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
        {
            IsValid = false;
            validatorIncomeDate.IsVisible = true;
        }
        else
        {
            validatorIncomeDate.IsVisible = false;
        }


        if (_vm.Income.IncomeAmount == 0)
        {
            IsValid = false;
            validatorIncomeAmount.IsVisible = true;
        }
        else
        {
            validatorIncomeAmount.IsVisible = false;
        }

        if (_vm.IncomeActiveText == "" || _vm.IncomeActiveText == null)
        {
            IsValid = false;
            validatorIncomeActiveYesNo.IsVisible = true;
        }
        else
        {
            validatorIncomeActiveYesNo.IsVisible = false;
        }

        if (_vm.IncomeTypeText == "" || _vm.IncomeTypeText == null)
        {
            IsValid = false;
            validatorIncomeRecurring.IsVisible = true;
        }
        else if(_vm.IncomeTypeText == "Recurring")
        {
            validatorIncomeRecurring.IsVisible = false;

            if (_vm.RecurringIncomeTypeText == "" || _vm.RecurringIncomeTypeText == null)
            {
                IsValid = false;
                validatorIncomeType.IsVisible = true;
            }
            else if (_vm.RecurringIncomeTypeText == "Everynth")
            {
                if (entEverynthValue.Text == "" || entEverynthValue.Text == null)
                {
                    IsValid = false;
                    validatorEveryNthDuration.IsVisible = true;
                }
                else
                {
                    validatorEveryNthDuration.IsVisible = false;
                }

            }
            else if (_vm.RecurringIncomeTypeText == "OfEveryMonth")
            {
                if (entOfEveryMonthValue.Text == "" || entOfEveryMonthValue.Text == "")
                {
                    IsValid = false;
                    validatorOfEveryMonthDuration.IsVisible = true;
                }
                else
                {
                    validatorOfEveryMonthDuration.IsVisible = false;
                }
            }
        } 
        else
        {
            validatorIncomeRecurring.IsVisible = false;
        }

        

        _vm.IsPageValid = IsValid;
        return IsValid;
    }
    private void ClearAllValidators()
    {
        validatorOfEveryMonthDuration.IsVisible = false;
        validatorEveryNthDuration.IsVisible = false;
        validatorIncomeRecurring.IsVisible = false;
        validatorIncomeType.IsVisible = false;
        validatorIncomeActiveYesNo.IsVisible = false;
        validatorIncomeAmount.IsVisible = false;
        validatorIncomeDate.IsVisible = false;
        validatorIncomeName.IsVisible = false;  

        _vm.IsPageValid = true;
    }

}