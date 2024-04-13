using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Converters;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using Microsoft.Maui.Primitives;
using System.Globalization;
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
        private string newEmail;
        [ObservableProperty]
        private string newPassword;        
        [ObservableProperty]
        private string newNickName;
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
        [ObservableProperty]
        private bool emailChangedMessageVisible;
        [ObservableProperty]
        private bool emailNotChangedMessageVisible;
        [ObservableProperty]
        private bool emailValid;
        [ObservableProperty]
        private bool emailRequired;
        [ObservableProperty]
        private bool nicknameChangedMessageVisible;
        [ObservableProperty]
        private bool nicknameNotChangedMessageVisible;
        [ObservableProperty]
        private bool nickNameRequired;
        [ObservableProperty]
        private string currentBudgetName;
        [ObservableProperty]
        private List<Budgets> userBudgets;  
        [ObservableProperty]
        private string currentSubStatus;
        [ObservableProperty]
        public BorderlessPicker switchBudgetPicker;        
        [ObservableProperty]
        public Budgets selectedBudget;        
        [ObservableProperty]
        public bool isDPA;


        public EditAccountSettingsViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
        }      
        
        public async Task OnLoad()
        {
            User = await _ds.GetUserDetailsAsync(App.UserDetails.Email);
            CurrentSubStatus = $"{User.SubscriptionType} expires on {User.SubscriptionExpiry.ToString("d", CultureInfo.CurrentCulture)}";

            UserBudgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "EditAccountSettings");

            Application.Current.Resources.TryGetValue("White", out var White);
            Application.Current.Resources.TryGetValue("Info", out var Info);
            Application.Current.Resources.TryGetValue("Primary", out var Primary);
            Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

            BorderlessPicker picker = new BorderlessPicker
            {
                Title = "Select a budget",
                ItemsSource = UserBudgets,
                TitleColor = (Color)Gray900,
                BackgroundColor = (Color)White,
                TextColor = (Color)Info,
                Margin = new Thickness(20, 0, 0, 0),
            };

            picker.ItemDisplayBinding = new Binding(".", BindingMode.Default, new ChangeBudgetStringConvertor());

            picker.SelectedIndexChanged += async (s, e) =>
            {
                var picker = (Picker)s;
                var SelectedBudget = (Budgets)picker.SelectedItem;

                await _pt.ChangeDefaultBudget(App.UserDetails.UserID, SelectedBudget.BudgetID, false);
                CurrentBudgetName = SelectedBudget.BudgetName;
            };            
            
            for (int i = 0; i < UserBudgets.Count; i++)
            {
                if (UserBudgets[i].BudgetID == User.DefaultBudgetID)
                {
                    CurrentBudgetName = UserBudgets[i].BudgetName;
                    picker.SelectedItem = UserBudgets[i];
                }
            }

            SwitchBudgetPicker = picker;

            IsDPA = User.IsDPAPermissions;
        }

        [RelayCommand]
        private async Task CloseSettings(object obj)
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
        private async Task DeleteUserAccount()
        {
            var Email = await Application.Current.MainPage.DisplayPromptAsync($"Are you sure you want to delete your account?", $"Enter the accounts email address to delete the account", "Ok", "Cancel");
            if (Email != null)
            {
                if(string.Equals(Email, App.UserDetails.Email, StringComparison.OrdinalIgnoreCase))
                {
                    string result = await _ds.DeleteUserAccount(App.UserDetails.UserID);
                    if (result == "OK")
                    {
                        AppShell Shell = new AppShell();
                        await Shell.Logout();

                        await Application.Current.MainPage.DisplayAlert($"Account Deleted", $"Your account has been permanently deleted and there is no way to recover it now. If you want to start budgeting again create a new account", "Ok");

                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert($"Opps something went wrong", $"There was an issue deleting your account, please try again and if the issue persists please contact us so can help", "Ok");

                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("That is not the correct email", "Your account has not been deleted please try again." ,"OK");
                }
            }
        }

        [RelayCommand]
        private async Task UpdatePassword()
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
        private async Task UpdateNickname()
        {
            await Task.Delay(15);

            if (NickNameRequired)
            {
                bool UpdateNickName = await Application.Current.MainPage.DisplayAlert($"Are you sure you want to update your nickname?", $"Are you sure you want to update your nickaname to {NewNickName}?", "Yes", "No");
                if (UpdateNickName)
                {
                    List<PatchDoc> UserUpdate = new List<PatchDoc>();

                    PatchDoc EmailPatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/NickName",
                        value = NewNickName
                    };

                    UserUpdate.Add(EmailPatch);

                    string result = await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

                    if (result == "OK")
                    {
                        User.NickName = NewNickName;

                        NicknameChangedMessageVisible = true;
                        NicknameNotChangedMessageVisible = false;
                        NewNickName = "";
                        NickNameRequired = true;
                        
                    }
                    else
                    {
                        NicknameChangedMessageVisible = false;
                        NicknameNotChangedMessageVisible = true;
                        NewNickName = "";
                        NickNameRequired = true;
                    }

                }
            }
        }


        [RelayCommand]
        private async Task UpdateEmail()
        {
            await Task.Delay(5);

            if (EmailRequired & EmailValid)
            {
                bool UpdateEmail = await Application.Current.MainPage.DisplayAlert($"Are you sure you want to update your email?", $"Are you sure you want to update your email to {NewEmail}? Make sure you have access to this email or you might have some issues!", "Yes", "No");
                if (UpdateEmail)
                {
                    UserDetailsModel UserDetails = await _ds.GetUserDetailsAsync(NewEmail);
                    if (UserDetails.Error != null)
                    {
                        List<PatchDoc> UserUpdate = new List<PatchDoc>();

                        PatchDoc EmailPatch = new PatchDoc
                        {
                            op = "replace",
                            path = "/Email",
                            value = NewEmail
                        };

                        UserUpdate.Add(EmailPatch);

                        string result = await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

                        if(result == "OK")
                        {
                            User.Email = NewEmail;

                            EmailChangedMessageVisible = true;
                            EmailNotChangedMessageVisible = false;
                            NewEmail = "";
                            EmailValid = true;
                            EmailRequired = true;
                            
                        }
                        else
                        {
                            EmailChangedMessageVisible = false;
                            EmailNotChangedMessageVisible = true;
                            NewEmail = "";
                            EmailValid = true;
                            EmailRequired = true;
                        }
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Opps", "An account already exists with this email. Please use a different email or recover the account if this is your email", "OK");
                    }


                }
            }
        }
            

        [RelayCommand]
        private async Task ChangeSelectedProfilePic()
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

            await page.ShowAsync();
        }

        public async Task<Stream> GetUserProfilePictureStream(int UserID)
        {
            return await _ds.DownloadUserProfilePicture(UserID);
        }

        [RelayCommand]
        private async Task CreateNewBudget()
        {
            string? SubType = "Basic";
            string BudgetType = "";

            if (!string.IsNullOrEmpty(App.UserDetails.SubscriptionType))
            {
                SubType = App.UserDetails.SubscriptionType;
            }

            string action = "Basic";

            if (SubType == "Premium")
            {
                action = await Shell.Current.DisplayActionSheet("What type of budget would you like to create?", "Cancel", null, "Basic", "Premium");
            }
            else if (SubType == "PremiumPlus")
            {
                action = await Shell.Current.DisplayActionSheet("What type of budget would you like to create?", "Cancel", null, "Basic", "Premium", "PremiumPlus");
            }

            if (action != "Cancel")
            {
                BudgetType = action;

                bool result = await Shell.Current.DisplayAlert("Create a new budget?", $"Are you sure you want to create a new {BudgetType} budget?", "Yes", "No");
                if (result)
                {
                    Budgets NewBudget = await _ds.CreateNewBudget(App.UserDetails.Email, BudgetType);
                    await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.CreateNewBudget)}?BudgetID={NewBudget.BudgetID}&NavigatedFrom=Budget Settings");

                }
            }
        }

        partial void OnIsDPAChanged(bool oldValue, bool newValue)
        {
            List<PatchDoc> UserUpdate = new List<PatchDoc>();

            PatchDoc IsDPAPermissions = new PatchDoc
            {
                op = "replace",
                path = "/isDPAPermissions",
                value = IsDPA
            };

            UserUpdate.Add(IsDPAPermissions);

            _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);
        }

    }
}
