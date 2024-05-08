using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopupSyncBankBalanceViewModel : BaseViewModel
    {
        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }
        public double ButtonOneWidth { get; }
        public double ButtonTwoWidth { get; }
        public double ButtonThreeWidth { get; }

        public decimal OriginalAmount { get; set; }
        [ObservableProperty]
        public decimal amount;


        [ObservableProperty]
        public Budgets budget;

        public PopupSyncBankBalanceViewModel()
        {
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.6;
            ButtonOneWidth = ((PopupWidth - 60) / 2);
            ButtonTwoWidth = ((PopupWidth - 140) / 3);
            ButtonThreeWidth = ((PopupWidth - 260) / 2);

        }





    }
}
