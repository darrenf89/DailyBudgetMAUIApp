using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpPageVariableInputViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool returnDataError;
        [ObservableProperty]
        private bool isSubDesc;
        [ObservableProperty]
        private DateTime dateTimeInput;
        [ObservableProperty]
        private decimal decimalInput;
        [ObservableProperty]
        private decimal stringInput;
        [ObservableProperty]
        private string type;
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
