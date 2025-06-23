using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Bills : ObservableObject
    {
        [ObservableProperty]
        public partial int BillID { get; set; }

        [ObservableProperty]
        public partial string? BillName { get; set; }

        [ObservableProperty]
        public partial string? BillType { get; set; }

        [ObservableProperty]
        public partial int? BillValue { get; set; }

        [ObservableProperty]
        public partial string? BillDuration { get; set; }

        [ObservableProperty]
        public partial decimal? BillAmount { get; set; }

        [ObservableProperty]
        public partial DateTime? BillDueDate { get; set; }

        [ObservableProperty]
        public partial decimal BillCurrentBalance { get; set; }

        [ObservableProperty]
        public partial decimal BillBalanceAtLastPayDay { get; set; }

        [ObservableProperty]
        public partial bool? IsRecuring { get; set; }

        [ObservableProperty]
        public partial DateTime LastUpdatedDate { get; set; }

        [ObservableProperty]
        public partial bool IsClosed { get; set; }

        [ObservableProperty]
        public partial decimal? RegularBillValue { get; set; }

        [ObservableProperty]
        public partial string BillPayee { get; set; }

        [ObservableProperty]
        public partial string? Category { get; set; } = "";

        [ObservableProperty]
        public partial int? CategoryID { get; set; } = 0;

        [ObservableProperty]
        public partial int? AccountID { get; set; }

    }
}
