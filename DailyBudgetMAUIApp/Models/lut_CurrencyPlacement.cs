using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_CurrencyPlacement : ObservableObject
    {
        [ObservableProperty]
        public int _id;
        [ObservableProperty]
        public string _currencyPlacement = "";
        [ObservableProperty]
        public int _currencyPositivePatternRef  = 0;
    }
}
