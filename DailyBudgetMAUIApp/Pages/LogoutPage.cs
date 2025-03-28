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

        if (SecureStorage.Default.GetAsync("Session").Result != null)
        {
            SecureStorage.Default.Remove("Session");
        }

        App.DefaultBudgetID = 0;
        App.DefaultBudget = null;

        Application.Current!.MainPage = new AppShell();
        LocalNotificationCenter.Current.CancelAll();

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
    }
}