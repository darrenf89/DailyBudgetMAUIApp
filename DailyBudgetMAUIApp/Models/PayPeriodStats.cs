using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyBudgetMAUIApp.Models
{
    public partial class PayPeriodStats : ObservableObject
    {
        [ObservableProperty]
        public int _payPeriodID;
        [ObservableProperty]
        public bool _isCurrentPeriod;
        [ObservableProperty]
        public DateTime _startDate;
        [ObservableProperty]
        public DateTime _endDate;
        [ObservableProperty]
        public int _durationOfPeriod;
        [ObservableProperty]
        public decimal _savingsToDate;
        [ObservableProperty]
        public decimal _billsToDate;
        [ObservableProperty]
        public decimal _incomeToDate;
        [ObservableProperty]
        public decimal _spendToDate;
        [ObservableProperty]
        public decimal? _startLtSDailyAmount;
        [ObservableProperty]
        public decimal? _startLtSPeiordAmount;
        [ObservableProperty]
        public decimal? _startBBPeiordAmount;
        [ObservableProperty]
        public decimal? _startMaBPeiordAmount;
        [ObservableProperty]
        public decimal? _endLtSDailyAmount;
        [ObservableProperty]
        public decimal? _endLtSPeiordAmount;
        [ObservableProperty]
        public decimal? _endBBPeiordAmount;
        [ObservableProperty]
        public decimal? _endMaBPeiordAmount;

    }
}
