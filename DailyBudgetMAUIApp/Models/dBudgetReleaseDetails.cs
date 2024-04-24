using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class dBudgetReleaseDetails : ObservableObject
    {
        [ObservableProperty]
        public int releaseID;
        [ObservableProperty]
        public string versionName;
        [ObservableProperty]
        public string versionNumber;
        [ObservableProperty]
        public DateTime releaseDate;
        [ObservableProperty]
        public bool isMajorVersion;
    }  
}
