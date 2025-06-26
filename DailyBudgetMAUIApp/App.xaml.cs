using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui;
using System.Drawing;
using Color = Microsoft.Maui.Graphics.Color;
using Microsoft.Maui.Platform;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;

#if IOS
using UIKit;
using Foundation;
using DailyBudgetMAUIApp.Platforms.iOS.Mappers;
#endif

#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Android.Widget;
using DailyBudgetMAUIApp.Platforms.Android.Mappers;
using System.Runtime.ExceptionServices;
using Android.Content;
using MAUISample.Platforms.Android;
using CommunityToolkit.Maui;
using static Android.Telephony.CarrierConfigManager;
#endif

namespace DailyBudgetMAUIApp;

public partial class App : Application
{
	public static UserDetailsModel UserDetails;	
	public static FamilyUserAccount FamilyUserDetails;	
	public static bool IsPremiumAccount;	
	public static int DefaultBudgetID;
	public static bool IsBudgetUpdated;
	public static bool IsFamilyAccount;
    public static Budgets DefaultBudget;
    public static DateTime SessionLastUpdate;
    public static bool HasVisitedCreatePage;
    public static BudgetSettingValues CurrentSettings;
    public static BottomSheet CurrentBottomSheet = null;
    public static TabBar MainTabBar;
    public static TabBar ViewTabBar;
    public static List<Brush> ChartBrush;
    public static List<Color> ChartColor;
    public static double NavBarHeight;
    public static double StatusBarHeight;
    public NetworkAccess PreviousNetworkAccess;

    public static int SessionPeriod = 7;
    public static bool IsBackgrounded;

    public App()
	{       
		
        InitializeComponent();
        LoadChartBrush();

        Microsoft.Maui.Handlers.ElementHandler.ElementMapper.AppendToMapping("Classic", (handler, view) =>
        {
            if (view is BorderEntry)
            {
#if __ANDROID__
                BorderEntryMapper.Map(handler, view);

#elif __IOS__
                BorderEntryMapper.Map(handler, view);

#endif
            }
        });

        Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping(nameof(Microsoft.Maui.Controls.Button), (handler, view) =>
        {
#if ANDROID
               handler.PlatformView.Gravity = Android.Views.GravityFlags.Center;  
#endif
        });

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(BorderlessEntry), (handler, view) =>
        {
            if (view is BorderlessEntry)
            {               
#if __ANDROID__
                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.EmojiCompatEnabled = false;
#elif __IOS__
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            }
        });

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(nameof(BorderlessPicker), (handler, view) =>
        {

            if (view is BorderlessPicker)
            {
#if ANDROID
                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
                handler.PlatformView.EmojiCompatEnabled = false;
#elif __IOS__
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;             
#endif
            }
        });

        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(BorderlessDatePicker), (handler, view) =>
        {
            if (view is BorderlessDatePicker)
            {
#if ANDROID
                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
                handler.PlatformView.EmojiCompatEnabled = false;
#elif __IOS__
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.Layer.BorderWidth = 0;
#endif
            }
        });

        Microsoft.Maui.Handlers.SearchBarHandler.Mapper.AppendToMapping(nameof(SearchBar), (h, v) =>
        {
#if ANDROID
            var children = h.PlatformView.GetChildrenOfType<ImageView>();
            foreach (var child in children)
            {
                child.SetColorFilter(Colors.DarkGray.ToPlatform());
            }
#endif
        });

        //// Entry
        //Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("DisableEmojiCompat", (handler, view) =>
        //{
        //    handler.PlatformView.EmojiCompatEnabled = false;
        //});

        //// Editor
        //Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("DisableEmojiCompat", (handler, view) =>
        //{
        //    handler.PlatformView.EmojiCompatEnabled = false;
        //});

        //MainPage = new AppShell();
    }

    public static void HandleAppAction(AppAction appAction)
    {
        App.Current.Dispatcher.Dispatch(async () =>
        {
            switch (appAction.Id)
            {
                case "quick_transaction":
#if ANDROID
                    Android.Content.Context context = Android.App.Application.Context;
                    var currentActivity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;

                    if (!Android.Provider.Settings.CanDrawOverlays(context))
                    {
                        //currentActivity.StartActivityForResult(new Android.Content.Intent(Android.Provider.Settings.ActionManageOverlayPermission, Android.Net.Uri.Parse("package:" + context.PackageName)), 0);
                        Toast.MakeText(currentActivity.ApplicationContext, "We need permissions to show quick transaction", ToastLength.Short).Show();
                    }
                    else
                    {
                        if (currentActivity != null) 
                        {       
                            if(App.UserDetails == null && App.FamilyUserDetails == null)
                            {
                                Toast.MakeText(currentActivity.ApplicationContext, "Please login to use quick transaction", ToastLength.Short).Show();
                                break;
                            }

                            if (App.DefaultBudgetID == 0)
                            {
                                Toast.MakeText(currentActivity.ApplicationContext, "Unable to find budget for transaction", ToastLength.Short).Show();
                                break;
                            }

                            while(App.DefaultBudget == null)
                            {
                                await Task.Delay(100);
                            }

                            Android.Content.Intent floatingIntent = new Android.Content.Intent(context, typeof(FloatingService));
                            currentActivity.StartService(floatingIntent);

                        } 
                    }
#endif
                    break;

                default:

                    break;
            }
        });
    }

    protected override async void OnStart()
    {

        await Task.Delay(1);
        base.OnStart();
        IsBackgrounded = false;
        await CheckConnection();
    }

    protected override void OnSleep()
    {
        IsBackgrounded = true;
        base.OnSleep();
    }

    protected override async void OnResume()
    {
        await Task.Delay(1);
        base.OnResume();
        IsBackgrounded = false;
        await CheckConnection();
    }

    private async Task CheckConnection()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            var mps = IPlatformApplication.Current.Services.GetService<IModalPopupService>();
            await mps.ShowAsync<PopUpNoNetwork>(() => IPlatformApplication.Current.Services.GetService<PopUpNoNetwork>());

            int i = 0;
            while (Connectivity.Current.NetworkAccess != NetworkAccess.Internet && i < 30)
            {
                await Task.Delay(1000);
                i++;
            }

            await mps.CloseAsync<PopUpNoNetwork>();

            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                //TODO: SHOW POPUP SAYING INTERNET RETURNED WOULD THEY LIKE TO RETURN TO THEIR BUDGETTING
            }
            else
            {
                await Shell.Current.GoToAsync($"{nameof(NoNetworkAccess)}");
            }
        }
    }

    protected override Window CreateWindow(IActivationState activationState) 
    { 
        return new Window(new AppShell());   
    }

    private void LoadChartBrush()
    {
        ChartBrush = new List<Brush>
        {
            new SolidColorBrush(Color.FromArgb("#19526C")),
            new SolidColorBrush(Color.FromArgb("#ffa600")),
            new SolidColorBrush(Color.FromArgb("#2f4b7c")),
            new SolidColorBrush(Color.FromArgb("#ff7c43")),            
            new SolidColorBrush(Color.FromArgb("#665191")),
            new SolidColorBrush(Color.FromArgb("#f95d6a")),
            new SolidColorBrush(Color.FromArgb("#a05195")),
            new SolidColorBrush(Color.FromArgb("#d45087")),
            new SolidColorBrush(Color.FromArgb("#19526C")),
            new SolidColorBrush(Color.FromArgb("#ffa600")),
            new SolidColorBrush(Color.FromArgb("#2f4b7c")),
            new SolidColorBrush(Color.FromArgb("#ff7c43")),
            new SolidColorBrush(Color.FromArgb("#665191")),
            new SolidColorBrush(Color.FromArgb("#f95d6a")),
            new SolidColorBrush(Color.FromArgb("#a05195")),
            new SolidColorBrush(Color.FromArgb("#d45087"))
        };

        ChartColor = new List<Color>
        {
            Color.FromArgb("#19526C"),
            Color.FromArgb("#ffa600"),
            Color.FromArgb("#2f4b7c"),
            Color.FromArgb("#ff7c43"),
            Color.FromArgb("#665191"),
            Color.FromArgb("#f95d6a"),
            Color.FromArgb("#a05195"),
            Color.FromArgb("#d45087"),
            Color.FromArgb("#19526C"),
            Color.FromArgb("#ffa600"),
            Color.FromArgb("#2f4b7c"),
            Color.FromArgb("#ff7c43"),
            Color.FromArgb("#665191"),
            Color.FromArgb("#f95d6a"),
            Color.FromArgb("#a05195"),
            Color.FromArgb("#d45087")
        };
    }
}
