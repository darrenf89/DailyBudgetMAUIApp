using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Error), nameof(Error))]
    public partial class ErrorPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ErrorLog _error;
        [ObservableProperty]
        private string txtErrorMessage = "";

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        public ErrorPageViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
        }

        [RelayCommand]
        async void GoToLandingPage()
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
                await _pt.HandleException(ex, "ErrorPage", "GoToLandingPage");
            }
        }

        public async Task HandleException(Exception ex, string page, string Method)
        {
            await _pt.HandleException(ex, page, Method);
        }
    }
}
