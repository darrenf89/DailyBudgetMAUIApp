using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class CustomerSupportMessage : ObservableObject
    {
        [ObservableProperty]
        public partial int MessageID { get; set; }

        [ObservableProperty]
        public partial string Message { get; set; }

        [ObservableProperty]
        public partial DateTime Whenadded { get; set; }

        [ObservableProperty]
        public partial bool IsRead { get; set; }

        [ObservableProperty]
        public partial bool IsCustomerReply { get; set; }
    }
}
