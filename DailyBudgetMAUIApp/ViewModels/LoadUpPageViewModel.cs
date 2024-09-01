using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class LoadUpPageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        public LoadUpPageViewModel(IProductTools pt)
        {
            _pt = pt;
        }

        [RelayCommand]
        async void Logon()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(LogonPage));
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "LoadUpPage", "Logon");
            }

            
        }

        [RelayCommand]
        async void Register()
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(RegisterPage));
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "LoadUpPage", "Register");
            }
            
        }  
    }
}
