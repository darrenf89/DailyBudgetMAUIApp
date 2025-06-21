using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Syncfusion.Maui.Core;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class FamilyAccountsManageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        public partial double SignOutButtonWidth { get; set; }

        [ObservableProperty]
        public partial List<FamilyUserAccount> FamilyAccounts { get; set; }


        public FamilyAccountsManageViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Family Accounts";

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            SignOutButtonWidth = ScreenWidth - 60;
        }

        public async Task LoadFamilyAccounts()
        {
            FamilyAccounts = await _ds.GetUserFamilyAccounts(App.UserDetails.UserID);

            foreach(FamilyUserAccount account in FamilyAccounts)
            {
                account.AvatarView = new SfAvatarView
                {
                    HeightRequest = 75,
                    WidthRequest = 75,
                    CornerRadius = 5,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    StrokeThickness = 0
                };

                if (account.ProfilePicture.Contains("Avatar"))
                {
                    bool Success = Enum.TryParse(account.ProfilePicture, out AvatarCharacter Avatar);
                    if (Success)
                    {
                        account.AvatarView.ContentType = ContentType.AvatarCharacter;
                        account.AvatarView.AvatarCharacter = Avatar;
                        int Number = Convert.ToInt32(account.ProfilePicture[account.ProfilePicture.Length - 1]);
                        Math.DivRem(Number, 8, out int index);
                        account.AvatarView.Background = App.ChartColor[index];
                    }
                    else
                    {
                        account.AvatarView.AvatarCharacter = AvatarCharacter.Avatar1;
                        account.AvatarView.Background = App.ChartColor[1];
                    }
                }
                else
                {
                    Stream ProfilePicStream = await _ds.DownloadUserProfilePicture(account.UniqueUserID);
                    account.AvatarView.ContentType = ContentType.Custom;
                    account.AvatarView.ImageSource = ImageSource.FromStream(() => ProfilePicStream);
                }

                if(account.Budgets != null && account.Budgets.Count > 0)
                {
                    account.IsBudgetCreated = account.Budgets[0].IsCreated;
                }
                else
                {
                    account.IsBudgetCreated = true;
                }
            }
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
                await _pt.HandleException(ex, "FamilyAccountsManage", "AddNewFamilyAccount");
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
                await Task.Delay(1);

                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountsManage", "BackButton");
            }
        }


        [RelayCommand]
        private async Task CompleteBudgetSetUp(object obj)
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

                FamilyUserAccount User = (FamilyUserAccount)obj;
                await Shell.Current.GoToAsync($"{nameof(CreateNewFamilyAccounts)}?AccountID={User.UserID}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountsManage", "CompleteBudgetSetUp");
            }
        }


        [RelayCommand]
        private async Task ManageFamilyAccount(object obj)
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

                FamilyUserAccount User = (FamilyUserAccount)obj;
                await Shell.Current.GoToAsync($"//{nameof(FamilyAccountsView)}?FamilyAccountID={User.UserID}");

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountsManage", "ManageFamilyAccount");
            }
        }


        [RelayCommand]
        private async Task ActivateDeactivate(object obj)
        {            
            try
            {
                FamilyUserAccount User = (FamilyUserAccount)obj;

                bool IsAccept = await Shell.Current.DisplayAlert(User.IsActive ? "Deactivate account?" : "Reactivate account?", User.IsActive ? "Are you sure you want to deactivate this account? The user will no longer be able to log into the app." : "Are you sure you want to ractivate this account? The user will be able to access the application again.", "Yes", "Cancel");

                if (IsAccept)
                {
                    List<PatchDoc> UpdateUserDetails = new List<PatchDoc>();

                    PatchDoc IsActive = new PatchDoc
                    {
                        op = "replace",
                        path = "/IsActive",
                        value = User.IsActive ? false : true
                    };

                    UpdateUserDetails.Add(IsActive);

                    await _ds.PatchFamilyUserAccount(User.UserID, UpdateUserDetails);
                    await LoadFamilyAccounts();
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountsManage", "ManageFamilyAccount");
            }
        }

        [RelayCommand]
        private async Task DeleteFamilyAccount(object obj)
        {            
            try
            {
                FamilyUserAccount User = (FamilyUserAccount)obj;

                bool IsAccept = await Shell.Current.DisplayAlert("Are you sure you want to Abandon Account Creation?", "You haven't completed setting up your budget yet. If you'd like, you can abandon the account creation process and delete all associated data. Are you sure you want to proceed? Tap 'Yes' to continue.", "Yes", "Cancel");

                if (IsAccept)
                {
                    string result = await _ds.DeleteFamilyUserAccount(User.UserID);
                    if (result != "OK")
                    {
                        await _pt.MakeSnackBar("Sorry there was an issue deleting the account. Please try again or contact our support.", null, null, TimeSpan.FromSeconds(5), "Error");
                    }
                    else
                    {
                        await _pt.MakeSnackBar("Family Account successfully Deleted", null, null, TimeSpan.FromSeconds(5), "Success");
                        await LoadFamilyAccounts();
                    }
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountsManage", "DeleteFamilyAccount");
            }
        }
    }
}
