namespace DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Popups;
using Plugin.LocalNotification;

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

        Application.Current!.MainPage = new AppShell();
        LocalNotificationCenter.Current.CancelAll();

        await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
    }
}