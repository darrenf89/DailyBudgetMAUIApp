using CommunityToolkit.Maui.Behaviors;
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
        [ObservableProperty]
        private string currentPassword;
        [ObservableProperty]
        private string newPassword;
        [ObservableProperty]
        private string newPasswordConfirm;
        [ObservableProperty]
        private bool currentPasswordValid = true;
        [ObservableProperty]
        private bool newPasswordValid;
        [ObservableProperty]
        private bool newPasswordMatch = true;
        [ObservableProperty]
        private bool passwordRequired;
        [ObservableProperty]
        private bool newPasswordRequired;
        [ObservableProperty]
        private bool passwordConfirmRequired;
        [ObservableProperty]
        private bool passwordChangedMessageVisible;
        [ObservableProperty]
        private bool passwordNotChangedMessageVisible;

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
        private async void UpdatePassword()
        {


            await Task.Delay(5);

            if (NewPasswordRequired & PasswordRequired & PasswordConfirmRequired & NewPasswordValid)
            {
                bool UpdatePassword = await Application.Current.MainPage.DisplayAlert($"Are you sure you want to update your password?", $"Forgot your current password and you can reset it from the logon screen using your email", "Yes", "No");
                if (UpdatePassword)
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Task.Delay(1);

                    string salt = await _ds.GetUserSaltAsync(App.UserDetails.Email);
                    UserDetailsModel userDetails = await _ds.GetUserDetailsAsync(App.UserDetails.Email);
                    string HashPassword = _pt.GenerateHashedPassword(CurrentPassword, salt);

                    if (userDetails.Password != HashPassword)
                    {
                        CurrentPasswordValid = false;
                        NewPasswordMatch = true;
                    }
                    else
                    {
                        if(NewPassword != NewPasswordConfirm)
                        {
                            NewPasswordMatch = false;
                            CurrentPasswordValid = true;
                        }
                        else
                        {
                            RegisterModel NewPasswordUser = new RegisterModel
                            {
                                Password = NewPassword
                            };


                            NewPasswordUser = _pt.CreateUserSecurityDetails(NewPasswordUser);

                            if(!string.IsNullOrEmpty(NewPasswordUser.Salt))
                            {
                                List<PatchDoc> UserUpdate = new List<PatchDoc>();

                                PatchDoc Salt = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/Salt",
                                    value = NewPasswordUser.Salt
                                };

                                PatchDoc Password = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/Password",
                                    value = NewPasswordUser.Password
                                };

                                UserUpdate.Add(Salt);
                                UserUpdate.Add(Password);

                                string result = await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);
                                if(result == "OK")
                                {
                                    PasswordChangedMessageVisible = true;
                                    PasswordNotChangedMessageVisible = false;

                                    CurrentPassword = "";
                                    NewPassword = "";
                                    NewPasswordConfirm = "";

                                    NewPasswordRequired = true;
                                    PasswordRequired = true;
                                    PasswordConfirmRequired = true;
                                    NewPasswordValid = true;
                                    NewPasswordMatch = true;
                                    CurrentPasswordValid = true;

                                }
                                else
                                {
                                    PasswordNotChangedMessageVisible = true;
                                    PasswordChangedMessageVisible = false;

                                    CurrentPassword = "";
                                    NewPassword = "";
                                    NewPasswordConfirm = "";

                                    NewPasswordRequired = true;
                                    PasswordRequired = true;
                                    PasswordConfirmRequired = true;
                                    NewPasswordValid = true;
                                    NewPasswordMatch = true;
                                    CurrentPasswordValid = true;
                                }

                            }
                        }
                    }
                }
            }

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
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
