using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_DateFormat : ObservableObject
    {
        [ObservableProperty]
        public int  id;
        [ObservableProperty]
        public int  dateSeperatorID;
        [ObservableProperty]
        public int  shortDatePatternID;
        [ObservableProperty]
        public string  dateFormat = "";
    }
}
