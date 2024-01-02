using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class ShareBudgetRequest : ObservableObject
    {
        [ObservableProperty]
        public int _sharedBudgetRequestID;
        [ObservableProperty]
        public int _sharedBudgetID;
        [ObservableProperty]
        public int _sharedWithUserAccountID;
        [ObservableProperty]
        public string? _sharedWithUserEmail;
        [ObservableProperty]
        public string? _sharedByUserEmail;
        [ObservableProperty]
        public bool _isVerified;

    }
}