using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui.Layouts;
using Syncfusion.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class EditProfilePictureBottomSheet : BottomSheet
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }

    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public EditProfilePictureBottomSheet(IProductTools pt, IRestDataService ds)
	{
		InitializeComponent();

        BindingContext = this;
        _pt = pt;
        _ds = ds;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;

        MainScrollView.MaximumHeightRequest = ScreenHeight - App.NavBarHeight - App.StatusBarHeight - 80;

        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, AbsoluteLayout.AutoSize));
        MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));

        lblTitle.Text = $"Edit profile picture";

        this.PropertyChanged += ViewTransactionFilterBottomSheet_PropertyChanged;
        try
        {
            FillAvatarFlexLayout();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "EditProfilePictureBottomSheet", "EditProfilePictureBottomSheet");
        }

    }

    private void ViewTransactionFilterBottomSheet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            string PropertyChange = (string)e.PropertyName;
            if (PropertyChange == "SelectedDetent")
            {
                double Height = this.Height;

                BottomSheet Sender = (BottomSheet)sender;

                if (Sender.SelectedDetent is FullscreenDetent)
                {
                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.None);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, Height - 60, ScreenWidth, AbsoluteLayout.AutoSize));
                }
                else if (Sender.SelectedDetent is MediumDetent)
                {
                    MediumDetent detent = (MediumDetent)Sender.SelectedDetent;

                    double NewHeight = (Height * detent.Ratio) - 60;

                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.None);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, NewHeight, ScreenWidth, AbsoluteLayout.AutoSize));
                }
                else if (Sender.SelectedDetent is FixedContentDetent)
                {
                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));
                }

            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "EditProfilePictureBottomSheet", "ViewTransactionFilterBottomSheet_PropertyChanged");
        }
    }

    private void acrAvatar_Tapped(object sender, TappedEventArgs e)
    {
        if (!Avatar.IsVisible)
        {
            Avatar.IsVisible = true;
            AvatarIcon.Glyph = "\ue5cf";
        }
        else
        {
            Avatar.IsVisible = false;
            AvatarIcon.Glyph = "\ue5ce";
        }
    }

    private void FillAvatarFlexLayout()
    {
        Dictionary<string, string> FilteredIcons = new Dictionary<string, string>();

        flxAvatars.Children.Clear();

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        foreach (string Avatar in Enum.GetNames(typeof(AvatarCharacter)))
        {
            SfAvatarView AvatarView = new SfAvatarView
            {
                WidthRequest = 50,
                HeightRequest = 50,
                CornerRadius = 15,
                Stroke = (Color)White,
                StrokeThickness = 6,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                ContentType = ContentType.AvatarCharacter
            };

            bool Success = Enum.TryParse(Avatar, out AvatarCharacter AvatarOut);
            if (Success)
            {
                AvatarView.AvatarCharacter = AvatarOut;
                int Number = Convert.ToInt32(Avatar[Avatar.Length - 1]);
                Math.DivRem(Number, 8, out int index);
                AvatarView.Background = App.ChartColor[index];
            }

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();
            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += async (s, e) => {
                try
                {
                    await UpdateSelectedAvatar(Avatar);
                }
                catch (Exception ex)
                {
                    await _pt.HandleException(ex, "EditProfilePictureBottomSheet", "TapGesture.Tapped");
                }                
            }; 
            AvatarView.GestureRecognizers.Add(TapGesture);

            flxAvatars.Children.Add(AvatarView);
        }
    }

    private async Task UpdateSelectedAvatar(string Avatar)
    {
        List<PatchDoc> UserUpdate = new List<PatchDoc>();

        PatchDoc ProfilePicture = new PatchDoc
        {
            op = "replace",
            path = "/ProfilePicture",
            value = Avatar
        };

        UserUpdate.Add(ProfilePicture);

        await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

        EditAccountSettings CurrentPage = (EditAccountSettings)Shell.Current.CurrentPage;
        CurrentPage.UpdatedAvatar = Avatar;

        if (App.CurrentBottomSheet != null)
        {
            await this.DismissAsync();
            App.CurrentBottomSheet = null;
        }
    }

    private async void UploadPicture_Clicked(object sender, EventArgs e)
    {
        try
        {
            FileResult UploadFile = await MediaPicker.PickPhotoAsync();

            if (UploadFile is null) return;

            if (UploadFile.OpenReadAsync().Result.Length < 3000000)
            {
                string result = await _ds.UploadUserProfilePicture(App.UserDetails.UserID, UploadFile);

                if(result == "OK")
                {
                    List<PatchDoc> UserUpdate = new List<PatchDoc>();

                    PatchDoc ProfilePicture = new PatchDoc
                    {
                        op = "replace",
                        path = "/ProfilePicture",
                        value = "Upload"
                    };

                    UserUpdate.Add(ProfilePicture);

                    await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);

                    EditAccountSettings CurrentPage = (EditAccountSettings)Shell.Current.CurrentPage;
                    CurrentPage.ProfilePicStream = await UploadFile.OpenReadAsync();

                    if (App.CurrentBottomSheet != null)
                    {
                        await this.DismissAsync();
                        App.CurrentBottomSheet = null;
                    }
                }    
                else
                {

                }

            }
            else
            {

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditProfilePictureBottomSheet", "UploadPicture_Clicked");
        }
    }
}