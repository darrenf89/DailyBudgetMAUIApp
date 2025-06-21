using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DailySpendWebApp.Models
{
    public partial class FamilyUserBudgetsAllowance : ObservableObject
    {
        [ObservableProperty]
        public partial int AllowancePaymentID { get; set; }

        [ObservableProperty]
        public partial int ParentUserID { get; set; }

        [ObservableProperty]
        public partial int FamilyUserID { get; set; }

        [ObservableProperty]
        public partial int ParentBudgetID { get; set; }

        [ObservableProperty]
        public partial DateTime AllowancePaymentDate { get; set; }

        [ObservableProperty]
        public partial double AllowancePaymentAmount { get; set; }

        [ObservableProperty]
        public partial bool IsParentAdded { get; set; }
    }
}
