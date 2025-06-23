using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_NumberFormat : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial int CurrencyDecimalDigitsID { get; set; }

        [ObservableProperty]
        public partial int CurrencyDecimalSeparatorID { get; set; }

        [ObservableProperty]
        public partial int CurrencyGroupSeparatorID { get; set; }

        [ObservableProperty]
        public partial string NumberFormat { get; set; } = "";
    }
}
