using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_BudgetTimeZone : ObservableObject
    {

        [ObservableProperty]
        public int _timeZoneID;
        [ObservableProperty]
        public string _timeZoneName;
        [ObservableProperty]
        public int _timeZoneUTCOffset;
        [ObservableProperty]
        public string _timeZoneDisplayName;

    }
}
