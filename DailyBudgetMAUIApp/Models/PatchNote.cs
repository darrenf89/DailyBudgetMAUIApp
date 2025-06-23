using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace DailyBudgetMAUIApp.Models
{
    public partial class PatchNote : ObservableObject
    {
        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial DateTime Date { get; set; }

        [ObservableProperty]
        public partial string Description { get; set; }

        [ObservableProperty]
        public partial List<string> Changes { get; set; }
    }
}
