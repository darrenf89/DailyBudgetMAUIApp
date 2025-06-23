using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpPageVariableInputViewModel : BaseViewModel
    {
        [ObservableProperty]
        public partial bool ReturnDataError { get; set; }

        [ObservableProperty]
        public partial bool IsSubDesc { get; set; }

        [ObservableProperty]
        public partial DateTime DateTimeInput { get; set; }

        [ObservableProperty]
        public partial decimal DecimalInput { get; set; }

        [ObservableProperty]
        public partial decimal StringInput { get; set; }

        [ObservableProperty]
        public partial string Type { get; set; }

        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }

        public PopUpPageVariableInputViewModel()
        {
            ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.8;
        }
    }
}
