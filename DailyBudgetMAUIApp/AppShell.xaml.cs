using DailyBudgetMAUIApp.Pages;

namespace DailyBudgetMAUIApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(LoadUpPage), typeof(LoadUpPage));
        Routing.RegisterRoute(nameof(LogonPage), typeof(LogonPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
    }
}
