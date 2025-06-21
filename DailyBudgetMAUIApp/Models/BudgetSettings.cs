using CommunityToolkit.Mvvm.ComponentModel;



namespace DailyBudgetMAUIApp.Models
{
    public partial class BudgetSettings : ObservableObject
    {
        [ObservableProperty]
        public partial int SettingsID { get; set; }
        [ObservableProperty]
        public partial int? BudgetID { get; set; }
        [ObservableProperty]
        public partial int? CurrencyPattern  { get; set; } = 1;
        [ObservableProperty]
        public partial int? CurrencySymbol  { get; set; } = 1;
        [ObservableProperty]
        public partial int? CurrencyDecimalDigits  { get; set; } = 2;
        [ObservableProperty]
        public partial int? CurrencyDecimalSeparator  { get; set; } = 1;
        [ObservableProperty]
        public partial int? CurrencyGroupSeparator  { get; set; } = 2;
        [ObservableProperty]
        public partial int? DateSeperator  { get; set; } = 1;
        [ObservableProperty]       
        public partial int? ShortDatePattern  { get; set; } = 2;
        [ObservableProperty]
        public partial int? TimeZone { get; set; } = 47;

    }
}
