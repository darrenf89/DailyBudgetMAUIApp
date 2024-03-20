using CommunityToolkit.Mvvm.ComponentModel;

namespace DailySpendWebApp.Models
{
    public partial class FirebaseDevices : ObservableObject
    {
        [ObservableProperty]
        public int  firebaseDeviceID;
        [ObservableProperty]
        public string?  firebaseToken;
        [ObservableProperty]
        public string?  deviceName;
        [ObservableProperty]
        public string?  deviceModel;
        [ObservableProperty]
        public int  userAccountID;
        [ObservableProperty]
        public DateTime  loginExpiryDate;
    }
}
