using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Handlers;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class BudgetOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public Budgets Budget { get; set; }
    public Picker SwitchBudgetPicker { get; set; }

    public BudgetOptionsBottomSheet(Budgets InputBudget, IProductTools pt, IRestDataService ds)
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

        if (Budget.IsSharedValidated && Budget.SharedUserID != 0)
        {
            ViewShareBudget.IsVisible = true;
            NewShareBudget.IsVisible = false;
        }
        else
        {
            ViewShareBudget.IsVisible = false;
            NewShareBudget.IsVisible = true;
        }

        if(App.UserDetails.SubscriptionType == "Basic")
        {
            VSLUpgradeSubscription.IsVisible = true;
            VSLViewSubscription.IsVisible = false;
        }
        else
        {
            VSLUpgradeSubscription.IsVisible = false;
            VSLViewSubscription.IsVisible = true;
        }

    }

    private void btnDismiss_Clicked(object sender, EventArgs e)
    {
        this.DismissAsync();
    }

    private async void ViewTransactions_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }
        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewTransactions)}");
    }

    private async void ViewBills_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewBills)}");
    }

    private async void ViewSavings_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }
        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewSavings)}");
    }

    private async void ViewIncomes_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }
        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewIncomes)}");
    }

    private void EditPayInfo_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void EditBudgetSettings_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        await Task.Delay(500);

        EditBudgetSettings page = new EditBudgetSettings(new EditBudgetSettingsViewModel(new ProductTools(new RestDataService()), new RestDataService()));

        await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
    }

    private void SyncBankBalance_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void UserSettings_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        await Task.Delay(500);

        EditAccountSettings page = new EditAccountSettings(new EditAccountSettingsViewModel(new ProductTools(new RestDataService()), new RestDataService()));

        await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
    }

    private async void CreateNewBudget_Tapped(object sender, TappedEventArgs e)
    {
        var result = await Shell.Current.DisplayPromptAsync("Create a new budget?", "Before you start creating a new budget give it a name and then let's get going!","Ok","Cancel");
        if (result != null)
        {
            try
            {
                if (App.CurrentBottomSheet != null)
                {
                    await this.DismissAsync();
                    App.CurrentBottomSheet = null;
                }
            }
            catch (Exception)
            {

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
    }

    private void EditShareBudget_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void ShareBudget_Tapped(object sender, TappedEventArgs e)
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

        ShareBudget page = new ShareBudget(SBR, new RestDataService());

        page.Detents = new DetentsCollection()
        {
            new FixedContentDetent(),
            new FullscreenDetent()

        };

        page.HasBackdrop = true;
        page.CornerRadius = 30;

        App.CurrentBottomSheet = page;
        page.ShowAsync();
    }

    private async void SwitchBudget_Tapped(object sender, TappedEventArgs e)
    {
        SwitchBudgetPicker = await _pt.SwitchBudget("Budget Options");
        SwitchBudgetPicker.HeightRequest = 0.1;
        MainVSL.Children.Add(SwitchBudgetPicker);
        SwitchBudgetPicker.Focus();
    }

    private async void ViewEnvelopes_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewEnvelopes)}");
    }

    private void UpgradeSubscription_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void ViewSubscription_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void ViewCalendar_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewCalendar)}");
    }
}