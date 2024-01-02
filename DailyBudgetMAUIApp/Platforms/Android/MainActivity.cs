using Android.App;
using Android.Content.PM;
using Android.OS;
using Firebase.Messaging;

namespace DailyBudgetMAUIApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    internal static readonly string Channel_ID = "ShareBudget";
    internal static readonly int NotificationID = 101;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        if(Intent.Extras != null)
        {
            foreach(var key in Intent.Extras.KeySet())
            {
                if(key == "NavigationType")
                {
                    string NavigationType = Intent.Extras.GetString(key);
                    if(Preferences.ContainsKey("NavigationType"))
                    {
                        Preferences.Remove("NavigationType");
                    }

                    Preferences.Set("NavigationType", NavigationType);

                    if(NavigationType == "ShareBudget")
                    {
                        string NavigationID = Intent.Extras.GetString("NavigationID");
                        if (Preferences.ContainsKey("NavigationID"))
                        {
                            Preferences.Remove("NavigationID");
                        }
                        Preferences.Set("NavigationID", NavigationID);
                    }
                }
            }
        }
        CreateNotificationChannel();
    }


    private void CreateNotificationChannel()
    {
        if(OperatingSystem.IsOSPlatformVersionAtLeast("android",26))
        {
            var channel = new NotificationChannel(Channel_ID, "Share budget notification channel", NotificationImportance.Default);

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);

            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
