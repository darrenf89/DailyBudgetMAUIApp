using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Diagnostics;
using CommunityToolkit.Maui.Views;
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
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _rememberMe;

        [ObservableProperty]
        private bool _emailRequired;

        [ObservableProperty]
        private bool _emailValid;

        [ObservableProperty]
        private bool _passwordRequired;

        [ObservableProperty]
        private bool _emailValidatedSuccess;

        [ObservableProperty]
        private bool _resetPasswordSuccess;

        [ObservableProperty]
        private bool _resetPasswordFail;

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

        [ICommand]
        async void NavigateRegister()
        {
            await Shell.Current.GoToAsync($"../{nameof(RegisterPage)}");
        }

        [ICommand]
        async void ResetPassword()
        {
            await ResetSuccessFailureMessage();

            var popup = new PopUpOTP(0, new PopUpOTPViewModel(new RestDataService()), "ResetPassword", new ProductTools(new RestDataService()), new RestDataService());
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

        [ICommand]
        async void Login()
        {
            await ResetSuccessFailureMessage();
            try
            {
                if (!PageIsValid())
                {
                    return;
                }

                var page = new LoadingPage();
                await Application.Current.MainPage.Navigation.PushModalAsync(page);

                if (!string.IsNullOrEmpty(Email))
                {
                    if (!string.IsNullOrEmpty(Password))
                    {
                        UserDetailsModel userDetails = new UserDetailsModel();

                        string salt = await _ds.GetUserSaltAsync(Email);
                        
                        switch (salt)
                        {
                            case "User not found":
                                await Application.Current.MainPage.Navigation.PopModalAsync();
                                await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                break;
                            case not "":

                                userDetails = await _ds.GetUserDetailsAsync(Email);

                                if (userDetails == null)
                                {
                                    await Application.Current.MainPage.Navigation.PopModalAsync();
                                    await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                }
                                else
                                {
                                    string HashPassword = _pt.GenerateHashedPassword(Password, salt);
                                    if(userDetails.Password != HashPassword)
                                    {
                                        await Application.Current.MainPage.Navigation.PopModalAsync();
                                        await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                    }
                                    else
                                    {
                                        if (!userDetails.isEmailVerified)
                                        {
                                            await Application.Current.MainPage.Navigation.PopModalAsync();
                                            bool ValidateEmail = await Application.Current.MainPage.DisplayAlert("Mmmm, can't be doing that!", "You haven't verified your email! Would you like to now so you can log in?", "Verify email","Not now");
                                            if(ValidateEmail)
                                            {
                                                string status = await _ds.CreateNewOtpCode(userDetails.UserID, "ValidateEmail");
                                                if (status == "OK" || status == "MaxLimit")
                                                {
                                                    var popup = new PopUpOTP(userDetails.UserID, new PopUpOTPViewModel(new RestDataService()), "ValidateEmail", new ProductTools(new RestDataService()), new RestDataService());
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

                                            if (await SecureStorage.Default.GetAsync("FirebaseToken") != null)
                                            {
                                                int FirebaseID = Convert.ToInt32(await SecureStorage.Default.GetAsync("FirebaseID"));

                                                FirebaseDevices UserDevice = new FirebaseDevices
                                                {
                                                    FirebaseDeviceID = FirebaseID,
                                                    UserAccountID = userDetails.UserID,
                                                    LoginExpiryDate = userDetails.SessionExpiry
                                                };

                                                await _ds.UpdateDeviceUserDetails(UserDevice);
                                            }

                                            //TODO: Sign in or update User Session and save to DB
                                            await Application.Current.MainPage.Navigation.PopModalAsync();
                                            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                                        }
                                    }
                                }

                                break;
                            default:
                                throw new Exception("Error Calling API");                            
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.Navigation.PopModalAsync();
                        await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                    }
                }
                else 
                {
                    IsButtonBusy = false;
                    await Application.Current.MainPage.Navigation.PopModalAsync();
                    await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($" --> {ex.Message}");
                ErrorLog Error = await _pt.HandleCatchedException(ex, "LogonPage", "Login");
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }
        

    }
}
