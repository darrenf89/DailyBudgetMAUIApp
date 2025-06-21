using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_CurrencySymbol : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial string CurrencySymbol { get; set; } = "";

        [ObservableProperty]
        public partial string Name { get; set; } = "";

        [ObservableProperty]
        public partial string Code { get; set; } = "";
    }
}
