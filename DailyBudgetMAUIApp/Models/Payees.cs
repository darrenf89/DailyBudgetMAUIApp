using CommunityToolkit.Mvvm.ComponentModel;

using DailyBudgetMAUIApp.Handlers;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Payees : ObservableObject, IIndexable
    {
        [ObservableProperty]
        public string  payee;
        [ObservableProperty]
        public decimal  payeeSpendAllTime;
        [ObservableProperty]
        public decimal  payeeSpendPayPeriod;
        [ObservableProperty]
        public List<SpendPeriods>  payeeSpendPeriods = new List<SpendPeriods>();
        [ObservableProperty]
        public bool  isEditMode = false;
        public int Index { get; set; }
    }
}
