using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using DailySpendWebApp.Models;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class FamilyAccountLogonPageViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;
        private readonly IModalPopupService _ps;

        public FamilyAccountLogonPageViewModel(IRestDataService ds, IProductTools pt, IModalPopupService ps)
        {
            Title = "Sign In";
            _ds = ds;
            _pt = pt;
            _ps = ps;
        }

        [ObservableProperty]
        public partial string Email { get; set; }

        [ObservableProperty]
        public partial string Password { get; set; }

        [ObservableProperty]
        public partial bool RememberMe { get; set; }

        [ObservableProperty]
        public partial bool EmailRequired { get; set; }

        [ObservableProperty]
        public partial bool EmailValid { get; set; }

        [ObservableProperty]
        public partial bool PasswordRequired { get; set; }

        [ObservableProperty]
        public partial bool EmailValidatedSuccess { get; set; }

        [ObservableProperty]
        public partial bool ResetPasswordSuccess { get; set; }

        [ObservableProperty]
        public partial bool AccountCreationFail { get; set; }

        [ObservableProperty]
        public partial bool AccountCreationSuccess { get; set; }

        [ObservableProperty]
        public partial bool ResetPasswordFail { get; set; }


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
            await Task.Delay(1);
            ResetPasswordFail = false;
            ResetPasswordSuccess = false;
            EmailValidatedSuccess = false;
            AccountCreationSuccess = false;
            AccountCreationFail = false;
        }


        [RelayCommand]
        async Task Register()
        {
            try
            {
                await ResetSuccessFailureMessage();

                var queryAttributes = new Dictionary<string, object>
                {
                    [nameof(PopUpOTPViewModel.UserID)] = 0,
                    [nameof(PopUpOTPViewModel.OTPType)] = "FamilyAccountCreation"
                };

                var popupOptions = new PopupOptions
                {
                    CanBeDismissedByTappingOutsideOfPopup = false,
                    PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
                };

                IPopupResult<object> popupResult = await _ps.PopupService.ShowPopupAsync<PopUpOTP, object>(
                    Shell.Current,
                    options: popupOptions,
                    shellParameters: queryAttributes,
                    cancellationToken: CancellationToken.None
                );

                if ((string)popupResult.Result.ToString() == "OK")
                {
                    AccountCreationSuccess = true;
                }
                else
                {
                    AccountCreationFail = true;
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountLogonPage", "Register");
            }

        }

        [RelayCommand]
        async Task ResetPassword()
        {
            try
            {
                await ResetSuccessFailureMessage();

                var queryAttributes = new Dictionary<string, object>
                {
                    [nameof(PopUpOTPViewModel.UserID)] = 0,
                    [nameof(PopUpOTPViewModel.OTPType)] = "ResetPasswordFamily"
                };

                var popupOptions = new PopupOptions
                {
                    CanBeDismissedByTappingOutsideOfPopup = false,
                    PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
                };

                IPopupResult<object> popupResult = await _ps.PopupService.ShowPopupAsync<PopUpOTP, object>(
                    Shell.Current,
                    options: popupOptions,
                    shellParameters: queryAttributes,
                    cancellationToken: CancellationToken.None
                );

                if((string)popupResult.Result.ToString() == "OK")
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
                await _pt.HandleException(ex, "FamilyAccountLogonPage", "ResetPassword");
            }
        }

        [RelayCommand]
        async Task NavigateNormalSignPage()
        {
            try
            {
                await Shell.Current.GoToAsync($"{nameof(LogonPage)}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountLogonPage", "NavigateNormalSignPage");
            }
        }

        [RelayCommand]
        async Task Login()
        {
            try
            {
                await Task.Delay(1);
                await ResetSuccessFailureMessage();

                if (!PageIsValid())
                {
                    return;
                }

                await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());
                if (!string.IsNullOrEmpty(Email))
                {
                    if (!string.IsNullOrEmpty(Password))
                    {
                        FamilyUserAccount userDetails = new FamilyUserAccount();
                        string salt = "";
                        try
                        {
                            salt = await _ds.GetFamilyUserSaltAsync(Email);
                            if (salt is null)
                            {
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("User not found"))
                            {
                                await _ps.CloseAsync<PopUpPage>();
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
                                await _ps.CloseAsync<PopUpPage>();
                                await Application.Current.Windows[0].Page.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                                break;
                            case not "":

                                userDetails = await _ds.GetFamilyUserDetailsAsync(Email);

                                if (userDetails == null)
                                {
                                    await _ps.CloseAsync<PopUpPage>();
                                    await Application.Current.Windows[0].Page.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                                }
                                else
                                {
                                    if (!userDetails.IsActive)
                                    {
                                        await _ps.CloseAsync<PopUpPage>();
                                        await Application.Current.Windows[0].Page.DisplayAlert("You aren't account isn't active", "This account is no longer active, if you'd still like to budget please get the owner of the parent account to reactive or create your own account with us!", "OK");
                                        return;
                                    }

                                    if (!userDetails.IsConfirmed)
                                    {
                                        await _ps.CloseAsync<PopUpPage>();
                                        await Application.Current.Windows[0].Page.DisplayAlert("Please confirm your set up!", "You haven't completed your account set up, please click complete set up to set up a password and finalise you account!", "OK");
                                        return;
                                    }
                                    
                                    string HashPassword = _pt.GenerateHashedPassword(Password, salt);
                                    if(userDetails.Password != HashPassword)
                                    {
                                        await _ps.CloseAsync<PopUpPage>();
                                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                                    }
                                    else
                                    {
                                        if (RememberMe)
                                        {
                                            userDetails.SessionExpiry = DateTime.UtcNow.AddDays(App.SessionPeriod);
                                        }
                                        else
                                        {
                                            userDetails.SessionExpiry = DateTime.UtcNow.AddDays(0);
                                        }

                                        if (Preferences.ContainsKey(nameof(App.FamilyUserDetails)))
                                        {
                                            Preferences.Remove(nameof(App.FamilyUserDetails));
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
                                        Preferences.Set(nameof(App.FamilyUserDetails), userDetailsStr);
                                        Preferences.Set(nameof(App.DefaultBudgetID), userDetails.BudgetID);
                                        Preferences.Set(nameof(App.IsFamilyAccount), true);

                                        App.UserDetails = null;
                                        App.FamilyUserDetails = userDetails;
                                        App.DefaultBudgetID = userDetails.BudgetID;
                                        App.IsFamilyAccount = true;
                                        App.HasVisitedCreatePage = true;
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
                                            catch (Exception)
                                            {
                                                //Log as non fatal error
                                            }
                                        }

                                        await _ps.CloseAsync<PopUpPage>();
                                        await Shell.Current.GoToAsync($"//{nameof(FamilyAccountMainPage)}");
                                        
                                    }
                                }

                                break;
                            default:
                                throw new Exception("Server Connectivity");                            
                        }
                    }
                    else
                    {
                        await _ps.CloseAsync<PopUpPage>();
                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                    }
                }
                else 
                {
                    IsButtonBusy = false;
                    await _ps.CloseAsync<PopUpPage>();
                    await Application.Current.Windows[0].Page.DisplayAlert("Opps", "That's not right ... check your details and try again!", "OK");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountLogonPage", "Login");
            }
        }        

    }
}
