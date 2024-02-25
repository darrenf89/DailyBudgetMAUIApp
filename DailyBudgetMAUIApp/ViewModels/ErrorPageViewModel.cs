using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;


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

        [ICommand]
        async void GoToLandingPage()
        {
            await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
        }
    }
}
