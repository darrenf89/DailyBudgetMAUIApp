using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_DateFormat : ObservableObject
    {
        [ObservableProperty]
        public int _id;
        [ObservableProperty]
        public int _dateSeperatorID;
        [ObservableProperty]
        public int _shortDatePatternID;
        [ObservableProperty]
        public string _dateFormat = "";
    }
}
