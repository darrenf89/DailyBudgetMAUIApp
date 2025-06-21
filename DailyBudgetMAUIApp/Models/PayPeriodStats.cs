using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DailyBudgetMAUIApp.Models
{
    public partial class PayPeriodStats : ObservableObject
    {
        [ObservableProperty]
        public partial int PayPeriodID { get; set; }

        [ObservableProperty]
        public partial bool IsCurrentPeriod { get; set; }

        [ObservableProperty]
        public partial DateTime StartDate { get; set; }

        [ObservableProperty]
        public partial DateTime EndDate { get; set; }

        [ObservableProperty]
        public partial int DurationOfPeriod { get; set; }

        [ObservableProperty]
        public partial decimal SavingsToDate { get; set; }

        [ObservableProperty]
        public partial decimal BillsToDate { get; set; }

        [ObservableProperty]
        public partial decimal IncomeToDate { get; set; }

        [ObservableProperty]
        public partial decimal SpendToDate { get; set; }

        [ObservableProperty]
        public partial decimal? StartLtSDailyAmount { get; set; }

        [ObservableProperty]
        public partial decimal? StartLtSPeiordAmount { get; set; }

        [ObservableProperty]
        public partial decimal? StartBBPeiordAmount { get; set; }

        [ObservableProperty]
        public partial decimal? StartMaBPeiordAmount { get; set; }

        [ObservableProperty]
        public partial decimal? EndLtSDailyAmount { get; set; }

        [ObservableProperty]
        public partial decimal? EndLtSPeiordAmount { get; set; }

        [ObservableProperty]
        public partial decimal? EndBBPeiordAmount { get; set; }

        [ObservableProperty]
        public partial decimal? EndMaBPeiordAmount { get; set; }
    }
}
