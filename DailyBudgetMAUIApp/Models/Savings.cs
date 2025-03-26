using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;



namespace DailyBudgetMAUIApp.Models
{
    public partial class Savings : ObservableObject
    {
        [ObservableProperty]
        private string? savingsType;
        [ObservableProperty]
        private string? savingsName;
        [ObservableProperty]
        private decimal? currentBalance = 0;
        [ObservableProperty]
        private DateTime lastUpdatedDate = DateTime.UtcNow;
        [ObservableProperty]
        private DateTime? goalDate = null;
        [ObservableProperty]
        private decimal? lastUpdatedValue;
        [ObservableProperty]
        private bool isSavingsClosed = false;
        [ObservableProperty]
        private decimal? savingsGoal = 0;
        [ObservableProperty]
        private bool canExceedGoal;
        [ObservableProperty]
        private bool isDailySaving;
        [ObservableProperty]
        private bool isRegularSaving;
        [ObservableProperty]
        private decimal? regularSavingValue;
        [ObservableProperty]
        private decimal? periodSavingValue;
        [ObservableProperty]
        private bool isAutoComplete;
        [ObservableProperty]
        private bool isTopUp;
        [ObservableProperty]
        private string ddlSavingsPeriod;
        [ObservableProperty]
        private int savingID = 0;
    }

}
