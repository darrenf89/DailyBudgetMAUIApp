using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class FilterModel : ObservableObject
    {
        [ObservableProperty]
        public DateFilter?  dateFilter;
        [ObservableProperty]
        public List<string>?  transactionEventTypeFilter;
        [ObservableProperty]
        public List<string>?  payeeFilter;
        [ObservableProperty]
        public List<int>?  categoryFilter;
        [ObservableProperty]
        public List<int>?  savingFilter;
    }

    public partial class DateFilter : ObservableObject
    {
        [ObservableProperty]
        public DateTime?  dateFrom;
        [ObservableProperty]
        public DateTime?  dateTo;
    }
}
