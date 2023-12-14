using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Helpers;
using Microsoft.Extensions.Logging;

namespace DailyBudgetMAUIApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialDesignIcons");
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if __ANDROID__
                handlers.AddHandler(typeof(RefreshView), typeof(Handlers.CustomRefreshViewHandler));
#endif

            });

        builder.Services.AddSingleton<IRestDataService, RestDataService>();
        builder.Services.AddSingleton<IProductTools, ProductTools>();

        //Pages
        builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<LogonPage>();
        builder.Services.AddTransient<LoadUpPage>();
		builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ErrorPage>();
        builder.Services.AddTransient<AddBill>();
        builder.Services.AddTransient<AddTransaction>();
        builder.Services.AddTransient<AddIncome>();
        builder.Services.AddTransient<AddSaving>();
        builder.Services.AddTransient<CreateNewBudget>();


        //ViewModes
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<LogonPageViewModel>();
        builder.Services.AddTransient<LoadUpPageViewModel>();
        builder.Services.AddTransient<ErrorPageViewModel>();
        builder.Services.AddTransient<RegisterPageViewModel>();
        builder.Services.AddTransient<AddBillViewModel>();
        builder.Services.AddTransient<AddTransactionViewModel>();
        builder.Services.AddTransient<AddIncomeViewModel>();
        builder.Services.AddTransient<AddSavingViewModel>();
        builder.Services.AddTransient<CreateNewBudgetViewModel>();

        //Popups
        builder.Services.AddTransient<PopUpPage>();
        builder.Services.AddTransient<PopUpPageSingleInput, PopUpPageSingleInputViewModel>();
        builder.Services.AddTransient<PopupInfo>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
