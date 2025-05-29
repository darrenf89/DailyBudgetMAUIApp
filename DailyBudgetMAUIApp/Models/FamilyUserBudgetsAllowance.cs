using CommunityToolkit.Mvvm.ComponentModel;

namespace DailySpendWebApp.Models
{
    public partial class FamilyUserBudgetsAllowance : ObservableObject
    {
        [ObservableProperty]
        private int allowancePaymentID;
        [ObservableProperty]
        private int parentUserID;
        [ObservableProperty]
        private int familyUserID;
        [ObservableProperty]
        private int parentBudgetID;
        [ObservableProperty]
        private DateTime allowancePaymentDate;
        [ObservableProperty]
        private double allowancePaymentAmount;
        [ObservableProperty]
        private bool isParentAdded;

    }
}
