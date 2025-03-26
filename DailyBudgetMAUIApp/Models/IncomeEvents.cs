using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class IncomeEvents : ObservableObject
    {
        [ObservableProperty]
        private int  incomeEventID;
        [ObservableProperty]
        private decimal  incomeAmount;
        [ObservableProperty]
        private string  incomeName  = "";
        [ObservableProperty]
        private DateTime  incomeActiveDate  = DateTime.UtcNow;
        [ObservableProperty]
        private DateTime  dateOfIncomeEvent  = DateTime.UtcNow;
        [ObservableProperty]
        private bool  isRecurringIncome;
        [ObservableProperty]
        private string?  recurringIncomeType;
        [ObservableProperty]
        private int?  recurringIncomeValue;
        [ObservableProperty]
        private string?  recurringIncomeDuration;
        [ObservableProperty]
        private bool  isClosed;
        [ObservableProperty]
        private bool?  isInstantActive;
        [ObservableProperty]
        private bool?  isIncomeAddedToBalance  = false;
        [ObservableProperty]
        public int? accountID;        

    }
}
