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
