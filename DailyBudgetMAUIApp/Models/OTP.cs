using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class OTP : ObservableObject
    {
        [ObservableProperty]
        public partial int OTPID { get; set; }

        [ObservableProperty]
        public partial string OTPCode { get; set; }

        [ObservableProperty]
        public partial DateTime OTPExpiryTime { get; set; }

        [ObservableProperty]
        public partial int UserAccountID { get; set; }

        [ObservableProperty]
        public partial bool IsValidated { get; set; }

        [ObservableProperty]
        public partial string OTPType { get; set; }
    }
}
