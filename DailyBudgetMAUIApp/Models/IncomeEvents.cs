using System.ComponentModel.DataAnnotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class IncomeEvents : ObservableObject
    {
        [ObservableProperty]
        private int _incomeEventID;
        [ObservableProperty]
        private decimal _incomeAmount;
        [ObservableProperty]
        private string _incomeName  = "";
        [ObservableProperty]
        private DateTime _incomeActiveDate  = DateTime.UtcNow;
        [ObservableProperty]
        private DateTime _dateOfIncomeEvent  = DateTime.UtcNow;
        [ObservableProperty]
        private bool _isRecurringIncome;
        [ObservableProperty]
        private string? _recurringIncomeType;
        [ObservableProperty]
        private int? _recurringIncomeValue;
        [ObservableProperty]
        private string? _recurringIncomeDuration;
        [ObservableProperty]
        private bool _isClosed;
        [ObservableProperty]
        private bool? _isInstantActive;
        [ObservableProperty]
        private bool? _isIncomeAddedToBalance  = false;
    }
}
