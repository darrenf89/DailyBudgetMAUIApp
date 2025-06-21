using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;


namespace DailyBudgetMAUIApp.Models
{
    public partial class Budgets : ObservableObject
    {
        [ObservableProperty]
        public partial int  BudgetID { get; set; }
        [ObservableProperty]
        public partial string?  BudgetName { get; set; }
        [ObservableProperty]
        public partial DateTime BudgetCreatedOn { get; set; } = DateTime.UtcNow;
        [ObservableProperty]
        public partial decimal?  BankBalance  { get; set; }
        [ObservableProperty]
        public partial decimal?  MoneyAvailableBalance  { get; set; }
        [ObservableProperty]
        public partial decimal?  LeftToSpendBalance  { get; set; }        
        [ObservableProperty]
        public partial decimal? PlusStashSpendBalance { get; set; }
        [ObservableProperty]
        public partial DateTime?  NextIncomePayday  { get; set; }
        [ObservableProperty]
        public partial DateTime?  NextIncomePaydayCalculated  { get; set; }
        [ObservableProperty]
        public partial decimal?  PaydayAmount  { get; set; }
        [ObservableProperty]
        public partial string?  PaydayType  { get; set; }
        [ObservableProperty]
        public partial int?  PaydayValue  { get; set; }
        [ObservableProperty]
        public partial string?  PaydayDuration  { get; set; }
        [ObservableProperty]
        public partial bool  IsCreated  { get; set; }
        [ObservableProperty]
        public partial bool IsMultipleAccounts { get; set; }
        [ObservableProperty]
        public partial DateTime  LastUpdated  { get; set; }
        [ObservableProperty]
        public partial List<IncomeEvents> IncomeEvents { get; set; } = new List<IncomeEvents>();
        [ObservableProperty]
        public partial List<Savings> Savings { get; set; } = new List<Savings>();
        [ObservableProperty]
        public partial List<Transactions> Transactions { get; set; } = new List<Transactions>();
        [ObservableProperty]
        public partial List<Categories> Categories { get; set; } = new List<Categories>();
        [ObservableProperty]
        public partial List<Bills> Bills { get; set; } = new List<Bills>();
        [ObservableProperty]
        public partial List<PayPeriodStats> PayPeriodStats { get; set; } = new List<PayPeriodStats>();
        [ObservableProperty]
        public partial List<BudgetHstoryLastPeriod> BudgetHistory { get; set; } = new List<BudgetHstoryLastPeriod>();
        [ObservableProperty]
        public partial List<BankAccounts>? BankAccounts { get; set; }
        [ObservableProperty]
        public partial string?  CurrencyType  { get; set; }
        [ObservableProperty]
        public partial int? AproxDaysBetweenPay { get; set; } = 30;
        [ObservableProperty]
        public partial DateTime BudgetValuesLastUpdated { get; set; } = DateTime.UtcNow;
        [ObservableProperty]
        public partial decimal  DailySavingOutgoing  { get; set; }
        [ObservableProperty]
        public partial decimal  DailyBillOutgoing  { get; set; }
        [ObservableProperty]
        public partial decimal  LeftToSpendDailyAmount  { get; set; }
        [ObservableProperty]
        public partial decimal?  StartDayDailyAmount  { get; set; }
        [ObservableProperty]
        public partial ErrorClass? Error { get; set; } = null;
        [ObservableProperty]
        public partial int Stage { get; set; } = 1;
        [ObservableProperty]
        public partial int SharedUserID { get; set; } = 0;
        [ObservableProperty]
        public partial bool  IsSharedValidated { get; set; }
        [ObservableProperty]
        public partial string BudgetType { get; set; } = "Basic";
        [ObservableProperty]
        public partial AccountInfo AccountInfo { get; set; } = new AccountInfo();
        [ObservableProperty]
        public partial bool IsBorrowPay { get; set; } = true;
        [ObservableProperty]
        public partial decimal CurrentActiveIncome { get; set; } = 0;

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
        public int NumberPendingQuickTransactions { get; set; }

    }

    public partial class EnvelopeStats : ObservableObject
    {
        [ObservableProperty]
        public partial int NumberOfEnvelopes { get; set; }
        [ObservableProperty]
        public partial string  EnvelopeTotalString { get; set; }
        [ObservableProperty]
        public partial string  EnvelopeCurrentString { get; set; }
        [ObservableProperty]
        public partial string  AmountPerDayString { get; set; }
        [ObservableProperty]
        public partial decimal  EnvelopeTotal { get; set; }
        [ObservableProperty]
        public partial decimal  EnvelopeCurrent { get; set; }
        [ObservableProperty]
        public partial decimal  AmountPerDay { get; set; }
        [ObservableProperty]
        public partial int  DaysLeftToSpend { get; set; }

        public EnvelopeStats(List<Savings> Savings)
        {
            foreach(Savings Saving in Savings)
            {
                if(!Saving.IsRegularSaving && !Saving.IsSavingsClosed)
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
