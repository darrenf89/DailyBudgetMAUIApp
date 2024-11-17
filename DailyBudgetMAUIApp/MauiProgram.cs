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
using DotNet.Meteor.HotReload.Plugin;
using DailyBudgetMAUIApp.Popups;
using Maui.FreakyEffects;
using Plugin.MauiMTAdmob;
using Plugin.Maui.AppRating;


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
            .UseMauiMTAdmob()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialDesignIcons");
                fonts.AddFont("manolo-mono.ttf", "ManoloMono");
            })
#if DEBUG
            .EnableHotReload()
#endif
            .ConfigureMauiHandlers(handlers =>
            {
#if __ANDROID__
                handlers.AddHandler(typeof(RefreshView), typeof(Handlers.CustomRefreshViewHandler));
                handlers.AddHandler(typeof(Shell), typeof(DailyBudgetMAUIApp.Platforms.Android.Renderers.MyShellRenderer));
#endif
            })
            .ConfigureEffects(effects =>
            {
                  effects.InitFreakyEffects();
            });

        builder.Services.AddSingleton<IRestDataService, RestDataService>();
        builder.Services.AddSingleton<IProductTools, ProductTools>();
        builder.Services.AddSingleton<IAppRating>(AppRating.Default);

        //Pages
        builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<LogonPage>();
        builder.Services.AddTransient<LoadUpPage>();
        builder.Services.AddTransient<LandingPage>();
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
        builder.Services.AddTransient<ViewCategory>();
        builder.Services.AddTransient<ViewSavings>();
        builder.Services.AddTransient<ViewBills>();
        builder.Services.AddTransient<ViewEnvelopes>();
        builder.Services.AddTransient<ViewIncomes>();
        builder.Services.AddTransient<ViewFilteredTransactions>();
        builder.Services.AddTransient<ViewPayees>();
        builder.Services.AddTransient<ViewCalendar>();
        builder.Services.AddTransient<EditBudgetSettings>();
        builder.Services.AddTransient<EditAccountSettings>();
        builder.Services.AddTransient<EditAccountDetails>();
        builder.Services.AddTransient<NoNetworkAccess>();
        builder.Services.AddTransient<NoServerAccess>();
        builder.Services.AddTransient<PatchNotes>();
        builder.Services.AddTransient<ViewSupport>();
        builder.Services.AddTransient<ViewSupports>();
        builder.Services.AddTransient<ViewSupports>();
        builder.Services.AddTransient<ViewAccounts>();

        //ViewModes
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<LogonPageViewModel>();
        builder.Services.AddTransient<LoadUpPageViewModel>();
        builder.Services.AddTransient<LandingPageViewModel>();
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
        builder.Services.AddTransient<ViewCategoryViewModel>();
        builder.Services.AddTransient<ViewSavingsViewModel>();
        builder.Services.AddTransient<ViewBillsViewModel>();
        builder.Services.AddTransient<ViewEnvelopesViewModel>();
        builder.Services.AddTransient<ViewIncomesViewModel>();
        builder.Services.AddTransient<ViewFilteredTransactionsViewModel>();
        builder.Services.AddTransient<ViewPayeesViewModel>();
        builder.Services.AddTransient<ViewCalendarViewModel>();
        builder.Services.AddTransient<EditBudgetSettingsViewModel>();
        builder.Services.AddTransient<EditAccountSettingsViewModel>();
        builder.Services.AddTransient<EditAccountDetailsViewModel>();
        builder.Services.AddTransient<NoNetworkAccessViewModel>();
        builder.Services.AddTransient<NoServerAccessViewModel>();
        builder.Services.AddTransient<PatchNotesViewModel>();
        builder.Services.AddTransient<ViewSupportViewModel>();
        builder.Services.AddTransient<ViewSupportsViewModel>();
        builder.Services.AddTransient<ViewAccountsViewModel>();

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
        builder.Services.AddTransient<PopupReassignCategories, PopupReassignCategoriesViewModel>();
        builder.Services.AddTransient<PopupEditNextPayInfo, PopupEditNextPayInfoViewModel>();
        builder.Services.AddTransient<PopupMoveBalance, PopupMoveBalanceViewModel>();
        builder.Services.AddTransient<LoadingPage>();
        builder.Services.AddTransient<LoadingPageTwo>();
        builder.Services.AddTransient<PopUpNoNetwork, PopUpNoNetworkViewModel>();
        builder.Services.AddTransient<PopUpNoServer, PopUpNoServerViewModel>();
        builder.Services.AddTransient<PopupMoveAccountBalance, PopupMoveAccountBalanceViewModel>();

        //BottomSheets
        builder.Services.AddTransient<ViewTransactionFilterBottomSheet>();
        builder.Services.AddTransient<BudgetOptionsBottomSheet>();
        builder.Services.AddTransient<ShareBudget>();
        builder.Services.AddTransient<EnvelopeOptionsBottomSheet>();
        builder.Services.AddTransient<TransactionOptionsBottomSheet>();
        builder.Services.AddTransient<EditCategoryBottomSheet>();
        builder.Services.AddTransient<AddNewCategoryBottomSheet>();
        builder.Services.AddTransient<AddSubCategoryBottomSheet>();
        builder.Services.AddTransient<EditProfilePictureBottomSheet>();
        builder.Services.AddTransient<CategoryOptionsBottomSheet>();
        builder.Services.AddTransient<PayeeOptionsBottomSheet>();
        builder.Services.AddTransient<MultipleAccountsBottomSheet>();

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


