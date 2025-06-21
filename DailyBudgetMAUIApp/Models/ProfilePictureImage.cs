using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace DailyBudgetMAUIApp.Models
{
    public partial class ProfilePictureImage : ObservableObject
    {
        [ObservableProperty]
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial int UserID { get; set; }

        [ObservableProperty]
        public partial string FileLocation { get; set; }

        [ObservableProperty]
        public partial DateTime WhenAdded { get; set; }

        [ObservableProperty]
        public partial Stream File { get; set; }

        [ObservableProperty]
        public partial string FileName { get; set; }
    }
}
