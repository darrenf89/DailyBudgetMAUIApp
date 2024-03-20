using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_CurrencyPlacement : ObservableObject
    {
        [ObservableProperty]
        public int  id;
        [ObservableProperty]
        public string  currencyPlacement = "";
        [ObservableProperty]
        public int  currencyPositivePatternRef  = 0;
    }
}
