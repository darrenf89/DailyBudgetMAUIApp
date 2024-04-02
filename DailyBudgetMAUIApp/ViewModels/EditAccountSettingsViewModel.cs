using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class EditAccountSettingsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private UserDetailsModel user;

        public EditAccountSettingsViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
        }      
        
        public async Task OnLoad()
        {
            User = await _ds.GetUserDetailsAsync(App.UserDetails.Email);
        }

        [RelayCommand]
        private async void CloseSettings(object obj)
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.MainPage.ShowPopup(PopUp);
            }

            await Task.Delay(500);

            await Shell.Current.GoToAsync($"..");
        }

        [RelayCommand]
        private async void DeleteUserAccount()
        {
            bool DeleteUser = await Application.Current.MainPage.DisplayAlert($"Are you sure you want to delete {App.DefaultBudget.BudgetName} budget?", $"Deleting the budget is permanent make sure you are sure before hitting yes?", "Yes", "No");
            if (DeleteUser)
            {

            }
        }

        [RelayCommand]
        private async void ChangeSelectedProfilePic()
        {
            EditProfilePictureBottomSheet page = new EditProfilePictureBottomSheet( new ProductTools(new RestDataService()), new RestDataService());

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent            
                {
                    IsDefault = true
                },
                new MediumDetent(),
                new FullscreenDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            page.ShowAsync();
        }

        public async Task<Stream> GetUserProfilePictureStream(int UserID)
        {
            return await _ds.DownloadUserProfilePicture(UserID);
        }

    }
}
