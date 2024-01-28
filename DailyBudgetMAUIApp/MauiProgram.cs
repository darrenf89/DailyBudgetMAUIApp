using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using Microsoft.Extensions.Logging;
using IeuanWalker.Maui.Switch;
using Syncfusion.Maui.Core.Hosting;
using Maui.FixesAndWorkarounds;
using The49.Maui.BottomSheet;


namespace DailyBudgetMAUIApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseSwitch()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .ConfigureMauiWorkarounds()
            .UseBottomSheet()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialDesignIcons");
                fonts.AddFont("manolo-mono.ttf", "ManoloMono");
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
        builder.Services.AddTransient<ShareBudget>();
        builder.Services.AddTransient<SelectSavingCategoryPage>();
        builder.Services.AddTransient<SelectPayeePage>();
        builder.Services.AddTransient<SelectCategoryPage>();
        builder.Services.AddTransient<ViewTransactions>();
        builder.Services.AddTransient<ViewCategories>();
        builder.Services.AddTransient<ViewSavings>();
        builder.Services.AddTransient<ViewBills>();

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
        builder.Services.AddTransient<SelectSavingCategoryPageViewModel>();
        builder.Services.AddTransient<SelectPayeePageViewModel>();
        builder.Services.AddTransient<SelectCategoryPageViewModel>();
        builder.Services.AddTransient<BaseViewModel>();
        builder.Services.AddTransient<ViewTransactionsViewModel>();
        builder.Services.AddTransient<ViewCategoriesViewModel>();
        builder.Services.AddTransient<ViewSavingsViewModel>();
        builder.Services.AddTransient<ViewBillsViewModel>();

        //Popups
        builder.Services.AddTransient<PopUpPage>();
        builder.Services.AddTransient<PopUpPageSingleInput, PopUpPageSingleInputViewModel>();
        builder.Services.AddTransient<PopupInfo>();
        builder.Services.AddTransient<PopUpOTP, PopUpOTPViewModel>();
        builder.Services.AddTransient<PopupDailySaving, PopupDailySavingViewModel>();
        builder.Services.AddTransient<PopupDailyBill, PopupDailyBillViewModel>();
        builder.Services.AddTransient<PopupDailyPayDay, PopupDailyPayDayViewModel>();
        builder.Services.AddTransient<PopupDailyIncome, PopupDailyIncomeViewModel>();
        builder.Services.AddTransient<PopupDailyTransaction, PopupDailyTransactionViewModel>();
        builder.Services.AddTransient<LoadingPage>();
        builder.Services.AddTransient<LoadingPageTwo>();

        //BottomSheets
        builder.Services.AddTransient<ViewTransactionFilterBottomSheet>();
        builder.Services.AddTransient<BudgetOptionsBottomSheet>();
        builder.Services.AddTransient<ShareBudget>();
        builder.Services.AddTransient<EnvelopeOptionsBottomSheet>();


#if WINDOWS
        SetWindowHandlers(); 
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif


		return builder.Build();
	}

#if WINDOWS
    public static void SetWindowHandlers()
    {
      Microsoft.Maui.Handlers.SwitchHandler.Mapper
        .AppendToMapping("Custom", (h, v) =>
      {
        // Get rid of On/Off label beside switch, to match other platforms
        h.PlatformView.OffContent = string.Empty;
        h.PlatformView.OnContent = string.Empty;

        h.PlatformView.MinWidth = 0;
      });
    }
#endif
}


