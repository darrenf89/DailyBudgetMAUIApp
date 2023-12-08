using System.ComponentModel.DataAnnotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Transactions : ObservableObject
    {
        [ObservableProperty]
        public int _transactionID;
        [ObservableProperty]
        public bool _isSpendFromSavings;
        [ObservableProperty]
        public int? _savingID;
        [ObservableProperty]
        public string? _savingName;
        [ObservableProperty]
        public DateTime? _transactionDate;
        [ObservableProperty]
        public DateTime? _whenAdded  = DateTime.UtcNow;
        public bool _isIncome;
        [ObservableProperty]
        public decimal? _transactionAmount;
        [ObservableProperty]
        public string? _category;
        [ObservableProperty]
        public string? _payee;
        [ObservableProperty]
        public string? _notes;
        [ObservableProperty]
        public int? _categoryID;
        [ObservableProperty]
        public bool _isTransacted;
        [ObservableProperty]
        public string? _savingsSpendType;
        [ObservableProperty]
        public string _stage;
        [ObservableProperty]
        public string _eventType;
    }
}
