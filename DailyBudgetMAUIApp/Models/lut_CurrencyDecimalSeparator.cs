 using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{

    public partial class lut_CurrencyDecimalSeparator : ObservableObject
    {
        [ObservableProperty]
        public int  id;
        [ObservableProperty]
        public string currencyDecimalSeparator;
    }

}