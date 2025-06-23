using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class ChartClass : ObservableObject
    {
        [ObservableProperty]
        public partial double XAxesDouble { get; set; }

        [ObservableProperty]
        public partial decimal XAxesDecimal { get; set; }

        [ObservableProperty]
        public partial string XAxesString { get; set; }

        [ObservableProperty]
        public partial DateTime XAxesDate { get; set; }

        [ObservableProperty]
        public partial int XAxesInt { get; set; }

        [ObservableProperty]
        public partial double YAxesDouble { get; set; }

        [ObservableProperty]
        public partial string YAxesString { get; set; }

        [ObservableProperty]
        public partial DateTime YAxesDate { get; set; }

        [ObservableProperty]
        public partial int YAxesInt { get; set; }

        [ObservableProperty]
        public partial decimal YAxesDecimal { get; set; }
    }
}

