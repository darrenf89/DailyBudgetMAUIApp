using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class Budgets : ObservableObject
    {
        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private string? _budgetName;
        [ObservableProperty]
        private DateTime _budgetCreatedOn = DateTime.Now;
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
    }
}
