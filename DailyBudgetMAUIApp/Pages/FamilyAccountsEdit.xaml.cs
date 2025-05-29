using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using System.Text.RegularExpressions;

namespace DailyBudgetMAUIApp.Pages;

public partial class FamilyAccountsEdit : BasePage
{
    private readonly FamilyAccountsEditViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public FamilyAccountsEdit(FamilyAccountsEditViewModel vm, IProductTools pt, IRestDataService ds)
    {
        InitializeComponent();
        _vm = vm;
        _pt = pt;
        BindingContext = _vm;
        _ds = ds;
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }

            _vm.IsPageBusy = true;

            await _vm.OnLoad();

            _vm.SwitchBudgetPicker.SelectedIndexChanged += async (s, e) =>
            {
                _vm.IsPageBusy = true;
                await Task.Delay(1);

                var picker = (Picker)s;
                var SelectedAccount = (FamilyUserAccount)picker.SelectedItem;
                _vm.FamilyUserAccount = SelectedAccount;

                _vm.Budget = await _ds.GetBudgetDetailsAsync(SelectedAccount.BudgetID, "Full");
                _vm.FamilyAccountID = SelectedAccount.UserID;
                await _vm.LoadBudgetDetails();
                UpdateSelectedOption(_vm.PayDayTypeText);

                _vm.IsPageBusy = false;
            };

            UpdateSelectedOption(_vm.PayDayTypeText);

            vslPckrSwitchBudget.Content = _vm.SwitchBudgetPicker;
            _vm.IsPageBusy = false;

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "FamilyAccountsEdit", "OnAppearing");
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
            _pt.HandleException(ex, "FamilyAccountsEdit", "Option1Select_Tapped");
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
            _pt.HandleException(ex, "FamilyAccountsEdit", "Option2Select_Tapped");
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
            _pt.HandleException(ex, "FamilyAccountsEdit", "Option3Select_Tapped");
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
            _pt.HandleException(ex, "EditBudgetSettings", "Option4Select_Tapped");
        }
    }

    private void UpdateSelectedOption(string option)
    {

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
            _pt.HandleException(ex, "FamilyAccountsEdit", "EveryNthValue_Changed");
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
            _pt.HandleException(ex, "FamilyAccountsEdit", "WorkingDaysValue_Changed");
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
            _pt.HandleException(ex, "FamilyAccountsEdit", "OfEveryMonthValue_Changed");
        }
    }

}