using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.Popups;
using DailyBudgetMAUIApp.ViewModels;
using DotNet.Meteor.HotReload.Plugin;
using IeuanWalker.Maui.Switch;
using Maui.FreakyEffects;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.Maui.AppRating;
using Plugin.MauiMTAdmob;
using Syncfusion.Licensing;
using Syncfusion.Maui.Core.Hosting;
using The49.Maui.BottomSheet;
using YourAppNamespace.Droid;

namespace DailyBudgetMAUIApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        SyncfusionLicenseProvider.RegisterLicense("Mzg4NzM1MEAzMjM5MmUzMDJlMzAzYjMyMzkzYlgwUFBySk55Y1pzTjduK1UwT3VJTDdWZlVSM3pacnpJRlpzb0N2TmJCTHc9");

        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .UseSwitch()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .UseBottomSheet()
            .UseMauiMTAdmob()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialDesignIcons");
                fonts.AddFont("manolo-mono.ttf", "ManoloMono");
            })
            .ConfigureEssentials(essentials =>
            {
                essentials
                    .AddAppAction("quick_transaction", "Quick Transaction", "click here to add a quick transaction to your latest budget", "transaction")
                    .OnAppAction(App.HandleAppAction);
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
        builder.Services.AddSingleton<INotificationPermissions, NotificationPermissionsImplementation>();
        builder.Services.AddSingleton<IKeyboardService, KeyboardService>();
        builder.Services.AddSingleton<ILogService, LogService>();

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
        builder.Services.AddTransient<FamilyAccountsManage>();
        builder.Services.AddTransient<CreateNewFamilyAccounts>();
        builder.Services.AddTransient<FamilyAccountLogonPage>();
        builder.Services.AddTransient<FamilyAccountMainPage>();
        builder.Services.AddTransient<ViewBudgets>();
        builder.Services.AddTransient<FamilyAccountsView>();
        builder.Services.AddTransient<FamilyAccountsEdit>();

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
        builder.Services.AddTransient<FamilyAccountsManageViewModel>();
        builder.Services.AddTransient<CreateNewFamilyAccountsViewModel>();
        builder.Services.AddTransient<FamilyAccountLogonPageViewModel>();
        builder.Services.AddTransient<FamilyAccountMainPageViewModel>();
        builder.Services.AddTransient<ViewBudgetsViewModel>();
        builder.Services.AddTransient<FamilyAccountsViewViewModel>();
        builder.Services.AddTransient<FamilyAccountsEditViewModel>();

        //Popups
        builder.Services.AddSingleton<PopUpPage>();
        builder.Services.AddTransientPopup<PopUpPageSingleInput, PopUpPageSingleInputViewModel>();
        builder.Services.AddTransientPopup<PopupInfo>();
        builder.Services.AddTransientPopup<PopUpOTP, PopUpOTPViewModel>();
        builder.Services.AddTransientPopup<PopupDailySaving, PopupDailySavingViewModel>();
        builder.Services.AddTransientPopup<PopupDailyBill, PopupDailyBillViewModel>();
        builder.Services.AddTransientPopup<PopupDailyPayDay, PopupDailyPayDayViewModel>();
        builder.Services.AddTransientPopup<PopupDailyIncome, PopupDailyIncomeViewModel>();
        builder.Services.AddTransientPopup<PopupDailyTransaction, PopupDailyTransactionViewModel>();
        builder.Services.AddTransientPopup<PopupReassignCategories, PopupReassignCategoriesViewModel>();
        builder.Services.AddTransientPopup<PopupEditNextPayInfo, PopupEditNextPayInfoViewModel>();
        builder.Services.AddTransientPopup<PopupMoveBalance, PopupMoveBalanceViewModel>();
        builder.Services.AddTransientPopup<LoadingPage>();
        builder.Services.AddTransientPopup<LoadingPageTwo>();
        builder.Services.AddTransientPopup<PopUpNoNetwork, PopUpNoNetworkViewModel>();
        builder.Services.AddTransientPopup<PopUpNoServer, PopUpNoServerViewModel>();
        builder.Services.AddTransientPopup<PopupMoveAccountBalance, PopupMoveAccountBalanceViewModel>();
        builder.Services.AddTransientPopup<PopupDailyAllowance, PopupDailyAllowanceViewModel>();

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

        builder.Services.AddTransient<BaseViewModel>();

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


