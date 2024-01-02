using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailySpendWebApp.Models
{
    public partial class FirebaseDevices : ObservableObject
    {
        [ObservableProperty]
        public int _firebaseDeviceID;
        [ObservableProperty]
        public string? _firebaseToken;
        [ObservableProperty]
        public string? _deviceName;
        [ObservableProperty]
        public string? _deviceModel;
        [ObservableProperty]
        public int _userAccountID;
        [ObservableProperty]
        public DateTime _loginExpiryDate;
    }
}
