using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class dBudgetReleaseDetails : ObservableObject
    {
        [ObservableProperty]
        public partial int ReleaseID { get; set; }

        [ObservableProperty]
        public partial string VersionName { get; set; }

        [ObservableProperty]
        public partial string VersionNumber { get; set; }

        [ObservableProperty]
        public partial DateTime ReleaseDate { get; set; }

        [ObservableProperty]
        public partial bool IsMajorVersion { get; set; }
    }
}
