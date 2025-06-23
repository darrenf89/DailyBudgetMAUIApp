using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Helpers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using System.Globalization;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class EditBudgetSettingsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        private readonly IPopupService _ps;

        public Picker SwitchBudgetPicker { get; set; }
        public VerticalStackLayout BtnApply { get; set; }

        [ObservableProperty]
        public partial List<lut_CurrencySymbol> CurrencySearchResults { get; set; }

        [ObservableProperty]
        public partial lut_CurrencySymbol SelectedCurrencySymbol { get; set; }

        [ObservableProperty]
        public partial bool HasCurrencySymbolChanged { get; set; }

        [ObservableProperty]
        public partial bool SearchVisible { get; set; } = false;

        [ObservableProperty]
        public partial List<lut_CurrencyPlacement> CurrencyPlacements { get; set; }

        [ObservableProperty]
        public partial lut_CurrencyPlacement SelectedCurrencyPlacement { get; set; }

        [ObservableProperty]
        public partial bool HasCurrencyPlacementChanged { get; set; }

        [ObservableProperty]
        public partial List<lut_DateFormat> DateFormats { get; set; }

        [ObservableProperty]
        public partial List<lut_BudgetTimeZone> TimeZones { get; set; }

        [ObservableProperty]
        public partial lut_DateFormat SelectedDateFormats { get; set; }

        [ObservableProperty]
        public partial bool HasDateFormatChanged { get; set; }

        [ObservableProperty]
        public partial List<lut_NumberFormat> NumberFormats { get; set; }

        [ObservableProperty]
        public partial lut_NumberFormat SelectedNumberFormats { get; set; }

        [ObservableProperty]
        public partial bool HasNumberFormatsChanged { get; set; }

        [ObservableProperty]
        public partial lut_BudgetTimeZone SelectedTimeZone { get; set; }

        [ObservableProperty]
        public partial bool HasTimeZoneChanged { get; set; }

        [ObservableProperty]
        public partial bool IsBorrowPay { get; set; }

        [ObservableProperty]
        public partial BudgetSettings BudgetSettings { get; set; }

        [ObservableProperty]
        public partial Budgets Budget { get; set; }

        [ObservableProperty]
        public partial string CurrentTime { get; set; }

        [ObservableProperty]
        public partial CultureInfo TimeCultureInfo { get; set; } = new CultureInfo("en-gb");

        [ObservableProperty]
        public partial string CurrencySettingValue { get; set; }

        [ObservableProperty]
        public partial CultureInfo CurrencyCultureInfo { get; set; } = new CultureInfo("en-gb");

        [ObservableProperty]
        public partial string PayDaySettings { get; set; }

        [ObservableProperty]
        public partial decimal PayAmount { get; set; }

        [ObservableProperty]
        public partial string PayAmountString { get; set; }

        [ObservableProperty]
        public partial int PayAmountCursorPosition { get; set; }

        [ObservableProperty]
        public partial bool HasPayAmountChanged { get; set; }

        [ObservableProperty]
        public partial DateTime PayDayDate { get; set; }

        [ObservableProperty]
        public partial bool HasPayDayDateChanged { get; set; }

        [ObservableProperty]
        public partial bool HasPageLoaded { get; set; }

        [ObservableProperty]
        public partial bool ValidatorPayDay { get; set; }

        [ObservableProperty]
        public partial bool ValidatorPayDayAmount { get; set; }

        [ObservableProperty]
        public partial string PayDayTypeText { get; set; }

        [ObservableProperty]
        public partial bool ValidatorPayType { get; set; }

        [ObservableProperty]
        public partial bool HasPayDayTypeTextChanged { get; set; }

        [ObservableProperty]
        public partial bool HasPayDayOptionsChanged { get; set; }

        [ObservableProperty]
        public partial string EveryNthDuration { get; set; }

        [ObservableProperty]
        public partial string EveryNthValue { get; set; }

        [ObservableProperty]
        public partial bool ValidatorEveryNthDuration { get; set; }

        [ObservableProperty]
        public partial string WorkingDaysValue { get; set; }

        [ObservableProperty]
        public partial bool ValidatorWorkingDayDuration { get; set; }

        [ObservableProperty]
        public partial string EveryMonthValue { get; set; }

        [ObservableProperty]
        public partial bool ValidatorOfEveryMonthDuration { get; set; }

        [ObservableProperty]
        public partial string LastOfTheMonthDuration { get; set; }

        [ObservableProperty]
        public partial string PayDayTwoSettings { get; set; }

        [ObservableProperty]
        public partial bool HasBorrowPayChanged { get; set; }

        [ObservableProperty]
        public partial bool IsMultipleAccounts { get; set; }

        [ObservableProperty]
        public partial string CurrencySearchText { get; set; }



        [RelayCommand]
        public async Task IsBorrowPayToggledCommand()
        {
            HasBorrowPayChanged = true;
        }

        public EditBudgetSettingsViewModel(IProductTools pt, IRestDataService ds, IPopupService ps)
        {
            _pt = pt;
            _ds = ds;
            _ps = ps;

        }      
        
        public async Task OnLoad()
        {
            Title = "Edit your budget's settings";
            CurrencyPlacements = await _ds.GetCurrencyPlacements("");
            DateFormats = await _ds.GetDateFormatsByString("");
            NumberFormats = await _ds.GetNumberFormats();
            TimeZones = await _ds.GetBudgetTimeZones("");

            BudgetSettings = await _ds.GetBudgetSettings(App.DefaultBudgetID);
            Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited");

            SelectedCurrencySymbol = (await _ds.GetCurrencySymbols(BudgetSettings.CurrencySymbol.ToString()))[0];
            SelectedCurrencyPlacement = (await _ds.GetCurrencyPlacements(BudgetSettings.CurrencyPattern.ToString()))[0];
            SelectedDateFormats = await _ds.GetDateFormatsById(BudgetSettings.ShortDatePattern ?? 1, BudgetSettings.DateSeperator ?? 1);
            SelectedNumberFormats = await _ds.GetNumberFormatsById(
                BudgetSettings.CurrencyDecimalDigits ?? 2,
                BudgetSettings.CurrencyDecimalSeparator ?? 2,
                BudgetSettings.CurrencyGroupSeparator ?? 1
            );
            SelectedTimeZone = await _ds.GetTimeZoneById(BudgetSettings.TimeZone.GetValueOrDefault());

            TimeCultureInfo.DateTimeFormat.ShortDatePattern = (await _ds.GetShortDatePatternById(SelectedDateFormats.ShortDatePatternID)).ShortDatePattern;
            TimeCultureInfo.DateTimeFormat.DateSeparator = (await _ds.GetDateSeperatorById(SelectedDateFormats.DateSeperatorID)).DateSeperator;

            await UpdateCurrencySettingValue();

            PayAmount = Budget.PaydayAmount ?? 0;
            PayAmountString = PayAmount.ToString("c", CultureInfo.CurrentCulture);

            PayDayDate = Budget.NextIncomePayday.GetValueOrDefault();

            IsBorrowPay = Budget.IsBorrowPay;

            if(Budget.PaydayType == "Everynth")
            {
                PayDayTypeText = "Everynth";
                EveryNthDuration = Budget.PaydayDuration;
                EveryNthValue = Budget.PaydayValue.ToString();

            }
            else if (Budget.PaydayType == "WorkingDays")
            {
                PayDayTypeText = "WorkingDays";
                WorkingDaysValue = Budget.PaydayValue.ToString();
            }
            else if (Budget.PaydayType == "OfEveryMonth")
            {
                PayDayTypeText = "OfEveryMonth";
                EveryMonthValue = Budget.PaydayValue.ToString();
            }
            else if (Budget.PaydayType == "LastOfTheMonth")
            {
                PayDayTypeText = "LastOfTheMonth";
                LastOfTheMonthDuration = Budget.PaydayDuration;
            }

            IsMultipleAccounts = Budget.IsMultipleAccounts;

            UpdatePayDaySettingsValue();
        }

        async partial void OnIsMultipleAccountsChanged(bool oldValue, bool newValue)
        {
            try
            {
                if(HasPageLoaded)
                {
                    if (newValue)
                    {

                        List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                        PatchDoc IsMultipleAccountsPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/IsMultipleAccounts",
                            value = true
                        };

                        BudgetUpdate.Add(IsMultipleAccountsPatch);
                        await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);

                        BankAccounts Account = new BankAccounts
                        {
                            BankAccountName = "My Default Account",
                            AccountBankBalance = App.DefaultBudget.BankBalance,
                            IsDefaultAccount = true

                        };

                        await _ds.AddBankAccounts(App.DefaultBudgetID, Account);

                        MultipleAccountsBottomSheet page = new MultipleAccountsBottomSheet(_pt, _ds, _ps);

                        page.Detents = new DetentsCollection()
                        {
                            new ContentDetent(),
                            new FullscreenDetent()
                        };

                        page.HasBackdrop = true;
                        page.CornerRadius = 0;

                        App.CurrentBottomSheet = page;

                        if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
                        await page.ShowAsync();

                    }
                    else
                    {
                        await _ds.DeleteBankAccounts(App.DefaultBudgetID);

                        List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                        PatchDoc IsMultipleAccountsPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/IsMultipleAccounts",
                            value = false
                        };

                        BudgetUpdate.Add(IsMultipleAccountsPatch);
                        await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);
                    }
                    var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
                    App.DefaultBudget = Budget;
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditBudgetSettings", "OnIsMultipleAccountsChanged");
            }
        }

        partial void OnEveryNthDurationChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                    UpdatePayDaySettingsValue();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnEveryNthDurationChanged");
            }
        }

        partial void OnEveryNthValueChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                    UpdatePayDaySettingsValue();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnEveryNthValueChanged");
            }
        }

        partial void OnWorkingDaysValueChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                    UpdatePayDaySettingsValue();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnWorkingDaysValueChanged");
            }
        }

        partial void OnEveryMonthValueChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                    UpdatePayDaySettingsValue();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnEveryMonthValueChanged");
            }
        }
        partial void OnLastOfTheMonthDurationChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                    UpdatePayDaySettingsValue();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnLastOfTheMonthDurationChanged");
            }
        }

        partial void OnPayDayTypeTextChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayTypeTextChanged = true;
                    UpdatePayDaySettingsValue();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnPayDayTypeTextChanged");
            }
        }

        partial void OnPayAmountStringChanged(string value)
        {
            try
            {
                decimal ValueNumber = (decimal)_pt.FormatCurrencyNumber(value);
                if(PayAmount != ValueNumber)
                {
                    PayAmount = ValueNumber;
                    PayAmountString = PayAmount.ToString("c", CultureInfo.CurrentCulture);
                    //PayAmountCursorPosition = _pt.FindCurrencyCursorPosition(PayAmountString);
                    HasPayAmountChanged = true;
                    UpdatePayDaySettingsValue();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnPayAmountStringChanged");
            }
        }

        partial void OnPayDayDateChanged(DateTime oldValue, DateTime newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayDateChanged = true;
                    UpdatePayDaySettingsValue();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnPayDayDateChanged");
            }
        }

        private void UpdatePayDaySettingsValue()
        {
            PayDaySettings = PayAmountString + " on the " + PayDayDate.ToString("d");
            if (PayDayTypeText == "Everynth")
            {
                if(EveryNthValue == "1")
                {
                    PayDayTwoSettings = "Then once a " + EveryNthDuration.Replace("s", "");
                }
                else
                {
                    PayDayTwoSettings = "Then every " + EveryNthValue + " " + EveryNthDuration;
                }
            }
            else if (PayDayTypeText == "WorkingDays")
            {
                if (WorkingDaysValue == "1")
                {
                    PayDayTwoSettings = "Then on the last working day of the month";
                }
                else
                {
                    PayDayTwoSettings = "Then " + WorkingDaysValue + " working days before end of month";
                }

            }
            else if (PayDayTypeText == "OfEveryMonth")
            {
                if (EveryMonthValue == "1")
                {
                    PayDayTwoSettings = "Then on the 1st of every month";
                }
                else if(EveryMonthValue == "2")
                {
                    PayDayTwoSettings = "Then on the 2nd of every month";
                }
                else if (EveryMonthValue == "3")
                {
                    PayDayTwoSettings = "Then on the 3rd of every month";
                }
                else
                {
                    PayDayTwoSettings = "Then on the " + EveryMonthValue + "th of every month";
                }
            }
            else if (PayDayTypeText == "LastOfTheMonth")
            {
                PayDayTwoSettings = "Then on the last " + LastOfTheMonthDuration + " of the month";
            }
        }

        private async Task UpdateCurrencySettingValue()
        {
            if(SelectedNumberFormats != null && SelectedCurrencyPlacement != null && SelectedCurrencySymbol != null)
            {
                CurrencyCultureInfo.NumberFormat.CurrencySymbol = SelectedCurrencySymbol.CurrencySymbol;
                if (SelectedNumberFormats.CurrencyDecimalSeparatorID != 0)
                {
                    var decimalSeparatorResult = await _ds.GetCurrencyDecimalSeparatorById(SelectedNumberFormats.CurrencyDecimalSeparatorID);
                    CurrencyCultureInfo.NumberFormat.CurrencyDecimalSeparator = decimalSeparatorResult.CurrencyDecimalSeparator;
                }

                if (SelectedNumberFormats.CurrencyGroupSeparatorID != 0)
                {
                    var groupSeparatorResult = await _ds.GetCurrencyGroupSeparatorById(SelectedNumberFormats.CurrencyGroupSeparatorID);
                    CurrencyCultureInfo.NumberFormat.CurrencyGroupSeparator = groupSeparatorResult.CurrencyGroupSeparator;
                }

                if (SelectedNumberFormats.CurrencyDecimalDigitsID != 0)
                {
                    var decimalDigitsResult = await _ds.GetCurrencyDecimalDigitsById(SelectedNumberFormats.CurrencyDecimalDigitsID);
                    CurrencyCultureInfo.NumberFormat.CurrencyDecimalDigits = Convert.ToInt32(decimalDigitsResult.CurrencyDecimalDigits);
                }
                CurrencyCultureInfo.NumberFormat.CurrencyPositivePattern = SelectedCurrencyPlacement.CurrencyPositivePatternRef;
            }

            CurrencySettingValue = 9001.ToString("c", CurrencyCultureInfo);
        }

        partial void OnSelectedNumberFormatsChanged(lut_NumberFormat value)
        {
            try
            {
                HasNumberFormatsChanged = true;
                UpdateCurrencySettingValue().FireAndForgetSafeAsync();
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnSelectedNumberFormatsChanged");
            }
        }

        partial void OnSelectedCurrencyPlacementChanged(lut_CurrencyPlacement value)
        {
            try
            {
                HasCurrencyPlacementChanged = true;
                UpdateCurrencySettingValue();
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnSelectedCurrencyPlacementChanged");
            }
        }

        partial void OnSelectedCurrencySymbolChanged(lut_CurrencySymbol value)
        {
            try
            {
                HasCurrencySymbolChanged = true;
                UpdateCurrencySettingValue();
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnSelectedCurrencySymbolChanged");
            }
        }

        async partial void OnSelectedDateFormatsChanged(lut_DateFormat value)
        {
            try
            {
                if (SelectedDateFormats != null)
                {
                    HasDateFormatChanged = true;

                    TimeCultureInfo.DateTimeFormat.ShortDatePattern = SelectedDateFormats.DateFormat;
                    TimeCultureInfo.DateTimeFormat.LongDatePattern = SelectedDateFormats.DateFormat + " HH:mm:ss";
                    TimeCultureInfo.DateTimeFormat.DateSeparator = (await _ds.GetDateSeperatorById(SelectedDateFormats.DateSeperatorID)).DateSeperator; 
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditBudgetSettings", "OnSelectedDateFormatsChanged");
            }

        }

        partial void OnSelectedTimeZoneChanged(lut_BudgetTimeZone value)
        {
            HasTimeZoneChanged = true;
        }
        
        public async Task ChangeSelectedCurrency()
        {
            SearchVisible = true;
            CurrencySearchResults = await _ds.GetCurrencySymbols("");
        }

        [RelayCommand]
        async Task ChangeBudgetName()
        {
            try
            {
                string Description = "Every budget needs a name, let us know how you'd like your budget to be known so we can use this to identify it for you in the future.";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";

                var queryAttributes = new Dictionary<string, object>
                {
                    [nameof(PopUpPageSingleInputViewModel.Description)] = Description,
                    [nameof(PopUpPageSingleInputViewModel.DescriptionSub)] = DescriptionSub,
                    [nameof(PopUpPageSingleInputViewModel.InputTitle)] = "Budget Name",
                    [nameof(PopUpPageSingleInputViewModel.Placeholder)] = "Enter a budget name!",
                    [nameof(PopUpPageSingleInputViewModel.Input)] = Budget.BudgetName
                };

                var popupOptions = new PopupOptions
                {
                    CanBeDismissedByTappingOutsideOfPopup = false,
                    PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
                };

                IPopupResult<object> popupResult = await _ps.ShowPopupAsync<PopUpPageSingleInput, object>(
                    Shell.Current,
                    options: popupOptions,
                    shellParameters: queryAttributes,
                    cancellationToken: CancellationToken.None

                );

                if (popupResult.Result != null || (string)popupResult.Result != "")
                {
                    Budget.BudgetName = (string)popupResult.Result;

                    List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                    PatchDoc BudgetName = new PatchDoc
                    {
                        op = "replace",
                        path = "/BudgetName",
                        value = Budget.BudgetName
                    };

                    BudgetUpdate.Add(BudgetName);
                    await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);
                    App.DefaultBudget.BudgetName = Budget.BudgetName;
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditBudgetSettings", "ChangeBudgetName");
            }
        }

        async partial void OnCurrencySearchTextChanged(string value)
        {
            try
            {
                CurrencySearchResults = await _ds.GetCurrencySymbols(value);
            }
            catch (Exception ex)
            {
                if (ex.Message == "One or more errors occurred. (No currencies found)")
                {
                    lut_CurrencySymbol cs = new lut_CurrencySymbol();
                    cs.Code = "No results please, try again!";
                    CurrencySearchResults.Clear();
                    CurrencySearchResults.Add(cs);
                }
                else
                {
                    await _pt.HandleException(ex, "CreateNewFamilyAccounts", "CurrencySearch");
                }
            }
        }

        async partial void OnSelectedCurrencySymbolChanged(lut_CurrencySymbol oldValue, lut_CurrencySymbol newValue)
        {
            try
            {
                SearchVisible = false;
                CurrencySearchResults = null;
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "CurrencySymbolSelected");
            }
        }

        [RelayCommand]
        private async Task CloseSettings(object obj)
        {
            try
            {
                if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                await Task.Delay(500);

                await Shell.Current.GoToAsync($"..");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditBudgetSettings", "CloseSettings");
            }

        }

        public async Task UpdateTime()
        {
            try
            {
                await Task.Delay(1);
                DateTime CurrentDateTime = DateTime.UtcNow.AddHours(SelectedTimeZone.TimeZoneUTCOffset);
                CurrentTime = CurrentDateTime.ToString(TimeCultureInfo.DateTimeFormat.LongDatePattern, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditBudgetSettings", "UpdateTime");
            }
        }

        [RelayCommand]
        private async Task SaveBudgetSettings()
        {
            try
            {

                bool EditBudget = await Application.Current.Windows[0].Page.DisplayAlert($"Update settings?", $"Are you sure you want to update your budgets settings?", "Yes", "No");
                if (EditBudget)
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Task.Delay(100);


                        List<PatchDoc> BudgetUpdate = new List<PatchDoc>();
                        List<PatchDoc> BudgetSettingsUpdate = new List<PatchDoc>();

                        if (HasCurrencySymbolChanged)
                        {
                            PatchDoc CurrencySymbol = new PatchDoc
                            {
                                op = "replace",
                                path = "/CurrencySymbol",
                                value = SelectedCurrencySymbol.Id
                            };

                            BudgetSettingsUpdate.Add(CurrencySymbol);
                        }

                        if (HasCurrencyPlacementChanged)
                        {
                            PatchDoc CurrencyPattern = new PatchDoc
                            {
                                op = "replace",
                                path = "/CurrencyPattern",
                                value = SelectedCurrencyPlacement.Id
                            };

                            BudgetSettingsUpdate.Add(CurrencyPattern);
                        }


                        if (HasNumberFormatsChanged)
                        {
                            PatchDoc CurrencyDecimalDigits = new PatchDoc
                            {
                                op = "replace",
                                path = "/CurrencyDecimalDigits",
                                value = SelectedNumberFormats.CurrencyDecimalDigitsID
                            };

                            BudgetSettingsUpdate.Add(CurrencyDecimalDigits);

                            PatchDoc CurrencyDecimalSeparator = new PatchDoc
                            {
                                op = "replace",
                                path = "/CurrencyDecimalSeparator",
                                value = SelectedNumberFormats.CurrencyDecimalSeparatorID
                            };

                            BudgetSettingsUpdate.Add(CurrencyDecimalSeparator);

                            PatchDoc CurrencyGroupSeparator = new PatchDoc
                            {
                                op = "replace",
                                path = "/CurrencyGroupSeparator",
                                value = SelectedNumberFormats.CurrencyGroupSeparatorID
                            };

                            BudgetSettingsUpdate.Add(CurrencyGroupSeparator);
                        }

                        if(HasDateFormatChanged)
                        {
                            PatchDoc DateSeperator = new PatchDoc
                            {
                                op = "replace",
                                path = "/DateSeperator",
                                value = SelectedDateFormats.DateSeperatorID
                            };

                            BudgetSettingsUpdate.Add(DateSeperator);

                            PatchDoc ShortDatePattern = new PatchDoc
                            {
                                op = "replace",
                                path = "/ShortDatePattern",
                                value = SelectedDateFormats.ShortDatePatternID
                            };

                            BudgetSettingsUpdate.Add(ShortDatePattern);
                        }

                        if(HasTimeZoneChanged)
                        {
                            PatchDoc TimeZone = new PatchDoc
                            {
                                op = "replace",
                                path = "/TimeZone",
                                value = SelectedTimeZone.TimeZoneID
                            };

                            BudgetSettingsUpdate.Add(TimeZone);
                        }

                        if(HasBorrowPayChanged)
                        {
                            PatchDoc BorrowPay = new PatchDoc
                            {
                                op = "replace",
                                path = "/IsBorrowPay",
                                value = IsBorrowPay
                            };

                            BudgetUpdate.Add(BorrowPay);
                        }

                        if (HasPayAmountChanged)
                        {
                            PatchDoc PayDayAmount = new PatchDoc
                            {
                                op = "replace",
                                path = "/PayDayAmount",
                                value = PayAmount
                            };

                            BudgetUpdate.Add(PayDayAmount);
                        }

                        if (HasPayDayDateChanged)
                        {
                            PatchDoc NextIncomePayday = new PatchDoc
                            {
                                op = "replace",
                                path = "/NextIncomePayday",
                                value = PayDayDate
                            };

                            BudgetUpdate.Add(NextIncomePayday);
                        }


                        if(HasPayDayTypeTextChanged)
                        {
                            PatchDoc PayType = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayType",
                                value = PayDayTypeText
                            };

                            BudgetUpdate.Add(PayType);
                        }

                        if(HasPayDayOptionsChanged)
                        {
                            if (PayDayTypeText == "Everynth")
                            {
                                PatchDoc PaydayValue = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PaydayValue",
                                    value = EveryNthValue
                                };

                                BudgetUpdate.Add(PaydayValue);

                                PatchDoc PaydayDuration = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PaydayDuration",
                                    value = EveryNthDuration
                                };

                                BudgetUpdate.Add(PaydayDuration);

                            }
                            else if (PayDayTypeText == "WorkingDays")
                            {
                                PatchDoc PaydayValue = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PaydayValue",
                                    value = WorkingDaysValue
                                };

                                BudgetUpdate.Add(PaydayValue);

                                PatchDoc PaydayDuration = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PaydayDuration",
                                    value = ""
                                };

                                BudgetUpdate.Add(PaydayDuration);
                            }
                            else if (PayDayTypeText == "OfEveryMonth")
                            {
                                PatchDoc PaydayValue = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PaydayValue",
                                    value = EveryMonthValue
                                };

                                BudgetUpdate.Add(PaydayValue);

                                PatchDoc PaydayDuration = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PaydayDuration",
                                    value = ""
                                };

                                BudgetUpdate.Add(PaydayDuration);
                            }
                            else if (PayDayTypeText == "LastOfTheMonth")
                            {
                                PatchDoc PaydayValue = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PaydayValue",
                                    value = ""
                                };

                                BudgetUpdate.Add(PaydayValue);

                                PatchDoc PaydayDuration = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PaydayDuration",
                                    value = LastOfTheMonthDuration
                                };

                                BudgetUpdate.Add(PaydayDuration);
                            }
                        }

                        if (BudgetUpdate.Count() > 0)
                        {
                            await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);
                        }

                        if (BudgetSettingsUpdate.Count() > 0)
                        {
                            await _ds.PatchBudgetSettings(App.DefaultBudgetID, BudgetSettingsUpdate);
                        }



                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}?SnackBar=BudgetSettingsUpdated&SnackID=0");

                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditBudgetSettings", "SaveBudgetSettings");
            }
        }

        [RelayCommand]
        private async Task DeleteBudget()
        {
            try
            {
                var BudgetName = await Application.Current.Windows[0].Page.DisplayPromptAsync($"Are you sure you want to delete {App.DefaultBudget.BudgetName} budget?", $"Deleting the budget is permanent, enter the budget name to delete the budget", "Ok", "Cancel");
                if (BudgetName != null)
                {
                    if (string.Equals(BudgetName, App.DefaultBudget.BudgetName, StringComparison.OrdinalIgnoreCase))
                    {
                        await Task.Delay(100);

                        string result = await _ds.DeleteBudget(App.DefaultBudgetID, App.UserDetails.UserID);
                        if (result == "LastBudget")
                        {
                            await Application.Current.Windows[0].Page.DisplayAlert($"You can't delete this!", $"You can't delete this budget as it is your last budget and you must have at least one budget on the app", "Ok");

                        }
                        else if (result == "SharedBudget")
                        {
                            await Application.Current.Windows[0].Page.DisplayAlert($"This is a shared budget!", $"You can't delete a budget that you didn't create, someone kindly shared it with you so don't try and delete it", "Ok");

                        }
                        else
                        {
                            List<Budgets> Budgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "EditBudgetSettings");

                            App.DefaultBudgetID = Budgets[0].BudgetID;
                            App.DefaultBudget = Budgets[0];
                            BudgetSettingValues Settings = await _ds.GetBudgetSettingsValues(App.DefaultBudgetID);
                            App.CurrentSettings = Settings;

                            if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                            {
                                Preferences.Remove(nameof(App.DefaultBudgetID));
                            }
                            Preferences.Set(nameof(App.DefaultBudgetID), Budgets[0].BudgetID);

                            List<PatchDoc> UserUpdate = new List<PatchDoc>();

                            PatchDoc DefaultBudgetID = new PatchDoc
                            {
                                op = "replace",
                                path = "/DefaultBudgetID",
                                value = App.DefaultBudgetID
                            };

                            UserUpdate.Add(DefaultBudgetID);

                            PatchDoc PreviousDefaultBudgetID = new PatchDoc
                            {
                                op = "replace",
                                path = "/PreviousDefaultBudgetID",
                                value = 0
                            };

                            UserUpdate.Add(PreviousDefaultBudgetID);
                            await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

                            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                        }
                    
                    }
                    else
                    {
                         await Application.Current.Windows[0].Page.DisplayAlert($"Incorrect Budget Name", "Please check the budget name that was entered and confirm it is correct!", "Ok, thanks");
                    }

                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditBudgetSettings", "DeleteBudget");
            }
        }

        [RelayCommand]
        private async Task GoToBorrowPayVideos()
        {
            try
            {
                List<string> SubTitle = new List<string>{
                    "",
                    "",
                    "",
                    "",
                };

                List<string> Info = new List<string>{
                    "Effectively managing your bill payments is essential for maintaining financial stability. dBudget offers two distinct methods for handling bill payments, each designed to accommodate different budgeting preferences and financial situations.",
                    "\"Cover Bills When Paid\" method aligns with the common practice of paying bills as income is received. When you get paid, dBudget allocates funds to cover bills up to that payday and any bills due within the current budget cycle. Bills due outside the budget cycle accrue their balance daily from your bank balance, but the funds don't transfer until the next payday. This approach mirrors real-life bill payment habits and helps prevent your daily spending allowance from appearing negative. However, careful if the next pay day doesn't come on time you might not have the money in your account to cover all your bills.",
                    "\"Cover Bills From Balance Every Day\" method involves allocating funds to fully cover each bill for the upcoming period upon receiving your paycheck. Daily, the allocated amounts are deducted from your available balance and transferred into the bill balance THEN AND THERE! This strategy ensures that bills are entirely covered before their due dates, providing clarity on financial obligations. However, it necessitates having sufficient funds available upfront, which may not be feasible for everyone. Additionally, this approach can make your daily spending allowance appear lower than it actually is, potentially affecting budgeting flexibility.",
                    "If accumulating the necessary funds to cover bills upfront is challenging, it's advisable to use the \"Cover Bills When Paid\" option. This method aligns with typical financial practices and provides a more accurate reflection of your daily spending capacity. As you work towards building savings, you might consider transitioning to the \"Cover Bills From Balance Every Day\" method for enhanced financial predictability.",
                };

                var queryAttributes = new Dictionary<string, object>
                {
                    [nameof(PopupInfo.Info)] = Info,
                    [nameof(PopupInfo.SubTitles)] = SubTitle,
                    [nameof(PopupInfo.TitleText)] = "Bill accrual"

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
                await _pt.HandleException(ex, "CreateNewBudget", "GoToBorrowPayVideos");
            }
        }

        partial void OnIsBorrowPayChanged(bool oldValue, bool newValue)
        {
            try
            {
                if (oldValue)
                {
                    CheckIsBorrowPay();
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "EditBudgetSettings", "OnIsBorrowPayChanged");
            }

        }

        private async Task CheckIsBorrowPay()
        {
            bool result = await Shell.Current.DisplayAlert("Start paying outgoings each day?", "\nAre you sure you want to change the setting and start \"paying\" your outgoings each day? \n \nCareful! If you don't have the money put aside you might end up with no money left to spend. \n \nIf you would like more information on how dBudget calculates your daily budget using this setting check out our cool videos and documentation!", "Yes", "Cancel");
            if (!result)
            {
                IsBorrowPay = true;
            }
        }


        [RelayCommand]
        public async Task BackButton()
        {
            try
            {
                if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                await Task.Delay(1);

                await Shell.Current.GoToAsync($"//{(App.IsFamilyAccount ? nameof(FamilyAccountMainPage) : nameof(MainPage))}");

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "BackButton");
            }
        }

    }
}
