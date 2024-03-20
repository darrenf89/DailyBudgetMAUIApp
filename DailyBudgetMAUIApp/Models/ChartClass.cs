using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class ChartClass : ObservableObject
    {
        [ObservableProperty]
        private double  xAxesDouble;
        [ObservableProperty]
        private decimal  xAxesDecimal;
        [ObservableProperty]
        private string  xAxesString;
        [ObservableProperty]
        private DateTime  xAxesDate;
        [ObservableProperty]
        private int  xAxesInt;
        [ObservableProperty]
        private double  yAxesDouble;
        [ObservableProperty]
        private string  yAxesString;
        [ObservableProperty]
        private DateTime  yAxesDate;
        [ObservableProperty]
        private int  yAxesInt;
        [ObservableProperty]
        private decimal  yAxesDecimal;
    }

}
