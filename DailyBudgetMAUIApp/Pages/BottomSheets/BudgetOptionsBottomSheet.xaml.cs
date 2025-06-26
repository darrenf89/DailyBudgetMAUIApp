using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Handlers;
using The49.Maui.BottomSheet;
using DailyBudgetMAUIApp.ViewModels;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class BudgetOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;
    private readonly IModalPopupService _ps;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public Budgets Budget { get; set; }
    public Picker SwitchBudgetPicker { get; set; }

    public BudgetOptionsBottomSheet(Budgets InputBudget, IProductTools pt, IRestDataService ds, IModalPopupService ps)
    {
        InitializeComponent();

        BindingContext = this;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        var ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;

        btnDismiss.WidthRequest = ButtonWidth;

        Budget = InputBudget;

        lblBudgetName.Text = $"{Budget.BudgetName} Budget Details";
        MainScrollView.MaximumHeightRequest = ScreenHeight - 280;
        _pt = pt;
        _ds = ds;
        _ps = ps;

        if (App.IsFamilyAccount)
        {
            EditPayInfobrd.IsVisible = false;
            BudgetEditFillTwo.IsVisible = true;
            BudgetEditFillOne.IsVisible = true;
            CreateNewBudgetbrd.IsVisible = false;
            SwitchBudgetbrd.IsVisible = false;
        }

        if (!App.IsPremiumAccount || App.IsFamilyAccount)
        {
            ViewShareBudget.IsVisible = false;
            NewShareBudget.IsVisible = false;
            CalendarEvent.IsVisible = false;
            CalendarEventReplace.IsVisible = true;
        }
        else if (Budget.IsSharedValidated && Budget.SharedUserID != 0)
        {
            ViewShareBudget.IsVisible = true;
            NewShareBudget.IsVisible = false;
        }
        else
        {
            ViewShareBudget.IsVisible = false;
            NewShareBudget.IsVisible = true;
        }

        if(App.UserDetails != null && App.UserDetails.SubscriptionType == "Basic")
        {
            VSLUpgradeSubscription.IsVisible = true;
            VSLViewSubscription.IsVisible = false;
        }
        else if(!App.IsFamilyAccount)
        {
            VSLUpgradeSubscription.IsVisible = false;
            VSLViewSubscription.IsVisible = true;
        }
        else
        {
            VSLUpgradeSubscription.IsVisible = false;
            VSLViewSubscription.IsVisible = false;
        }

        if (App.IsPremiumAccount)
        {
            if (App.DefaultBudget.IsMultipleAccounts)
            {
                viewMultipleAccounts.IsVisible = true;
                viewSyncBankBalance.IsVisible = false;

                vslMultipleAccounts.IsVisible = false;
                NotMultipleAccounts.IsVisible = true;
            }
            else
            {
                viewMultipleAccounts.IsVisible = false;
                viewSyncBankBalance.IsVisible = true;

                vslMultipleAccounts.IsVisible = true;
                NotMultipleAccounts.IsVisible = false;
            }

        }
        else
        {
            viewMultipleAccounts.IsVisible = false;
            viewSyncBankBalance.IsVisible = true;

            vslMultipleAccounts.IsVisible = false;
            NotMultipleAccounts.IsVisible = false;
        }

    }

    private void btnDismiss_Clicked(object sender, EventArgs e)
    {
        this.DismissAsync();
    }

    private async void TransactPayDay_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await _pt.PayPayDayNow(App.DefaultBudget);

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "ViewTransactions_Tapped");
        }
    }

    private async void ViewTransactions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewTransactions)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "ViewTransactions_Tapped");
        }
    }

    private async void ViewBills_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewBills)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "ViewBills_Tapped");
        }
    }

    private async void ViewSavings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewSavings)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "ViewSavings_Tapped");
        }
    }

    private async void ViewIncomes_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewIncomes)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "ViewIncomes_Tapped");
        }
    }

    private async void EditPayInfo_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopupEditNextPayInfoViewModel.Budget)] = App.DefaultBudget
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            IPopupResult<object> popupResult = await _ps.PopupService.ShowPopupAsync<PopupEditNextPayInfo, object>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes,
                cancellationToken: CancellationToken.None
            );

            if (popupResult.Result is Budgets result)
            {
                App.DefaultBudget = result;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "EditPayInfo_Tapped");
        }
    }

    private async void EditBudgetSettings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            await Shell.Current.GoToAsync($"{nameof(EditBudgetSettings)}");

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "EditBudgetSettings_Tapped");
        }
    }

    private async void SyncBankBalance_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopupSyncBankBalanceViewModel.Budget)] = App.DefaultBudget
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            IPopupResult<object> popupResult = await _ps.PopupService.ShowPopupAsync<PopupSyncBankBalance, object>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes,
                cancellationToken: CancellationToken.None
            );

            if (popupResult.Result is Budgets result)
            {
                App.DefaultBudget = result;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "SyncBankBalance_Tapped");
        }

    }

    private async void UserSettings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            await Shell.Current.GoToAsync($"{nameof(EditAccountSettings)}");

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "UserSettings_Tapped");
        }
    }

    private async void CreateNewBudget_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var result = await Shell.Current.DisplayPromptAsync("Create a new budget?", "Before you start creating a new budget give it a name and then let's get going!","Ok","Cancel");
            if (result != null)
            {
                if (App.CurrentBottomSheet != null)
                {
                    await this.DismissAsync();
                    App.CurrentBottomSheet = null;
                }

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


                await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(DailyBudgetMAUIApp.Pages.CreateNewBudget)}?BudgetID={NewBudget.BudgetID}&NavigatedFrom=Budget Settings");

            }
            else
            {
                await Application.Current.Windows[0].Page.DisplayAlert($"Please enter a budget name", "You need to enter a budget name to set up a new budget", "Ok, thanks");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "CreateNewBudget_Tapped");
        }
    }

    private void EditShareBudget_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void ShareBudget_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            ShareBudgetRequest SBR = new ShareBudgetRequest
            {
                SharedBudgetID = App.DefaultBudgetID,
                IsVerified = false,
                SharedByUserEmail = App.UserDetails.Email,
                RequestInitiated = DateTime.UtcNow
            };

            ShareBudget page = new ShareBudget(SBR, _ds, _pt);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent
                {
                    IsDefault = true                
                },
                new FullscreenDetent()

            };

            page.HasBackdrop = true;
            page.CornerRadius = 30;

            App.CurrentBottomSheet = page;
            page.ShowAsync();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "ShareBudget_Tapped");
        }
    }

    private async void SwitchBudget_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            SwitchBudgetPicker = await _pt.SwitchBudget("Budget Options");
            SwitchBudgetPicker.HeightRequest = 0.1;
            MainVSL.Children.Add(SwitchBudgetPicker);
            SwitchBudgetPicker.Focus();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "SwitchBudget_Tapped");
        }
    }

    private async void ViewEnvelopes_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewEnvelopes)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "ViewEnvelopes_Tapped");
        }
    }

    private void UpgradeSubscription_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void ViewSubscription_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            // Intent to open the Google Play app's subscription page directly
            var playStoreUri = "market://details?id=com.android.vending&url=https://play.google.com/store/account/subscriptions";

            if (await Launcher.CanOpenAsync(playStoreUri))
            {
                await Launcher.OpenAsync(playStoreUri);
            }
            else
            {
                var subscriptionUrl = "https://play.google.com/store/account/subscriptions";

                if (await Launcher.CanOpenAsync(subscriptionUrl))
                {
                    await Launcher.OpenAsync(subscriptionUrl);
                }
                else
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Error", "Unable to open the subscription page.", "OK");
                }
            }

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "ViewSubscription_Tapped");
        }
    }

    private async void ViewCalendar_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewCalendar)}");
    }

    private async void MoveBalance_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopupMoveBalanceViewModel.Budget)] = App.DefaultBudget,
                [nameof(PopupMoveBalanceViewModel.Type)] = "Budget",
                [nameof(PopupMoveBalanceViewModel.Id)] = 0,
                [nameof(PopupMoveBalanceViewModel.IsCoverOverSpend)] = false
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            IPopupResult<string> popupResult = await _ps.PopupService.ShowPopupAsync<PopupMoveBalance, string>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes,
                cancellationToken: CancellationToken.None
            );

            await Task.Delay(1);
            if (popupResult.Result.ToString() == "OK")
            {
                var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
                App.DefaultBudget = Budget;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "MoveBalance_Tapped");
        }
    }

    private async void MultipleAccounts_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

            PatchDoc IsMultipleAccountsPatch = new PatchDoc
            {
                op = "replace",
                path = "/IsMultipleAccounts",
                value = true
            };

            BudgetUpdate.Add(IsMultipleAccountsPatch);
            await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);

            BankAccounts Account = new BankAccounts
            {
                BankAccountName = "My Default Account",
                AccountBankBalance = App.DefaultBudget.BankBalance,
                IsDefaultAccount = true

            };

            await _ds.AddBankAccounts(App.DefaultBudgetID, Account);

            MultipleAccountsBottomSheet page = new MultipleAccountsBottomSheet(_pt, _ds, _ps);

            page.Detents = new DetentsCollection()
            {
                new ContentDetent(),
                new FullscreenDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
            var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
            App.DefaultBudget = Budget;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "MultipleAccounts_Tapped");
        }


    }

    private async void viewMultipleAccountsButton_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(ViewAccounts)}");

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "viewMultipleAccountsButton_Tapped");
        }
    }

    private async void DeactivateMultipleAccounts_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var result = await Shell.Current.DisplayAlert("Careful, before you proceed!", "Are you sure you want to delete your accounts and budget with only a single balance to manage?", "Yes", "No");
            if (result)
            {

                await _ds.DeleteBankAccounts(App.DefaultBudgetID);

                List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                PatchDoc IsMultipleAccountsPatch = new PatchDoc
                {
                    op = "replace",
                    path = "/IsMultipleAccounts",
                    value = false
                };

                BudgetUpdate.Add(IsMultipleAccountsPatch);
                await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);
                var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
                App.DefaultBudget = Budget;

                if (App.CurrentBottomSheet != null)
                {
                    await App.CurrentBottomSheet.DismissAsync();
                    App.CurrentBottomSheet = null;
                }
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "BudgetOptionsBottomSheet", "DeactivateMultipleAccounts_Tapped");
        }
    }
}