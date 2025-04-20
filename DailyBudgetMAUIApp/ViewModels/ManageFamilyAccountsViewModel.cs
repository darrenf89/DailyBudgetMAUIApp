using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using static Java.Lang.ProcessBuilder;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ManageFamilyAccountsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private double signOutButtonWidth;
        [ObservableProperty]
        private List<FamilyUserAccount> familyAccounts;

        public ManageFamilyAccountsViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Family Accounts";

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            SignOutButtonWidth = ScreenWidth - 60;
        }

        public async Task LoadFamilyAccounts()
        {

        }

        [RelayCommand]
        public async Task AddNewFamilyAccount()
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Task.Delay(1);

                await Shell.Current.GoToAsync($"{nameof(CreateNewFamilyAccounts)}?AccountID={0}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ManageFamilyAccounts", "AddNewFamilyAccount");
            }
        }


        [RelayCommand]
        public async Task BackButton()
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ManageFamilyAccounts", "BackButton");
            }
        }
    }
}
