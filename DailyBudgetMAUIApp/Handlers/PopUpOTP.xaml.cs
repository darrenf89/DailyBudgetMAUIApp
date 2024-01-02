using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpOTP : Popup
{

    private readonly PopUpOTPViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public PopUpOTP(int UserID, PopUpOTPViewModel viewModel, string OTPType, IProductTools pt, IRestDataService ds)
    {
        InitializeComponent();

        double width = viewModel.PopupWidth -11;
        Rect rt = new Rect(width, 123, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
        AbsLayout.SetLayoutBounds(btnClose, rt);

        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        if (OTPType == "ShareBudget")
        {
            _vm.ShareBudgetRequest = _ds.GetShareBudgetRequestByID(UserID).Result;
            int OTPUserID = _ds.GetUserIdFromEmail(_vm.ShareBudgetRequest.SharedByUserEmail).Result;
            _vm.UserID = OTPUserID;
        }
        else
        {
            _vm.UserID = UserID;
        }

        _vm.OTPType = OTPType;
        _vm.OTP = new OTP();
        _vm.OTP.OTPCode = "";

        LoadOTPType();
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
    }

    private void ResetSuccessFailureMessage()
    {        
        _vm.ResendSuccess = false;
        _vm.MaxLimitFailure = false;
        _vm.ResendFailure = false;
        _vm.PasswordResetFailure = false;
        _vm.EmailNotFound = false;
        _vm.OTPRequired = false;
    }

    private void ValidateOTP_Popup(object sender, EventArgs e)
	{
        ResetSuccessFailureMessage();
        if (_vm.OTPType == "ValidateEmail")
        {
            if (_vm.UserID == 0)
            {
                if(_vm.EmailRequired && _vm.EmailValid)
                {
                    _vm.UserID = _ds.GetUserIdFromEmail(_vm.UserEmail).Result;
                    _vm.OTP.UserAccountID = _vm.UserID;

                    if (_vm.UserID == 0)
                    {
                        _vm.EmailNotFound = true;
                    }
                    else
                    {
                        LoadOTPType();
                    }                        
                }
            }
            else
            {
                if(_vm.OTP.OTPCode.Length < 8)
                {
                    _vm.OTPRequired = true;
                }
                else
                {
                    _vm.OTP.OTPExpiryTime = DateTime.UtcNow;
                    _vm.OTP.UserAccountID = _vm.UserID;
                    _vm.OTP.OTPType = _vm.OTPType;
                    string status = _ds.ValidateOTPCodeEmail(_vm.OTP).Result;
                    if(status == "OK")
                    {
                        _vm.OTPValidated = true;
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
                if (_vm.EmailRequired && _vm.EmailValid)
                {
                    _vm.UserID = _ds.GetUserIdFromEmail(_vm.UserEmail).Result;
                    _vm.OTP.UserAccountID = _vm.UserID;

                    if (_vm.UserID == 0)
                    {
                        _vm.EmailNotFound = true;
                    }
                    else
                    {
                        string status = _ds.CreateNewOtpCode(_vm.UserID, _vm.OTPType).Result;
                        if (status == "OK")
                        {
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
                if(_vm.PasswordSameSame && _vm.PasswordStrong && _vm.PasswordRequired)
                {
                    RegisterModel User = new RegisterModel();
                    User.Salt = _ds.GetUserSaltAsync(_vm.UserEmail).Result;
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

                    string status = _ds.PatchUserAccount(_vm.UserID, UserDetails).Result;

                    if(status == "OK")
                    {
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
                if (_vm.OTP.OTPCode.Length < 8)
                {
                    _vm.OTPRequired = true;
                }
                else
                {
                    _vm.OTP.OTPExpiryTime = DateTime.UtcNow;
                    _vm.OTP.UserAccountID = _vm.UserID;
                    _vm.OTP.OTPType = _vm.OTPType;
                    string status = _ds.ValidateOTPCodeEmail(_vm.OTP).Result;
                    if (status == "OK")
                    {
                        _vm.OTPValidated = true;
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
            if (_vm.OTP.OTPCode.Length < 8)
            {
                _vm.OTPRequired = true;
            }
            else
            {
                _vm.OTP.OTPExpiryTime = DateTime.UtcNow;
                _vm.OTP.UserAccountID = _vm.UserID;
                _vm.OTP.OTPType = _vm.OTPType;
                string status = _ds.ValidateOTPCodeShareBudget(_vm.OTP, _vm.ShareBudgetRequest.SharedBudgetRequestID).Result;
                if (status == "OK")
                {
                    _vm.ShareBudgetRequest.IsVerified = true;
                    _vm.OTPValidated = true;
                    this.Close(_vm.ShareBudgetRequest);
                }
                else
                {
                    _vm.OTPNotFound = true;
                }
            }
        }
    }

    private void Close_Window(object sender, EventArgs e)
    { 
        this.Close("User Closed");
    }

    private void txtEmail_Loaded(object sender, EventArgs e)
    {
        entEmailBox.Focus();
    }

    private void entOTP_TextChanged(object sender, TextChangedEventArgs e)
    {
        _vm.OTP.OTPCode = entOTPOne.Text + entOTPTwo.Text + entOTPThree.Text + entOTPFour.Text + entOTPFive.Text + entOTPSix.Text + entOTPSeven.Text + entOTPEight.Text;

        var entOTP = (BorderlessEntry)sender;

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
                entOTPOne.Focus();
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
                entOTPTwo.Focus();
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
                entOTPThree.Focus();
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
                entOTPFour.Focus();
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
                entOTPFive.Focus();
            }
            else
            {
                entOTPSeven.Focus();
            }
        }
        else if (entOTP.Identifier == "entOTPSeven")
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                entOTPSix.Focus();
            }
            else
            {
                entOTPEight.Focus();
            }
        }
        else if (entOTP.Identifier == "entOTPEight")
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                entOTPSeven.Focus();
            }
        }
    }

    private void entOTP_Focused(object sender, FocusEventArgs e)
    {
        _vm.OTPNotFound = false;
        _vm.OTPRequired = false;
    }
}