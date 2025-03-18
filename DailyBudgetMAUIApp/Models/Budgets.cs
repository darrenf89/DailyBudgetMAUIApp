using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;


namespace DailyBudgetMAUIApp.Models
{
    public partial class Budgets : ObservableObject
    {
        [ObservableProperty]
        private int  budgetID;
        [ObservableProperty]
        private string?  budgetName;
        [ObservableProperty]
        private DateTime  budgetCreatedOn = DateTime.UtcNow;
        [ObservableProperty]
        private decimal?  bankBalance ;
        [ObservableProperty]
        private decimal?  moneyAvailableBalance ;
        [ObservableProperty]
        private decimal?  leftToSpendBalance ;        
        [ObservableProperty]
        private decimal? plusStashSpendBalance;
        [ObservableProperty]
        private DateTime?  nextIncomePayday ;
        [ObservableProperty]
        private DateTime?  nextIncomePaydayCalculated ;
        [ObservableProperty]
        private decimal?  paydayAmount ;
        [ObservableProperty]
        private string?  paydayType ;
        [ObservableProperty]
        private int?  paydayValue ;
        [ObservableProperty]
        private string?  paydayDuration ;
        [ObservableProperty]
        private bool  isCreated ;
        [ObservableProperty]
        private bool isMultipleAccounts;
        [ObservableProperty]
        private DateTime  lastUpdated ;
        [ObservableProperty]
        private List<IncomeEvents>  incomeEvents  = new List<IncomeEvents>();
        [ObservableProperty]
        private List<Savings>  savings  = new List<Savings>();
        [ObservableProperty]
        private List<Transactions>  transactions  = new List<Transactions>();
        [ObservableProperty]
        private List<Categories>  categories  = new List<Categories>();
        [ObservableProperty]
        private List<Bills>  bills  = new List<Bills>();
        [ObservableProperty]
        private List<PayPeriodStats>  payPeriodStats  = new List<PayPeriodStats>();
        [ObservableProperty]
        private List<BudgetHstoryLastPeriod>  budgetHistory  = new List<BudgetHstoryLastPeriod>();
        [ObservableProperty]
        private List<BankAccounts>? bankAccounts;
        [ObservableProperty]
        private string?  currencyType ;
        [ObservableProperty]
        private int?  aproxDaysBetweenPay  = 30;
        [ObservableProperty]
        private DateTime  budgetValuesLastUpdated  = DateTime.UtcNow;
        [ObservableProperty]
        private decimal  dailySavingOutgoing ;
        [ObservableProperty]
        private decimal  dailyBillOutgoing ;
        [ObservableProperty]
        private decimal  leftToSpendDailyAmount ;
        [ObservableProperty]
        private decimal?  startDayDailyAmount ;
        [ObservableProperty]
        private ErrorClass?  error  = null;
        [ObservableProperty]
        private int  stage = 1;
        [ObservableProperty]
        private int  sharedUserID = 0;
        [ObservableProperty]
        private bool  isSharedValidated;
        [ObservableProperty]
        private string  budgetType = "Basic";
        [ObservableProperty]
        private AccountInfo  accountInfo = new AccountInfo();
        [ObservableProperty]
        private bool  isBorrowPay = true;
        [ObservableProperty]
        private decimal  currentActiveIncome = 0;
        public string ChangeBudgetStringConvertor { get; set; }
        public bool IsBudgetNotSharedBudgetToBool { get; set; }
        public bool IsBudgetSharedBudgetToBool { get; set; }
        public bool LeftToSpendDailyAmountHideShow { get; set; }
        public bool LeftToSpendBalanceHideShow { get; set; }

        partial void OnBudgetNameChanged(string value)
        {
            UpdateSavingProgressBarMaxString();
        }

        partial void OnSharedUserIDChanged(int value)
        {
            UpdateSavingProgressBarMaxString();
            UpdateIsBudgetNotSharedBudgetToBool();
            UpdateIsBudgetSharedBudgetToBool();
        }

        partial void OnLastUpdatedChanged(DateTime value)
        {
            UpdateSavingProgressBarMaxString();
        }

        partial void OnIsSharedValidatedChanged(bool value)
        {
            UpdateIsBudgetNotSharedBudgetToBool();
            UpdateIsBudgetSharedBudgetToBool();

        }

        partial void OnLeftToSpendDailyAmountChanged(decimal value)
        {
            UpdateLeftToSpendDailyAmountHideShow();
        }

        partial void OnLeftToSpendBalanceChanged(decimal? value)
        {
            UpdateLeftToSpendDailyAmountHideShow();
            UpdateLeftToSpendBalanceHideShow();
        }

        private void UpdateSavingProgressBarMaxString()
        {
            if (App.UserDetails.UserID == this.SharedUserID)
            {
                ChangeBudgetStringConvertor = $"[Shared]{this.BudgetName} ({this.LastUpdated.ToString("dd MMM yy")})";
            }
            else
            {
                ChangeBudgetStringConvertor = $"{this.BudgetName} ({this.LastUpdated.ToString("dd MMM yy")})";
            }
        }

        private void UpdateIsBudgetNotSharedBudgetToBool()
        {

            if (this.IsSharedValidated && this.SharedUserID != 0)
            {
                IsBudgetNotSharedBudgetToBool = false;
            }
            else
            {
                if (App.IsPremiumAccount)
                {
                    IsBudgetNotSharedBudgetToBool = true;
                }
                else
                {
                    IsBudgetNotSharedBudgetToBool = false;
                }

            }
        }

        private void UpdateIsBudgetSharedBudgetToBool()
        {

            if (this.IsSharedValidated && this.SharedUserID != 0)
            {
                IsBudgetSharedBudgetToBool = true;
            }
            else
            {
                IsBudgetSharedBudgetToBool = false;
            }
        }

        private void UpdateLeftToSpendDailyAmountHideShow()
        {
            if (this.LeftToSpendDailyAmount <= 0 && this.LeftToSpendBalance > 0)
            {
                LeftToSpendDailyAmountHideShow = true;
            }
            else
            {
                LeftToSpendDailyAmountHideShow = false;
            }
        }

        private void UpdateLeftToSpendBalanceHideShow()
        {
            if (this.LeftToSpendBalance <= 0)
            {
                LeftToSpendBalanceHideShow = true;
            }
            else
            {
                LeftToSpendBalanceHideShow = false;
            }
        }
    }

    public class AccountInfo
    {
        public int BudgetShareRequestID { get; set; } = 0;
        public decimal TransactionValueToday { get; set; }
        public int NumberOfTransactionsToday { get; set; } = 0;
        public decimal TransactionValueThisPeriod { get; set; }
        public decimal IncomeThisPeriod { get; set; }
        public int NumberOfTransactions { get; set; }
        public int NumberOfBills { get; set; }
        public int NumberOfIncomeEvents { get; set; }
        public int NumberOfSavings { get; set; }
        public bool IsSupportOpen { get; set; }
        public int NumberUnreadMessages { get; set; }

    }

    public partial class EnvelopeStats : ObservableObject
    {
        [ObservableProperty]
        private int  numberOfEnvelopes;
        [ObservableProperty]
        private string  envelopeTotalString;
        [ObservableProperty]
        private string  envelopeCurrentString;
        [ObservableProperty]
        private string  amountPerDayString;
        [ObservableProperty]
        private decimal  envelopeTotal;
        [ObservableProperty]
        private decimal  envelopeCurrent;
        [ObservableProperty]
        private decimal  amountPerDay;
        [ObservableProperty]
        private int  daysLeftToSpend;

        public EnvelopeStats(List<Savings> Savings)
        {
            foreach(Savings Saving in Savings)
            {
                if(!Saving.IsRegularSaving)
                {
                    this.DaysLeftToSpend = (int)Math.Ceiling((Saving.GoalDate.GetValueOrDefault().Date - DateTime.Now.Date).TotalDays);

                    this.NumberOfEnvelopes += 1;
                    this.EnvelopeCurrent += Saving.CurrentBalance.GetValueOrDefault();
                    this.EnvelopeTotal += Saving.PeriodSavingValue.GetValueOrDefault();
                }
            }
            this.AmountPerDay = this.DaysLeftToSpend == 0 ? 0 : this.EnvelopeCurrent / this.DaysLeftToSpend;   

            this.EnvelopeTotalString = this.EnvelopeTotal.ToString("c", CultureInfo.CurrentCulture);
            this.EnvelopeCurrentString = this.NumberOfEnvelopes == 0 ? "You have no envelope savings!" : this.EnvelopeCurrent.ToString("c", CultureInfo.CurrentCulture);
            this.AmountPerDayString = this.AmountPerDay.ToString("c", CultureInfo.CurrentCulture);
        }
    }
}
