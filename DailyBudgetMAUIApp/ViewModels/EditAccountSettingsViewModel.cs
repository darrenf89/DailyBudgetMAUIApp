using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Converters;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using Plugin.LocalNotification;
using System.Globalization;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class EditAccountSettingsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        private readonly INotificationPermissions _notificationPermissions;
        private readonly IModalPopupService _ps;

        [ObservableProperty]
        public partial UserDetailsModel User { get; set; }

        [ObservableProperty]
        public partial FamilyUserAccount FamilyUser { get; set; }

        [ObservableProperty]
        public partial string CurrentPassword { get; set; }

        [ObservableProperty]
        public partial string NewEmail { get; set; }

        [ObservableProperty]
        public partial string NewPassword { get; set; }

        [ObservableProperty]
        public partial string NewNickName { get; set; }

        [ObservableProperty]
        public partial string NewPasswordConfirm { get; set; }

        [ObservableProperty]
        public partial bool CurrentPasswordValid { get; set; } = true;

        [ObservableProperty]
        public partial bool NewPasswordValid { get; set; }

        [ObservableProperty]
        public partial bool NewPasswordMatch { get; set; } = true;

        [ObservableProperty]
        public partial bool PasswordRequired { get; set; }

        [ObservableProperty]
        public partial bool NewPasswordRequired { get; set; }

        [ObservableProperty]
        public partial bool PasswordConfirmRequired { get; set; }

        [ObservableProperty]
        public partial bool PasswordChangedMessageVisible { get; set; }

        [ObservableProperty]
        public partial bool PasswordNotChangedMessageVisible { get; set; }

        [ObservableProperty]
        public partial bool EmailChangedMessageVisible { get; set; }

        [ObservableProperty]
        public partial bool EmailNotChangedMessageVisible { get; set; }

        [ObservableProperty]
        public partial bool EmailValid { get; set; }

        [ObservableProperty]
        public partial bool EmailRequired { get; set; }

        [ObservableProperty]
        public partial bool NicknameChangedMessageVisible { get; set; }

        [ObservableProperty]
        public partial bool NicknameNotChangedMessageVisible { get; set; }

        [ObservableProperty]
        public partial bool NickNameRequired { get; set; }

        [ObservableProperty]
        public partial string CurrentBudgetName { get; set; }

        [ObservableProperty]
        public partial List<Budgets> UserBudgets { get; set; }

        [ObservableProperty]
        public partial string CurrentSubStatus { get; set; }

        [ObservableProperty]
        public partial string SubscriptionRenewal { get; set; }

        [ObservableProperty]
        public partial string VersionNumber { get; set; }

        [ObservableProperty]
        public partial BorderlessPicker SwitchBudgetPicker { get; set; }

        [ObservableProperty]
        public partial Budgets SelectedBudget { get; set; }

        [ObservableProperty]
        public partial bool IsDPA { get; set; }

        [ObservableProperty]
        public partial bool IsPushNotificationsEnabled { get; set; }

        [ObservableProperty]
        public partial bool IsChanging { get; set; }

        [ObservableProperty]
        public partial string IsPushNotificationsEnabledLabelText { get; set; }

        [ObservableProperty]
        public partial bool IsBudgetHidden { get; set; }



        public EditAccountSettingsViewModel(IProductTools pt, IRestDataService ds, INotificationPermissions notificationPermissions, IModalPopupService ps)
        {
            _pt = pt;
            _ds = ds;
            _ps = ps;
            _notificationPermissions = notificationPermissions;
        }

        public async Task OnLoad()
        {
            Title = "Edit your account's settings";
            IsChanging = true;
            VersionNumber = $"V{AppInfo.Current.VersionString}";
            if(App.IsFamilyAccount)
            {
                FamilyUser = await _ds.GetFamilyUserDetailsAsync(App.FamilyUserDetails.Email);
                IsBudgetHidden = FamilyUser.IsBudgetHidden;
            }
            else
            {
                User = await _ds.GetUserDetailsAsync(App.UserDetails.Email);
                CurrentSubStatus = $"{User.SubscriptionType} expires on {User.SubscriptionExpiry.ToString("d", CultureInfo.CurrentCulture)}";
                SubscriptionRenewal = "Monthly";

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
            }


            IsDPA = App.IsFamilyAccount ? FamilyUser.IsDPAPermissions : User.IsDPAPermissions;
            IsPushNotificationsEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
            if(IsPushNotificationsEnabled)
            {
                IsPushNotificationsEnabledLabelText = "Push Notifications Enabled";
            }
            else
            {
                IsPushNotificationsEnabledLabelText = "Push Notifications Disabled";
            }
            IsChanging = false;
        }

        [RelayCommand]
        private async Task CloseSettings(object obj)
        {
            try
            {
                await Shell.Current.GoToAsync($"..");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "CloseSettings");
            }
        }

        [RelayCommand]
        private async Task DeleteUserAccount()
        {
            try
            {
                var Email = await Application.Current.Windows[0].Page.DisplayPromptAsync($"Are you sure you want to delete your account?", $"Enter the accounts email address to delete the account", "Ok", "Cancel");
                if (Email != null)
                {
                    if(string.Equals(Email, (App.IsFamilyAccount ? App.FamilyUserDetails.Email : App.UserDetails.Email), StringComparison.OrdinalIgnoreCase))
                    {
                        string result = App.IsFamilyAccount ? await _ds.DeleteFamilyUserAccount(App.FamilyUserDetails.UserID) : await _ds.DeleteUserAccount(App.UserDetails.UserID);

                        if (result == "OK")
                        {
                            AppShell Shell = new AppShell();
                            await Shell.Logout();

                            await Application.Current.Windows[0].Page.DisplayAlert($"Account Deleted", $"Your account has been permanently deleted and there is no way to recover it now. If you want to start budgeting again create a new account", "Ok");

                        }
                        else
                        {
                            await Application.Current.Windows[0].Page.DisplayAlert($"Opps something went wrong", $"There was an issue deleting your account, please try again and if the issue persists please contact us so can help", "Ok");

                        }
                    }
                    else
                    {
                        await Application.Current.Windows[0].Page.DisplayAlert("That is not the correct email", "Your account has not been deleted please try again." ,"OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "DeleteUserAccount");
            }
        }

        [RelayCommand]
        private async Task UpdatePassword()
        {
            try
            {
                await Task.Delay(5);

                if (NewPasswordRequired & PasswordRequired & PasswordConfirmRequired & NewPasswordValid)
                {
                    bool UpdatePassword = await Application.Current.Windows[0].Page.DisplayAlert($"Are you sure you want to update your password?", $"Forgot your current password and you can reset it from the logon screen using your email", "Yes", "No");
                    if (UpdatePassword)
                    {
                        await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());
                        string salt = App.IsFamilyAccount ? await _ds.GetFamilyUserSaltAsync(App.FamilyUserDetails.Email) : await _ds.GetUserSaltAsync(App.UserDetails.Email);
                        string Password = "";
                        if (App.IsFamilyAccount)
                        {
                            UserDetailsModel userDetails = await _ds.GetUserDetailsAsync(App.UserDetails.Email);
                            Password = userDetails.Password;
                        }
                        else
                        {
                            FamilyUserAccount familyUserAccount = await _ds.GetFamilyUserDetailsAsync(App.FamilyUserDetails.Email);
                            Password = familyUserAccount.Password;
                        }

                        string HashPassword = _pt.GenerateHashedPassword(CurrentPassword, salt);

                        if (Password != HashPassword)
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

                                    PatchDoc PasswordPatch = new PatchDoc
                                    {
                                        op = "replace",
                                        path = "/Password",
                                        value = NewPasswordUser.Password
                                    };

                                    UserUpdate.Add(Salt);
                                    UserUpdate.Add(PasswordPatch);

                                    string result = App.IsFamilyAccount ? await _ds.PatchFamilyUserAccount(App.FamilyUserDetails.UserID, UserUpdate) :await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);
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

                await _ps.CloseAsync<PopUpPage>();
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "UpdatePassword");
            }
        }

        [RelayCommand]
        private async Task UpdateNickname()
        {
            try
            {
                await Task.Delay(15);

                if (NickNameRequired)
                {
                    bool UpdateNickName = await Application.Current.Windows[0].Page.DisplayAlert($"Are you sure you want to update your nickname?", $"Are you sure you want to update your nickaname to {NewNickName}?", "Yes", "No");
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

                        string result = App.IsFamilyAccount ? await _ds.PatchFamilyUserAccount(App.FamilyUserDetails.UserID, UserUpdate) : await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

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
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "UpdateNickname");
            }
        }


        [RelayCommand]
        private async Task UpdateEmail()
        {
            try
            {
                await Task.Delay(5);

                if (EmailRequired & EmailValid)
                {
                    bool UpdateEmail = await Application.Current.Windows[0].Page.DisplayAlert($"Are you sure you want to update your email?", $"Are you sure you want to update your email to {NewEmail}? Make sure you have access to this email or you might have some issues!", "Yes", "No");
                    if (UpdateEmail)
                    {
                        bool IsEmailExists = false;
                        if(App.IsFamilyAccount)
                        {
                            UserDetailsModel UserDetails = await _ds.GetUserDetailsAsync(NewEmail);
                            IsEmailExists = UserDetails.Error == null;
                        }
                        else
                        {
                            FamilyUserAccount familyUserAccount = await _ds.GetFamilyUserDetailsAsync(NewEmail);
                            IsEmailExists = familyUserAccount != null;
                        }

                        if (!IsEmailExists)
                        {
                            List<PatchDoc> UserUpdate = new List<PatchDoc>();

                            PatchDoc EmailPatch = new PatchDoc
                            {
                                op = "replace",
                                path = "/Email",
                                value = NewEmail
                            };

                            UserUpdate.Add(EmailPatch);

                            string result = App.IsFamilyAccount ? await _ds.PatchFamilyUserAccount(App.FamilyUserDetails.UserID, UserUpdate) : await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

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
                            await Application.Current.Windows[0].Page.DisplayAlert("Opps", "An account already exists with this email. Please use a different email or recover the account if this is your email", "OK");
                        }


                    }
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "UpdateEmail");
            }
        }
            

        [RelayCommand]
        private async Task ChangeSelectedProfilePic()
        {
            try
            {
                EditProfilePictureBottomSheet page = new EditProfilePictureBottomSheet( IPlatformApplication.Current.Services.GetService<IProductTools>(), IPlatformApplication.Current.Services.GetService<IRestDataService>());

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
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "ChangeSelectedProfilePic");
            }
        }

        public async Task<Stream> GetUserProfilePictureStream(int UserID)
        {
            return await _ds.DownloadUserProfilePicture(UserID);
        }

        [RelayCommand]
        private async Task CreateNewBudget()
        {
            try
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
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "CreateNewBudget");
            }
        }

        async partial void OnIsDPAChanged(bool oldValue, bool newValue)
        {
            try
            {
                if(!IsChanging)
                {
                    IsChanging = true;
                    List<PatchDoc> UserUpdate = new List<PatchDoc>();

                    PatchDoc IsDPAPermissions = new PatchDoc
                    {
                        op = "replace",
                        path = "/isDPAPermissions",
                        value = IsDPA
                    };

                    UserUpdate.Add(IsDPAPermissions);

                    string result = App.IsFamilyAccount ? await _ds.PatchFamilyUserAccount(App.FamilyUserDetails.UserID, UserUpdate) : await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);
                    IsChanging = false;
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "OnIsDPAChanged");
            }
        }

        async partial void OnIsBudgetHiddenChanged(bool oldValue, bool newValue)
        {
            try
            {
                if(!IsChanging)
                {
                    IsChanging = true;
                    List<PatchDoc> UserUpdate = new List<PatchDoc>();

                    PatchDoc IsBudgetHiddenValue = new PatchDoc
                    {
                        op = "replace",
                        path = "/IsBudgetHidden",
                        value = IsBudgetHidden
                    };

                    UserUpdate.Add(IsBudgetHiddenValue);

                    string result = App.IsFamilyAccount ? await _ds.PatchFamilyUserAccount(App.FamilyUserDetails.UserID, UserUpdate) : await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);
                    IsChanging = false;
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "OnIsDPAChanged");
            }
        }

        async partial void OnIsPushNotificationsEnabledChanged(bool oldValue, bool newValue)
        {
            try
            {
                if(!IsChanging)
                {
                    IsChanging = true;

                    await _notificationPermissions.OpenNotificationSettingsAsync();

                    if (IsPushNotificationsEnabled)
                    {
                        IsPushNotificationsEnabledLabelText = "Push Notifications Enabled";
                    }
                    else
                    {

                        IsPushNotificationsEnabledLabelText = "Push Notifications Disabled";
                    }
                    IsChanging = false;
                }
                

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountSettings", "OnIsPushNotificationsEnabledChanged");
            }
        }


        [RelayCommand]
        public async Task BackButton()
        {
            try
            {
                await Shell.Current.GoToAsync($"//{(App.IsFamilyAccount ? nameof(FamilyAccountMainPage) : nameof(MainPage))}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "BackButton");
            }
        }

    }
}
