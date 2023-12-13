using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class OTP : ObservableObject
    {
        [ObservableProperty]
        public int _oTPID;
        [ObservableProperty]
        public string _oTPCode;
        [ObservableProperty]
        public DateTime _oTPExpiryTime;
        [ObservableProperty]
        public int _userAccountID;
        [ObservableProperty]
        public bool _isValidated;
    }

}
