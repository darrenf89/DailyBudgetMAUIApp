 using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{

    public partial class lut_CurrencyDecimalDigits : ObservableObject
    {
        [ObservableProperty]
        public int  id;
        [ObservableProperty]
        public string currencyDecimalDigits;
    }

}


