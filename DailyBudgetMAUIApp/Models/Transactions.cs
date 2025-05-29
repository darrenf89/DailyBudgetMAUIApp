using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Transactions : ObservableObject
    {
        [ObservableProperty]
        private int  transactionID;
        [ObservableProperty]
        private bool  isSpendFromSavings;
        [ObservableProperty]
        private int?  savingID;
        [ObservableProperty]
        private string?  savingName;
        [ObservableProperty]
        private DateTime?  transactionDate;
        [ObservableProperty]
        private DateTime?  whenAdded  = DateTime.UtcNow;
        [ObservableProperty]
        private bool  isIncome;
        [ObservableProperty]
        private bool isQuickTransaction;
        [ObservableProperty]
        private decimal?  transactionAmount;
        [ObservableProperty]
        private string?  category;
        [ObservableProperty]
        private string?  payee;
        [ObservableProperty]
        private string?  notes;
        [ObservableProperty]
        private int?  categoryID;
        [ObservableProperty]
        private int?  accountID;
        [ObservableProperty]
        private bool  isTransacted;
        [ObservableProperty]
        private string?  savingsSpendType;
        [ObservableProperty]
        private string  stage;
        [ObservableProperty]
        private string  eventType;
        [ObservableProperty]
        private decimal?  runningTotal;
        [ObservableProperty]
        private bool  isVisible;
    }
}
