using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class NoNetworkAccessViewModel : ObservableObject
    {
        [ObservableProperty]
        private string txtSubHeading = "It looks like you have no internet connection.\nPlease make sure your WIFI and mobile network are turned on and try again.";
        [ObservableProperty]
        private bool btnIsEnabled;
        [ObservableProperty]
        private string txtConnectionStatus = "";
        [ObservableProperty]
        private Color colorConnectionStatus;
        private readonly IProductTools _pt;

        public NoNetworkAccessViewModel(IProductTools pt)
        {
            _pt = pt;
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }
        ~NoNetworkAccessViewModel()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                Application.Current.Resources.TryGetValue("Success", out var Success);

                TxtConnectionStatus = "Internet Connection Status: Connected";
                ColorConnectionStatus = (Color)Success;
                BtnIsEnabled = true;
            }
            else
            {
                Application.Current.Resources.TryGetValue("Danger", out var Danger);

                TxtConnectionStatus = "Internet Connection Status: Disconnected";
                ColorConnectionStatus = (Color)Danger;
                BtnIsEnabled = false;
            }
        }

        [RelayCommand]
        async Task GoToLandingPage()
        {
            try
            {
                if (App.UserDetails is not null && App.UserDetails.SessionExpiry > DateTime.UtcNow)
                {
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(LandingPage)}");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "NoNetworkAccess", "GoToLandingPage");
            }
        }
    }
}
