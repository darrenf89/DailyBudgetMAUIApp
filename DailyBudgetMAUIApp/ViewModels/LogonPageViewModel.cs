using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
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

        [ICommand]
        async void Login()
        {
            if(!string.IsNullOrEmpty(Email))
            {
                if (!string.IsNullOrEmpty(Password))
                {
                    UserDetailsModel userDetails = new UserDetailsModel();

                    string salt = await _ds.GetUserSaltAsync(Email);
                    
                    switch (salt)
                    {
                        case "User not found":
                            //TODO: Throw some validation that UserName or Password isn't valid
                            break;
                        case not "":

                            userDetails = await _ds.GetUserDetailsAsync(Email);

                            if (userDetails == null)
                            {
                                //TODO: Throw some validation that UserName or Password isn't valid
                            }
                            else
                            {
                                string HashPassword = _pt.GenerateHashedPassword(Password, salt);
                                if(userDetails.Password != HashPassword)
                                {
                                    //TODO: Throw some validation that UserName or Password isn't valid
                                }
                                else
                                {
                                    if (!userDetails.isEmailVerified)
                                    {
                                        //TODO: Throw some validation that Email isn't verified
                                    }
                                    else
                                    {
                                        //TODO: if Remember me set session date otherwise set to current time.
                                        userDetails.SessionExpiry = DateTime.UtcNow.AddDays(App.SessionPeriod);

                                        if (Preferences.ContainsKey(nameof(App.UserDetails)))
                                        {
                                            Preferences.Remove(nameof(App.UserDetails));
                                        }

                                        string userDetailsStr = JsonConvert.SerializeObject(userDetails);
                                        Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                                        Preferences.Set(nameof(App.DefaultBudgetID), userDetails.DefaultBudgetID);

                                        App.UserDetails = userDetails;
                                        App.DefaultBudgetID = userDetails.DefaultBudgetID;
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
                    //TODO: Throw some validation to enter UserName or Password
                }
            }
            else 
            {
                //TODO: Throw some validation to enter UserName or Password
            }

        }

    }
}
