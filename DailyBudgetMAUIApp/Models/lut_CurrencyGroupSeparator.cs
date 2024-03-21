using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{

    public partial class lut_CurrencyGroupSeparator : ObservableObject
    {
        [ObservableProperty]
        public int  id;
        [ObservableProperty]
        public string currencyGroupSeparator;
    }

}