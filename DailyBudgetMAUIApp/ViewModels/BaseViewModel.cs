using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Pages;
using static Android.Telephony.CarrierConfigManager;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsPageBusy { get; set; } = false;

        [ObservableProperty]
        public partial bool IsButtonBusy { get; set; }

        [ObservableProperty]
        public partial string Title { get; set; }

        [ObservableProperty]
        public partial bool IsPremiumAccount { get; set; } = App.IsPremiumAccount;


        public BaseViewModel()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }
        ~BaseViewModel()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            Console.WriteLine("Connectivity changed event started...");

            // Example async operation (e.g., checking a server or updating UI)
            await HandleConnectivityChangeAsync(e);

            Console.WriteLine("Connectivity changed event completed.");

        }

        private async Task HandleConnectivityChangeAsync(ConnectivityChangedEventArgs e)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet && !App.IsBackgrounded)
            {
                var mps = IPlatformApplication.Current.Services.GetService<IModalPopupService>();
                await mps.ShowAsync<PopUpNoNetwork>(() => IPlatformApplication.Current.Services.GetService<PopUpNoNetwork>());

                int i = 0;
                while (Connectivity.Current.NetworkAccess != NetworkAccess.Internet && i < 30)
                {
                    await Task.Delay(1000);
                    i++;
                }

                await mps.CloseAsync<PopUpNoNetwork>();

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    //TODO: SHOW POPUP SAYING INTERNET RETURNED WOULD THEY LIKE TO RETURN TO THEIR BUDGETTING
                }
                else 
                {
                    await Shell.Current.GoToAsync($"{nameof(NoNetworkAccess)}");
                }

            }
        }
    }
}
