using System.ComponentModel.DataAnnotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Bills : ObservableObject
    {
        [ObservableProperty]
        private int _billID = 0;
        [ObservableProperty]
        public string? _billName = "";
        [ObservableProperty]
        public string? _billType = "";
        [ObservableProperty]
        public int? _billValue = 0;
        [ObservableProperty]
        public string? _billDuration = "";
        [ObservableProperty]
        public decimal? _billAmount = 0;
        [ObservableProperty]
        public DateTime? _billDueDate = DateTime.Now;
        [ObservableProperty]
        public decimal _billCurrentBalance = 0;
        [ObservableProperty]
        public bool _isRecuring;
        [ObservableProperty]
        public DateTime _lastUpdatedDate = DateTime.Now;
        [ObservableProperty]
        public bool _isClosed = false;
        [ObservableProperty]
        public decimal? _regularBillValue = 0;


    }

    

}
