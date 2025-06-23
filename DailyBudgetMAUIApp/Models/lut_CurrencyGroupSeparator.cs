using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_CurrencyGroupSeparator : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial string CurrencyGroupSeparator { get; set; }
    }
}
