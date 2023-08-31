using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp;

public partial class App : Application
{
	public static UserDetailsModel UserDetails;
	public static int SessionPeriod = 7;
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
