
using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class UserDetailsModel : ObservableObject
    {
        [ObservableProperty]
        public int userID;
        [ObservableProperty]
        public string nickName;
        [ObservableProperty]
        public string email;
        [ObservableProperty]
        public string password;
        [ObservableProperty]
        public bool isEmailVerified;
        [ObservableProperty]
        public DateTime sessionExpiry;
        [ObservableProperty]
        public int defaultBudgetID;
        [ObservableProperty]
        public int previousDefaultBudgetID;
        [ObservableProperty]
        public string? defaultBudgetType;
        public ErrorClass? Error { get; set; } = null;
        [ObservableProperty]
        public bool isDPAPermissions;
        [ObservableProperty]
        public bool isAgreedToTerms;
        [ObservableProperty]
        public string? subscriptionType;
        [ObservableProperty]
        public DateTime subscriptionExpiry;
        [ObservableProperty]
        public string? profilePicture;

    }
}
