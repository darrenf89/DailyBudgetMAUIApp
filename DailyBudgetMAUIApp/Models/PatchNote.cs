using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class PatchNote : ObservableObject
    {
        [ObservableProperty]
        public string title;
        [ObservableProperty]
        public DateTime date;
        [ObservableProperty]
        public string description;
        [ObservableProperty]
        public List<string> changes;
    }
}
