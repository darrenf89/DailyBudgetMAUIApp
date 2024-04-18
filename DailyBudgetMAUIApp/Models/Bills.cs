using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class Bills : ObservableObject
    {
        [ObservableProperty]
        private int  billID;
        [ObservableProperty]
        public string?  billName;
        [ObservableProperty]
        public string?  billType;
        [ObservableProperty]
        public int?  billValue;
        [ObservableProperty]
        public string?  billDuration;
        [ObservableProperty]
        public decimal?  billAmount;
        [ObservableProperty]
        public DateTime?  billDueDate;
        [ObservableProperty]
        public decimal  billCurrentBalance;
        [ObservableProperty]
        public decimal  billBalanceAtLastPayDay;
        [ObservableProperty]
        public bool  isRecuring;
        [ObservableProperty]
        public DateTime  lastUpdatedDate;
        [ObservableProperty]
        public bool  isClosed;
        [ObservableProperty]
        public decimal?  regularBillValue;
        [ObservableProperty]
        public string  billPayee;
        [ObservableProperty]
        public string? category = "";
        [ObservableProperty]
        public int? categoryID = 0;
    }  
}
