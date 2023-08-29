using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Extensions.Logging;

namespace DailyBudgetMAUIApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder			
			. UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<IRestDataService, RestDataService>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddTransient<LogonPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
