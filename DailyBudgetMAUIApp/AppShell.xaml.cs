using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Popups;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.ComponentModel;
using System.Reflection;

namespace DailyBudgetMAUIApp;

public partial class AppShell : Shell
{

    public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(LoadUpPage), typeof(LoadUpPage));
        Routing.RegisterRoute(nameof(LandingPage), typeof(LandingPage));
        Routing.RegisterRoute(nameof(LogonPage), typeof(LogonPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(ErrorPage), typeof(ErrorPage));
        Routing.RegisterRoute(nameof(AddBill), typeof(AddBill));
        Routing.RegisterRoute(nameof(AddTransaction), typeof(AddTransaction));
        Routing.RegisterRoute(nameof(AddIncome), typeof(AddIncome));
        Routing.RegisterRoute(nameof(AddSaving), typeof(AddSaving));
        Routing.RegisterRoute(nameof(LogoutPage), typeof(LogoutPage));
        Routing.RegisterRoute(nameof(CreateNewBudget), typeof(CreateNewBudget));
        Routing.RegisterRoute(nameof(LoadingPage), typeof(LoadingPage));
        Routing.RegisterRoute(nameof(LoadingPageTwo), typeof(LoadingPageTwo));
        Routing.RegisterRoute(nameof(ViewTransactions), typeof(ViewTransactions));
        Routing.RegisterRoute(nameof(ViewCategories), typeof(ViewCategories));
        Routing.RegisterRoute(nameof(ViewCategory), typeof(ViewCategory));
        Routing.RegisterRoute(nameof(ViewSavings), typeof(ViewSavings));
        Routing.RegisterRoute(nameof(ViewBills), typeof(ViewBills));
        Routing.RegisterRoute(nameof(ViewEnvelopes), typeof(ViewEnvelopes));
        Routing.RegisterRoute(nameof(ViewIncomes), typeof(ViewIncomes));
        Routing.RegisterRoute(nameof(ViewFilteredTransactions), typeof(ViewFilteredTransactions));
        Routing.RegisterRoute(nameof(ViewPayees), typeof(ViewPayees));
        Routing.RegisterRoute(nameof(ViewCalendar), typeof(ViewCalendar));

        App.MainTabBar = MainTabBar;
        App.ViewTabBar = ViewTabBar;

        this.BindingContext = this;
    }

    [ICommand]
    async void Logout()
    {
        if (Preferences.ContainsKey(nameof(App.UserDetails)))
        {
            Preferences.Remove(nameof(App.UserDetails));
        }

        if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
        {
            Preferences.Remove(nameof(App.DefaultBudgetID));
        }

        Application.Current!.MainPage = new AppShell();

        await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
    }


}
