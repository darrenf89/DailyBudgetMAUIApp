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

        [ObservableProperty]
        private string oTPOne;
        [ObservableProperty]
        private string oTPTwo;
        [ObservableProperty]
        private string oTPThree;
        [ObservableProperty]
        private string oTPFour;
        [ObservableProperty]
        private string oTPFive;
        [ObservableProperty]
        private string oTPSix;        
        [ObservableProperty]
        private string oTPCopyErrorMessage;        
        [ObservableProperty]
        private bool oTPCopyContentValid = false;

        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }
        public double OTPWidth { get; }

        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;

        public PopUpOTPViewModel(IRestDataService ds, IProductTools pt)
        {
            ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.8;
            OTPWidth = (EntryWidth - 50) / 6;

            _ds = ds;
            _pt = pt;
        }

        private async Task ClearClipboard() =>
            await Clipboard.Default.SetTextAsync(null);

        [RelayCommand]
        public async Task PasteOTP()
        {
            try
            {
                if (Clipboard.Default.HasText)
                {
                    string CopiedText = await Clipboard.Default.GetTextAsync();
                    CopiedText = CopiedText.Trim();
                    if (CopiedText.Length == 6 && int.TryParse(CopiedText, out int n))
                    {
                        OTPOne = char.ToString(CopiedText[0]);
                        OTPTwo = char.ToString(CopiedText[1]);
                        OTPThree = char.ToString(CopiedText[2]);
                        OTPFour = char.ToString(CopiedText[3]);
                        OTPFive = char.ToString(CopiedText[4]);
                        OTPSix = char.ToString(CopiedText[5]);

                        await ClearClipboard();
                    }
                    else
                    {
                        OTPCopyErrorMessage = "Copied content is not the correct format";
                        OTPCopyContentValid = true;
                    }

               
                }
                else
                {
                    OTPCopyErrorMessage = "Clipboard is empty";
                    OTPCopyContentValid = true;
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "PopUpOTP", "PasteOTP");
            }

        }


        [RelayCommand]
        public async Task Resend()
        {
            try
            {
                string status = "";

                if (OTPType == "ShareBudget")
                {
                    status = await _ds.CreateNewOtpCodeShareBudget(UserID, ShareBudgetRequest.SharedBudgetRequestID);
                }
                else
                {
                    status = await _ds.CreateNewOtpCode(UserID, OTPType);
                }            

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
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "PopUpOTP", "ChangeSelectedProfilePic");
            }
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
