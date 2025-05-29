using Android.Net.Eap;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpOTP : Popup
{

    private readonly PopUpOTPViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    private readonly int _userID;
    private readonly string _otpType;


    public PopUpOTP(int UserID, PopUpOTPViewModel viewModel, string OTPType, IProductTools pt, IRestDataService ds)
    {
        InitializeComponent();

        double width = viewModel.PopupWidth -11;
        Rect rt = new Rect(width, 123, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
        AbsLayout.SetLayoutBounds(btnClose, rt);

        _userID = UserID;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;
        _otpType = OTPType;
        BindingContext = _vm;

        Opened += async (s, e) => await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            if (_otpType == "ShareBudget")
            {
                _vm.ShareBudgetRequest = await _ds.GetShareBudgetRequestByID(_userID);
                _vm.UserID = _vm.ShareBudgetRequest.SharedWithUserAccountID;
            }
            else
            {
                _vm.UserID = _userID;
            }

            _vm.OTPType = _otpType;
            _vm.OTP = new OTP();
            _vm.OTP.OTPCode = "";

            LoadOTPType();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "PopUpOTP", "PopUpOTP");
        }        
    }


    private void LoadOTPType()
    {
        if(_vm.OTPType == "ValidateEmail")
        {
            if (_vm.UserID == 0)
            {
                lblTitleEmail.Text = "Verify Your Email";
                lblDescriptionEmail.Text = "Please enter your accounts Email";
                btnSave.Text = "Confirm Email";

                entEmail.IsVisible = true;
                entOTPCode.IsVisible = false;
            }
            else
            {
                lblTitleOTPCode.Text = "Verify Your Email";
                lblDescriptionOTPCode.Text = "Please enter the OTP provided";
                btnSave.Text = "Verify Account";

                entEmail.IsVisible = false;
                entOTPCode.IsVisible = true;

                entOTPOne.Focus();
            }
        }
        else if (_vm.OTPType == "ResetPassword")
        {
            if (_vm.UserID == 0)
            {
                lblTitleEmail.Text = "Reset Your Password";
                lblDescriptionEmail.Text = "Please enter your accounts Email";
                btnSave.Text = "Confirm Email";

                entEmail.IsVisible = true;
                entOTPCode.IsVisible = false;
                entPasswordReset.IsVisible = false;
            }
            else if (_vm.OTPValidated)
            {
                lblTitlePassword.Text = "Reset Your Password";
                lblDescriptionPassword.Text = "Please enter your desired password";
                btnSave.Text = "Change Password";

                entEmail.IsVisible = false;
                entOTPCode.IsVisible = false;
                entPasswordReset.IsVisible = true;

                entPassword.Focus();
            }
            else
            {
                lblTitleOTPCode.Text = "Reset Your Password";
                lblDescriptionOTPCode.Text = "Please enter the OTP provided";
                btnSave.Text = "Enter Code";

                entEmail.IsVisible = false;
                entOTPCode.IsVisible = true;
                entPasswordReset.IsVisible = false;

                entOTPOne.Focus();
            }
        }
        else if (_vm.OTPType == "ResetPasswordFamily")
        {
            if (_vm.UserID == 0)
            {
                lblTitleEmail.Text = "Reset Your Password";
                lblDescriptionEmail.Text = "Please enter your accounts Email";
                btnSave.Text = "Confirm Email";

                entEmail.IsVisible = true;
                entOTPCode.IsVisible = false;
                entPasswordReset.IsVisible = false;
            }
            else if (_vm.OTPValidated)
            {
                lblTitlePassword.Text = "Reset Your Password";
                lblDescriptionPassword.Text = "Please enter your desired password";
                btnSave.Text = "Change Password";

                entEmail.IsVisible = false;
                entOTPCode.IsVisible = false;
                entPasswordReset.IsVisible = true;

                entPassword.Focus();
            }
            else
            {
                lblTitleOTPCode.Text = "Reset Your Password";
                lblDescriptionOTPCode.Text = "Please enter the OTP provided";
                btnSave.Text = "Enter Code";

                entEmail.IsVisible = false;
                entOTPCode.IsVisible = true;
                entPasswordReset.IsVisible = false;

                entOTPOne.Focus();
            }
        }
        else if(_vm.OTPType == "ShareBudget")
        {
            lblTitleOTPCode.Text = "Start Sharing a Budget!";
            lblDescriptionOTPCode.Text = "To verify the share budget request, please enter the OTP provided";
            btnSave.Text = "Enter Code";

            entEmail.IsVisible = false;
            entOTPCode.IsVisible = true;
            entPasswordReset.IsVisible = false;

            entOTPOne.Focus();
        }
        else if (_vm.OTPType == "FamilyAccountCreation")
        {
            if (_vm.UserID == 0)
            {
                lblTitleEmail.Text = "Complete account set up";
                lblDescriptionEmail.Text = "Please enter your accounts Email";
                btnSave.Text = "Confirm Email";

                entEmail.IsVisible = true;
                entOTPCode.IsVisible = false;
            }
            else if (_vm.OTPValidated)
            {
                lblTitlePassword.Text = "Set your Password";
                lblDescriptionPassword.Text = "Please enter your desired password";
                btnSave.Text = "Change Password";

                entEmail.IsVisible = false;
                entOTPCode.IsVisible = false;
                entPasswordReset.IsVisible = true;

                entPassword.Focus();
            }
            else
            {
                lblTitleOTPCode.Text = "Complete account set up";
                lblDescriptionOTPCode.Text = "Please enter the OTP provided";
                btnSave.Text = "Verify Account";

                entEmail.IsVisible = false;
                entOTPCode.IsVisible = true;

                entOTPOne.Focus();
            }
        }
    }

    private void ResetSuccessFailureMessage()
    {        
        _vm.ResendSuccess = false;
        _vm.MaxLimitFailure = false;
        _vm.ResendFailure = false;
        _vm.PasswordResetFailure = false;
        _vm.EmailNotFound = false;
        _vm.OTPRequired = false;
        _vm.OTPCopyContentValid = false;
    }

    private async void ValidateOTP_Popup(object sender, EventArgs e)
    {
        try
        {
            var keyboardService = IPlatformApplication.Current.Services.GetService<IKeyboardService>();
            ResetSuccessFailureMessage();
            if (_vm.OTPType == "ValidateEmail")
            {
                if (_vm.UserID == 0)
                {
                    await ValidateEmail();
                    if (_vm.EmailValid && _vm.EmailRequired)
                    {
                        _vm.UserID = await _ds.GetUserIdFromEmail(_vm.UserEmail);
                        _vm.OTP.UserAccountID = _vm.UserID;

                        if (_vm.UserID == 0)
                        {
                            _vm.EmailNotFound = true;
                        }
                        else
                        {
                            keyboardService.HideKeyboard();
                            LoadOTPType();
                        }                        
                    }
                }
                else
                {
                    if(_vm.OTP.OTPCode.Length < 6)
                    {
                        _vm.OTPRequired = true;
                    }
                    else
                    {
                        _vm.OTP.OTPExpiryTime = DateTime.UtcNow;
                        _vm.OTP.UserAccountID = _vm.UserID;
                        _vm.OTP.OTPType = _vm.OTPType;
                        string status = await _ds.ValidateOTPCodeEmail(_vm.OTP);
                        if(status == "OK")
                        {
                            _vm.OTPValidated = true;
                            keyboardService.HideKeyboard();
                            App.CurrentPopUp = null;
                            this.Close("OK");
                        }
                        else
                        {
                            _vm.OTPNotFound = true;
                        }
                    }                
                }
            }
            else if(_vm.OTPType == "ResetPassword")
            {
                if (_vm.UserID == 0)
                {
                    await ValidateEmail();
                    if (_vm.EmailValid && _vm.EmailRequired)
                    {
                        _vm.UserID = await _ds.GetUserIdFromEmail(_vm.UserEmail);
                        _vm.OTP.UserAccountID = _vm.UserID;

                        if (_vm.UserID == 0)
                        {
                            _vm.EmailNotFound = true;
                        }
                        else
                        {
                            string status = await _ds.CreateNewOtpCode(_vm.UserID, _vm.OTPType);
                            if (status == "OK")
                            {
                                keyboardService.HideKeyboard();
                                LoadOTPType();
                            }
                            else if (status == "MaxLimit")
                            {
                                _vm.MaxLimitFailure = true;
                            }
                            else
                            {
                                _vm.ResendFailure = true;
                            }
                        }
                    }
                }
                else if (_vm.OTPValidated)
                {
                    await ValidatePassword();
                    if (_vm.PasswordSameSame && _vm.PasswordRequired && _vm.PasswordStrong)
                    {
                        RegisterModel User = new RegisterModel();
                        User.Salt = await _ds.GetUserSaltAsync(_vm.UserEmail);
                        User.Password = _vm.Password;
                        User = _pt.ResetUserPassword(User);

                        List<PatchDoc> UserDetails = new List<PatchDoc>();

                        PatchDoc NewSalt = new PatchDoc
                        {
                            op = "replace",
                            path = "/Salt",
                            value = User.Salt
                        };

                        PatchDoc NewPassword = new PatchDoc
                        {
                            op = "replace",
                            path = "/Password",
                            value = User.Password
                        };

                        UserDetails.Add(NewSalt);
                        UserDetails.Add(NewPassword);

                        string status = await _ds.PatchUserAccount(_vm.UserID, UserDetails);

                        if(status == "OK")
                        {
                            keyboardService.HideKeyboard();
                            App.CurrentPopUp = null;
                            this.Close("OK");
                        }
                        else
                        {
                            _vm.PasswordResetFailure = true;
                        }
                    }
                }
                else
                {
                    if (_vm.OTP.OTPCode.Length < 6)
                    {
                        _vm.OTPRequired = true;
                    }
                    else
                    {
                        _vm.OTP.OTPExpiryTime = DateTime.UtcNow;
                        _vm.OTP.UserAccountID = _vm.UserID;
                        _vm.OTP.OTPType = _vm.OTPType;
                        string status = await _ds.ValidateOTPCodeEmail(_vm.OTP);
                        if (status == "OK")
                        {
                            _vm.OTPValidated = true;
                            keyboardService.HideKeyboard();
                            LoadOTPType();
                        }
                        else
                        {
                            _vm.OTPNotFound = true;
                        }
                    }
                }
            }
            else if(_vm.OTPType == "ResetPasswordFamily")
            {
                if (_vm.UserID == 0)
                {
                    await ValidateEmail();
                    if (_vm.EmailValid && _vm.EmailRequired)
                    {
                        _vm.UserID = await _ds.GetUserIdFamilyAccountFromEmail(_vm.UserEmail);
                        _vm.OTP.UserAccountID = _vm.UserID;

                        if (_vm.UserID == 0)
                        {
                            _vm.EmailNotFound = true;
                        }
                        else
                        {
                            FamilyUserAccount User = await _ds.GetFamilyUserDetailsAsync(_vm.UserEmail);

                            if (User.IsActive != true)
                            {
                                _vm.FamilyAccountSetUpFailure = true;
                                _vm.FamilyAccountSetUpFailureText = "your parent or guardian hasn't activated your account.";
                            }
                            else if (User.IsConfirmed != true)
                            {
                                _vm.FamilyAccountSetUpFailure = true;
                                _vm.FamilyAccountSetUpFailureText = "You have not set up your account, go to complete set up to complete your account set up.";
                            }
                            else
                            {                                
                                string status = await _ds.CreateNewOtpCode(_vm.UserID, _vm.OTPType);
                                if (status == "OK")
                                {
                                    keyboardService.HideKeyboard();
                                    LoadOTPType();
                                }
                                else if (status == "MaxLimit")
                                {
                                    _vm.MaxLimitFailure = true;
                                }
                                else
                                {
                                    _vm.ResendFailure = true;
                                }
                            }                            
                        }
                    }
                }
                else if (_vm.OTPValidated)
                {
                    await ValidatePassword();
                    if (_vm.PasswordSameSame && _vm.PasswordRequired && _vm.PasswordStrong)
                    {
                        RegisterModel User = new RegisterModel();
                        User.Salt = await _ds.GetFamilyUserSaltAsync(_vm.UserEmail);
                        User.Password = _vm.Password;
                        User = _pt.ResetUserPassword(User);

                        List<PatchDoc> UserDetails = new List<PatchDoc>();

                        PatchDoc NewSalt = new PatchDoc
                        {
                            op = "replace",
                            path = "/Salt",
                            value = User.Salt
                        };

                        PatchDoc NewPassword = new PatchDoc
                        {
                            op = "replace",
                            path = "/Password",
                            value = User.Password
                        };

                        UserDetails.Add(NewSalt);
                        UserDetails.Add(NewPassword);

                        string status = await _ds.PatchFamilyUserAccount(_vm.UserID, UserDetails);

                        if(status == "OK")
                        {
                            keyboardService.HideKeyboard();
                            App.CurrentPopUp = null;
                            this.Close("OK");
                        }
                        else
                        {
                            _vm.PasswordResetFailure = true;
                        }
                    }
                }
                else
                {
                    if (_vm.OTP.OTPCode.Length < 6)
                    {
                        _vm.OTPRequired = true;
                    }
                    else
                    {
                        _vm.OTP.OTPExpiryTime = DateTime.UtcNow;
                        _vm.OTP.UserAccountID = _vm.UserID;
                        _vm.OTP.OTPType = _vm.OTPType;
                        string status = await _ds.ValidateOTPCodeEmail(_vm.OTP);
                        if (status == "OK")
                        {
                            _vm.OTPValidated = true;
                            keyboardService.HideKeyboard();
                            LoadOTPType();
                        }
                        else
                        {
                            _vm.OTPNotFound = true;
                        }
                    }
                }
            }
            else if (_vm.OTPType == "ShareBudget")
            {
                if (_vm.OTP.OTPCode.Length < 6)
                {
                    _vm.OTPRequired = true;
                }
                else
                {
                    _vm.OTP.OTPExpiryTime = DateTime.UtcNow;
                    _vm.OTP.UserAccountID = _vm.UserID;
                    _vm.OTP.OTPType = _vm.OTPType;
                    string status = await _ds.ValidateOTPCodeShareBudget(_vm.OTP, _vm.ShareBudgetRequest.SharedBudgetRequestID);
                    if (status == "OK")
                    {
                        _vm.ShareBudgetRequest.IsVerified = true;
                        _vm.OTPValidated = true;
                        keyboardService.HideKeyboard();
                        App.CurrentPopUp = null;
                        this.Close(_vm.ShareBudgetRequest);
                    }
                    else if(status == "Error")
                    {
                        return;
                    }
                    else
                    {
                        _vm.OTPNotFound = true;
                    }
                }
            }
            else if (_vm.OTPType == "FamilyAccountCreation")
            {
                if (_vm.UserID == 0)
                {
                    await ValidateEmail();
                    if (_vm.EmailValid && _vm.EmailRequired)
                    {
                        _vm.UserID = await _ds.GetUserIdFamilyAccountFromEmail(_vm.UserEmail);
                        _vm.OTP.UserAccountID = _vm.UserID;                        

                        if (_vm.UserID == 0)
                        {
                            _vm.EmailNotFound = true;
                        }
                        else
                        {
                            FamilyUserAccount User = await _ds.GetFamilyUserDetailsAsync(_vm.UserEmail);
                            if (await SecureStorage.Default.GetAsync("Session") != null)
                            {
                                SecureStorage.Default.Remove("Session");
                            }

                            AuthDetails Auth = new()
                            {
                                ClientID = DeviceInfo.Current.Name,
                                ClientSecret = User.Password,
                                UserID = User.UniqueUserID
                            };

                            SessionDetails Session = await _ds.CreateSession(Auth);
                            string SessionString = JsonConvert.SerializeObject(Session);
                            await SecureStorage.Default.SetAsync("Session", SessionString);

                            if(User.IsActive != true)
                            {
                                _vm.FamilyAccountSetUpFailure = true;
                                _vm.FamilyAccountSetUpFailureText = "your parent or guardian hasn't activated your account.";
                            }
                            else if(User.IsConfirmed == true)
                            {
                                _vm.FamilyAccountSetUpFailure = true;
                                _vm.FamilyAccountSetUpFailureText = "you have already set up your account, use the password reset if you are having issues logging in";
                            }
                            else
                            {
                                keyboardService.HideKeyboard();
                                LoadOTPType();
                            }

                            if (await SecureStorage.Default.GetAsync("Session") != null)
                            {
                                SecureStorage.Default.Remove("Session");
                            }
                        }
                    }
                }
                else if (_vm.OTPValidated)
                {
                    await ValidatePassword();
                    if (_vm.PasswordSameSame && _vm.PasswordRequired && _vm.PasswordStrong)
                    {
                        FamilyUserAccount familyUserAccount = await _ds.GetFamilyUserDetailsAsync(_vm.UserEmail);
                        if (await SecureStorage.Default.GetAsync("Session") != null)
                        {
                            SecureStorage.Default.Remove("Session");
                        }

                        AuthDetails Auth = new()
                        {
                            ClientID = DeviceInfo.Current.Name,
                            ClientSecret = familyUserAccount.Password,
                            UserID = familyUserAccount.UniqueUserID
                        };

                        SessionDetails Session = await _ds.CreateSession(Auth);
                        string SessionString = JsonConvert.SerializeObject(Session);
                        await SecureStorage.Default.SetAsync("Session", SessionString);

                        RegisterModel User = new RegisterModel();
                        User.Salt = await _ds.GetFamilyUserSaltAsync(_vm.UserEmail);
                        User.Password = _vm.Password;
                        User = _pt.ResetUserPassword(User);

                        List<PatchDoc> UserDetails = new List<PatchDoc>();

                        PatchDoc NewSalt = new PatchDoc
                        {
                            op = "replace",
                            path = "/Salt",
                            value = User.Salt
                        };

                        PatchDoc NewPassword = new PatchDoc
                        {
                            op = "replace",
                            path = "/Password",
                            value = User.Password
                        };

                        UserDetails.Add(NewSalt);
                        UserDetails.Add(NewPassword);

                        try
                        {
                            string status = await _ds.PatchFamilyUserAccount(_vm.UserID, UserDetails);
                            status = await _ds.ConfirmFamilyAccountSetUp(_vm.UserID) != "OK" ? "Error" : status;

                            if (status == "OK")
                            {
                                keyboardService.HideKeyboard();
                                App.CurrentPopUp = null;
                                this.Close("OK");
                            }
                            else
                            {
                                _vm.FamilyAccountSetUpFailure = true;
                                _vm.FamilyAccountSetUpFailureText = "we weren't able to set up your account please try again.";
                            }
                        }
                        catch (Exception)
                        {
                            _vm.FamilyAccountSetUpFailure = true;
                            _vm.FamilyAccountSetUpFailureText = "we weren't able to set up your account please try again.";
                        }

                        if (await SecureStorage.Default.GetAsync("Session") != null)
                        {
                            SecureStorage.Default.Remove("Session");
                        }
                    }
                }
                else
                {
                    if (_vm.OTP.OTPCode.Length < 6)
                    {
                        _vm.OTPRequired = true;
                    }
                    else
                    {
                        _vm.OTP.OTPExpiryTime = DateTime.UtcNow;
                        _vm.OTP.UserAccountID = _vm.UserID;
                        _vm.OTP.OTPType = _vm.OTPType;
                        string status = await _ds.ValidateOTPCodeFamilyAccount(_vm.OTP);
                        if (status == "OK")
                        {
                            _vm.OTPValidated = true;
                            keyboardService.HideKeyboard();
                            LoadOTPType();
                        }
                        else
                        {
                            _vm.OTPNotFound = true;                            
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "PopUpOTP", "ValidateOTP_Popup");
        }

    }

    private async Task ValidateEmail()
    {
        await Task.Delay(1);
        if (string.IsNullOrWhiteSpace(_vm.UserEmail))
        {
            _vm.EmailRequired = false;
        }
        else
        {
            _vm.EmailRequired = true;
        }

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if(!Regex.IsMatch(_vm.UserEmail ?? "", pattern))
        {
            _vm.EmailValid = false;
        }
        else
        {
            _vm.EmailValid = true;
        }
    }

    private async Task ValidatePassword()
    {
        await Task.Delay(1);
        if (string.IsNullOrWhiteSpace(_vm.Password))
        {
            _vm.PasswordRequired = false;
        }
        else
        {
            _vm.PasswordRequired = true;
        }

        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";
        if (!Regex.IsMatch(_vm.Password ?? "", pattern))
        {
            _vm.PasswordStrong = false;
        }
        else
        {
            _vm.PasswordStrong = true;
        }

        if (!string.Equals(_vm.Password ?? "", _vm.PasswordConfirm ?? "", StringComparison.OrdinalIgnoreCase))
        {
            _vm.PasswordSameSame = false;
        }
        else
        {
            _vm.PasswordSameSame = true;
        }
    }

    private void Close_Window(object sender, EventArgs e)
    {
        App.CurrentPopUp = null;
        this.Close("User Closed");
    }

    private void txtEmail_Loaded(object sender, EventArgs e)
    {
        entEmailBox.Focus();
    }

    private void entOTP_TextChanged(object sender, TextChangedEventArgs e)
    {
        _vm.OTP.OTPCode = entOTPOne.Text + entOTPTwo.Text + entOTPThree.Text + entOTPFour.Text + entOTPFive.Text + entOTPSix.Text;

        var entOTP = (FocusedEntry)sender;

        if (entOTP.Identifier == "entOTPOne")
        {
            if (!string.IsNullOrEmpty(e.NewTextValue))
            {
                entOTPTwo.Focus();
            }
        }
        else if (entOTP.Identifier == "entOTPTwo")
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                
            }
            else
            {
                entOTPThree.Focus();
            }

        }
        else if (entOTP.Identifier == "entOTPThree")
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                
            }
            else
            {
                entOTPFour.Focus();
            }
        }
        else if (entOTP.Identifier == "entOTPFour")
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                
            }
            else
            {
                entOTPFive.Focus();
            }
        }
        else if (entOTP.Identifier == "entOTPFive")
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                
            }
            else
            {
                entOTPSix.Focus();
            }
        }
        else if (entOTP.Identifier == "entOTPSix")
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                
            }
        }
    }

    private void entOTP_Focused(object sender, FocusEventArgs e)
    {
        _vm.OTPNotFound = false;
        _vm.OTPRequired = false;
        _vm.OTPCopyContentValid = false;
    }
}