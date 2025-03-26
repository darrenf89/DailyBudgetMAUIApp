using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(NavigatedFrom), nameof(NavigatedFrom))]
    public partial class CreateNewBudgetViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private string? navigatedFrom;
        [ObservableProperty]
        private int budgetID;
        [ObservableProperty]
        private BudgetSettings budgetSettings;
        [ObservableProperty]
        private Budgets budget;        
        [ObservableProperty]
        private string budgetName;
        [ObservableProperty]
        private string _stage = "Budget Settings";
        public double StageWidth { get; }
        public double AcceptTermsWidth { get; }
        [ObservableProperty]
        private List<lut_CurrencySymbol> currencySearchResults;
        [ObservableProperty]
        private lut_CurrencySymbol selectedCurrencySymbol;
        [ObservableProperty]
        private bool searchVisible = false;
        [ObservableProperty]
        private List<lut_CurrencyPlacement> currencyPlacements;
        [ObservableProperty]
        private lut_CurrencyPlacement selectedCurrencyPlacement;
        [ObservableProperty]
        private List<lut_DateFormat> dateFormats;
        [ObservableProperty]
        private List<lut_BudgetTimeZone> timeZones;
        [ObservableProperty]
        private lut_DateFormat selectedDateFormats;
        [ObservableProperty]
        private List<lut_NumberFormat> numberFormats;
        [ObservableProperty]
        private lut_NumberFormat selectedNumberFormats;
        [ObservableProperty]
        private lut_BudgetTimeZone selectedTimeZone;
        [ObservableProperty]
        private bool isBorrowPay;

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
            TimeZones = _ds.GetBudgetTimeZones("").Result;

        }

        [RelayCommand]
        async void ChangeBudgetName()
        {
            try
            {
                string Description = "Every budget needs a name, let us know how you'd like your budget to be known so we can use this to identify it for you in the future.";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Budget Name", Description, DescriptionSub, "Enter a budget name!", Budget.BudgetName , new PopUpPageSingleInputViewModel());
                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    Budget.BudgetName = (string)result;
                }

                SaveStage("Budget Name");

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewBudget", "ChangeBudgetName");
            }
        }

        [RelayCommand]
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
                    cs.Code = "No results please, try again!";
                    CurrencySearchResults.Clear();
                    CurrencySearchResults.Add(cs);
                }
                else
                {
                    await _pt.HandleException(ex, "CreateNewBudget", "CurrencySearch");
                }
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

                var popup = new PopupInfo("Bill accrual", SubTitle, Info);
                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewBudget", "GoToBorrowPayVideos");
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
        async void ContinueSettings()
        {
            try
            {
                await SaveStage("Budget Settings");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewBudget", "ContinueSettings");
            }
        }

        partial void OnIsBorrowPayChanged(bool oldValue, bool newValue)
        {
            if (oldValue)
            {
                CheckIsBorrowPay();
            }
        }

        private async Task CheckIsBorrowPay()
        {
            bool result = await Shell.Current.DisplayAlert("Start paying outgoings each day?", "\nAre you sure you want to change the setting and start \"paying\" your outgoings each day?\n \nCareful! If you don't have the money put aside you might end up with no money left to spend.", "Yes", "Cancel");
            if (!result)
            {
                IsBorrowPay = true;
            }
        }

        public async Task SaveStage(string CurrentStage)
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
                BudgetSettings.TimeZone = SelectedTimeZone.TimeZoneID;

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

                if (IsBorrowPay != Budget.IsBorrowPay)
                {
                    UpdateBudgetFlag = true;
                    Budget.IsBorrowPay = IsBorrowPay;
                    PatchDoc IsBorrow = new PatchDoc
                    {
                        op = "replace",
                        path = "/IsBorrowPay",
                        value = Budget.IsBorrowPay
                    };
                    BudgetUpdate.Add(IsBorrow);
                }

                if (Balance != Budget.BankBalance)
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

                AproxDaysBetweenPay = _pt.CalculateBudgetDaysBetweenPay(Budget);

                if (AproxDaysBetweenPay != Budget.AproxDaysBetweenPay)
                {
                    UpdateBudgetFlag = true;
                    Budget.AproxDaysBetweenPay = AproxDaysBetweenPay;
                    PatchDoc AproxDaysBetweenPayPatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/AproxDaysBetweenPay",
                        value = Budget.AproxDaysBetweenPay
                    };
                    BudgetUpdate.Add(AproxDaysBetweenPayPatch);
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
                    PatchDoc IsCreated = new PatchDoc
                    {
                        op = "replace",
                        path = "/IsCreated",
                        value = true
                    };

                    BudgetUpdate.Add(IsCreated);

                    PatchDoc BudgetLastUpdated = new PatchDoc
                    {
                        op = "replace",
                        path = "/BudgetValuesLastUpdated",
                        value = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date
                    };

                    BudgetUpdate.Add(BudgetLastUpdated);

                    await _ds.PatchBudget(BudgetID, BudgetUpdate);
                break;
            }

            Budget = _ds.GetBudgetDetailsAsync(BudgetID, "Full").Result;

        }
    }
}
