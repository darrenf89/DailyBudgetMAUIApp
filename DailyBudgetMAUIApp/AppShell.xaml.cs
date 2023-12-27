using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.ComponentModel;

namespace DailyBudgetMAUIApp;

public partial class AppShell : Shell
{

    public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute(nameof(LoadUpPage), typeof(LoadUpPage));
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

        await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
    }


}
