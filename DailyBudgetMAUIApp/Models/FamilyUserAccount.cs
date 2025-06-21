
using CommunityToolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.Core;
using System.Text.Json.Serialization;

namespace DailyBudgetMAUIApp.Models
{
    public partial class FamilyUserAccount : ObservableObject
    {
        [ObservableProperty]
        public partial int UserID { get; set; }

        [ObservableProperty]
        public partial int UniqueUserID { get; set; }

        [ObservableProperty]
        public partial string NickName { get; set; }

        [ObservableProperty]
        public partial string Email { get; set; }

        [ObservableProperty]
        public partial string Password { get; set; }

        [ObservableProperty]
        public partial string Salt { get; set; }

        [ObservableProperty]
        public partial bool IsEmailVerified { get; set; }

        [ObservableProperty]
        public partial DateTime SessionExpiry { get; set; }

        [ObservableProperty]
        public partial int BudgetID { get; set; }

        [ObservableProperty]
        public partial bool IsDPAPermissions { get; set; }

        [ObservableProperty]
        public partial bool IsAgreedToTerms { get; set; }

        [ObservableProperty]
        public partial string? ProfilePicture { get; set; }

        [ObservableProperty]
        public partial bool IsActive { get; set; }

        [ObservableProperty]
        public partial bool IsConfirmed { get; set; }

        [ObservableProperty]
        public partial int ParentUserID { get; set; }

        [ObservableProperty]
        public partial int? AssignedBudgetID { get; set; }

        [ObservableProperty]
        public partial DateTime AccountCreated { get; set; }

        [ObservableProperty]
        public partial DateTime LastLoggedOn { get; set; }

        [ObservableProperty]
        public partial bool IsBudgetHidden { get; set; }

        [ObservableProperty]
        public partial List<Budgets> Budgets { get; set; } = new List<Budgets>();

        [ObservableProperty]
        public partial bool IsBudgetCreated { get; set; }

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
