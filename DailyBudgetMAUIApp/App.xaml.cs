using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using Microsoft.Maui.Platform;

namespace DailyBudgetMAUIApp;

public partial class App : Application
{
	public static UserDetailsModel UserDetails;	
	public static int DefaultBudgetID;
    public static Budgets DefaultBudget;
    public static DateTime SessionLastUpdate;

    public static int SessionPeriod = 7;

    public App()
	{
		InitializeComponent();

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(BorderlessEntry), (handler, view) =>
        {
            if (view is BorderlessEntry)
            {
#if __ANDROID__
                handler.PlatformView.SetBackgroundColor(Colors.Transparent.ToPlatform());
#elif __IOS__
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
            }
        });

        MainPage = new AppShell();
	}
}
