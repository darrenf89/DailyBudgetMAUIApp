namespace DailyBudgetMAUIApp.Pages;

using CommunityToolkit.Maui;
using Plugin.LocalNotification;

public class LogoutPage : ContentPage
{
    private readonly IPopupService _ps;
	public LogoutPage(IPopupService ps)
	{
        _ps = ps;
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

        if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
        {
            Preferences.Remove(nameof(App.IsFamilyAccount));
        }

        if (await SecureStorage.Default.GetAsync("Session") != null)
        {
            SecureStorage.Default.Remove("Session");
        }

        if (Preferences.ContainsKey("IsTopStickyVisible"))
        {
            Preferences.Remove("IsTopStickyVisible");
        }

        App.DefaultBudgetID = 0;
        App.DefaultBudget = null;

        LocalNotificationCenter.Current.CancelAll();

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
    }
}