﻿using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using DailySpendWebApp.Models;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class LogonPageViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;

        public LogonPageViewModel(IRestDataService ds, IProductTools pt)
        {
            Title = "Sign In";
            _ds = ds;
            _pt = pt;
        }

        [ObservableProperty]
        private string  email;

        [ObservableProperty]
        private string  password;

        [ObservableProperty]
        private bool  rememberMe;

        [ObservableProperty]
        private bool  emailRequired;

        [ObservableProperty]
        private bool  emailValid;

        [ObservableProperty]
        private bool  passwordRequired;

        [ObservableProperty]
        private bool  emailValidatedSuccess;

        [ObservableProperty]
        private bool  resetPasswordSuccess;

        [ObservableProperty]
        private bool  resetPasswordFail;

        public bool PageIsValid()
        {
            bool IsValid = true;
            if (Password == "" || Password == null)
            {
                PasswordRequired = false;
                IsValid = false;
            }

            if (Email == "" || Email == null)
            {
                EmailRequired = false;
                IsValid = false;
            }

            return IsValid;
        }

        private async Task ResetSuccessFailureMessage()
        {
            ResetPasswordFail = false;
            ResetPasswordSuccess = false;
            EmailValidatedSuccess = false;
        }

        [RelayCommand]
        async void NavigateRegister()
        {
            await Shell.Current.GoToAsync($"../{nameof(RegisterPage)}");
        }

        [RelayCommand]
        async void ResetPassword()
        {
            try
            {
                await ResetSuccessFailureMessage();

                var popup = new PopUpOTP(0, new PopUpOTPViewModel(new RestDataService(), new ProductTools(new RestDataService())), "ResetPassword", new ProductTools(new RestDataService()), new RestDataService());
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if((string)result.ToString() == "OK")
                {
                    ResetPasswordSuccess = true;
                }
                else
                {
                    ResetPasswordFail = true;
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "LogonPage", "ResetPassword");
            }
        }

        [RelayCommand]
        async void Login()
        {
            try
            {
                await ResetSuccessFailureMessage();

                if (!PageIsValid())
                {
                    return;
                }

                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                if (!string.IsNullOrEmpty(Email))
                {
                    if (!string.IsNullOrEmpty(Password))
                    {
                        UserDetailsModel userDetails = new UserDetailsModel();

                        string salt = await _ds.GetUserSaltAsync(Email);
                        if (salt is null)
                        {
                            return;
                        }

                        switch (salt)
                        {
                            case "User not found":
                                await App.CurrentPopUp.CloseAsync();
                                App.CurrentPopUp = null;
                                await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                break;
                            case not "":

                                userDetails = await _ds.GetUserDetailsAsync(Email);

                                if (userDetails == null)
                                {
                                    await App.CurrentPopUp.CloseAsync();
                                    App.CurrentPopUp = null;
                                    await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                }
                                else
                                {
                                    string HashPassword = _pt.GenerateHashedPassword(Password, salt);
                                    if(userDetails.Password != HashPassword)
                                    {
                                        await App.CurrentPopUp.CloseAsync();
                                        App.CurrentPopUp = null;
                                        await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                    }
                                    else
                                    {
                                        if (!userDetails.IsEmailVerified)
                                        {
                                            await App.CurrentPopUp.CloseAsync();
                                            App.CurrentPopUp = null;
                                            bool ValidateEmail = await Application.Current.MainPage.DisplayAlert("Mmmm, can't be doing that!", "You haven't verified your email! Would you like to now so you can log in?", "Verify email","Not now");
                                            if(ValidateEmail)
                                            {
                                                string status = await _ds.CreateNewOtpCode(userDetails.UserID, "ValidateEmail");
                                                if (status == "OK" || status == "MaxLimit")
                                                {
                                                    var popup = new PopUpOTP(userDetails.UserID, new PopUpOTPViewModel(new RestDataService(), new ProductTools(new RestDataService())), "ValidateEmail", new ProductTools(new RestDataService()), new RestDataService());
                                                    var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                                                    if ((string)result.ToString() == "OK")
                                                    {
                                                        EmailValidatedSuccess = true;
                                                    }
                                                }

                                                Email = "";
                                                Password = "";
                                            }
                                        }
                                        else
                                        {
                                            if(RememberMe)
                                            {
                                                userDetails.SessionExpiry = DateTime.UtcNow.AddDays(App.SessionPeriod);
                                            }
                                            else
                                            {
                                                userDetails.SessionExpiry = DateTime.UtcNow.AddDays(0);
                                            }

                                            if (Preferences.ContainsKey(nameof(App.UserDetails)))
                                            {
                                                Preferences.Remove(nameof(App.UserDetails));
                                            }

                                            if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                                            {
                                                Preferences.Remove(nameof(App.DefaultBudgetID));
                                            }

                                            string userDetailsStr = JsonConvert.SerializeObject(userDetails);
                                            Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                                            Preferences.Set(nameof(App.DefaultBudgetID), userDetails.DefaultBudgetID);

                                            App.UserDetails = userDetails;
                                            App.DefaultBudgetID = userDetails.DefaultBudgetID;
                                            App.HasVisitedCreatePage = false;
                                            await _pt.SetSubDetails();

                                            if (await SecureStorage.Default.GetAsync("FirebaseToken") != null)
                                            {
                                                int FirebaseID = Convert.ToInt32(await SecureStorage.Default.GetAsync("FirebaseID"));

                                                FirebaseDevices UserDevice = new FirebaseDevices
                                                {
                                                    FirebaseDeviceID = FirebaseID,
                                                    UserAccountID = userDetails.UserID,
                                                    LoginExpiryDate = userDetails.SessionExpiry,
                                                    FirebaseToken = SecureStorage.Default.GetAsync("FirebaseToken").Result
                                                };

                                                try
                                                {
                                                    await _ds.UpdateDeviceUserDetails(UserDevice);
                                                }
                                                catch (Exception ex)
                                                {
                                                    //Log as non fatal error
                                                }
                                            }

                                            //await _pt.LoadTabBars(App.UserDetails.SubscriptionType, App.UserDetails.SubscriptionExpiry, App.UserDetails.DefaultBudgetType);
                                            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                                        }
                                    }
                                }

                                break;
                            default:
                                throw new Exception("Server Connectivity");                            
                        }
                    }
                    else
                    {
                        await App.CurrentPopUp.CloseAsync();
                        App.CurrentPopUp = null;
                        await Application.Current.MainPage.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                    }
                }
                else 
                {
                    IsButtonBusy = false;
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                    await Application.Current.MainPage.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "LogonPage", "Login");
            }
        }        

    }
}
