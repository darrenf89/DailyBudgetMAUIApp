using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class ProfilePictureImage : ObservableObject
    {
        [ObservableProperty]
        public int id;
        [ObservableProperty]
        public int userID;
        [ObservableProperty]
        public string fileLocation;
        [ObservableProperty]
        public DateTime whenAdded;
        [ObservableProperty]
        public Stream file;
        [ObservableProperty]
        public string fileName;
    }   
}
