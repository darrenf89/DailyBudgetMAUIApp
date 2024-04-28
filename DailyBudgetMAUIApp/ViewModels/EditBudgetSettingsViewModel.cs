using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Diagnostics;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class EditBudgetSettingsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        public Picker SwitchBudgetPicker { get; set; }
        public VerticalStackLayout BtnApply { get; set; }

        [ObservableProperty]
        private List<lut_CurrencySymbol> currencySearchResults;
        [ObservableProperty]
        private lut_CurrencySymbol selectedCurrencySymbol;
        [ObservableProperty]
        private bool hasCurrencySymbolChanged;
        [ObservableProperty]
        private bool searchVisible = false;
        [ObservableProperty]
        private List<lut_CurrencyPlacement> currencyPlacements;
        [ObservableProperty]
        private lut_CurrencyPlacement selectedCurrencyPlacement;
        [ObservableProperty]
        private bool hasCurrencyPlacementChanged;
        [ObservableProperty]
        private List<lut_DateFormat> dateFormats;
        [ObservableProperty]
        private List<lut_BudgetTimeZone> timeZones;
        [ObservableProperty]
        private lut_DateFormat selectedDateFormats;
        [ObservableProperty]
        private bool hasDateFormatChanged;
        [ObservableProperty]
        private List<lut_NumberFormat> numberFormats;
        [ObservableProperty]
        private lut_NumberFormat selectedNumberFormats;
        [ObservableProperty]
        private bool hasNumberFormatsChanged;
        [ObservableProperty]
        private lut_BudgetTimeZone selectedTimeZone;
        [ObservableProperty]
        private bool hasTimeZoneChanged;
        [ObservableProperty]
        private bool isBorrowPay;
        [ObservableProperty]
        private BudgetSettings budgetSettings;
        [ObservableProperty]
        private Budgets budget;
        [ObservableProperty]
        private string currentTime;
        [ObservableProperty]
        private CultureInfo timeCultureInfo = new CultureInfo("en-gb");
        [ObservableProperty]
        private string currencySettingValue;
        [ObservableProperty]
        private CultureInfo currencyCultureInfo = new CultureInfo("en-gb");
        [ObservableProperty]
        private string payDaySettings;
        [ObservableProperty]
        private decimal payAmount;
        [ObservableProperty]
        private string payAmountString;
        [ObservableProperty]
        private int payAmountCursorPosition;
        [ObservableProperty]
        private bool hasPayAmountChanged;
        [ObservableProperty]
        private DateTime payDayDate;
        [ObservableProperty]
        private bool hasPayDayDateChanged;
        [ObservableProperty]
        private bool validatorPayDay;
        [ObservableProperty]
        private bool validatorPayDayAmount;
        [ObservableProperty]
        private string payDayTypeText;
        [ObservableProperty]
        private bool validatorPayType;
        [ObservableProperty]
        private bool hasPayDayTypeTextChanged;
        [ObservableProperty]
        private bool hasPayDayOptionsChanged;
        [ObservableProperty]
        private string everyNthDuration;
        [ObservableProperty]
        private string everyNthValue;
        [ObservableProperty]
        private bool validatorEveryNthDuration;
        [ObservableProperty]
        private string workingDaysValue;
        [ObservableProperty]
        private bool validatorWorkingDayDuration;
        [ObservableProperty]
        private string everyMonthValue;
        [ObservableProperty]
        private bool validatorOfEveryMonthDuration;
        [ObservableProperty]
        private string lastOfTheMonthDuration;
        [ObservableProperty]
        private string payDayTwoSettings;
        [ObservableProperty]
        private bool hasBorrowPayChanged;


        public EditBudgetSettingsViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            CurrencyPlacements = _ds.GetCurrencyPlacements("").Result;
            DateFormats = _ds.GetDateFormatsByString("").Result;
            NumberFormats = _ds.GetNumberFormats().Result;
            TimeZones = _ds.GetBudgetTimeZones("").Result;
        }      
        
        public async Task OnLoad()
        {

            BudgetSettings = _ds.GetBudgetSettings(App.DefaultBudgetID).Result;
            Budget = _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited").Result;

            SelectedCurrencySymbol = _ds.GetCurrencySymbols(BudgetSettings.CurrencySymbol.ToString()).Result[0];
            SelectedCurrencyPlacement = _ds.GetCurrencyPlacements(BudgetSettings.CurrencyPattern.ToString()).Result[0];
            SelectedDateFormats = _ds.GetDateFormatsById(BudgetSettings.ShortDatePattern ?? 1, BudgetSettings.DateSeperator ?? 1).Result;
            SelectedNumberFormats = _ds.GetNumberFormatsById(BudgetSettings.CurrencyDecimalDigits ?? 2, BudgetSettings.CurrencyDecimalSeparator ?? 2, BudgetSettings.CurrencyGroupSeparator ?? 1).Result;
            SelectedTimeZone = _ds.GetTimeZoneById(BudgetSettings.TimeZone.GetValueOrDefault()).Result;

            TimeCultureInfo.DateTimeFormat.ShortDatePattern = _ds.GetShortDatePatternById(SelectedDateFormats.ShortDatePatternID).Result.ShortDatePattern;
            TimeCultureInfo.DateTimeFormat.DateSeparator = _ds.GetDateSeperatorById(SelectedDateFormats.DateSeperatorID).Result.DateSeperator;

            UpdateCurrencySettingValue();

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

            UpdatePayDaySettingsValue();
        }

        partial void OnEveryNthDurationChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                HasPayDayOptionsChanged = true;
                UpdatePayDaySettingsValue();
            }
        }

        partial void OnEveryNthValueChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                HasPayDayOptionsChanged = true;
                UpdatePayDaySettingsValue();
            }
        }

        partial void OnWorkingDaysValueChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                HasPayDayOptionsChanged = true;
                UpdatePayDaySettingsValue();
            }
        }

        partial void OnEveryMonthValueChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                HasPayDayOptionsChanged = true;
                UpdatePayDaySettingsValue();
            }
        }
        partial void OnLastOfTheMonthDurationChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                HasPayDayOptionsChanged = true;
                UpdatePayDaySettingsValue();
            }
        }

        partial void OnPayDayTypeTextChanged(string oldValue, string newValue)
        {
            if (oldValue != newValue)
            {
                HasPayDayTypeTextChanged = true;
                UpdatePayDaySettingsValue();
            }
        }

        partial void OnPayAmountStringChanged(string value)
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

        partial void OnPayDayDateChanged(DateTime oldValue, DateTime newValue)
        {
            if(oldValue != newValue)
            {
                HasPayDayDateChanged = true;
                UpdatePayDaySettingsValue();
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

        private void UpdateCurrencySettingValue()
        {
            if(SelectedNumberFormats != null && SelectedCurrencyPlacement != null && SelectedCurrencySymbol != null)
            {
                CurrencyCultureInfo.NumberFormat.CurrencySymbol = SelectedCurrencySymbol.CurrencySymbol;
                if(SelectedNumberFormats.CurrencyDecimalSeparatorID != 0)
                {
                    CurrencyCultureInfo.NumberFormat.CurrencyDecimalSeparator = _ds.GetCurrencyDecimalSeparatorById(SelectedNumberFormats.CurrencyDecimalSeparatorID).Result.CurrencyDecimalSeparator;
                }
                if (SelectedNumberFormats.CurrencyGroupSeparatorID != 0)
                {
                    CurrencyCultureInfo.NumberFormat.CurrencyGroupSeparator = _ds.GetCurrencyGroupSeparatorById(SelectedNumberFormats.CurrencyGroupSeparatorID).Result.CurrencyGroupSeparator;
                }
                if(SelectedNumberFormats.CurrencyDecimalDigitsID != 0)
                {
                    CurrencyCultureInfo.NumberFormat.CurrencyDecimalDigits = Convert.ToInt32(_ds.GetCurrencyDecimalDigitsById(SelectedNumberFormats.CurrencyDecimalDigitsID).Result.currencyDecimalDigits);
                }
                CurrencyCultureInfo.NumberFormat.CurrencyPositivePattern = SelectedCurrencyPlacement.CurrencyPositivePatternRef;
            }

            CurrencySettingValue = 9001.ToString("c", CurrencyCultureInfo);
        }

        partial void OnSelectedNumberFormatsChanged(lut_NumberFormat value)
        {
            HasNumberFormatsChanged = true;
            UpdateCurrencySettingValue();
        }

        partial void OnSelectedCurrencyPlacementChanged(lut_CurrencyPlacement value)
        {
            HasCurrencyPlacementChanged = true;
            UpdateCurrencySettingValue();
        }

        partial void OnSelectedCurrencySymbolChanged(lut_CurrencySymbol value)
        {
            HasCurrencySymbolChanged = true;
            UpdateCurrencySettingValue();
        }

        partial void OnSelectedDateFormatsChanged(lut_DateFormat value)
        {
            if(SelectedDateFormats != null)
            {
                HasDateFormatChanged = true;

                TimeCultureInfo.DateTimeFormat.ShortDatePattern = SelectedDateFormats.DateFormat;
                TimeCultureInfo.DateTimeFormat.LongDatePattern = SelectedDateFormats.DateFormat + " HH:mm:ss";
                TimeCultureInfo.DateTimeFormat.DateSeparator = _ds.GetDateSeperatorById(SelectedDateFormats.DateSeperatorID).Result.DateSeperator;
            }

        }

        partial void OnSelectedTimeZoneChanged(lut_BudgetTimeZone value)
        {
            HasTimeZoneChanged = true;
        }
        
        public void ChangeSelectedCurrency()
        {
            SearchVisible = true;
            CurrencySearchResults = _ds.GetCurrencySymbols("").Result;
        }

        [RelayCommand]
        async Task ChangeBudgetName()
        {
            try
            {
                string Description = "Every budget needs a name, let us know how you'd like your budget to be known so we can use this to identify it for you in the future.";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Budget Name", Description, DescriptionSub, "Enter a budget name!", Budget.BudgetName, new PopUpPageSingleInputViewModel());
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    Budget.BudgetName = (string)result;

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
                Debug.WriteLine($" --> {ex.Message}");
                ErrorLog Error = _pt.HandleCatchedException(ex, "EditBudgetSettings", "ChangeName").Result;
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        [RelayCommand]
        async Task CurrencySearch(string query)
        {
            try
            {
                CurrencySearchResults = await _ds.GetCurrencySymbols(query);
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
                    Debug.WriteLine($" --> {ex.Message}");
                    ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "CurrencySymbol").Result;
                    await Shell.Current.GoToAsync(nameof(ErrorPage),
                        new Dictionary<string, object>
                        {
                            ["Error"] = Error
                        });
                }
            }
        }

        

        [RelayCommand]
        private void CurrencySymbolSelected(lut_CurrencySymbol item)
        {
            SelectedCurrencySymbol = item;
            SearchVisible = false;
            CurrencySearchResults = null;
        }

        [RelayCommand]
        private async Task CloseSettings(object obj)
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.MainPage.ShowPopup(PopUp);
            }

            await Task.Delay(500);

            await Shell.Current.GoToAsync($"..");
        }

        public async Task UpdateTime()
        {
            await Task.Delay(1);
            DateTime CurrentDateTime = DateTime.UtcNow.AddHours(SelectedTimeZone.TimeZoneUTCOffset);
            CurrentTime = CurrentDateTime.ToString(TimeCultureInfo.DateTimeFormat.LongDatePattern, CultureInfo.InvariantCulture);
        }

        [RelayCommand]
        private async Task SaveBudgetSettings()
        {

            bool EditBudget = await Application.Current.MainPage.DisplayAlert($"Update settings?", $"Are you sure you want to update your budgets settings?", "Yes", "No");
            if (EditBudget)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Task.Delay(100);

                try
                {
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

                }
                catch (Exception ex)
                {
                    ErrorLog Error = _pt.HandleCatchedException(ex, "EditBudgetSettings", "SaveSettings").Result;
                    await Shell.Current.GoToAsync(nameof(ErrorPage),
                        new Dictionary<string, object>
                        {
                            ["Error"] = Error
                        });
                }

                await Shell.Current.GoToAsync($"///{nameof(MainPage)}?SnackBar=BudgetSettingsUpdated&SnackID=0");

            }

        }

        [RelayCommand]
        private async Task DeleteBudget()
        {
            var BudgetName = await Application.Current.MainPage.DisplayPromptAsync($"Are you sure you want to delete {App.DefaultBudget.BudgetName} budget?", $"Deleting the budget is permanent, enter the budget name to delete the budget", "Ok", "Cancel");
            if (BudgetName != null)
            {
                if (string.Equals(BudgetName, App.DefaultBudget.BudgetName, StringComparison.OrdinalIgnoreCase))
                {
                    await Task.Delay(100);

                    string result = await _ds.DeleteBudget(App.DefaultBudgetID, App.UserDetails.UserID);
                    if (result == "LastBudget")
                    {
                        await Application.Current.MainPage.DisplayAlert($"You can't delete this!", $"You can't delete this budget as it is your last budget and you must have at least one budget on the app", "Ok");

                    }
                    else if (result == "SharedBudget")
                    {
                        await Application.Current.MainPage.DisplayAlert($"This is a shared budget!", $"You can't delete a budget that you didn't create, someone kindly shared it with you so don't try and delete it", "Ok");

                    }
                    else
                    {
                        List<Budgets> Budgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "EditBudgetSettings");

                        App.DefaultBudgetID = Budgets[0].BudgetID;
                        App.DefaultBudget = Budgets[0];
                        BudgetSettingValues Settings = _ds.GetBudgetSettingsValues(App.DefaultBudgetID).Result;
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
                        await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

                        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                    }
                    
                }

            }
        }

        partial void OnIsBorrowPayChanged(bool oldValue, bool newValue)
        {
            if(oldValue)
            {
                CheckIsBorrowPay();
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
    }
}
