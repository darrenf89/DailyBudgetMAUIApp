
using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpPageSingleInputViewModel : BaseViewModel
    {

        public PopUpPageSingleInputViewModel()
        {
            ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.8;
        }

        [ObservableProperty]
        public partial bool ReturnDataRequired { get; set; }

        [ObservableProperty]
        public partial bool IsSubDesc { get; set; }

        [ObservableProperty]
        public partial string ReturnData { get; set; }

        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }



    }
}
