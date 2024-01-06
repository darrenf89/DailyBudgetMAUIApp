using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using Firebase.Messaging;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp;



[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    internal static readonly string Channel_ID = "ShareBudget";
    internal static readonly int NotificationID = 101;

    protected async override void OnResume()
    {
        base.OnResume();

        if (Intent.Extras != null)
        {
            foreach (var key in Intent.Extras.KeySet())
            {
                if (key == "NavigationType")
                {
                    string NavigationType = Intent.Extras.GetString(key);
                    if (Preferences.ContainsKey("NavigationType"))
                    {
                        Preferences.Remove("NavigationType");
                    }

                    Preferences.Set("NavigationType", NavigationType);

                    if (NavigationType == "ShareBudget")
                    {
                        string NavigationID = Intent.Extras.GetString("NavigationID");
                        if (Preferences.ContainsKey("NavigationID"))
                        {
                            Preferences.Remove("NavigationID");
                        }
                        Preferences.Set("NavigationID", NavigationID);
                    }

                    IProductTools pt = new ProductTools(new RestDataService());
                    await pt.NavigateFromPendingIntent(Preferences.Get("NavigationType", ""));
                }
            }
        }

        CreateNotificationChannel();
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
    }

    private void CreateNotificationChannel()
    {
        if(OperatingSystem.IsOSPlatformVersionAtLeast("android",26))
        {
            var channel = new NotificationChannel(Channel_ID, "Share budget notifications", NotificationImportance.Default);
            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
