using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class PayPeriodStats : ObservableObject
    {
        [ObservableProperty]
        public int  payPeriodID;
        [ObservableProperty]
        public bool  isCurrentPeriod;
        [ObservableProperty]
        public DateTime  startDate;
        [ObservableProperty]
        public DateTime  endDate;
        [ObservableProperty]
        public int  durationOfPeriod;
        [ObservableProperty]
        public decimal  savingsToDate;
        [ObservableProperty]
        public decimal  billsToDate;
        [ObservableProperty]
        public decimal  incomeToDate;
        [ObservableProperty]
        public decimal  spendToDate;
        [ObservableProperty]
        public decimal?  startLtSDailyAmount;
        [ObservableProperty]
        public decimal?  startLtSPeiordAmount;
        [ObservableProperty]
        public decimal?  startBBPeiordAmount;
        [ObservableProperty]
        public decimal?  startMaBPeiordAmount;
        [ObservableProperty]
        public decimal?  endLtSDailyAmount;
        [ObservableProperty]
        public decimal?  endLtSPeiordAmount;
        [ObservableProperty]
        public decimal?  endBBPeiordAmount;
        [ObservableProperty]
        public decimal?  endMaBPeiordAmount;

    }
}
