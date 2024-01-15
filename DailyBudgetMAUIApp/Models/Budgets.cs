using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Globalization;


namespace DailyBudgetMAUIApp.Models
{
    public partial class Budgets : ObservableObject
    {
        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private string? _budgetName;
        [ObservableProperty]
        private DateTime _budgetCreatedOn = DateTime.UtcNow;
        [ObservableProperty]
        private decimal? _bankBalance ;
        [ObservableProperty]
        private decimal? _moneyAvailableBalance ;
        [ObservableProperty]
        private decimal? _leftToSpendBalance ;
        [ObservableProperty]
        private DateTime? _nextIncomePayday ;
        [ObservableProperty]
        private DateTime? _nextIncomePaydayCalculated ;
        [ObservableProperty]
        private decimal? _paydayAmount ;
        [ObservableProperty]
        private string? _paydayType ;
        [ObservableProperty]
        private int? _paydayValue ;
        [ObservableProperty]
        private string? _paydayDuration ;
        [ObservableProperty]
        private bool _isCreated ;
        [ObservableProperty]
        private DateTime _lastUpdated ;
        [ObservableProperty]
        private List<IncomeEvents> _incomeEvents  = new List<IncomeEvents>();
        [ObservableProperty]
        private List<Savings> _savings  = new List<Savings>();
        [ObservableProperty]
        private List<Transactions> _transactions  = new List<Transactions>();
        [ObservableProperty]
        private List<Categories> _categories  = new List<Categories>();
        [ObservableProperty]
        private List<Bills> _bills  = new List<Bills>();
        [ObservableProperty]
        private List<PayPeriodStats> _payPeriodStats  = new List<PayPeriodStats>();
        [ObservableProperty]
        private List<BudgetHstoryLastPeriod> _budgetHistory  = new List<BudgetHstoryLastPeriod>();
        [ObservableProperty]
        private string? _currencyType ;
        [ObservableProperty]
        private int? _aproxDaysBetweenPay  = 30;
        [ObservableProperty]
        private DateTime _budgetValuesLastUpdated  = DateTime.UtcNow;
        [ObservableProperty]
        private decimal _dailySavingOutgoing ;
        [ObservableProperty]
        private decimal _dailyBillOutgoing ;
        [ObservableProperty]
        private decimal _leftToSpendDailyAmount ;
        [ObservableProperty]
        private decimal? _startDayDailyAmount ;
        [ObservableProperty]
        private ErrorClass? _error  = null;
        [ObservableProperty]
        private int _stage = 1;
        [ObservableProperty]
        private int _sharedUserID = 0;
        [ObservableProperty]
        private bool _isSharedValidated;
        [ObservableProperty]
        private string _budgetType = "Basic";
        [ObservableProperty]
        private AccountInfo _accountInfo = new AccountInfo();
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
        private int _numberOfEnvelopes;
        [ObservableProperty]
        private string _envelopeTotalString;
        [ObservableProperty]
        private string _envelopeCurrentString;
        [ObservableProperty]
        private string _amountPerDayString;
        [ObservableProperty]
        private decimal _envelopeTotal;
        [ObservableProperty]
        private decimal _envelopeCurrent;
        [ObservableProperty]
        private decimal _amountPerDay;
        [ObservableProperty]
        private int _daysLeftToSpend;

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
