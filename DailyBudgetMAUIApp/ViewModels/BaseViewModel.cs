using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
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
                IPopupService ps = IPlatformApplication.Current.Services.GetService<IPopupService>();

                //TODO: SHOW POPUP
                if (App.IsPopupShowing) { App.IsPopupShowing = false; await ps.ClosePopupAsync(Application.Current.Windows[0].Page); }
                ps.ShowPopup<PopUpNoNetwork>(Application.Current.Windows[0].Page, options: new PopupOptions { CanBeDismissedByTappingOutsideOfPopup = false, PageOverlayColor = Color.FromArgb("#80000000") });
                App.IsPopupShowing = true;
                await Task.Delay(1);

                int i = 0;
                while (Connectivity.Current.NetworkAccess != NetworkAccess.Internet && i < 30)
                {
                    await Task.Delay(1000);
                    i++;
                }

                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    //TODO: SHOW POPUP SAYING INTERNET RETURNED WOULD THEY LIKE TO RETURN TO THEIR BUDGETTING
                }
                else 
                {
                    await Shell.Current.GoToAsync($"{nameof(NoNetworkAccess)}");
                }

                if (App.IsPopupShowing) { App.IsPopupShowing = false; await ps.ClosePopupAsync(Application.Current.Windows[0].Page); }
            }
        }
    }
}
