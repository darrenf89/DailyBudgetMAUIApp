using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class OTP : ObservableObject
    {
        [ObservableProperty]
        public int  oTPID;
        [ObservableProperty]
        public string  oTPCode;
        [ObservableProperty]
        public DateTime  oTPExpiryTime;
        [ObservableProperty]
        public int  userAccountID;
        [ObservableProperty]
        public bool  isValidated;
        [ObservableProperty]
        public string  oTPType;
    }

}
