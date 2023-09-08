using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        [ICommand]
        async void NavigateRegister()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }

        [ICommand]
        async void Login()
        {
            try
            {
                if (!PageIsValid())
                {
                    return;
                }

                if (!string.IsNullOrEmpty(Email))
                {
                    if (!string.IsNullOrEmpty(Password))
                    {
                        UserDetailsModel userDetails = new UserDetailsModel();

                        string salt = await _ds.GetUserSaltAsync(Email);
                        
                        switch (salt)
                        {
                            case "User not found":
                                await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                break;
                            case not "":

                                userDetails = await _ds.GetUserDetailsAsync(Email);

                                if (userDetails == null)
                                {
                                    await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                }
                                else
                                {
                                    string HashPassword = _pt.GenerateHashedPassword(Password, salt);
                                    if(userDetails.Password != HashPassword)
                                    {
                                        await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                                    }
                                    else
                                    {
                                        if (!userDetails.isEmailVerified)
                                        {
                                            await Application.Current.MainPage.DisplayAlert("Opps", "You haven't validated your email .. do that and come back!", "OK");
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

                                            if (Preferences.ContainsKey(nameof(App.SessionLastUpdate)))
                                            {
                                                Preferences.Remove(nameof(App.SessionLastUpdate));
                                            }

                                            string userDetailsStr = JsonConvert.SerializeObject(userDetails);
                                            Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                                            Preferences.Set(nameof(App.DefaultBudgetID), userDetails.DefaultBudgetID);

                                            App.UserDetails = userDetails;
                                            App.DefaultBudgetID = userDetails.DefaultBudgetID;

                                            //TODO: Sign in or update User Session and save to DB

                                            await Shell.Current.GoToAsync(nameof(MainPage));
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
                        await Application.Current.MainPage.DisplayAlert("Opps", "Thats not right ... check your details and try again!", "OK");
                    }
                }
                else 
                {
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
