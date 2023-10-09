using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Xml.Linq;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    public partial class CreateNewBudgetViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private BudgetSettings _budgetSettings;
        [ObservableProperty]
        private Budgets _budget;
        [ObservableProperty]
        private string _budgetName;
        [ObservableProperty]
        private string _stage = "Budget Settings";
        public double StageWidth { get; }
        [ObservableProperty]
        private List<lut_CurrencySymbol> _currencySearchResults;
        [ObservableProperty]
        private lut_CurrencySymbol _selectedCurrencySymbol;
        [ObservableProperty]
        private bool _searchVisible = false;
        [ObservableProperty]
        private List<lut_CurrencyPlacement> _currencyPlacements;
        [ObservableProperty]
        private lut_CurrencyPlacement _selectedCurrencyPlacement;
        [ObservableProperty]
        private List<lut_DateFormat> _dateFormats;
        [ObservableProperty]
        private lut_DateFormat _selectedDateFormats;
        [ObservableProperty]
        private List<lut_NumberFormat> _numberFormats;
        [ObservableProperty]
        private lut_NumberFormat _selectedNumberFormats;

        public CreateNewBudgetViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
            StageWidth = (((DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 52) / 5);
            CurrencyPlacements = _ds.GetCurrencyPlacements("").Result;
            DateFormats = _ds.GetDateFormatsByString("").Result;
            NumberFormats = _ds.GetNumberFormats().Result;
        }

        [ICommand]
        async void ChangeBudgetName()
        {
            try
            {
                string Description = "Every budget needs a name, let us know how you'd like your budget to be known so we can use this to identify it for you in the future.";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Budget Name", Description, DescriptionSub, "Enter a budget name!", Budget.BudgetName , new PopUpPageSingleInputViewModel());
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    Budget.BudgetName = (string)result;
                }

                SaveStage("Budget Name");

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

        [ICommand]
        async void CurrencySearch(string query)
        {
            try
            {            
                CurrencySearchResults = _ds.GetCurrencySymbols(query).Result;
            }
            catch (Exception ex)
            {
                if(ex.Message == "One or more errors occurred. (No currencies found)")
                {
                    lut_CurrencySymbol cs = new lut_CurrencySymbol();
                    cs._code = "No results please, try again!";
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

        [ICommand]
        private void CurrencySymbolSelected(lut_CurrencySymbol item)
        {
            SelectedCurrencySymbol = item;
            SearchVisible = false;
            CurrencySearchResults = null;
        }

        [ICommand]
        async void ContinueSettings()
        {
            SaveStage("Budget Settings");
        }

        [ICommand]
        async void ContinueBudgetDetails()
        {
            SaveStage("Budget Details");
        }

        private async void SaveStage(string CurrentStage)
        {
            try
            {
                List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                switch (CurrentStage)
                {
                case "Budget Name":

                    PatchDoc BudgetName = new PatchDoc
                    {
                        op = "replace",
                        path = "/BudgetName",
                        value = Budget.BudgetName
                    };

                    BudgetUpdate.Add(BudgetName);
                    await _ds.PatchBudget(BudgetID, BudgetUpdate);
                    break;

                case "Budget Settings":

                    BudgetSettings.CurrencyPattern = SelectedCurrencyPlacement.Id;
                    BudgetSettings.CurrencySymbol = SelectedCurrencySymbol.Id;
                    BudgetSettings.CurrencyDecimalDigits = SelectedNumberFormats.CurrencyDecimalDigitsID;
                    BudgetSettings.CurrencyDecimalSeparator = SelectedNumberFormats.CurrencyDecimalSeparatorID;
                    BudgetSettings.CurrencyGroupSeparator = SelectedNumberFormats.CurrencyGroupSeparatorID;
                    BudgetSettings.DateSeperator = SelectedDateFormats.DateSeperatorID;
                    BudgetSettings.ShortDatePattern = SelectedDateFormats.ShortDatePatternID;

                    await _ds.UpdateBudgetSettings(BudgetID, BudgetSettings);

                    App.CurrentSettings.IsUpdatedFlag = true;

                    break;
                case "Budget Details":

                    decimal Balance = (decimal)_pt.FormatCurrencyNumber(entBankBalance.Text);

                    if(Balance != Budget.BankBalance)
                    {
                        Budget.BankBalance = Balance;
                        PatchDoc BankBalancePatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/BankBalance",
                            value = Budget.BankBalance
                        };
                        BudgetUpdate.Add(BankBalancePatch);                      
                    }

                    //TODO: Recalcualte MAB
                    decimal MoneyAvailableBalance = 0;
                    if(MoneyAvailableBalance != Budget.MoneyAvailableBalance)
                    {
                        Budget.MoneyAvailableBalance = MoneyAvailableBalance;
                        PatchDoc MoneyAvailableBalancePatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/MoneyAvailableBalance",
                            value = Budget.MoneyAvailableBalance
                        };
                        BudgetUpdate.Add(MoneyAvailableBalancePatch);
                    }


                    //TODO: RECACLCULATE LTSB
                    decimal LeftToSpendBalance = 0;
                    if(LeftToSpendBalance != Budget.LeftToSpendBalance)
                    {
                        Budget.LeftToSpendBalance = LeftToSpendBalance;
                        PatchDoc LeftToSpendBalancePatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/LeftToSpendBalance",
                            value = Budget.LeftToSpendBalance
                        };
                        BudgetUpdate.Add(LeftToSpendBalancePatch);
                    }

                    NextIncomePayday = dtpckPayDay.Date;
                    if(NextIncomePayday != Budget.NextIncomePayday)
                    {
                        Budget.NextIncomePayday = NextIncomePayday;
                        PatchDoc NextIncomePaydayPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/NextIncomePayday",
                            value = Budget.NextIncomePayday
                        };
                        BudgetUpdate.Add(NextIncomePaydayPatch);
                    
                        Budget.NextIncomePaydayCalculated = NextIncomePayday;
                        PatchDoc NextIncomePaydayCalculatedPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/NextIncomePaydayCalculated",
                            value = Budget.NextIncomePaydayCalculated
                        };
                        BudgetUpdate.Add(NextIncomePaydayCalculatedPatch);
                    }


                    PatchDoc PayDayType = new PatchDoc
                    {
                        op = "replace",
                        path = "/PayDayType",
                        value = Budget.PayDayType
                    };
                    BudgetUpdate.Add(PayDayType);

                    string PayDayDuration;
                    int PayDayValue;
                    int AproxDaysBetweenPay;

                    if(Budget.PayDayType == "Everynth")
                    {
                        PayDayValue = (int)entEverynthValue.Text ?? 1;
                        PayDayDuration = pckrEverynthDuration.SelectedItem ?? "days";
                        int Duration = new int();
                        if (PaydayDuration == "days")
                        {
                            Duration = 1;
                        }
                        else if (PaydayDuration == "weeks")
                        {
                            Duration = 7;
                        }
                        else if (PaydayDuration == "years")
                        {
                            Duration = 365;
                        }
                        AproxDaysBetweenPay = Duration * PaydayValue;
                    }
                    else if (Budget.PayDayType == "WorkingDays")
                    {
                        PayDayValue = (int)entWorkingDaysValue.Text ?? 1;
                        PayDayDuration = "";
                        AproxDaysBetweenPay = _pt.GetNumberOfDaysLastWorkingDay(PayDayValue);
                    }
                    else if (Budget.PayDayType == "OfEveryMonth")
                    {
                        PayDayValue = (int)entOfEveryMonthValue.Text ?? 1;
                        PayDayDuration = "";

                        int year = DateTime.Now.Year;
                        int month = DateTime.Now.Month;
                        int days = DateTime.DaysInMonth(year, month);
                        AproxDaysBetweenPay = days;
                    }
                    else if (Budget.PayDayType == "LastOfTheMonth")
                    {
                        PayDayValue = 0;
                        PayDayDuration = pckrLastOfTheMonthDuration.SelectedItem ?? "Monday";

                        int dayNumber = ((int)Enum.Parse(typeof(DayOfWeek), PayDayDuration));
                        AproxDaysBetweenPay = _pt.GetNumberOfDaysLastDayOfWeek(dayNumber);
                    }
                    
                    if(AproxDaysBetweenPay != Budget.AproxDaysBetweenPay)
                    {
                        Budtet.AproxDaysBetweenPay = AproxDaysBetweenPay;
                        PatchDoc AproxDaysBetweenPayPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/AproxDaysBetweenPay",
                            value = Budget.AproxDaysBetweenPay
                        };
                        BudgetUpdate.Add(AproxDaysBetweenPayPatch);
                    }

                    if(PayDayDuration != Budget.PayDayDuration)
                    {
                        Budget.PayDayDuration = PayDayDuration;
                        PatchDoc PayDayDurationPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/PayDayDuration",
                            value = Budget.PayDayDuration
                        };
                        BudgetUpdate.Add(PayDayDurationPatch);
                    }

                    if(PayDayValue != Budget.PayDayValue)
                    {
                        Budget.PayDayValue = PayDayValue;
                        PatchDoc PayDayValuePatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/PayDayValue",
                            value = Budget.PayDayValue
                        };
                        BudgetUpdate.Add(PayDayValuePatch); 
                    }

                    decimal PayDayAmount = (decimal)_pt.FormatCurrencyNumber(entPayAmount.Text);
                    if(PaydayAmount != Budget.PaydayAmount)
                    {
                        Budget.PaydayAmount = PayDayAmount;
                        PatchDoc PaydayAmountPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/PaydayAmount",
                            value = Budget.PaydayAmount
                        };
                        BudgetUpdate.Add(PaydayAmountPatch);
                    }


                    TimeSpan t = Budget.NextIncomePayday - DateTime.UtcNow ?? new TimeSpan();
                    decimal LeftToSpendDailyAmount = Budget.BankBalance / t.Days;

                    if(LeftToSpendDailyAmount != Budget.LeftToSpendDailyAmount)
                    {
                        Budget.LeftToSpendDailyAmount = LeftToSpendDailyAmount;
                        PatchDoc LeftToSpendDailyAmountPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/LeftToSpendDailyAmount",
                            value = Budget.LeftToSpendDailyAmount
                        };
                        BudgetUpdate.Add(LeftToSpendDailyAmountPatch);
                    }


                    await _ds.PatchBudget(BudgetID, BudgetUpdate);

                    break;
                case "Budget Outgoings":

                    break;
                case "Budget Savings":

                    break;
                case "Budget Extra Income":

                    break;
                }                
            }
            catch (Exception ex)
            {
                ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "UpdateBudget").Result;
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }

        }
    }
}
