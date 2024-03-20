using CommunityToolkit.Mvvm.ComponentModel;

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


    }
}
