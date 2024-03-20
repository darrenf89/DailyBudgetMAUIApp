using CommunityToolkit.Mvvm.ComponentModel;



namespace DailyBudgetMAUIApp.Models
{
    public partial class BudgetSettings : ObservableObject
    {
        [ObservableProperty]
        public int settingsID; 
        [ObservableProperty]
        public int? budgetID; 
        [ObservableProperty]
        public int? currencyPattern  = 1;
        [ObservableProperty]
        public int? currencySymbol  = 1;
        [ObservableProperty]
        public int? currencyDecimalDigits  = 2;
        [ObservableProperty]
        public int? currencyDecimalSeparator  = 1;
        [ObservableProperty]
        public int? currencyGroupSeparator  = 2;
        [ObservableProperty]
        public int? dateSeperator  = 1;
        [ObservableProperty]       
        public int? shortDatePattern  = 2;
        [ObservableProperty]
        public int? timeZone = 47;

    }
}
