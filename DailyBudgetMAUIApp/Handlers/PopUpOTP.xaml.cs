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

        _vm.OTPType = OTPType;
        _vm.UserID = UserID;
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
                lblTitle.Text = "Verify Your Email";
                lblDescription.Text = "Please enter the email address attached to your account";
                btnSave.Text = "Confirm Email";

                entEmail.IsVisible = true;
                entOTPCode.IsVisible = false;
            }
            else
            {
                lblTitle.Text = "Verify Your Email";
                lblDescription.Text = "Please enter the One-time Passcode we sent to your email, can't find it request again using the resend button.";
                btnSave.Text = "Verify Account";

                entEmail.IsVisible = false;
                entOTPCode.IsVisible = true;

                entOTPOne.Focus();
            }
        }
        else if (_vm.OTPType == "ResetPassword")
        {

        }
    }

    private void ValidateOTP_Popup(object sender, EventArgs e)
	{
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

        if(entOTP.Identifier == "entOTPOne")
        {
            entOTPTwo.Focus();
        }
        else if(entOTP.Identifier == "entOTPTwo")
        {
            entOTPThree.Focus();
        }
        else if(entOTP.Identifier == "entOTPThree")
        {
            entOTPFour.Focus();
        }
        else if(entOTP.Identifier == "entOTPFour")
        {
            entOTPFive.Focus();
        }
        else if(entOTP.Identifier == "entOTPFive")
        {
            entOTPSix.Focus();
        }
        else if(entOTP.Identifier == "entOTPSix")
        {
            entOTPSeven.Focus();
        }
        else if(entOTP.Identifier == "entOTPSeven")
        {
            entOTPEight.Focus();
        }
    }

    private void entOTP_Focused(object sender, FocusEventArgs e)
    {
        _vm.OTPNotFound = false;
        _vm.OTPRequired = false;
    }
}