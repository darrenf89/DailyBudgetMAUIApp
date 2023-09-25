using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_CurrencySymbol : ObservableObject
    {
        [ObservableProperty]
        public int _id;
        [ObservableProperty]        
        public string _currencySymbol = "";
        [ObservableProperty]
        public string _name = "";
        [ObservableProperty]
        public string _code = "";
    }
}
