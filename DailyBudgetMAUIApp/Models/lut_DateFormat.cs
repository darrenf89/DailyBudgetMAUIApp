using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_DateFormat : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial int DateSeperatorID { get; set; }

        [ObservableProperty]
        public partial int ShortDatePatternID { get; set; }

        [ObservableProperty]
        public partial string DateFormat { get; set; } = "";
    }
}
