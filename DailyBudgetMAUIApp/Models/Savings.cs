using System.ComponentModel.DataAnnotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class Savings : ObservableObject
    {
        [ObservableProperty]
        private int _savingID;
        [ObservableProperty]
        private string? _savingsType;
        [ObservableProperty]
        private string? _savingsName;
        [ObservableProperty]
        private decimal? _currentBalance  = 0;
        [ObservableProperty]
        private DateTime _lastUpdatedDate = DateTime.Now;
        [ObservableProperty]
        private DateTime? _goalDate = null;
        [ObservableProperty]
        private decimal? _lastUpdatedValue;
        [ObservableProperty]
        private bool _isSavingsClosed = false;
        [ObservableProperty]
        private decimal? _savingsGoal = 0;
        [ObservableProperty]
        private bool _canExceedGoal;
        [ObservableProperty]
        private bool _isDailySaving;
        [ObservableProperty]
        private bool _isRegularSaving;
        [ObservableProperty]
        private decimal? _regularSavingValue;
        [ObservableProperty]
        private decimal? _periodSavingValue;
        [ObservableProperty]
        private bool _isAutoComplete;
        [ObservableProperty]
        private string _ddlSavingsPeriod;

    }
}
