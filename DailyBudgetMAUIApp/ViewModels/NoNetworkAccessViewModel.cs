using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class NoNetworkAccessViewModel : BaseViewModel
    {
        public NoNetworkAccessViewModel()
        {

        }

        [RelayCommand]
        async void GoToLandingPage()
        {
            await Shell.Current.GoToAsync($"//{nameof(LandingPage)}");
        }
    }
}
