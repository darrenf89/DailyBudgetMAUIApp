using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_CurrencyDecimalDigits : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial string CurrencyDecimalDigits { get; set; }
    }
}
