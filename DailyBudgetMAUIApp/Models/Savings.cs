using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Globalization;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Savings : ObservableObject
    {
        [ObservableProperty]
        public partial string? SavingsType { get; set; }

        [ObservableProperty]
        public partial string? SavingsName { get; set; }

        [ObservableProperty]
        public partial decimal? CurrentBalance { get; set; } = 0;

        [ObservableProperty]
        public partial DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

        [ObservableProperty]
        public partial DateTime? GoalDate { get; set; } = null;

        [ObservableProperty]
        public partial decimal? LastUpdatedValue { get; set; }

        [ObservableProperty]
        public partial bool IsSavingsClosed { get; set; } = false;

        [ObservableProperty]
        public partial bool IsSavingsPaused { get; set; } = false;

        [ObservableProperty]
        public partial decimal? SavingsGoal { get; set; } = 0;

        [ObservableProperty]
        public partial bool CanExceedGoal { get; set; }

        [ObservableProperty]
        public partial bool IsDailySaving { get; set; }

        [ObservableProperty]
        public partial bool IsRegularSaving { get; set; }

        [ObservableProperty]
        public partial decimal? RegularSavingValue { get; set; }

        [ObservableProperty]
        public partial decimal? PeriodSavingValue { get; set; }

        [ObservableProperty]
        public partial bool IsAutoComplete { get; set; }

        [ObservableProperty]
        public partial bool IsTopUp { get; set; }

        [ObservableProperty]
        public partial string DdlSavingsPeriod { get; set; }

        [ObservableProperty]
        public partial int SavingID { get; set; } = 0;
    }
}
