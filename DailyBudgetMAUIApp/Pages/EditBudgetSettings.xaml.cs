using DailyBudgetMAUIApp.ViewModels;
using Microsoft.Maui.Layouts;
using System.Text.RegularExpressions;
using IeuanWalker.Maui.Switch.Events;
using IeuanWalker.Maui.Switch.Helpers;
using IeuanWalker.Maui.Switch;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Pages;

public partial class EditBudgetSettings : BasePage
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }

    private readonly EditBudgetSettingsViewModel _vm;
    private readonly IProductTools _pt;
    private IDispatcherTimer _timer;

    public EditBudgetSettings(EditBudgetSettingsViewModel viewModel, IProductTools pt)
	{
		InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 60;
        ButtonWidth = ScreenWidth - 40;
    }

    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            TopBV.WidthRequest = ScreenWidth;
            MainAbs.WidthRequest = ScreenWidth;
            MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
            MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));       

            lblTitle.Text = $"Budget settings";

            await _vm.OnLoad();

            pckrSymbolPlacement.SelectedIndex = _vm.SelectedCurrencyPlacement.Id - 1;            
            pckrDateFormat.SelectedIndex = _vm.SelectedDateFormats.Id - 1;
            pckrNumberFormat.SelectedIndex = _vm.SelectedNumberFormats.Id - 1;
            pckrTimeZone.SelectedIndex = _vm.SelectedTimeZone.TimeZoneID - 1;

            UpdateSelectedOption(_vm.PayDayTypeText);

            dtpckPayDay.MinimumDate = DateTime.UtcNow.AddDays(1);

            _vm.HasCurrencyPlacementChanged = false;
            _vm.HasCurrencySymbolChanged = false;
            _vm.HasTimeZoneChanged = false;
            _vm.HasNumberFormatsChanged = false;
            _vm.HasDateFormatChanged = false;
            _vm.HasPayAmountChanged = false;
            _vm.HasPayDayDateChanged = false;
            _vm.HasPayDayTypeTextChanged = false;
            _vm.HasBorrowPayChanged = false;
            _vm.HasPayDayOptionsChanged = false;

            var timer = Application.Current.Dispatcher.CreateTimer();
            _timer = timer;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += async (s, e) =>
            {
                try
                {
                    await _vm.UpdateTime();
                }
                catch (Exception ex)
                {
                    await _pt.HandleException(ex, "EditBudgetSettings", "timer.Tick");
                }
            };
            timer.Start();

            _vm.BtnApply = BtnApply;

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
            _vm.HasPageLoaded = true;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditBudgetSettings", "OnAppearing");
        }
    }

    public void CustomSwitch_SwitchPanUpdate(CustomSwitch customSwitch, SwitchPanUpdatedEventArgs e)
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

        _vm.HasBorrowPayChanged = true;
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        _timer.Stop();
    }

    private void acrCurrencySetting_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!CurrencySetting.IsVisible)
            {
                CurrencySetting.IsVisible = true;
                CurrencySettingIcon.Glyph = "\ue5cf";
            }
            else
            {
                CurrencySetting.IsVisible = false;
                CurrencySettingIcon.Glyph = "\ue5ce";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "EditBudgetSettings", "acrCurrencySetting_Tapped");
        }
    }

    private void acrDateTimeSetting_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!DateTimeSetting.IsVisible)
            {
                DateTimeSetting.IsVisible = true;
                DateTimeIcon.Glyph = "\ue5cf";
            }
            else
            {
                DateTimeSetting.IsVisible = false;
                DateTimeIcon.Glyph = "\ue5ce";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "EditBudgetSettings", "acrDateTimeSetting_Tapped");
        }
    }

    private void acrPayDaySetting_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!PayDaySetting.IsVisible)
            {
                PayDaySetting.IsVisible = true;
                PayDayIcon.Glyph = "\ue5cf";
            }
            else
            {
                PayDaySetting.IsVisible = false;
                PayDayIcon.Glyph = "\ue5ce";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "EditBudgetSettings", "acrPayDaySetting_Tapped");
        }
    }

    private void acrBudgetToggleSetting_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!BudgetToggleSetting.IsVisible)
            {
                BudgetToggleSetting.IsVisible = true;
                BudgetToggleIcon.Glyph = "\ue5cf";
            }
            else
            {
                BudgetToggleSetting.IsVisible = false;
                BudgetToggleIcon.Glyph = "\ue5ce";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "EditBudgetSettings", "acrBudgetToggleSetting_Tapped");
        }
    }

    private void ChangeSelectedCurrency_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.ChangeSelectedCurrency();
            CurrencySearch.Text = "";
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "EditBudgetSettings", "ChangeSelectedCurrency_Tapped");
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
            _pt.HandleException(ex, "EditBudgetSettings", "Option1Select_Tapped");
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
            _pt.HandleException(ex, "EditBudgetSettings", "Option2Select_Tapped");
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
            _pt.HandleException(ex, "EditBudgetSettings", "Option3Select_Tapped");
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
            _pt.HandleException(ex, "EditBudgetSettings", "EveryNthValue_Changed");
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
            _pt.HandleException(ex, "EditBudgetSettings", "WorkingDaysValue_Changed");
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
            _pt.HandleException(ex, "EditBudgetSettings", "OfEveryMonthValue_Changed");
        }
    }

    private void MultipleAccountsToggle_Toggled(CustomSwitch customSwitch, SwitchPanUpdatedEventArgs e)
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
}