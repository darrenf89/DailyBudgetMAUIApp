﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;
using DailyBudgetMAUIApp.DataServices;
using Plugin.MauiMTAdmob;
using static Android.Provider.Settings;
using AndroidApp = Android.App.Application;
using Setting = Android.Provider.Settings;
using Plugin.MauiMTAdmob.Extra;
using static Microsoft.Maui.ApplicationModel.Platform;
using MAUISample.Platforms.Android;
using Intent = Android.Content.Intent;
using Android.Provider;
using Android.Runtime;


namespace DailyBudgetMAUIApp;



[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, ScreenOrientation = ScreenOrientation.Portrait)]
[IntentFilter(new[] { Platform.Intent.ActionAppAction },
              Categories = new[] { global::Android.Content.Intent.CategoryDefault })]
public class MainActivity : MauiAppCompatActivity
{
    internal static readonly string Channel_ID = "ShareBudget";
    internal static readonly string Support_Channel_ID = "CustomerSupport";
    internal static readonly int NotificationID = 101;
    Intent intent;
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        string appId = "ca-app-pub-4910594160495086~4251634683";

        string license = "ZIrXQSue1fdSLeYTwKjtCd2RPUg/6+cflQTVqDHKyxQN3KYugmFcLLk6FnFUdD6h+9OScoRNWRxQ+LggwGODL+tDDeCeDvXMvF0zBw9Xmw==";
        string deviceId = Setting.Secure.GetString(AndroidApp.Context.ContentResolver, Secure.AndroidId); 
        
        //CrossMauiMTAdmob.Current.Init(this, appId, license,"","",true,false, deviceId, true, DebugGeography.DEBUG_GEOGRAPHY_EEA,false);
        CrossMauiMTAdmob.Current.Init(this, appId);

        CrossMauiMTAdmob.Current.OnConsentFormDismissed += (sender, args) => {
            
        };

        CrossMauiMTAdmob.Current.OnConsentFormLoadFailure += (sender, args) => {
            
        };

        CrossMauiMTAdmob.Current.OnConsentInfoUpdateSuccess += (sender, args) => {
            
        };

        CrossMauiMTAdmob.Current.OnConsentInfoUpdateFailure += (sender, args) => {
            
        };

        CrossMauiMTAdmob.Current.OnConsentFormLoadSuccess += (sender, args) => {
            
        };

    }

    protected async override void OnResume()
    {
        base.OnResume();
        Platform.OnResume(this);

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

                    if (NavigationType == "ShareBudget" || NavigationType == "SupportReplay")
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
                    Intent.RemoveExtra(key);
                }
            }
        }        

        CreateNotificationChannel();
        CrossMauiMTAdmob.Current.OnResume();
    }

    protected override void OnNewIntent(Android.Content.Intent intent)
    {
        base.OnNewIntent(intent);
        Platform.OnNewIntent(intent);
        if(intent.Action != null)
        {
            if(intent.Action == "ACTION_XE_APP_ACTION")
            {
                MoveTaskToBack(true);
            }
        }    
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        if (requestCode == 0)
        {
            if (!Settings.CanDrawOverlays(this))
            {


            }
            else
            {
                StartService(new Intent(this, typeof(FloatingService)));
            }
        }
    }

    private void CreateNotificationChannel()
    {
        if(OperatingSystem.IsOSPlatformVersionAtLeast("android",26))
        {
            var channel = new NotificationChannel(Channel_ID, "Share budget notifications", NotificationImportance.Default);
            var supportChannel = new NotificationChannel(Support_Channel_ID, "Support Inquiry notifications", NotificationImportance.High);

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);            
            notificationManager.CreateNotificationChannel(supportChannel);
        }
    }
}
