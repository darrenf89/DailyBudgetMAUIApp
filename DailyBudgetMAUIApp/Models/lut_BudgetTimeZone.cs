using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_BudgetTimeZone : ObservableObject
    {
        [ObservableProperty]
        public partial int TimeZoneID { get; set; }

        [ObservableProperty]
        public partial string TimeZoneName { get; set; }

        [ObservableProperty]
        public partial int TimeZoneUTCOffset { get; set; }

        [ObservableProperty]
        public partial string TimeZoneDisplayName { get; set; }
    }
}
