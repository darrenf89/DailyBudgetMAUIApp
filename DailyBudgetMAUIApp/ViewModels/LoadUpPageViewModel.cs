using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Pages;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class LoadUpPageViewModel : BaseViewModel
    {

        public LoadUpPageViewModel()
        {

        }

        [RelayCommand]
        async void Logon()
        {
            await Shell.Current.GoToAsync(nameof(LogonPage));
        }

        [RelayCommand]
        async void Register()
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }  
    }
}
