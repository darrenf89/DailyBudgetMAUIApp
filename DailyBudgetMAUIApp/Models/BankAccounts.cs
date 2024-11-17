using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class BankAccounts : ObservableObject
    {
        [ObservableProperty]
        private int  iD;
        [ObservableProperty]
        private string bankAccountName;
        [ObservableProperty]
        private decimal? accountBankBalance;
        [ObservableProperty]
        private bool isDefaultAccount;

    }  
}
