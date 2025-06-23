using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DailyBudgetMAUIApp.Models
{
    public partial class ShareBudgetRequest : ObservableObject
    {
        [ObservableProperty]
        public partial int SharedBudgetRequestID { get; set; }

        [ObservableProperty]
        public partial int SharedBudgetID { get; set; }

        [ObservableProperty]
        public partial int SharedWithUserAccountID { get; set; }

        [ObservableProperty]
        public partial string? SharedWithUserEmail { get; set; }

        [ObservableProperty]
        public partial string? SharedByUserEmail { get; set; }

        [ObservableProperty]
        public partial bool IsVerified { get; set; }

        [ObservableProperty]
        public partial DateTime RequestInitiated { get; set; } = DateTime.UtcNow;
    }
}
