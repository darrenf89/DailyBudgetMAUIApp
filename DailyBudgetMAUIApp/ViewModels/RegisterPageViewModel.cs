using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
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
        private string _email;
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _passwordConfirm;
        [ObservableProperty]
        private string _nickName;
        [ObservableProperty]
        private bool _isDPAPermissions;
        [ObservableProperty]
        private bool _isAgreedToTerms;
        [ObservableProperty]
        private bool _emailValid;
        [ObservableProperty]
        private bool _emailRequired;
        [ObservableProperty]
        private bool _nickNameRequired;
        [ObservableProperty]
        private bool _passwordRequired;
        [ObservableProperty]
        private bool _passwordSameSame;

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

        [ICommand]        
        async void SignUp()
        {
            try
            {
                if(!PageIsValid())                
                {
                    return;
                }

                if(IsAgreedToTerms && String.Equals(Password, PasswordConfirm))
                {
                    UserDetailsModel UserDetails = await _ds.GetUserDetailsAsync(Email);
                    if(UserDetails.Error != null)
                    {
                        RegisterModel NewUser = new RegisterModel();
                        NewUser.Email = Email;
                        NewUser.Password = Password;
                        NewUser.isDPAPermissions = IsDPAPermissions;
                        NewUser.isAgreedToTerms = IsAgreedToTerms;
                        NewUser.NickName = NickName;
                        
                        NewUser = _pt.CreateUserSecurityDetails(NewUser);

                        UserDetailsModel ReturnUser = await _ds.RegisterNewUserAsync(NewUser);

                        if(ReturnUser.Error == null)
                        {
                            ReturnUser.SessionExpiry = DateTime.UtcNow.AddDays(0);

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

                            //TODO: Sign in User Session and save to DB

                            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Opps", "There was an error creating your User account, please try again!", "OK");
                        }

                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Opps", "This Email is already taken, reset your password or try a different Email", "OK");
                    }
                }
                else
                {
                    if(IsAgreedToTerms)
                    {
                        await Application.Current.MainPage.DisplayAlert("Opps", "Your Passwords don't match ...", "OK");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Opps", "You have to agree to our Terms of Service", "OK");
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error Trying to get User Details in DataRestServices --> {ex.Message}");
                ErrorLog Error = await _pt.HandleCatchedException(ex, "RegisterPage", "SignUp");
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        [ICommand]
        async void NavigateSignIn()
        {
            await Shell.Current.GoToAsync(nameof(LogonPage));
        }
    }
}
