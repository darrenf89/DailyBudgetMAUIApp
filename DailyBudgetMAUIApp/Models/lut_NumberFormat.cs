using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_NumberFormat : ObservableObject
    {
        [ObservableProperty]
        public int _id;
        [ObservableProperty]
        public int _currencyDecimalDigitsID;
        [ObservableProperty]
        public int _currencyDecimalSeparatorID;
        [ObservableProperty]
        public int _currencyGroupSeparatorID;
        [ObservableProperty]
        public string _numberFormat = "";
    }
}
