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
        [ObservableProperty]
        private int _countdownNumber = 120;
        [ObservableProperty]
        private bool _countdownVisible = false;
        [ObservableProperty]
        private bool _resendVisible = true;
        [ObservableProperty]
        private bool _resendSuccess;
        [ObservableProperty]
        private bool _maxLimitFailure;
        [ObservableProperty]
        private bool _resendFailure;
        [ObservableProperty]
        private bool _passwordRequired;
        [ObservableProperty]
        private bool _passwordSameSame;
        [ObservableProperty]
        private bool _passwordStrong;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _passwordConfirm;
        [ObservableProperty]
        private bool _passwordResetFailure;
        [ObservableProperty]
        private ShareBudgetRequest _shareBudgetRequest;

        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }
        public double OTPWidth { get; }

        private readonly IRestDataService _ds;

        public PopUpOTPViewModel(IRestDataService ds)
        {
            ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.8;
            OTPWidth = (EntryWidth - 70) / 8;

            _ds = ds;
        }


        [ICommand]
        public async void Resend()
        {
            
            string status = await _ds.CreateNewOtpCode(UserID, OTPType);

            CountdownVisible = true;
            ResendVisible = false;

            if (status == "OK")
            {
                ResendSuccess = true;

            }
            else if(status == "MaxLimit")
            {
                MaxLimitFailure = true;
            }
            else
            {
                ResendFailure = true;
            }

            await UpdateCountdownNumber();

            CountdownVisible = false;
            ResendVisible = true;
            ResendSuccess = false;
            MaxLimitFailure = false;
            ResendFailure = false;
        }

        private async Task UpdateCountdownNumber()
        {
            DateTime StartTime = DateTime.UtcNow;
            int Duration = CountdownNumber;

            while(CountdownNumber > 0)
            {
                TimeSpan ElapsedTime = (DateTime.UtcNow - StartTime);
                int SecondsElapsed = (int)Math.Ceiling(ElapsedTime.TotalSeconds);

                CountdownNumber = Duration - SecondsElapsed;

                await Task.Delay(500);
            }

            CountdownNumber = 120;
        }
    }
}
