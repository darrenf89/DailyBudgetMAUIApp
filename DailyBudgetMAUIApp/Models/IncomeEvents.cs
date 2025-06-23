using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DailyBudgetMAUIApp.Models
{
    public partial class IncomeEvents : ObservableObject
    {
        [ObservableProperty]
        public partial int IncomeEventID { get; set; }

        [ObservableProperty]
        public partial decimal IncomeAmount { get; set; }

        [ObservableProperty]
        public partial string IncomeName { get; set; } = "";

        [ObservableProperty]
        public partial DateTime IncomeActiveDate { get; set; } = DateTime.UtcNow;

        [ObservableProperty]
        public partial DateTime DateOfIncomeEvent { get; set; } = DateTime.UtcNow;

        [ObservableProperty]
        public partial bool IsRecurringIncome { get; set; }

        [ObservableProperty]
        public partial string? RecurringIncomeType { get; set; }

        [ObservableProperty]
        public partial int? RecurringIncomeValue { get; set; }

        [ObservableProperty]
        public partial string? RecurringIncomeDuration { get; set; }

        [ObservableProperty]
        public partial bool IsClosed { get; set; }

        [ObservableProperty]
        public partial bool? IsInstantActive { get; set; }

        [ObservableProperty]
        public partial bool? IsIncomeAddedToBalance { get; set; } = false;

        [ObservableProperty]
        public partial int? AccountID { get; set; }
    }
}
