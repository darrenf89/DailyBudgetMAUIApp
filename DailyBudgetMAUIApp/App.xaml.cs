using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp;

public partial class App : Application
{
	public static UserDetailsModel UserDetails;	
	public static int DefaultBudgetID;
    public static Budgets DefaultBudget;
    public static DateTime SessionLastUpdate;
    public static bool HasVisitedCreatePage;
    public static BudgetSettingValues CurrentSettings;
    public static BottomSheet CurrentBottomSheet;

    public static int SessionPeriod = 7;

    public App()
	{
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk4ODg4NEAzMjM0MmUzMDJlMzBtUDY3VFhTeER1WTJyd3VWaG9zQ3BaakFhOUZ1bDdGRDFET0p3VUkwNk5JPQ==");
		InitializeComponent();

        Microsoft.Maui.Handlers.ButtonHandler.Mapper.AppendToMapping(nameof(Button), (handler, view) =>
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
#endif
            }
        });

        //MainPage = new AppShell();
	}

    protected override Window CreateWindow(IActivationState activationState) 
    { 
        return new Window(new AppShell());   
    }
}
