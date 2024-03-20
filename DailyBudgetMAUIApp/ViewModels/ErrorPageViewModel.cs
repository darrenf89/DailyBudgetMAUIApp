using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Error), nameof(Error))]
    public partial class ErrorPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ErrorLog _error;

        public ErrorPageViewModel()
        {

        }

        [RelayCommand]
        async void GoToLandingPage()
        {
            await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
        }
    }
}
