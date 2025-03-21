using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Popups;
using Plugin.LocalNotification;

namespace DailyBudgetMAUIApp;

public partial class AppShell : Shell
{

    public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute($"{nameof(ViewTransactions)}/{nameof(AddTransaction)}", typeof(AddTransaction));
        Routing.RegisterRoute($"{nameof(MainPage)}/{nameof(AddTransaction)}", typeof(AddTransaction));
        Routing.RegisterRoute($"{nameof(ViewBills)}/{nameof(AddBill)}", typeof(AddBill));
        Routing.RegisterRoute($"{nameof(MainPage)}/{nameof(AddBill)}", typeof(AddBill));
        Routing.RegisterRoute($"{nameof(ViewSavings)}/{nameof(AddSaving)}", typeof(AddSaving));
        Routing.RegisterRoute($"{nameof(MainPage)}/{nameof(AddSaving)}", typeof(AddSaving));
        Routing.RegisterRoute($"{nameof(ViewIncomes)}/{nameof(AddIncome)}", typeof(AddIncome));
        Routing.RegisterRoute($"{nameof(MainPage)}/{nameof(AddIncome)}", typeof(AddIncome));
        Routing.RegisterRoute($"{nameof(ViewEnvelopes)}/{nameof(AddSaving)}", typeof(AddSaving));
        Routing.RegisterRoute($"{nameof(CreateNewBudget)}/{nameof(AddBill)}", typeof(AddBill));
        Routing.RegisterRoute($"{nameof(CreateNewBudget)}/{nameof(AddIncome)}", typeof(AddIncome));
        Routing.RegisterRoute($"{nameof(CreateNewBudget)}/{nameof(AddSaving)}", typeof(AddSaving));
        Routing.RegisterRoute($"{nameof(CreateNewBudget)}", typeof(CreateNewBudget));
        Routing.RegisterRoute(nameof(LogoutPage), typeof(LogoutPage));
        Routing.RegisterRoute(nameof(LoadingPage), typeof(LoadingPage));
        Routing.RegisterRoute(nameof(LoadingPageTwo), typeof(LoadingPageTwo));
        Routing.RegisterRoute(nameof(SelectPayeePage), typeof(SelectPayeePage));
        Routing.RegisterRoute(nameof(SelectCategoryPage), typeof(SelectCategoryPage));
        Routing.RegisterRoute(nameof(SelectSavingCategoryPage), typeof(SelectSavingCategoryPage));
        Routing.RegisterRoute(nameof(ErrorPage), typeof(ErrorPage));
        Routing.RegisterRoute(nameof(NoNetworkAccess), typeof(NoNetworkAccess));
        Routing.RegisterRoute(nameof(NoServerAccess), typeof(NoServerAccess));
        Routing.RegisterRoute($"{nameof(ViewCategories)}/{nameof(ViewCategory)}", typeof(ViewCategory));
        Routing.RegisterRoute($"{nameof(ViewFilteredTransactions)}", typeof(ViewFilteredTransactions));
        Routing.RegisterRoute($"{nameof(LogonPage)}", typeof(LogonPage));
        Routing.RegisterRoute($"{nameof(RegisterPage)}", typeof(RegisterPage));
        Routing.RegisterRoute($"{nameof(ViewSupport)}", typeof(ViewSupport));
        Routing.RegisterRoute($"{nameof(ViewSupports)}", typeof(ViewSupports));
        Routing.RegisterRoute($"{nameof(ViewAccounts)}", typeof(ViewAccounts));
        Routing.RegisterRoute($"{nameof(LogoutPage)}", typeof(LogoutPage));

        this.BindingContext = this;
    }

    [RelayCommand]
    public async Task Logout()
    {
        if (Preferences.ContainsKey(nameof(App.UserDetails)))
        {
            Preferences.Remove(nameof(App.UserDetails));
        }

        if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
        {
            Preferences.Remove(nameof(App.DefaultBudgetID));
        }

        if (await SecureStorage.Default.GetAsync("Session") != null)
        {
            SecureStorage.Default.Remove("Session");
        }

        App.DefaultBudgetID = 0;
        App.DefaultBudget = null;

        Application.Current!.MainPage = new AppShell();
        LocalNotificationCenter.Current.CancelAll();

        await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
    }


}
