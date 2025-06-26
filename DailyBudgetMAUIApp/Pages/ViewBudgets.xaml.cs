using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Core;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewBudgets : BasePage
{
    private readonly ViewBudgetsViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IModalPopupService _ps;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }

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

    public ViewBudgets(ViewBudgetsViewModel vm, IProductTools pt, IModalPopupService ps)
    {
        InitializeComponent();
        _vm = vm;
        _pt = pt;
        _ps = ps;
        BindingContext = _vm;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 60;
        ButtonWidth = ScreenWidth - 40;
    }


    protected async override void OnAppearing()
    {        
        try
        {
            if (_ps.CurrentPopup is not null)
                return;

            await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());

            base.OnAppearing();
            await _vm.LoadBudgets();

            if (App.IsFamilyAccount && _vm.FamilyUser.ProfilePicture.Contains("Avatar"))
            {
                ProfilePicture.ContentType = ContentType.AvatarCharacter;
                bool Success = Enum.TryParse(_vm.FamilyUser.ProfilePicture, out AvatarCharacter Avatar);
                if (Success)
                {
                    ProfilePicture.AvatarCharacter = Avatar;
                    int Number = Convert.ToInt32(_vm.FamilyUser.ProfilePicture[_vm.FamilyUser.ProfilePicture.Length - 1]);
                    Math.DivRem(Number, 8, out int index);
                    ProfilePicture.Background = App.ChartColor[index];
                }
                else
                {
                    ProfilePicture.AvatarCharacter = AvatarCharacter.Avatar1;
                    ProfilePicture.Background = App.ChartColor[1];
                }
            }
            else if (!App.IsFamilyAccount && _vm.User.ProfilePicture.Contains("Avatar"))
            {
                ProfilePicture.ContentType = ContentType.AvatarCharacter;
                bool Success = Enum.TryParse(_vm.User.ProfilePicture, out AvatarCharacter Avatar);
                if (Success)
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
                ProfilePicStream = await _vm.GetUserProfilePictureStream((App.IsFamilyAccount ? App.FamilyUserDetails.UniqueUserID : App.UserDetails.UniqueUserID));
            }

            if (App.IsFamilyAccount)
            {
                AddNewBudget.IsVisible = false;
            }

            await _ps.CloseAsync<PopUpPage>();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "FamilyAccountsManage", "OnAppearing");
        }
    }


}