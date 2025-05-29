using DailyBudgetMAUIApp.DataServices;
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
        async Task NavigateRegister()
        {
            await Shell.Current.GoToAsync($"../{nameof(RegisterPage)}");
        }

        [RelayCommand]
        async Task ResetPassword()
        {
            try
            {
                await ResetSuccessFailureMessage();

                var popup = new PopUpOTP(0, new PopUpOTPViewModel(IPlatformApplication.Current.Services.GetService<IRestDataService>(), IPlatformApplication.Current.Services.GetService<IProductTools>()), "ResetPassword", IPlatformApplication.Current.Services.GetService<IProductTools>(), IPlatformApplication.Current.Services.GetService<IRestDataService>());
                App.CurrentPopUp = popup;
                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

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
        async Task Login()
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
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                if (!string.IsNullOrEmpty(Email))
                {
                    if (!string.IsNullOrEmpty(Password))
                    {
                        UserDetailsModel userDetails = new UserDetailsModel();
                        string salt = "";
                        try
                        {
                            salt = await _ds.GetUserSaltAsync(Email);
                            if (salt is null)
                            {
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("User not found"))
                            {
                                await App.CurrentPopUp.CloseAsync();
                                App.CurrentPopUp = null;
                                await Application.Current.Windows[0].Page.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                            }
                            else
                            {
                                throw;
                            }
                        }

                        switch (salt)
                        {
                            case "User not found":
                                await App.CurrentPopUp.CloseAsync();
                                App.CurrentPopUp = null;
                                await Application.Current.Windows[0].Page.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                break;
                            case not "":

                                userDetails = await _ds.GetUserDetailsAsync(Email);

                                if (userDetails == null)
                                {
                                    await App.CurrentPopUp.CloseAsync();
                                    App.CurrentPopUp = null;
                                    await Application.Current.Windows[0].Page.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                }
                                else
                                {
                                    string HashPassword = _pt.GenerateHashedPassword(Password, salt);
                                    if(userDetails.Password != HashPassword)
                                    {
                                        await App.CurrentPopUp.CloseAsync();
                                        App.CurrentPopUp = null;
                                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                    }
                                    else
                                    {
                                        if (!userDetails.IsEmailVerified)
                                        {
                                            await App.CurrentPopUp.CloseAsync();
                                            App.CurrentPopUp = null;
                                            bool ValidateEmail = await Application.Current.Windows[0].Page.DisplayAlert("Mmmm, can't be doing that!", "You haven't verified your email! Would you like to now so you can log in?", "Verify email","Not now");
                                            if(ValidateEmail)
                                            {
                                                string status = await _ds.CreateNewOtpCode(userDetails.UserID, "ValidateEmail");
                                                if (status == "OK" || status == "MaxLimit")
                                                {
                                                    var popup = new PopUpOTP(userDetails.UserID, new PopUpOTPViewModel(IPlatformApplication.Current.Services.GetService<IRestDataService>(), IPlatformApplication.Current.Services.GetService<IProductTools>()), "ValidateEmail", IPlatformApplication.Current.Services.GetService<IProductTools>(), IPlatformApplication.Current.Services.GetService<IRestDataService>());
                                                    App.CurrentPopUp = popup;
                                                    var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

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

                                            if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
                                            {
                                                Preferences.Remove(nameof(App.IsFamilyAccount));
                                            }

                                            string userDetailsStr = JsonConvert.SerializeObject(userDetails);
                                            Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                                            Preferences.Set(nameof(App.DefaultBudgetID), userDetails.DefaultBudgetID);

                                            App.UserDetails = userDetails;
                                            App.FamilyUserDetails = null;
                                            App.DefaultBudgetID = userDetails.DefaultBudgetID;
                                            App.HasVisitedCreatePage = false;
                                            App.IsFamilyAccount = false;
                                            await _pt.SetSubDetails();

                                            if (await SecureStorage.Default.GetAsync("Session") != null)
                                            {
                                                SecureStorage.Default.Remove("Session");
                                            }

                                            AuthDetails Auth = new()
                                            {
                                                ClientID = DeviceInfo.Current.Name,
                                                ClientSecret = userDetails.Password,
                                                UserID = userDetails.UniqueUserID
                                            };

                                            SessionDetails Session = await _ds.CreateSession(Auth);
                                            string SessionString = JsonConvert.SerializeObject(Session);
                                            await SecureStorage.Default.SetAsync("Session", SessionString);

                                            if (await SecureStorage.Default.GetAsync("FirebaseToken") != null)
                                            {
                                                int FirebaseID = Convert.ToInt32(await SecureStorage.Default.GetAsync("FirebaseID"));

                                                FirebaseDevices UserDevice = new FirebaseDevices
                                                {
                                                    FirebaseDeviceID = FirebaseID,
                                                    UserAccountID = userDetails.UniqueUserID,
                                                    LoginExpiryDate = userDetails.SessionExpiry,
                                                    FirebaseToken = await SecureStorage.Default.GetAsync("FirebaseToken")
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

                                            BudgetSettingValues Settings = await _ds.GetBudgetSettingsValues(userDetails.DefaultBudgetID);
                                            App.CurrentSettings = Settings;

                                            _pt.SetCultureInfo(App.CurrentSettings);

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
                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                    }
                }
                else 
                {
                    IsButtonBusy = false;
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                    await Application.Current.Windows[0].Page.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "LogonPage", "Login");
            }
        }

        [RelayCommand]
        async Task NavigateFamilySignPage()
        {
            try
            {
                await Shell.Current.GoToAsync($"{nameof(FamilyAccountLogonPage)}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "LogonPage", "NavigateFamilySignPage");
            }
        }

    }
}
