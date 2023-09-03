using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp;

public partial class App : Application
{
	public static UserDetailsModel UserDetails;
	public static int SessionPeriod = 7;
	public static int DefaultBudgetID { get; set; } = 0;
    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
