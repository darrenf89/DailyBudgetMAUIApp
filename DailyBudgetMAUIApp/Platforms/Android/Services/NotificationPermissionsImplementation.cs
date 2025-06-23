using Android.Content;
using DailyBudgetMAUIApp.DataServices;

[assembly: Dependency(typeof(NotificationPermissionsImplementation))]
public class NotificationPermissionsImplementation : INotificationPermissions
{
    public Task OpenNotificationSettingsAsync()
    {
        Android.Content.Context context = Android.App.Application.Context;
        var currentActivity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;

        var intent = new Intent();
        intent.SetAction(Android.Provider.Settings.ActionAppNotificationSettings);
        intent.PutExtra(Android.Provider.Settings.ExtraAppPackage, context.PackageName);
        intent.PutExtra("NotificationPermissionsImplementation",true);
        intent.AddFlags(ActivityFlags.NewTask);
        currentActivity.StartActivity(intent);
        return Task.CompletedTask;
    }
}