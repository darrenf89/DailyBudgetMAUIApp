namespace DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Popups;
public class LogoutPage : ContentPage
{
	public LogoutPage()
	{
		Logout();
	}

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