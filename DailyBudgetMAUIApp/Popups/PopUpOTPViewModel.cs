using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpOTPViewModel : BaseViewModel
    {
        [ObservableProperty]
        public int  userID;
        [ObservableProperty]
        public string  oTPType;
        [ObservableProperty]
        public string  returnData;
        [ObservableProperty]
        public string  userEmail;
        [ObservableProperty]
        public OTP  oTP;
        [ObservableProperty]
        private bool  emailValid;
        [ObservableProperty]
        private bool  emailRequired;
        [ObservableProperty]
        private bool  oTPNotFound;
        [ObservableProperty]
        private bool  oTPRequired;
        [ObservableProperty]
        private bool  emailNotFound;
        [ObservableProperty]
        private bool  oTPValidated;
        [ObservableProperty]
        private int  countdownNumber = 120;
        [ObservableProperty]
        private bool  countdownVisible = false;
        [ObservableProperty]
        private bool  resendVisible = true;
        [ObservableProperty]
        private bool  resendSuccess;
        [ObservableProperty]
        private bool  maxLimitFailure;
        [ObservableProperty]
        private bool  resendFailure;
        [ObservableProperty]
        private bool  passwordRequired;
        [ObservableProperty]
        private bool  passwordSameSame;
        [ObservableProperty]
        private bool  passwordStrong;
        [ObservableProperty]
        private string  password;
        [ObservableProperty]
        private string  passwordConfirm;
        [ObservableProperty]
        private bool  passwordResetFailure;
        [ObservableProperty]
        private ShareBudgetRequest  shareBudgetRequest;

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


        [RelayCommand]
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
