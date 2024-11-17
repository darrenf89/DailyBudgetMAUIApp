using Android.App;
using Firebase.Messaging;
using DailyBudgetMAUIApp.DataServices;
using DailySpendWebApp.Models;
using AndroidX.Core.App;
using Android.Content;
using Android.Gms.Extensions;


namespace DailyBudgetMAUIApp.Platforms.Android.Services
{
    [Service(Exported = true)]
    [IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
    public class FIrebaseService : FirebaseMessagingService
    {
        public FIrebaseService()
        {

        }

        public async override void OnNewToken(string token)
        {     
            base.OnNewToken(token);
            bool success = true;

            try
            {
                if (await SecureStorage.Default.GetAsync("FirebaseToken") != null)
                {
                    success = SecureStorage.Default.Remove("FirebaseToken");
                    success = SecureStorage.Default.Remove("FirebaseID");
                }

            }
            catch (Java.Security.GeneralSecurityException ex)
            {                
                SecureStorage.Default.RemoveAll();
            }

            if (success)
            {
                RestDataService _ds = new RestDataService();

                FirebaseDevices NewDevice = new FirebaseDevices
                {
                    FirebaseToken = token,
                    DeviceModel = DeviceInfo.Current.Name,
                    DeviceName = DeviceInfo.Current.Model
                };

                if (App.UserDetails != null)
                {
                    if (App.UserDetails.UserID != 0)
                    {
                        NewDevice.UserAccountID = App.UserDetails.UserID;
                        NewDevice.LoginExpiryDate = App.UserDetails.SessionExpiry;
                    };
                }
                else
                {
                    NewDevice.UserAccountID = 0;
                    NewDevice.LoginExpiryDate = DateTime.UtcNow;
                }

                NewDevice = _ds.RegisterNewFirebaseDevice(NewDevice).Result;

                await SecureStorage.Default.SetAsync("FirebaseToken", token);
                await SecureStorage.Default.SetAsync("FirebaseID", NewDevice.FirebaseDeviceID.ToString());
            }         
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            var notification = message.GetNotification();

            SendNotification(notification.Body, notification.Title, message.Data);

        }


        private void SendNotification(string messageBody, string title, IDictionary<string,string> data)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            string notificationType = "";

            foreach(var key in data.Keys)
            {
                string value = data[key];
                intent.PutExtra(key, value);
                if(key == "NavigationType")
                {
                    notificationType = value;
                }
            }

            PendingIntent pendingIntent = null;
            if (OperatingSystem.IsOSPlatformVersionAtLeast("android", 31))
            {
                pendingIntent = PendingIntent.GetActivity(this, MainActivity.NotificationID, intent, PendingIntentFlags.Mutable);
            }
            else
            {
                pendingIntent = PendingIntent.GetActivity(this, MainActivity.NotificationID, intent, PendingIntentFlags.OneShot);
            }

            NotificationCompat.Builder notificationBuilder;

            switch (notificationType)
            {
                case "ShareBudget":
                    notificationBuilder = new NotificationCompat.Builder(this, MainActivity.Channel_ID)
                        .SetContentTitle(title)
                        .SetContentText(messageBody)
                        .SetSmallIcon(Resource.Mipmap.appicon)
                        .SetChannelId(MainActivity.Channel_ID)
                        .SetContentIntent(pendingIntent)
                        .SetPriority(1);
                    break;
                case "SupportReplay":
                    notificationBuilder = new NotificationCompat.Builder(this, MainActivity.Support_Channel_ID)
                        .SetContentTitle(title)
                        .SetContentText(messageBody)
                        .SetSmallIcon(Resource.Mipmap.appicon)
                        .SetChannelId(MainActivity.Channel_ID)
                        .SetContentIntent(pendingIntent)
                        .SetPriority(1);
                    break;
                default:
                    notificationBuilder = new NotificationCompat.Builder(this, MainActivity.Channel_ID)
                        .SetContentTitle(title)
                        .SetContentText(messageBody)
                        .SetSmallIcon(Resource.Mipmap.appicon)
                        .SetChannelId(MainActivity.Channel_ID)
                        .SetContentIntent(pendingIntent)
                        .SetPriority(1);
                    break;

            }

            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NotificationID, notificationBuilder.Build());               
                
        }
    }
}
