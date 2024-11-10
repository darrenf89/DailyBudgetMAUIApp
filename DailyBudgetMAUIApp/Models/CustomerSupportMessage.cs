using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class CustomerSupportMessage : ObservableObject
    {
        [ObservableProperty]
        public int messageID;
        [ObservableProperty]
        public string message;
        [ObservableProperty]
        public DateTime whenadded;
        [ObservableProperty]
        public bool isRead;
        [ObservableProperty]
        public bool isCustomerReply;
        [ObservableProperty]
        public int? supportID;
        [ObservableProperty]
        public CustomerSupport? customerSupport;
    }  
}
