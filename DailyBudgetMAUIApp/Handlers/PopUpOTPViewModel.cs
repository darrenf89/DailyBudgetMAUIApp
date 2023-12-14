using CommunityToolkit.Maui.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Newtonsoft.Json;
using System.Diagnostics;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpOTPViewModel : BaseViewModel
    {
        [ObservableProperty]
        public int _userID;
        [ObservableProperty]
        public string _oTPType;
        [ObservableProperty]
        public string _returnData;
        [ObservableProperty]
        public string _userEmail;
        [ObservableProperty]
        public OTP _oTP;
        [ObservableProperty]
        private bool _emailValid;
        [ObservableProperty]
        private bool _emailRequired;
        [ObservableProperty]
        private bool _oTPNotFound;
        [ObservableProperty]
        private bool _oTPRequired;
        [ObservableProperty]
        private bool _emailNotFound;
        [ObservableProperty]
        private bool _oTPValidated;

        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }
        public double OTPWidth { get; }

        public PopUpOTPViewModel()
        {
            ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.8;
            OTPWidth = (EntryWidth - 70) / 8;
        }





    }
}
