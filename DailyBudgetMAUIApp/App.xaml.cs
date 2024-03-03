using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui;
using System.Drawing;
using Color = Microsoft.Maui.Graphics.Color;
using Microsoft.Maui.Platform;

#if IOS
using UIKit;
using Foundation;
#endif

#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Android.Widget;
#endif

namespace DailyBudgetMAUIApp;

public partial class App : Application
{
	public static UserDetailsModel UserDetails;	
	public static int DefaultBudgetID;
    public static Budgets DefaultBudget;
    public static DateTime SessionLastUpdate;
    public static bool HasVisitedCreatePage;
    public static BudgetSettingValues CurrentSettings;
    public static BottomSheet CurrentBottomSheet = null;
    public static Popup CurrentPopUp = null;
    public static TabBar MainTabBar;
    public static TabBar ViewTabBar;
    public static List<Brush> ChartBrush;
    public static List<Color> ChartColor;
    public static double NavBarHeight;
    public static double StatusBarHeight;

    public static int SessionPeriod = 7;

    public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk4ODg4NEAzMjM0MmUzMDJlMzBtUDY3VFhTeER1WTJyd3VWaG9zQ3BaakFhOUZ1bDdGRDFET0p3VUkwNk5JPQ==");
		InitializeComponent();
        LoadChartBrush();

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

        //MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState) 
    { 
        return new Window(new AppShell());   
    }

    private void LoadChartBrush()
    {
        ChartBrush = new List<Brush>
        {
            new SolidColorBrush(Color.FromArgb("#003f5c")),
            new SolidColorBrush(Color.FromArgb("#ffa600")),
            new SolidColorBrush(Color.FromArgb("#2f4b7c")),
            new SolidColorBrush(Color.FromArgb("#ff7c43")),            
            new SolidColorBrush(Color.FromArgb("#665191")),
            new SolidColorBrush(Color.FromArgb("#f95d6a")),
            new SolidColorBrush(Color.FromArgb("#a05195")),
            new SolidColorBrush(Color.FromArgb("#d45087")),
            new SolidColorBrush(Color.FromArgb("#003f5c")),
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
            Color.FromArgb("#003f5c"),
            Color.FromArgb("#ffa600"),
            Color.FromArgb("#2f4b7c"),
            Color.FromArgb("#ff7c43"),
            Color.FromArgb("#665191"),
            Color.FromArgb("#f95d6a"),
            Color.FromArgb("#a05195"),
            Color.FromArgb("#d45087"),
            Color.FromArgb("#003f5c"),
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
