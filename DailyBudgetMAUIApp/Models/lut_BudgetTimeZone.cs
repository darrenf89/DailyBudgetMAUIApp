using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_BudgetTimeZone : ObservableObject
    {

        [ObservableProperty]
        public int  timeZoneID;
        [ObservableProperty]
        public string  timeZoneName;
        [ObservableProperty]
        public int  timeZoneUTCOffset;
        [ObservableProperty]
        public string  timeZoneDisplayName;

    }
}
