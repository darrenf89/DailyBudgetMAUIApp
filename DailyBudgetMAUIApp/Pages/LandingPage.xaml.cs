using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailySpendWebApp.Models;
using Newtonsoft.Json;

namespace DailyBudgetMAUIApp.Pages;

public partial class LandingPage : BasePage
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
        bool IsFamilyAccount = Preferences.Get(nameof(App.IsFamilyAccount), false);

        if (IsFamilyAccount)
        {
            string userDetailsStr = Preferences.Get(nameof(App.FamilyUserDetails), "");

            if (!string.IsNullOrEmpty(userDetailsStr))
            {
                FamilyUserAccount userDetails = JsonConvert.DeserializeObject<FamilyUserAccount>(userDetailsStr);
                Preferences.Remove(nameof(App.FamilyUserDetails));
                Preferences.Remove(nameof(App.UserDetails));

                if (userDetails.SessionExpiry > DateTime.UtcNow)
                {
                    userDetails = await _ds.GetFamilyUserDetailsAsync(userDetails.Email);

                    userDetails.SessionExpiry = DateTime.UtcNow.AddDays(App.SessionPeriod);
                    userDetailsStr = JsonConvert.SerializeObject(userDetails);
                    Preferences.Set(nameof(App.FamilyUserDetails), userDetailsStr);

                    if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                    {
                        Preferences.Remove(nameof(App.DefaultBudgetID));
                    }

                    if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
                    {
                        Preferences.Remove(nameof(App.IsFamilyAccount));
                    }

                    Preferences.Set(nameof(App.DefaultBudgetID), userDetails.BudgetID);
                    Preferences.Set(nameof(App.IsFamilyAccount), true);

                    App.UserDetails = null;
                    App.FamilyUserDetails = userDetails;
                    App.DefaultBudgetID = userDetails.BudgetID;
                    App.IsFamilyAccount = true;
                    await _pt.SetSubDetails();

                    if (await SecureStorage.Default.GetAsync("Session") == null)
                    {
                        AuthDetails Auth = new()
                        {
                            ClientID = DeviceInfo.Current.Name,
                            ClientSecret = userDetails.Password,
                            UserID = userDetails.UniqueUserID
                        };

                        SessionDetails Session = await _ds.CreateSession(Auth);
                        string SessionString = JsonConvert.SerializeObject(Session);
                        await SecureStorage.Default.SetAsync("Session", SessionString);

                    }


                    if (await SecureStorage.Default.GetAsync("FirebaseToken") != null)
                    {
                        int FirebaseID = Convert.ToInt32(await SecureStorage.Default.GetAsync("FirebaseID"));

                        FirebaseDevices UserDevice = new FirebaseDevices
                        {
                            FirebaseDeviceID = FirebaseID,
                            UserAccountID = userDetails.UniqueUserID,
                            LoginExpiryDate = userDetails.SessionExpiry,
                            FirebaseToken = await SecureStorage.Default.GetAsync("FirebaseToken")
                        };

                        try
                        {
                            await _ds.UpdateDeviceUserDetails(UserDevice);
                        }
                        catch (Exception)
                        {

                        }
                    }

                    BudgetSettingValues Settings = await _ds.GetBudgetSettingsValues(userDetails.BudgetID);
                    App.CurrentSettings = Settings;
                    _pt.SetCultureInfo(App.CurrentSettings);

                    await Shell.Current.GoToAsync($"//{nameof(FamilyAccountMainPage)}");
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

                    if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
                    {
                        Preferences.Remove(nameof(App.IsFamilyAccount));
                    }

                    if (await SecureStorage.Default.GetAsync("Session") != null)
                    {
                        SecureStorage.Default.Remove("Session");
                    }

                    await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
                }
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

                if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
                {
                    Preferences.Remove(nameof(App.IsFamilyAccount));
                }

                if (await SecureStorage.Default.GetAsync("Session") != null)
                {
                    SecureStorage.Default.Remove("Session");
                }

                if (await SecureStorage.Default.GetAsync("Session") != null)
                {
                    SecureStorage.Default.Remove("Session");
                }

                await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
            }
        }
        else
        { 
            string userDetailsStr = Preferences.Get(nameof(App.UserDetails), "");

            if (!string.IsNullOrEmpty(userDetailsStr))
            {
                UserDetailsModel userDetails = JsonConvert.DeserializeObject<UserDetailsModel>(userDetailsStr);
                Preferences.Remove(nameof(App.FamilyUserDetails));
                Preferences.Remove(nameof(App.UserDetails));

                if (userDetails.SessionExpiry > DateTime.UtcNow)
                {
                    userDetails = await _ds.GetUserDetailsAsync(userDetails.Email);

                    userDetails.SessionExpiry = DateTime.UtcNow.AddDays(App.SessionPeriod);
                    userDetailsStr = JsonConvert.SerializeObject(userDetails);
                    Preferences.Set(nameof(App.UserDetails), userDetailsStr);

                    if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                    {
                        Preferences.Remove(nameof(App.DefaultBudgetID));
                    }

                    if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
                    {
                        Preferences.Remove(nameof(App.IsFamilyAccount));
                    }

                    Preferences.Set(nameof(App.IsFamilyAccount), false);
                    Preferences.Set(nameof(App.DefaultBudgetID), userDetails.DefaultBudgetID);

                    App.UserDetails = userDetails;
                    App.FamilyUserDetails = null;
                    App.DefaultBudgetID = userDetails.DefaultBudgetID;
                    App.IsFamilyAccount = false;
                    await _pt.SetSubDetails();

                    if (await SecureStorage.Default.GetAsync("Session") == null)
                    {
                        AuthDetails Auth = new()
                        {
                            ClientID = DeviceInfo.Current.Name,
                            ClientSecret = userDetails.Password,
                            UserID = userDetails.UniqueUserID
                        };

                        SessionDetails Session = await _ds.CreateSession(Auth);
                        string SessionString = JsonConvert.SerializeObject(Session);
                        await SecureStorage.Default.SetAsync("Session", SessionString);

                    }


                    if (await SecureStorage.Default.GetAsync("FirebaseToken") != null)
                    {
                        int FirebaseID = Convert.ToInt32(await SecureStorage.Default.GetAsync("FirebaseID"));

                        FirebaseDevices UserDevice = new FirebaseDevices
                        {
                            FirebaseDeviceID = FirebaseID,
                            UserAccountID = userDetails.UniqueUserID,
                            LoginExpiryDate = userDetails.SessionExpiry,
                            FirebaseToken = await SecureStorage.Default.GetAsync("FirebaseToken")
                        };

                        try
                        {
                            await _ds.UpdateDeviceUserDetails(UserDevice);
                        }
                        catch (Exception)
                        {
                        }

                    }

                    BudgetSettingValues Settings = await _ds.GetBudgetSettingsValues(userDetails.DefaultBudgetID);
                    App.CurrentSettings = Settings;
                    _pt.SetCultureInfo(App.CurrentSettings);

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

                    if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
                    {
                        Preferences.Remove(nameof(App.IsFamilyAccount));
                    }

                    if (await SecureStorage.Default.GetAsync("Session") != null)
                    {
                        SecureStorage.Default.Remove("Session");
                    }

                    await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
                }
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

                if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
                {
                    Preferences.Remove(nameof(App.IsFamilyAccount));
                }

                if (await SecureStorage.Default.GetAsync("Session") != null)
                {
                    SecureStorage.Default.Remove("Session");
                }

                await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
            }
        }
    }

    protected async override void OnAppearing()
    {    
        try
        {
            base.OnAppearing();
            await Task.Delay(10);
            await CheckUserLoginDetails();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "LandingPage", "CheckUserLoginDetails");
        }     
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