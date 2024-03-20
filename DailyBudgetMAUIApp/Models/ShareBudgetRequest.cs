using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class ShareBudgetRequest : ObservableObject
    {
        [ObservableProperty]
        public int  sharedBudgetRequestID;
        [ObservableProperty]
        public int  sharedBudgetID;
        [ObservableProperty]
        public int  sharedWithUserAccountID;
        [ObservableProperty]
        public string?  sharedWithUserEmail;
        [ObservableProperty]
        public string?  sharedByUserEmail;
        [ObservableProperty]
        public bool  isVerified;
        [ObservableProperty]
        public DateTime  requestInitiated = DateTime.UtcNow;

    }
}