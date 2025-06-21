using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace DailyBudgetMAUIApp.Models
{
    public partial class FilterModel : ObservableObject
    {
        [ObservableProperty]
        public partial DateFilter? DateFilter { get; set; }

        [ObservableProperty]
        public partial List<string>? TransactionEventTypeFilter { get; set; }

        [ObservableProperty]
        public partial List<string>? PayeeFilter { get; set; }

        [ObservableProperty]
        public partial List<int>? CategoryFilter { get; set; }

        [ObservableProperty]
        public partial List<int>? SavingFilter { get; set; }
    }

    public partial class DateFilter : ObservableObject
    {
        [ObservableProperty]
        public partial DateTime? DateFrom { get; set; }

        [ObservableProperty]
        public partial DateTime? DateTo { get; set; }
    }
}
