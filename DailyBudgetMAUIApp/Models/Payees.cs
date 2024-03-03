using Microsoft.Maui.Layouts;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Handlers;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Payees : ObservableObject, IIndexable
    {
        [ObservableProperty]
        public string _payee;
        [ObservableProperty]
        public decimal _payeeSpendAllTime;
        [ObservableProperty]
        public decimal _payeeSpendPayPeriod;
        [ObservableProperty]
        public List<SpendPeriods> _payeeSpendPeriods = new List<SpendPeriods>();
        [ObservableProperty]
        public bool _isEditMode = false;
        public int Index { get; set; }
    }
}
