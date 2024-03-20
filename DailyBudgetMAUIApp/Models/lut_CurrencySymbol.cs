using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_CurrencySymbol : ObservableObject
    {
        [ObservableProperty]
        public int  id;
        [ObservableProperty]        
        public string  currencySymbol = "";
        [ObservableProperty]
        public string  name = "";
        [ObservableProperty]
        public string  code = "";
    }
}
