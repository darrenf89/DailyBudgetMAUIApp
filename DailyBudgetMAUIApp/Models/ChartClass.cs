
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class ChartClass : ObservableObject
    {
        [ObservableProperty]
        private double _xAxesDouble;
        [ObservableProperty]
        private decimal _xAxesDecimal;
        [ObservableProperty]
        private string _xAxesString;
        [ObservableProperty]
        private DateTime _xAxesDate;
        [ObservableProperty]
        private int _xAxesInt;
        [ObservableProperty]
        private double _yAxesDouble;
        [ObservableProperty]
        private string _yAxesString;
        [ObservableProperty]
        private DateTime _yAxesDate;
        [ObservableProperty]
        private int _yAxesInt;
        [ObservableProperty]
        private decimal _yAxesDecimal;
    }

}
