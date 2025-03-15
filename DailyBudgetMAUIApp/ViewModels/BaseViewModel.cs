using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Pages;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool  isPageBusy = false;
        [ObservableProperty]
        private bool  isButtonBusy;
        [ObservableProperty]
        private string  title;
        [ObservableProperty]
        private bool isPremiumAccount = App.IsPremiumAccount;

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

                //TODO: SHOW POPUP
                if (App.CurrentPopUp != null)
                {
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                }

                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpNoNetwork(new PopUpNoNetworkViewModel());
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

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

                if (App.CurrentPopUp != null)
                {
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                }

            }
        }
    }
}
