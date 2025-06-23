using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;
using IeuanWalker.Maui.Switch.Events;
using IeuanWalker.Maui.Switch.Helpers;
using Microsoft.Maui.Layouts;


namespace DailyBudgetMAUIApp.Pages;

public partial class EditAccountSettings : BasePage
{
    
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }

    private readonly EditAccountSettingsViewModel _vm;
    private readonly IProductTools _pt;

    public EditAccountSettings(EditAccountSettingsViewModel viewModel, IProductTools pt)
	{
		InitializeComponent();      

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 140;
        ButtonWidth = ScreenWidth - 40;
    }

    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            
            MainAbs.WidthRequest = ScreenWidth;
            MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
            MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));

            await _vm.OnLoad();            

            if (App.IsFamilyAccount)
            {
                vslPckrSwitchBudget.IsVisible = false;
                FamilyAccounts.IsVisible = false;
                BudgetVisibility.IsVisible = true;
            }
            else
            {
                vslPckrSwitchBudget.Content = _vm.SwitchBudgetPicker;
                BudgetVisibility.IsVisible = false;
            }

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountSettings", "OnAppearing");
        }
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
    }

    private void NewPassword_Focused(object sender, FocusEventArgs e)
    {
        _vm.CurrentPasswordValid = true;
        
    }

    private void PasswordConfirm_Focused(object sender, FocusEventArgs e)
    {
        _vm.NewPasswordMatch = true;
    }

    private async void ViewFamilyAccount_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.FamilyAccountsManage)}");

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountSettings", "ViewFamilyAccount_Clicked");
        }
    }
    private void UpdatePassword_Clicked(object sender, EventArgs e)
    {
        PasswordConfirmValidator.ForceValidate();
        PasswordRequiredValidator.ForceValidate();
        NewPasswordRequiredValidator.ForceValidate();
        if (_vm.NewPasswordRequired)
        {
            NewPasswordValidValidator.ForceValidate();
        }
        
    }

    private void UpdateEmail_Clicked(object sender, EventArgs e)
    {
        EmailRequiredValidator.ForceValidate();
        if (_vm.EmailRequired)
        {
            EmailValidValidator.ForceValidate();
        }
    }

    private void UpdateNickname_Clicked(object sender, EventArgs e)
    {
        NickNameRequiredValidator.ForceValidate();
    }

    static void CustomSwitch_SwitchPanUpdate(IeuanWalker.Maui.Switch.CustomSwitch customSwitch, SwitchPanUpdatedEventArgs e)
    {
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);

        //Switch Color Animation
        Color fromSwitchColor = e.IsToggled ? (Color)Primary : (Color)Gray400;
        Color toSwitchColor = e.IsToggled ? (Color)Gray400 : (Color)Primary;

        //BackGroundColor Animation
        Color fromColor = e.IsToggled ? (Color)Tertiary : (Color)PrimaryLight;
        Color toColor = e.IsToggled ? (Color)PrimaryLight : (Color)Tertiary;

        double t = e.Percentage * 0.01;

        customSwitch.KnobBackgroundColor = ColorAnimationUtil.ColorAnimation(fromSwitchColor, toSwitchColor, t);
        customSwitch.BackgroundColor = ColorAnimationUtil.ColorAnimation(fromColor, toColor, t);
    }

  
}