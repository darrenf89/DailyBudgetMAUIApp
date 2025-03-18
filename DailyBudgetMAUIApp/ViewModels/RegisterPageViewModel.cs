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
        private string  email;
        [ObservableProperty]
        private string  password;
        [ObservableProperty]
        private string  passwordConfirm;
        [ObservableProperty]
        private string  nickName;
        [ObservableProperty]
        private bool  isDPAPermissions;
        [ObservableProperty]
        private bool  isAgreedToTerms;
        [ObservableProperty]
        private bool  emailValid;
        [ObservableProperty]
        private bool  emailRequired;
        [ObservableProperty]
        private bool  nickNameRequired;
        [ObservableProperty]
        private bool  passwordRequired;
        [ObservableProperty]
        private bool  passwordSameSame;
        [ObservableProperty]
        private bool  passwordStrong;
        [ObservableProperty]
        private bool  registerSuccess;

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

        private async Task ResetSuccessFailureMessage()
        {
            RegisterSuccess = false;
        }

        [RelayCommand]    
        async void SignUp()
        {
            
            try
            {
                await ResetSuccessFailureMessage();
                if (!PageIsValid())                
                {
                    return;
                }

                var page = new LoadingPage();
                await Application.Current.Windows[0].Navigation.PushModalAsync(page);

                if (IsAgreedToTerms && String.Equals(Password, PasswordConfirm))
                {
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
                            await Application.Current.Windows[0].Navigation.PopModalAsync();
                            string status = await _ds.CreateNewOtpCode(ReturnUser.UserID, "ValidateEmail");
                            if (status == "OK")
                            {
                                RegisterSuccess = true;

                                var popup = new PopUpOTP(ReturnUser.UserID, new PopUpOTPViewModel(IPlatformApplication.Current.Services.GetService<IRestDataService>(), IPlatformApplication.Current.Services.GetService<IProductTools>()), "ValidateEmail", IPlatformApplication.Current.Services.GetService<IProductTools>(), IPlatformApplication.Current.Services.GetService<IRestDataService>());
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

                                    string userDetailsStr = JsonConvert.SerializeObject(ReturnUser);
                                    Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                                    Preferences.Set(nameof(App.DefaultBudgetID), ReturnUser.DefaultBudgetID);

                                    App.UserDetails = ReturnUser;
                                    App.DefaultBudgetID = ReturnUser.DefaultBudgetID;
                                    App.HasVisitedCreatePage = false;
                                    await _pt.SetSubDetails();

                                    await Application.Current.Windows[0].Navigation.PopModalAsync();
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
                                await Application.Current.Windows[0].Navigation.PopModalAsync();
                                await Application.Current.Windows[0].Page.DisplayAlert("Opps", "There was an error sending you an OTP code to verify you email! Please click the link to create a new one so you can continue your daily budgeting journey", "OK");

                            }
                        }
                        else
                        {
                            await Application.Current.Windows[0].Navigation.PopModalAsync();
                            await Application.Current.Windows[0].Page.DisplayAlert("Opps", "There was an error creating your User account, please try again!", "OK");
                        }

                    }
                    else
                    {
                        await Application.Current.Windows[0].Navigation.PopModalAsync();
                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "This Email is already taken, reset your password or try a different Email", "OK");
                    }
                }
                else
                {
                    if(IsAgreedToTerms)
                    {
                        await Application.Current.Windows[0].Navigation.PopModalAsync();
                        await Application.Current.Windows[0].Page.DisplayAlert("Opps", "Your Passwords don't match ...", "OK");
                    }
                    else
                    {
                        await Application.Current.Windows[0].Navigation.PopModalAsync();
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
        async void NavigateSignIn()
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
    }
}
