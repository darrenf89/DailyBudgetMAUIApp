using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class LoadUpPageViewModel : BaseViewModel
    {
        public LoadUpPageViewModel()
        {
            CheckUserLoginDetails();
        }

        [ICommand]
        async void Logon()
        {
            await Shell.Current.GoToAsync(nameof(LogonPage));
        }

        [ICommand]
        async void Register()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }

        private async void CheckUserLoginDetails()
        {
            string userDetailsStr = Preferences.Get(nameof(App.UserDetails),"");

            if (!string.IsNullOrEmpty(userDetailsStr))
            {
                UserDetailsModel userDetails = JsonConvert.DeserializeObject<UserDetailsModel>(userDetailsStr);

                if (userDetails.SessionExpiry > DateTime.UtcNow) 
                {
                    userDetails.SessionExpiry = DateTime.UtcNow.AddDays(App.SessionPeriod);
                    if (Preferences.ContainsKey(nameof(App.UserDetails)))
                    {
                        Preferences.Remove(nameof(App.UserDetails));
                    }

                    userDetailsStr = JsonConvert.SerializeObject(userDetails);
                    Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                    Preferences.Set(nameof(App.DefaultBudgetID), userDetails.DefaultBudgetID);

                    App.UserDetails = userDetails;
                    App.DefaultBudgetID = userDetails.DefaultBudgetID;

                    //TODO: Update User Session

                    await Shell.Current.GoToAsync(nameof(MainPage));
                }
            }
        }


    }
}
