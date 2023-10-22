using System.ComponentModel.DataAnnotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Bills : ObservableObject
    {
        [ObservableProperty]
        private int _billID;
        [ObservableProperty]
        public string? _billName;
        [ObservableProperty]
        public string? _billType;
        [ObservableProperty]
        public int? _billValue;
        [ObservableProperty]
        public string? _billDuration;
        [ObservableProperty]
        public decimal? _billAmount;
        [ObservableProperty]
        public DateTime? _billDueDate;
        [ObservableProperty]
        public decimal _billCurrentBalance;
        [ObservableProperty]
        public bool _isRecuring;
        [ObservableProperty]
        public DateTime _lastUpdatedDate;
        [ObservableProperty]
        public bool _isClosed;
        [ObservableProperty]
        public decimal? _regularBillValue;


    }

    

}
