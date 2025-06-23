using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_CurrencyPlacement : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial string CurrencyPlacement { get; set; } = "";

        [ObservableProperty]
        public partial int CurrencyPositivePatternRef { get; set; } = 0;
    }
}
