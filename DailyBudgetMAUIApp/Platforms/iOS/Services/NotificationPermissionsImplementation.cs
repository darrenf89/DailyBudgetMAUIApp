using DailyBudgetMAUIApp.DataServices;
using UIKit;

[assembly: Dependency(typeof(NotificationPermissionsImplementation))]
public class NotificationPermissionsImplementation : INotificationPermissions
{
    public Task OpenNotificationSettingsAsync()
    {
        var url = new Uri("app-settings:"); // Will take user to the app settings page
        if (UIApplication.SharedApplication.CanOpenUrl(url))
        {
            UIApplication.SharedApplication.OpenUrl(url);
        }
        return Task.CompletedTask;
    }
}