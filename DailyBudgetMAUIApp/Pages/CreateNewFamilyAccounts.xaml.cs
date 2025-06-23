using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Core;
using System.Globalization;
using System.Text.RegularExpressions;


namespace DailyBudgetMAUIApp.Pages;

public partial class CreateNewFamilyAccounts : BasePage
{
    private readonly CreateNewFamilyAccountsViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IPopupService _ps;

    public string _updatedAvatar = "";
    public string UpdatedAvatar
    {
        get => _updatedAvatar;
        set
        {
            if (_updatedAvatar != value)
            {
                _updatedAvatar = value;
                bool Success = Enum.TryParse(value, out AvatarCharacter Avatar);
                if (Success)
                {
                    _vm.ProfilePicture.ContentType = ContentType.AvatarCharacter;
                    _vm.ProfilePicture.AvatarCharacter = Avatar;
                    int Number = Convert.ToInt32(value[value.Length - 1]);
                    Math.DivRem(Number, 8, out int index);
                    _vm.ProfilePicture.Background = App.ChartColor[index];
                    _vm.ProfilePictureString = value;
                }
                else
                {
                    _vm.ProfilePicture.AvatarCharacter = AvatarCharacter.Avatar1;
                    _vm.ProfilePicture.Background = App.ChartColor[1];
                    _vm.ProfilePictureString = "Avatar1";
                }
            }
        }
    }

    public Stream _profilePicStream;
    public Stream ProfilePicStream
    {
        get => _profilePicStream;
        set
        {
            if (_profilePicStream != value)
            {
                _profilePicStream = value;
                _vm.ProfilePicture.ContentType = ContentType.Custom;
                _vm.ProfilePicture.ImageSource = ImageSource.FromStream(() => ProfilePicStream);
                _vm.ProfilePictureString = "Upload";
            }
        }
    }

    public CreateNewFamilyAccounts(CreateNewFamilyAccountsViewModel vm, IProductTools pt, IPopupService ps)
    {
        InitializeComponent();
        _vm = vm;
        _pt = pt;
        BindingContext = _vm;
        _ps = ps;
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _vm.NavigatedFrom = "";
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _vm.LoadFamilyAccount();

            pckrSymbolPlacement.SelectedIndex = _vm.SelectedCurrencyPlacement.Id - 1;
            pckrDateFormat.SelectedIndex = _vm.SelectedDateFormats.Id - 1;
            pckrNumberFormat.SelectedIndex = _vm.SelectedNumberFormats.Id - 1;
            pckrTimeZone.SelectedIndex = _vm.SelectedTimeZone.TimeZoneID - 1;

            double BankBalance = (double?)_vm.Budget.BankBalance ?? 0;
            entBankBalance.Text = BankBalance.ToString("c", CultureInfo.CurrentCulture);
            double PayAmount = (double?)_vm.Budget.PaydayAmount ?? 0;
            entPayAmount.Text = PayAmount.ToString("c", CultureInfo.CurrentCulture);

            dtpckPayDay.Date = _vm.Budget.NextIncomePayday ?? default;
            dtpckPayDay.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow);

            UpdateSelectedOption(_vm.Budget.PaydayType);

            if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewFamilyAccounts", "OnAppearing");
        }
    }

    private void ChangeSelectedCurrency_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.ChangeCurrency();

            CurrencySearch.Text = "";
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "ChangeSelectedCurrency_Tapped");
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

            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopupInfo.Info)] = Info,
                [nameof(PopupInfo.SubTitles)] = SubTitle,
                [nameof(PopupInfo.TitleText)] = "Bank Balance"

            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            await _ps.ShowPopupAsync<PopupInfo>(Shell.Current, options: popupOptions, shellParameters: queryAttributes, cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewFamilyAccounts", "BankBalanceInfo");
        }
    }

    void BankBalance_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            _vm.Budget.BankBalance = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entBankBalance);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "BankBalance_Changed");
        }

    }
    void PayAmount_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            _vm.Budget.PaydayAmount = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entPayAmount);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "PayAmount_Changed");
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

            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopupInfo.Info)] = Info,
                [nameof(PopupInfo.SubTitles)] = SubTitle,
                [nameof(PopupInfo.TitleText)] = "When is Pay day?"

            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            await _ps.ShowPopupAsync<PopupInfo>(Shell.Current, options: popupOptions, shellParameters: queryAttributes, cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewFamilyAccounts", "PayDayInfo");
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

            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopupInfo.Info)] = Info,
                [nameof(PopupInfo.SubTitles)] = SubTitle,
                [nameof(PopupInfo.TitleText)] = "How do you get paid?"

            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            await _ps.ShowPopupAsync<PopupInfo>(Shell.Current, options: popupOptions, shellParameters: queryAttributes, cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewFamilyAccounts", "PayDetailsInfo");
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

            _vm.EverynthDuration = _vm.Budget.PaydayDuration ?? "days";
            _vm.EverynthValue = _vm.Budget.PaydayValue.ToString() ?? "1";

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

            _vm.WorkingDaysValue = _vm.Budget.PaydayValue.ToString() ?? "1";

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

            _vm.OfEveryMonthValue = _vm.Budget.PaydayValue.ToString() ?? "1";

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

            _vm.LastOfTheMonthDuration = _vm.Budget.PaydayDuration ?? "Monday";

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
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "Option1Select_Tapped");
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
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "Option2Select_Tapped");
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
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "Option3Select_Tapped");
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
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "Option4Select_Tapped");
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
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "EveryNthValue_Changed");
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
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "WorkingDaysValue_Changed");
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
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "OnAppearing");
        }
    }
}