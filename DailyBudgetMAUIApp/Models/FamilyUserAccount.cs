
using CommunityToolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.Core;
using System.Text.Json.Serialization;

namespace DailyBudgetMAUIApp.Models
{
    public partial class FamilyUserAccount : ObservableObject
    {
        [ObservableProperty]
        private int userID;
        [ObservableProperty]
        private int uniqueUserID;
        [ObservableProperty]
        private string nickName;
        [ObservableProperty]
        private string email;
        [ObservableProperty]
        private string password;
        [ObservableProperty]
        private string salt;
        [ObservableProperty]
        private bool isEmailVerified;
        [ObservableProperty]
        private DateTime sessionExpiry;
        [ObservableProperty]
        private int budgetID;
        [ObservableProperty]
        private bool isDPAPermissions;
        [ObservableProperty]
        private bool isAgreedToTerms;
        [ObservableProperty]
        private string? profilePicture;
        [ObservableProperty]
        private bool isActive;
        [ObservableProperty]
        private bool isConfirmed;
        [ObservableProperty]
        private int parentUserID;
        [ObservableProperty]
        private int? assignedBudgetID;
        [ObservableProperty]
        private DateTime accountCreated;
        [ObservableProperty]
        private DateTime lastLoggedOn;
        [ObservableProperty]
        private bool isBudgetHidden;
        [ObservableProperty]
        private List<Budgets> budgets = new List<Budgets>();
        [ObservableProperty]
        private bool isBudgetCreated;

        [JsonIgnore]
        [ObservableProperty]
        private SfAvatarView avatarView;
    }

    public class FamilyUserAccountValidEmailObject
    {
        public bool? IsValid { get; set; }
        public string? InvalidReason { get; set; }
    }

}
