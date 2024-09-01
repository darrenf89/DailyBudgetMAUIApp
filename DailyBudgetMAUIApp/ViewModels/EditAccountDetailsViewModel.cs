using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Converters;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using System.Globalization;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class EditAccountDetailsViewModel : BaseViewModel
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
        private string subscriptionRenewal;
        [ObservableProperty]
        private string versionNumber;
        [ObservableProperty]
        public BorderlessPicker switchBudgetPicker;
        [ObservableProperty]
        public Budgets selectedBudget;
        [ObservableProperty]
        public bool isDPA;

        public EditAccountDetailsViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
        }      
        
        public async Task OnLoad()
        {

            VersionNumber = $"V{AppInfo.Current.VersionString}";
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

            IsDPA = User.IsDPAPermissions;
        }

        [RelayCommand]
        private async Task CloseSettings(object obj)
        {
            try
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
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountDetails", "CloseSettings");
            }
        }               

        [RelayCommand]
        private async Task ChangeSelectedProfilePic()
        {
            try
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
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountDetails", "ChangeSelectedProfilePic");
            }

        }

        public async Task<Stream> GetUserProfilePictureStream(int UserID)
        {
            return await _ds.DownloadUserProfilePicture(UserID);
        }

    }
}
