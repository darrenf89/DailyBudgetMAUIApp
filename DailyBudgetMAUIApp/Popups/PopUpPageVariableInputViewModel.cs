using Microsoft.Toolkit.Mvvm.ComponentModel;



namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpPageVariableInputViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool _returnDataError;
        [ObservableProperty]
        private bool _isSubDesc;
        [ObservableProperty]
        private DateTime _dateTimeInput;
        [ObservableProperty]
        private decimal _decimalInput;
        [ObservableProperty]
        private decimal _stringInput;
        [ObservableProperty]
        private string _type;
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
