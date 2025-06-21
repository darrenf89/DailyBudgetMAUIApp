using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class BankAccounts : ObservableObject
    {
        [ObservableProperty]
        public partial int  ID { get; set; }
        [ObservableProperty]
        public partial string BankAccountName { get; set; }
        [ObservableProperty]
        public partial decimal? AccountBankBalance { get; set; }
        [ObservableProperty]
        public partial bool IsDefaultAccount { get; set; }

    }  
}
