using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        public partial UserDetailsModel User { get; set; }

        [ObservableProperty]
        public partial string CurrentSubStatus { get; set; }

        [ObservableProperty]
        public partial string SubscriptionStatus { get; set; }

        [ObservableProperty]
        public partial string VersionNumber { get; set; }

        [ObservableProperty]
        public partial FamilyUserAccount FamilyUser { get; set; }


        public EditAccountDetailsViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
        }      
        
        public async Task OnLoad()
        {
            Title = "dBudget application details";
            VersionNumber = $"V{AppInfo.Current.VersionString}";
            if (App.IsFamilyAccount)
            {
                FamilyUser = await _ds.GetFamilyUserDetailsAsync(App.FamilyUserDetails.Email);
            }
            else
            {
                User = await _ds.GetUserDetailsAsync(App.UserDetails.Email);

                if (DateTime.Now > User.SubscriptionExpiry)
                {
                    SubscriptionStatus = "";
                }
                else
                {
                    SubscriptionStatus = "Monthly";
                }

                if (SubscriptionStatus == "Cancelled" || SubscriptionStatus == "")
                {
                    if (DateTime.Now > User.SubscriptionExpiry)
                    {
                        CurrentSubStatus = $"Click here to check out our subscription options";
                    }
                    else
                    {
                        CurrentSubStatus = $"Sub {SubscriptionStatus}. {User.SubscriptionType} benefits lost on {User.SubscriptionExpiry.ToString("dd MMM yy", CultureInfo.CurrentCulture)}";

                    }
                }
                else
                {
                    CurrentSubStatus = $"Renews {SubscriptionStatus} on {User.SubscriptionExpiry.ToString("dd MMM yy", CultureInfo.CurrentCulture)}";
                }
            }
                      

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
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
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
                await _pt.HandleException(ex, "EditAccountDetails", "ChangeSelectedProfilePic");
            }

        }

        public async Task<Stream> GetUserProfilePictureStream(int UserID)
        {
            return await _ds.DownloadUserProfilePicture(UserID);
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

                await Shell.Current.GoToAsync($"//{(App.IsFamilyAccount ? nameof(FamilyAccountMainPage) : nameof(MainPage))}");

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "BackButton");
            }
        }

    }
}
