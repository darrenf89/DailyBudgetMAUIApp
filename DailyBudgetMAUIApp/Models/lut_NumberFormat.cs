using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_NumberFormat : ObservableObject
    {
        [ObservableProperty]
        public int  id;
        [ObservableProperty]
        public int  currencyDecimalDigitsID;
        [ObservableProperty]
        public int  currencyDecimalSeparatorID;
        [ObservableProperty]
        public int  currencyGroupSeparatorID;
        [ObservableProperty]
        public string  numberFormat = "";
    }
}
