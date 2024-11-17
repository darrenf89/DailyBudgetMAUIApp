using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Transactions : ObservableObject
    {
        [ObservableProperty]
        public int  transactionID;
        [ObservableProperty]
        public bool  isSpendFromSavings;
        [ObservableProperty]
        public int?  savingID;
        [ObservableProperty]
        public string?  savingName;
        [ObservableProperty]
        public DateTime?  transactionDate;
        [ObservableProperty]
        public DateTime?  whenAdded  = DateTime.UtcNow;
        [ObservableProperty]
        public bool  isIncome;
        [ObservableProperty]
        public decimal?  transactionAmount;
        [ObservableProperty]
        public string?  category;
        [ObservableProperty]
        public string?  payee;
        [ObservableProperty]
        public string?  notes;
        [ObservableProperty]
        public int?  categoryID;
        [ObservableProperty]
        public int?  accountID;
        [ObservableProperty]
        public bool  isTransacted;
        [ObservableProperty]
        public string?  savingsSpendType;
        [ObservableProperty]
        public string  stage;
        [ObservableProperty]
        public string  eventType;
        [ObservableProperty]
        public decimal?  runningTotal;
        [ObservableProperty]
        public bool  isVisible;
    }
}
