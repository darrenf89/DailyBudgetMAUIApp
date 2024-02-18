using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailySpendWebApp.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.Pages;

public partial class LandingPage : ContentPage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    public LandingPage(LandingPageViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        _pt = pt;
        _ds = ds;

        InitializeComponent();
		this.BindingContext = viewModel;

    }

    private async Task CheckUserLoginDetails()
    {
        try
        {
            string userDetailsStr = Preferences.Get(nameof(App.UserDetails), "");

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

                    await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
                }
            }
            else
            {
                await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
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

    protected async override void OnAppearing()
    {    
        base.OnAppearing();

        //MainThread.BeginInvokeOnMainThread(async () =>
        //{
        //    await OpenAnimate();
        //});

        await CheckUserLoginDetails();

    }

    private async Task OpenAnimate()
    {
        var animation = new Animation(v => brdImage.Scale = v, 1, 1.2);
        
        animation.Commit(this, "OpenAnimation", 16, 1000, Easing.Linear, async (v, c) =>
        {
            await CloseAnimate();
        });
    }

    private async Task CloseAnimate()
    {
        var CloseAnimation = new Animation(v => brdImage.Scale = v, 1.2, 1);
        CloseAnimation.Commit(this, "CloseAnimation", 16, 1000, Easing.Linear, async (v, c) =>
        {
            await OpenAnimate();
        });
    }
}