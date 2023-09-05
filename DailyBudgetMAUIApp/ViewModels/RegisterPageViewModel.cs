using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [ICommand]        
        async void SignUp()
        {
            //TODO: Check that passwords match
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

                        string userDetailsStr = JsonConvert.SerializeObject(ReturnUser);
                        Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                        Preferences.Set(nameof(App.DefaultBudgetID), ReturnUser.DefaultBudgetID);

                        App.UserDetails = ReturnUser;
                        App.DefaultBudgetID = ReturnUser.DefaultBudgetID;

                        //TODO: Sign in User Session and save to DB

                        await Shell.Current.GoToAsync(nameof(MainPage));
                    }
                    else
                    {
                        //TODO: Error creating the user - return to error screen
                    }

                }
                else
                {
                    //TODO: Validate that username is taken / Something has gone wrong!

                }
            }
            else
            {
                if(IsAgreedToTerms)
                {
                    //TODO: Validate that you must agree to terms

                }
                else
                {
                    //TODO: or validate that passwords dont match

                }

            }
        }

        [ICommand]
        async void NavigateSignIn()
        {
            await Shell.Current.GoToAsync(nameof(LogonPage));
        }
    }
}
