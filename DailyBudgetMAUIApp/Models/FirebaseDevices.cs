using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DailySpendWebApp.Models
{
    public partial class FirebaseDevices : ObservableObject
    {
        [ObservableProperty]
        public partial int FirebaseDeviceID { get; set; }

        [ObservableProperty]
        public partial string? FirebaseToken { get; set; }

        [ObservableProperty]
        public partial string? DeviceName { get; set; }

        [ObservableProperty]
        public partial string? DeviceModel { get; set; }

        [ObservableProperty]
        public partial int UserAccountID { get; set; }

        [ObservableProperty]
        public partial DateTime LoginExpiryDate { get; set; }
    }
}
