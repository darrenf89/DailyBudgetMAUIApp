using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpOTPViewModel : BaseViewModel
    {
        [ObservableProperty]
        public partial int UserID { get; set; }

        [ObservableProperty]
        public partial string OTPType { get; set; }

        [ObservableProperty]
        public partial string ReturnData { get; set; }

        [ObservableProperty]
        public partial string UserEmail { get; set; }

        [ObservableProperty]
        public partial OTP OTP { get; set; }

        [ObservableProperty]
        public partial bool EmailValid { get; set; } = true;

        [ObservableProperty]
        public partial bool EmailRequired { get; set; } = true;

        [ObservableProperty]
        public partial bool OTPNotFound { get; set; }

        [ObservableProperty]
        public partial bool OTPRequired { get; set; }

        [ObservableProperty]
        public partial bool EmailNotFound { get; set; }

        [ObservableProperty]
        public partial bool OTPValidated { get; set; }

        [ObservableProperty]
        public partial int CountdownNumber { get; set; } = 120;

        [ObservableProperty]
        public partial bool CountdownVisible { get; set; } = false;

        [ObservableProperty]
        public partial bool ResendVisible { get; set; } = true;

        [ObservableProperty]
        public partial bool ResendSuccess { get; set; }

        [ObservableProperty]
        public partial bool MaxLimitFailure { get; set; }

        [ObservableProperty]
        public partial bool ResendFailure { get; set; }

        [ObservableProperty]
        public partial bool PasswordRequired { get; set; } = true;

        [ObservableProperty]
        public partial bool PasswordSameSame { get; set; } = true;

        [ObservableProperty]
        public partial bool PasswordStrong { get; set; } = true;

        [ObservableProperty]
        public partial string Password { get; set; }

        [ObservableProperty]
        public partial string PasswordConfirm { get; set; }

        [ObservableProperty]
        public partial bool PasswordResetFailure { get; set; }

        [ObservableProperty]
        public partial bool FamilyAccountSetUpFailure { get; set; }

        [ObservableProperty]
        public partial string FamilyAccountSetUpFailureText { get; set; }

        [ObservableProperty]
        public partial ShareBudgetRequest ShareBudgetRequest { get; set; }

        [ObservableProperty]
        public partial string OTPOne { get; set; }

        [ObservableProperty]
        public partial string OTPTwo { get; set; }

        [ObservableProperty]
        public partial string OTPThree { get; set; }

        [ObservableProperty]
        public partial string OTPFour { get; set; }

        [ObservableProperty]
        public partial string OTPFive { get; set; }

        [ObservableProperty]
        public partial string OTPSix { get; set; }

        [ObservableProperty]
        public partial string OTPCopyErrorMessage { get; set; }

        [ObservableProperty]
        public partial bool OTPCopyContentValid { get; set; } = false;


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
