using System.ComponentModel.DataAnnotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class FilterModel : ObservableObject
    {
        [ObservableProperty]
        public DateFilter? _dateFilter;
        [ObservableProperty]
        public List<string>? _transactionEventTypeFilter;
        [ObservableProperty]
        public List<string>? _payeeFilter;
        [ObservableProperty]
        public List<int>? _categoryFilter;
        [ObservableProperty]
        public List<int>? _savingFilter;
    }

    public partial class DateFilter : ObservableObject
    {
        [ObservableProperty]
        public DateTime? _dateFrom;
        [ObservableProperty]
        public DateTime? _dateTo;
    }
}
