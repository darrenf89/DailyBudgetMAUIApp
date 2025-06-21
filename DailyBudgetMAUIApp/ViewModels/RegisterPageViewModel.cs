using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Popups;
using Newtonsoft.Json;
using System.Diagnostics;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class RegisterPageViewModel : BaseViewModel  
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;
        public RegisterPageViewModel(IRestDataService ds, IProductTools pt)
        {
            Title = "Please Sign Up!";
            _ds = ds;
            _pt = pt;
        }

        [ObservableProperty]
        public partial string Email { get; set; }

        [ObservableProperty]
        public partial string Password { get; set; }

        [ObservableProperty]
        public partial string PasswordConfirm { get; set; }

        [ObservableProperty]
        public partial string NickName { get; set; }

        [ObservableProperty]
        public partial bool IsDPAPermissions { get; set; }

        [ObservableProperty]
        public partial bool IsAgreedToTerms { get; set; }

        [ObservableProperty]
        public partial bool EmailValid { get; set; }

        [ObservableProperty]
        public partial bool EmailRequired { get; set; }

        [ObservableProperty]
        public partial bool NickNameRequired { get; set; }

        [ObservableProperty]
        public partial bool PasswordRequired { get; set; }

        [ObservableProperty]
        public partial bool PasswordSameSame { get; set; }

        [ObservableProperty]
        public partial bool PasswordStrong { get; set; }

        [ObservableProperty]
        public partial bool RegisterSuccess { get; set; }


        public bool PageIsValid()
        {
            bool IsValid = true;
            if (Password == "" || Password == null)
            {
                PasswordRequired = false;
                IsValid = false;
            }

            if (!String.Equals(Password, PasswordConfirm) || Password == null || PasswordConfirm == null)
            {
                PasswordSameSame = false;
                IsValid = false;
            }

            if (NickName == "" || NickName == null)
            {
                NickNameRequired = false;
                IsValid = false;
            }

            if (Email == "" || Email == null)
            {
                EmailRequired = false;
                IsValid = false;
            }

            return IsValid;
        }

        private void ResetSuccessFailureMessage()
        {
            RegisterSuccess = false;
        }

        [RelayCommand]    
        async Task SignUp()
        {
            
            try
            {
                ResetSuccessFailureMessage();
                if (!PageIsValid())                
                {
                    return;
                }

                if (IsAgreedToTerms && String.Equals(Password, PasswordConfirm))
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }

                    await Task.Delay(1);

                    UserDetailsModel UserDetails = await _ds.GetUserDetailsAsync(Email);
                    if(UserDetails.Error != null)
                    {
                        RegisterModel NewUser = new RegisterModel();
                        NewUser.Email = Email;
                        NewUser.Password = Password;
                        NewUser.IsDPAPermissions = IsDPAPermissions;
                        NewUser.IsAgreedToTerms = IsAgreedToTerms;
                        NewUser.NickName = NickName;
                        NewUser.ProfilePicture = "Avatar1";
                        
                        NewUser = _pt.CreateUserSecurityDetails(NewUser);

                        UserDetailsModel ReturnUser = await _ds.RegisterNewUserAsync(NewUser);

                        if(ReturnUser.Error == null)
                        {
                            if (App.CurrentPopUp != null)
                            {
                                await App.CurrentPopUp.CloseAsync();
                                App.CurrentPopUp = null;
                            }

                            string status = await _ds.CreateNewOtpCode(ReturnUser.UserID, "ValidateEmail");
                            if (status == "OK")
                            {
                                RegisterSuccess = true;

                                var popup = new PopUpOTP(ReturnUser.UserID, new PopUpOTPViewModel(IPlatformApplication.Current.Services.GetService<IRestDataService>(), IPlatformApplication.Current.Services.GetService<IProductTools>()), "ValidateEmail", IPlatformApplication.Current.Services.GetService<IProductTools>(), IPlatformApplication.Current.Services.GetService<IRestDataService>());
                                App.CurrentPopUp = popup;
                                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

                                if((string)result.ToString() == "OK")
                                {                                    
                                    ReturnUser.SessionExpiry = DateTime.UtcNow.AddDays(1);

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

                                    string userDetailsStr = JsonConvert.SerializeObject(ReturnUser);
                                    Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                                    Preferences.Set(nameof(App.DefaultBudgetID), ReturnUser.DefaultBudgetID);

                                    App.UserDetails = ReturnUser;
                                    App.FamilyUserDetails = null;
                                    App.DefaultBudgetID = ReturnUser.DefaultBudgetID;
                                    App.HasVisitedCreatePage = false;
                                    await _pt.SetSubDetails();

                                    await Shell.Current.GoToAsync($"///{nameof(LandingPage)}");
                                }
                                else
                                {
                                    NickName = "";
                                    Email = "";
                                    Password = "";
                                    PasswordConfirm = "";
                                    IsDPAPermissions = false;
                                    IsAgreedToTerms = false;
                                }
                            }
                            else
                            {
                                if (App.CurrentPopUp != null)
                                {
                                    await App.CurrentPopUp.CloseAsync();
                                    App.CurrentPopUp = null;
                                }

                                await Application.Current.Windows[0].Page.DisplayAlert("Opps", "There was an error sending you an OTP code to verify you email! Please click the link to create a new one so you can continue your daily budgeting journey", "OK");

                            }
                        }
                        else
                        {
                            await Application.Current.Windows[0].Page.DisplayAlert("Opps", "There was an error creating your User account, please try again!", "OK");
                        }

                    }
                    else
                    {
                        if (App.CurrentPopUp != null)
                        {
                            await App.CurrentPopUp.CloseAsync();
                            App.CurrentPopUp = null;
                        }

                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "This Email is already taken, reset your password or try a different Email", "OK");
                    }
                }
                else
                {
                    if(IsAgreedToTerms)
                    {
                        if (App.CurrentPopUp != null)
                        {
                            await App.CurrentPopUp.CloseAsync();
                            App.CurrentPopUp = null;
                        }

                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "Your Passwords don't match ...", "OK");
                    }
                    else
                    {
                        if (App.CurrentPopUp != null)
                        {
                            await App.CurrentPopUp.CloseAsync();
                            App.CurrentPopUp = null;
                        }

                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "You have to agree to our Terms of Service", "OK");
                    }
                }
            }
            catch(Exception ex)
            {
                await _pt.HandleException(ex, "RegisterPage", "SignUp");
            }
        }

        [RelayCommand]
        async Task NavigateSignIn()
        {
            try
            {
                await Shell.Current.GoToAsync($"../{nameof(LogonPage)}");
            }
            catch (Exception ex)
            {

                await _pt.HandleException(ex, "RegisterPage", "NavigateSignIn");
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
                await _pt.HandleException(ex, "RegisterPage", "NavigateFamilySignPage");
            }
        }
    }
}
