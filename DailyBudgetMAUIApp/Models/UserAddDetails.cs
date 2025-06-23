using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace DailyBudgetMAUIApp.Models
{
    public partial class UserAddDetails : ObservableObject
    {
        [ObservableProperty]
        public partial int ID { get; set; }

        [ObservableProperty]
        public partial int UserID { get; set; }

        [ObservableProperty]
        public partial DateTime LastViewed { get; set; }

        [ObservableProperty]
        public partial int NumberOfViews { get; set; }
    }
}
