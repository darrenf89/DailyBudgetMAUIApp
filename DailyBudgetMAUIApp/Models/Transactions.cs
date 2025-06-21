using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Transactions : ObservableObject
    {
        [ObservableProperty]
        public partial int TransactionID { get; set; }

        [ObservableProperty]
        public partial bool IsSpendFromSavings { get; set; }

        [ObservableProperty]
        public partial int? SavingID { get; set; }

        [ObservableProperty]
        public partial string? SavingName { get; set; }

        [ObservableProperty]
        public partial DateTime? TransactionDate { get; set; }

        [ObservableProperty]
        public partial DateTime? WhenAdded { get; set; } = DateTime.UtcNow;

        [ObservableProperty]
        public partial bool IsIncome { get; set; }

        [ObservableProperty]
        public partial bool IsQuickTransaction { get; set; }

        [ObservableProperty]
        public partial decimal? TransactionAmount { get; set; }

        [ObservableProperty]
        public partial string? Category { get; set; }

        [ObservableProperty]
        public partial string? Payee { get; set; }

        [ObservableProperty]
        public partial string? Notes { get; set; }

        [ObservableProperty]
        public partial int? CategoryID { get; set; }

        [ObservableProperty]
        public partial int? AccountID { get; set; }

        [ObservableProperty]
        public partial bool IsTransacted { get; set; }

        [ObservableProperty]
        public partial string? SavingsSpendType { get; set; }

        [ObservableProperty]
        public partial string Stage { get; set; }

        [ObservableProperty]
        public partial string EventType { get; set; }

        [ObservableProperty]
        public partial decimal? RunningTotal { get; set; }

        [ObservableProperty]
        public partial bool IsVisible { get; set; }
    }
}
