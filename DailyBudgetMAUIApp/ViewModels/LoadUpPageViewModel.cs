using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class LoadUpPageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;

        public LoadUpPageViewModel(IProductTools pt)
        {
            CheckUserLoginDetails();
            _pt = pt;
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
            try
            {

                string userDetailsStr = Preferences.Get(nameof(App.UserDetails),"");

                if (!string.IsNullOrEmpty(userDetailsStr))
                {
                    UserDetailsModel userDetails = JsonConvert.DeserializeObject<UserDetailsModel>(userDetailsStr);
                    Preferences.Remove(nameof(App.UserDetails));

                    if (userDetails.SessionExpiry > DateTime.UtcNow) 
                    {
                        userDetails.SessionExpiry = DateTime.UtcNow.AddDays(App.SessionPeriod);
                        userDetailsStr = JsonConvert.SerializeObject(userDetails);
                        Preferences.Set(nameof(App.UserDetails), userDetailsStr);

                        if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                        {
                            Preferences.Remove(nameof(App.DefaultBudgetID));
                        }

                        Preferences.Set(nameof(App.DefaultBudgetID), userDetails.DefaultBudgetID);

                        App.UserDetails = userDetails;
                        App.DefaultBudgetID = userDetails.DefaultBudgetID;

                        //TODO: Update User Session

                        await Shell.Current.GoToAsync(nameof(MainPage));
                    }
                    else
                    {
                        if (Preferences.ContainsKey(nameof(App.UserDetails)))
                        {
                            Preferences.Remove(nameof(App.UserDetails));
                        }

                        if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                        {
                            Preferences.Remove(nameof(App.DefaultBudgetID));
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($" --> {ex.Message}");
                ErrorLog Error = await _pt.HandleCatchedException(ex, "LoadupPage", "CheckUserLoginDetails");
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }


    }
}
