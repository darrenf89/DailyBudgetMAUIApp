using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.ViewModels;
using Microsoft.Maui.Layouts;
using Syncfusion.Maui.Core;


namespace DailyBudgetMAUIApp.Pages;

public partial class EditAccountDetails : ContentPage
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

    private readonly EditAccountDetailsViewModel _vm;
    private readonly IProductTools _pt;

    public EditAccountDetails(EditAccountDetailsViewModel viewModel, IProductTools pt)
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

            if (_vm.User.ProfilePicture.Contains("Avatar"))
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
                ProfilePicStream = await _vm.GetUserProfilePictureStream(App.UserDetails.UserID);
            }

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "OnAppearing");
        }
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
    }

    private void ViewSubDetails_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }    
    
    private void TOS_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }    

    private void PrivacyPolicy_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }   
    
    private void Licences_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }    

    private void Rate_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }    
    
    private void Share_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }    
    
    private void Version_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }    
    
    private void Logout_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }    
    
    private void ContactUs_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }    
    
    private void Help_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {

        }
    }
}