using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;
using IeuanWalker.Maui.Switch;
using IeuanWalker.Maui.Switch.Events;
using IeuanWalker.Maui.Switch.Helpers;
using Microsoft.Maui.Layouts;
using Syncfusion.Maui.Core;


namespace DailyBudgetMAUIApp.Pages;

public partial class EditAccountSettings : BasePage
{
    public string _updatedAvatar = "";
    public string UpdatedAvatar
    {
        get => _updatedAvatar;
        set
        {
            if (_updatedAvatar != value)
            {
                _updatedAvatar = value;
                bool Success = Enum.TryParse(value, out AvatarCharacter Avatar);
                if (Success)
                {
                    ProfilePicture.ContentType = ContentType.AvatarCharacter;
                    ProfilePicture.AvatarCharacter = Avatar;
                    int Number = Convert.ToInt32(value[value.Length - 1]);
                    Math.DivRem(Number, 8, out int index);
                    ProfilePicture.Background = App.ChartColor[index];
                }
                else
                {
                    ProfilePicture.AvatarCharacter = AvatarCharacter.Avatar1;
                    ProfilePicture.Background = App.ChartColor[1];
                }
            }
        }
    }

    public Stream _profilePicStream;
    public Stream ProfilePicStream
    {
        get => _profilePicStream;
        set
        {
            if (_profilePicStream != value)
            {
                _profilePicStream = value;
                ProfilePicture.ContentType = ContentType.Custom;
                ProfilePicture.ImageSource = ImageSource.FromStream(() => ProfilePicStream);
            }
        }
    }

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
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 60;
        ButtonWidth = ScreenWidth - 40;
    }

    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            TopBV.WidthRequest = ScreenWidth;
            MainAbs.WidthRequest = ScreenWidth;
            MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
            MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));

            await _vm.OnLoad();

            if(_vm.User.ProfilePicture.Contains("Avatar"))
            {
                ProfilePicture.ContentType = ContentType.AvatarCharacter;
                bool Success = Enum.TryParse(_vm.User.ProfilePicture, out AvatarCharacter Avatar);
                if(Success)
                {
                    ProfilePicture.AvatarCharacter = Avatar;
                    int Number = Convert.ToInt32(_vm.User.ProfilePicture[_vm.User.ProfilePicture.Length - 1]);
                    Math.DivRem(Number, 8, out int index);
                    ProfilePicture.Background = App.ChartColor[index];
                }
                else
                {
                     ProfilePicture.AvatarCharacter = AvatarCharacter.Avatar1;
                     ProfilePicture.Background = App.ChartColor[1];
                }
            }
            else
            {
                ProfilePicStream = await _vm.GetUserProfilePictureStream(App.UserDetails.UserID);
            }

            vslPckrSwitchBudget.Content = _vm.SwitchBudgetPicker;

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

    private void NewPassword_Focused(object sender, FocusEventArgs e)
    {
        _vm.CurrentPasswordValid = true;
        
    }

    private void PasswordConfirm_Focused(object sender, FocusEventArgs e)
    {
        _vm.NewPasswordMatch = true;
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

    static void CustomSwitch_SwitchPanUpdate(CustomSwitch customSwitch, SwitchPanUpdatedEventArgs e)
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