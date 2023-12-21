using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using Microsoft.Maui.Platform;

namespace DailyBudgetMAUIApp;

public partial class App : Application
{
	public static UserDetailsModel UserDetails;	
	public static int DefaultBudgetID;
    public static Budgets DefaultBudget;
    public static DateTime SessionLastUpdate;
    public static bool HasVisitedCreatePage;
    public static BudgetSettingValues CurrentSettings;

    public static int SessionPeriod = 7;

    public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk4ODg4NEAzMjM0MmUzMDJlMzBtUDY3VFhTeER1WTJyd3VWaG9zQ3BaakFhOUZ1bDdGRDFET0p3VUkwNk5JPQ==");
		InitializeComponent();

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(BorderlessEntry), (handler, view) =>
        {
            if (view is BorderlessEntry)
            {
#if __ANDROID__
                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif __IOS__
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            }
        });

        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(BorderlessPicker), (handler, view) =>
        {

            if (view is BorderlessPicker)
            {
#if ANDROID

                handler.PlatformView.Background = null;
                handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif __IOS__
                handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
                //handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            }
        });

        MainPage = new AppShell();
	}
}
