using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Handlers;
using System.Collections.Generic;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Payees : ObservableObject, IIndexable
    {
        [ObservableProperty]
        public partial string Payee { get; set; }

        [ObservableProperty]
        public partial decimal PayeeSpendAllTime { get; set; }

        [ObservableProperty]
        public partial decimal PayeeSpendPayPeriod { get; set; }

        [ObservableProperty]
        public partial List<SpendPeriods> PayeeSpendPeriods { get; set; } = new List<SpendPeriods>();

        [ObservableProperty]
        public partial bool IsEditMode { get; set; } = false;

        public int Index { get; set; }
    }
}
