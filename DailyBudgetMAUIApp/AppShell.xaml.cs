using DailyBudgetMAUIApp.Pages;

namespace DailyBudgetMAUIApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(LogonPage), typeof(LogonPage));
	}
}
