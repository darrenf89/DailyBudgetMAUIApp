using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailySpendWebApp.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class LoadUpPageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        public LoadUpPageViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

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

        private async Task CheckUserLoginDetails()
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
                        userDetails = _ds.GetUserDetailsAsync(userDetails.Email).Result;

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

                        if (await SecureStorage.Default.GetAsync("FirebaseToken") != null)
                        {
                            int FirebaseID = Convert.ToInt32(await SecureStorage.Default.GetAsync("FirebaseID"));

                            FirebaseDevices UserDevice = new FirebaseDevices
                            {
                                FirebaseDeviceID = FirebaseID,
                                UserAccountID = userDetails.UserID,
                                LoginExpiryDate = userDetails.SessionExpiry
                            };

                            await _ds.UpdateDeviceUserDetails(UserDevice);
                        }

                        await _pt.LoadTabBars(App.UserDetails.SubscriptionType, App.UserDetails.SubscriptionExpiry, App.UserDetails.DefaultBudgetType);

                        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                        return;
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
