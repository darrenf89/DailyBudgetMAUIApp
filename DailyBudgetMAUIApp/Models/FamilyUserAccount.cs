
using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class FamilyUserAccount : ObservableObject
    {
        [ObservableProperty]
        public int userID;
        [ObservableProperty]
        public int uniqueUserID;
        [ObservableProperty]
        public string nickName;
        [ObservableProperty]
        public string email;
        [ObservableProperty]
        public string password;
        [ObservableProperty]
        public string salt;
        [ObservableProperty]
        public bool isEmailVerified;
        [ObservableProperty]
        public DateTime sessionExpiry;
        [ObservableProperty]
        public int budgetID;
        [ObservableProperty]
        public bool isDPAPermissions;
        [ObservableProperty]
        public bool isAgreedToTerms;
        [ObservableProperty]
        public string? profilePicture;
        [ObservableProperty]
        public bool isActive;
        [ObservableProperty]
        public bool isConfirmed;
        [ObservableProperty]
        public int parentUserID;
        [ObservableProperty]
        public int? assignedBudgetID;
        [ObservableProperty]
        public DateTime accountCreated;
        [ObservableProperty]
        public DateTime lastLoggedOn;

    }

    public class FamilyUserAccountValidEmailObject
    {
        public bool? IsValid { get; set; }
        public string? InvalidReason { get; set; }
    }

}
