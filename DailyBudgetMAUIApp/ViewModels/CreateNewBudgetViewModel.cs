using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(NavigatedFrom), nameof(NavigatedFrom))]
    public partial class CreateNewBudgetViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private string? _navigatedFrom;
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
        public double AcceptTermsWidth { get; }
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

        public string PayDayTypeText { get; set; }
        public string PayAmountText { get; set; }
        public string BankBalanceText { get; set; }
        public DateTime PayDayDateValue { get; set; }
        public string EveryNthValue { get; set; }
        public string EveryNthDuration { get; set; }
        public string WorkingDaysValue { get; set; }
        public string OfEveryMonthValue { get; set; }
        public string LastOfTheMonthDuration {  get; set; }

        public string BillsYesNoSelect { get; set; } = "";
        public string IncomesYesNoSelect { get; set; } = "";
        public string SavingsYesNoSelect { get; set; } = "";
        public string IncomeYesNoSelect { get; set; } = "";


        public CreateNewBudgetViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
            StageWidth = (((DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 52) / 5);
            AcceptTermsWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 80;
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

        public async void SaveStage(string CurrentStage)
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

                    if(Budget.Stage == 2)
                    {
                        PatchDoc BudgetStage2 = new PatchDoc
                        {
                            op = "replace",
                            path = "/Stage",
                            value = Budget.Stage
                        };

                        BudgetUpdate.Add(BudgetStage2);
                        await _ds.PatchBudget(BudgetID, BudgetUpdate);
                    }


                    break;
                case "Budget Details":

                    bool UpdateBudgetFlag = false;

                    decimal Balance = (decimal)_pt.FormatCurrencyNumber(BankBalanceText);

                    if(Balance != Budget.BankBalance)
                    {
                        UpdateBudgetFlag = true;
                        Budget.BankBalance = Balance;
                        PatchDoc BankBalancePatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/BankBalance",
                            value = Budget.BankBalance
                        };
                        BudgetUpdate.Add(BankBalancePatch);                      
                    }

                    if(PayDayDateValue != Budget.NextIncomePayday)
                    {
                        UpdateBudgetFlag = true;
                        Budget.NextIncomePayday = PayDayDateValue;
                        PatchDoc NextIncomePaydayPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/NextIncomePayday",
                            value = Budget.NextIncomePayday
                        };
                        BudgetUpdate.Add(NextIncomePaydayPatch);
                    
                        Budget.NextIncomePaydayCalculated = PayDayDateValue;
                        PatchDoc NextIncomePaydayCalculatedPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/NextIncomePaydayCalculated",
                            value = Budget.NextIncomePaydayCalculated
                        };
                        BudgetUpdate.Add(NextIncomePaydayCalculatedPatch);
                    }

                    if(PayDayTypeText != Budget.PaydayType)
                    {
                        Budget.PaydayType = PayDayTypeText;
                        PatchDoc PayDayTypePatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/PayDayType",
                            value = Budget.PaydayType
                        };
                        BudgetUpdate.Add(PayDayTypePatch);
                    }


                    string PayDayDuration = "";
                    int PayDayValue = 0;
                    int AproxDaysBetweenPay = 0;

                    if(PayDayTypeText == "Everynth")
                    {
                        PayDayValue = Convert.ToInt32(EveryNthValue ?? "1");
                        PayDayDuration = EveryNthDuration ?? "days";

                        int Duration = new int();
                        if (PayDayDuration == "days")
                        {
                            Duration = 1;
                        }
                        else if (PayDayDuration == "weeks")
                        {
                            Duration = 7;
                        }
                        else if (PayDayDuration == "years")
                        {
                            Duration = 365;
                        }                        
                    }
                    else if (PayDayTypeText == "WorkingDays") 
                    {
                        PayDayValue = Convert.ToInt32(WorkingDaysValue ?? "1");
                        PayDayDuration = "";
                    }
                    else if (PayDayTypeText == "OfEveryMonth")
                    {
                        PayDayValue = Convert.ToInt32(OfEveryMonthValue ?? "1"); 
                        PayDayDuration = "";
                    }
                    else if (PayDayTypeText == "LastOfTheMonth")
                    {
                        PayDayValue = 0;
                        PayDayDuration = LastOfTheMonthDuration ?? "Monday";
                    }

                    AproxDaysBetweenPay = (Budget.NextIncomePayday.GetValueOrDefault().Date - DateTime.Now.Date).Days;

                    if (AproxDaysBetweenPay != Budget.AproxDaysBetweenPay)
                    {
                        UpdateBudgetFlag= true;
                        Budget.AproxDaysBetweenPay = AproxDaysBetweenPay;
                        PatchDoc AproxDaysBetweenPayPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/AproxDaysBetweenPay",
                            value = Budget.AproxDaysBetweenPay
                        };
                        BudgetUpdate.Add(AproxDaysBetweenPayPatch);
                    }

                    if(PayDayDuration != Budget.PaydayDuration)
                    {
                        Budget.PaydayDuration = PayDayDuration;
                        PatchDoc PayDayDurationPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/PayDayDuration",
                            value = Budget.PaydayDuration
                        };
                        BudgetUpdate.Add(PayDayDurationPatch);
                    }

                    if(PayDayValue != Budget.PaydayValue)
                    {
                        Budget.PaydayValue = PayDayValue;
                        PatchDoc PayDayValuePatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/PayDayValue",
                            value = Budget.PaydayValue
                        };
                        BudgetUpdate.Add(PayDayValuePatch); 
                    }

                    decimal PayDayAmount = (decimal)_pt.FormatCurrencyNumber(PayAmountText);
                    if(PayDayAmount != Budget.PaydayAmount)
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

                    if(Budget.Stage == 3)
                    {
                        PatchDoc BudgetStage3 = new PatchDoc
                        {
                            op = "replace",
                            path = "/Stage",
                            value = Budget.Stage
                        };

                        BudgetUpdate.Add(BudgetStage3);
                    }

                    if (BudgetUpdate.Count != 0)
                    {
                        await _ds.PatchBudget(BudgetID, BudgetUpdate);
                    }

                    if (UpdateBudgetFlag)
                    {
                        await _ds.UpdateBudgetValues(BudgetID);
                    }

                    break;
                case "Budget Outgoings":

                    if(Budget.Stage == 4)
                    {
                        PatchDoc BudgetIsCreated = new PatchDoc
                        {
                            op = "replace",
                            path = "/Stage",
                            value = Budget.Stage
                        };

                        BudgetUpdate.Add(BudgetIsCreated);
                        await _ds.PatchBudget(BudgetID, BudgetUpdate);
                    }
                    break;
                case "Budget Savings":

                    if(Budget.Stage == 5)
                    {
                        PatchDoc BudgetStage5 = new PatchDoc
                        {
                            op = "replace",
                            path = "/Stage",
                            value = Budget.Stage
                        };

                        BudgetUpdate.Add(BudgetStage5);
                        await _ds.PatchBudget(BudgetID, BudgetUpdate);
                    }
                    break;
                case "Budget Extra Income":

                    if(Budget.Stage == 6)
                    {
                        PatchDoc BudgetStage6 = new PatchDoc
                        {
                            op = "replace",
                            path = "/Stage",
                            value = Budget.Stage
                        };

                        BudgetUpdate.Add(BudgetStage6);
                        await _ds.PatchBudget(BudgetID, BudgetUpdate);
                    }

                    break;
                case "Finalise Budget":

                    await _ds.UpdateBudgetValues(BudgetID);
                    break;

                case "Create Budget":
                        PatchDoc BudgetStage = new PatchDoc
                        {
                            op = "replace",
                            path = "/IsCreated",
                            value = true
                        };

                        BudgetUpdate.Add(BudgetStage);
                        await _ds.PatchBudget(BudgetID, BudgetUpdate);
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
