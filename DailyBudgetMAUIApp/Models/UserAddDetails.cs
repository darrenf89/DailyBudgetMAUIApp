
using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class UserAddDetails : ObservableObject
    {
        [ObservableProperty]
        public int iD;        
        [ObservableProperty]
        public int userID;     
        [ObservableProperty]
        public DateTime lastViewed;     
        [ObservableProperty]
        public int numberOfViews;
    }
}
