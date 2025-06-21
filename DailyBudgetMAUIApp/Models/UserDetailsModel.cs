using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DailyBudgetMAUIApp.Models
{
    public partial class UserDetailsModel : ObservableObject
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
        public partial bool IsEmailVerified { get; set; }

        [ObservableProperty]
        public partial DateTime SessionExpiry { get; set; }

        [ObservableProperty]
        public partial int DefaultBudgetID { get; set; }

        [ObservableProperty]
        public partial int PreviousDefaultBudgetID { get; set; }

        [ObservableProperty]
        public partial string? DefaultBudgetType { get; set; }

        public ErrorClass? Error { get; set; } = null;

        [ObservableProperty]
        public partial bool IsDPAPermissions { get; set; }

        [ObservableProperty]
        public partial bool IsAgreedToTerms { get; set; }

        [ObservableProperty]
        public partial string? SubscriptionType { get; set; }

        [ObservableProperty]
        public partial DateTime SubscriptionExpiry { get; set; }

        [ObservableProperty]
        public partial string? ProfilePicture { get; set; }

        [ObservableProperty]
        public partial bool HasFamilyAccounts { get; set; }

        [ObservableProperty]
        public partial bool IsFamilyAccounts { get; set; }
    }
}
