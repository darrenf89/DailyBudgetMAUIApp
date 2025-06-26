using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewBudgetsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        private readonly IModalPopupService _ps;


        [ObservableProperty]
        public partial double SignOutButtonWidth { get; set; }

        [ObservableProperty]
        public partial List<Budgets> Budgets { get; set; }

        [ObservableProperty]
        public partial UserDetailsModel User { get; set; }

        [ObservableProperty]
        public partial FamilyUserAccount FamilyUser { get; set; }

        [ObservableProperty]
        public partial string Email { get; set; }

        [ObservableProperty]
        public partial string NickName { get; set; }


        public ViewBudgetsViewModel(IProductTools pt, IRestDataService ds, IModalPopupService ps)
        {
            _pt = pt;
            _ds = ds;
            _ps = ps;

            Title = "Your Budgets";

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            SignOutButtonWidth = ScreenWidth - 60;
        }

        public async Task LoadBudgets()
        {

            if (App.IsFamilyAccount)
            {
                FamilyUser = await _ds.GetFamilyUserDetailsAsync(App.FamilyUserDetails.Email);
                Email = FamilyUser.Email;
                NickName = FamilyUser.NickName;
                var Budget = await _ds.GetBudgetDetailsAsync(App.FamilyUserDetails.BudgetID, "Full");
                Budgets = [Budget];
            }
            else
            {
                Budgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "ViewBudgets");
                User = await _ds.GetUserDetailsAsync(App.UserDetails.Email);
                Email = User.Email;
                NickName = User.NickName;
            }
        }

        [RelayCommand]
        public async Task AddNewBudget()
        {
            try
            {
                var result = await Shell.Current.DisplayPromptAsync("Create a new budget?", "Before you start creating a new budget give it a name and then let's get going!", "Ok", "Cancel");
                if (result != null)
                {

                    await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());
                    Budgets NewBudget = new Budgets();

                    if (!string.IsNullOrEmpty(result))
                    {
                        NewBudget = await _ds.CreateNewBudget(App.UserDetails.Email, "PremiumPlus");

                        List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                        PatchDoc BudgetName = new PatchDoc
                        {
                            op = "replace",
                            path = "/BudgetName",
                            value = result
                        };

                        BudgetUpdate.Add(BudgetName);

                        await _ds.PatchBudget(NewBudget.BudgetID, BudgetUpdate);
                        NewBudget.BudgetName = result;

                    }
                    await _pt.ChangeDefaultBudget(App.UserDetails.UserID, NewBudget.BudgetID, false);
                    App.DefaultBudgetID = NewBudget.BudgetID;
                    App.DefaultBudget = NewBudget;
                    App.HasVisitedCreatePage = true;

                    if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                    {
                        Preferences.Remove(nameof(App.DefaultBudgetID));
                    }
                    Preferences.Set(nameof(App.DefaultBudgetID), NewBudget.BudgetID);

                    await _ps.CloseAsync<PopUpPage>();
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(DailyBudgetMAUIApp.Pages.CreateNewBudget)}?BudgetID={NewBudget.BudgetID}&NavigatedFrom=View Budgets");
                }
            }

            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "AddNewBudget");
            }
        }


        [RelayCommand]
        private async Task CompleteBudgetSetUp(object obj)
        {            
            try
            {
                if (obj is not Budgets Budget)
                {
                    return;
                }

                await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={Budget.BudgetID}&NavigatedFrom=View Budgets");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "CompleteBudgetSetUp");
            }
        }


        [RelayCommand]
        private async Task SelectedBudget(object obj)
        {            
            try
            {
                if (obj is not Budgets Budget)
                {
                    return;
                }

                if (App.IsFamilyAccount)
                {
                    await Shell.Current.GoToAsync($"//{nameof(FamilyAccountMainPage)}");
                }
                else
                {
                    if (App.DefaultBudgetID == Budget.BudgetID)
                    {
                        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                    }
                    else
                    {
                        await _pt.ChangeDefaultBudget(App.UserDetails.UserID, Budget.BudgetID, true);
                    }

                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "SelectedBudget");
            }
        }


        [RelayCommand]
        private async Task ShareBudget(object obj)
        {            
            try
            {
                if (obj is not Budgets Budget)
                {
                    return;
                }

                if (App.IsFamilyAccount)
                {
                    await Shell.Current.DisplayAlert("Sorry You Can't Share Budget", "Sorry you can't share your budget because its a family account. Upgrade to your own account to get access to all details", "Ok");
                }
                else
                {
                    ShareBudgetRequest SBR = new ShareBudgetRequest
                    {
                        SharedBudgetID = Budget.BudgetID,
                        IsVerified = false,
                        SharedByUserEmail = App.UserDetails.Email,
                        RequestInitiated = DateTime.UtcNow
                    };

                    ShareBudget page = new ShareBudget(SBR, _ds, _pt);

                    page.Detents = new DetentsCollection()
                    {
                        new FixedContentDetent(),
                        new FullscreenDetent()

                    };

                    page.HasBackdrop = true;
                    page.CornerRadius = 30;

                    App.CurrentBottomSheet = page;
                    await page.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "ShareBudget");
            }
        }

        [RelayCommand]
        private async Task ViewShareBudget(object obj)
        {            
            try
            {

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "ViewShareBudget");
            }
        }

        [RelayCommand]
        private async Task DeleteBudget(object obj)
        {            
            try
            {
                if(App.IsFamilyAccount)
                {
                    await Shell.Current.DisplayAlert("Sorry You Can't Delete Budget", "You cannot delete a budget from a family account, please delete your account or contact support for assistance.", "Ok");
                } 
                else 
                {
                    var BudgetName = await Application.Current.Windows[0].Page.DisplayPromptAsync($"Are you sure you want to delete {App.DefaultBudget.BudgetName} budget?", $"Deleting the budget is permanent, enter the budget name to delete the budget", "Ok", "Cancel");
                    if (BudgetName != null)
                    {
                        if (string.Equals(BudgetName, App.DefaultBudget.BudgetName, StringComparison.OrdinalIgnoreCase))
                        {
                            await Task.Delay(100);

                            string result = await _ds.DeleteBudget(App.DefaultBudgetID, App.UserDetails.UserID);
                            if (result == "LastBudget")
                            {
                                await Application.Current.Windows[0].Page.DisplayAlert($"You can't delete this!", $"You can't delete this budget as it is your last budget and you must have at least one budget on the app", "Ok");

                            }
                            else if (result == "SharedBudget")
                            {
                                await Application.Current.Windows[0].Page.DisplayAlert($"This is a shared budget!", $"You can't delete a budget that you didn't create, someone kindly shared it with you so don't try and delete it", "Ok");

                            }
                            else
                            {
                                List<Budgets> Budgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "EditBudgetSettings");

                                App.DefaultBudgetID = Budgets[0].BudgetID;
                                App.DefaultBudget = Budgets[0];
                                BudgetSettingValues Settings = await _ds.GetBudgetSettingsValues(App.DefaultBudgetID);
                                App.CurrentSettings = Settings;

                                if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                                {
                                    Preferences.Remove(nameof(App.DefaultBudgetID));
                                }
                                Preferences.Set(nameof(App.DefaultBudgetID), Budgets[0].BudgetID);

                                List<PatchDoc> UserUpdate = new List<PatchDoc>();

                                PatchDoc DefaultBudgetID = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/DefaultBudgetID",
                                    value = App.DefaultBudgetID
                                };

                                UserUpdate.Add(DefaultBudgetID);

                                PatchDoc PreviousDefaultBudgetID = new PatchDoc
                                {
                                    op = "replace",
                                    path = "/PreviousDefaultBudgetID",
                                    value = 0
                                };

                                UserUpdate.Add(PreviousDefaultBudgetID);
                                await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

                                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                            }

                        }
                        else
                        {
                            await Application.Current.Windows[0].Page.DisplayAlert($"Incorrect Budget Name", "Please check the budget name that was entered and confirm it is correct!", "Ok, thanks");
                        }

                    }
                }
                
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewBudgets", "DeleteBudget");
            }
        }

        [RelayCommand]
        private async Task ChangeSelectedProfilePic()
        {
            try
            {
                EditProfilePictureBottomSheet page = new EditProfilePictureBottomSheet(IPlatformApplication.Current.Services.GetService<IProductTools>(), IPlatformApplication.Current.Services.GetService<IRestDataService>());

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
                await _pt.HandleException(ex, "ViewBudgets", "ChangeSelectedProfilePic");
            }
        }

        public async Task<Stream> GetUserProfilePictureStream(int UserID)
        {
            return await _ds.DownloadUserProfilePicture(UserID);
        }
    }
}
