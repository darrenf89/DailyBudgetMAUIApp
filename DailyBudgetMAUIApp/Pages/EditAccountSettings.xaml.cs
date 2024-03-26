using DailyBudgetMAUIApp.ViewModels;
using Microsoft.Maui.Layouts;
using Syncfusion.Maui.Core;


namespace DailyBudgetMAUIApp.Pages;

public partial class EditAccountSettings : ContentPage
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }

    private readonly EditAccountSettingsViewModel _vm;

    public EditAccountSettings(EditAccountSettingsViewModel viewModel)
	{
		InitializeComponent();      

        this.BindingContext = viewModel;
        _vm = viewModel;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 60;
        ButtonWidth = ScreenWidth - 40;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        TopBV.WidthRequest = ScreenWidth;
        MainAbs.WidthRequest = ScreenWidth;
        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));

        lblTitle.Text = $"Account settings";
        
        if(_vm.User.ProfilePicture.Contains("Avatar"))
        {
            ProfilePicture.ContentType = ContentType.AvatarCharacter;
            bool Success = Enum.TryParse(_vm.User.ProfilePicture, out AvatarCharacter Avatar);
            if(Success)
            {
                ProfilePicture.AvatarCharacter = Avatar;
                int Number = Convert.ToInt32(_vm.User.ProfilePicture[_vm.User.ProfilePicture.Length - 1]);
                Math.DivRem(Number, 8, out int index);
                ProfilePicture.Background = App.ChartColor[index];;
            }
            else
            {
                 ProfilePicture.AvatarCharacter = AvatarCharacter.Avatar1;
                 ProfilePicture.Background = App.ChartColor[1];
            }
        }
        else
        {
            
        }
        

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
    }

    private void acrCurrencySetting_Tapped(object sender, TappedEventArgs e)
    {
        if (!CurrencySetting.IsVisible)
        {
            CurrencySetting.IsVisible = true;
            CurrencySettingIcon.Glyph = "\ue5cf";
        }
        else
        {
            CurrencySetting.IsVisible = false;
            CurrencySettingIcon.Glyph = "\ue5ce";
        }
    }

    private void acrDateTimeSetting_Tapped(object sender, TappedEventArgs e)
    {
        if (!DateTimeSetting.IsVisible)
        {
            DateTimeSetting.IsVisible = true;
            DateTimeIcon.Glyph = "\ue5cf";
        }
        else
        {
            DateTimeSetting.IsVisible = false;
            DateTimeIcon.Glyph = "\ue5ce";
        }
    }

    private void acrPayDaySetting_Tapped(object sender, TappedEventArgs e)
    {
        if (!PayDaySetting.IsVisible)
        {
            PayDaySetting.IsVisible = true;
            PayDayIcon.Glyph = "\ue5cf";
        }
        else
        {
            PayDaySetting.IsVisible = false;
            PayDayIcon.Glyph = "\ue5ce";
        }
    }

    private void acrBudgetToggleSetting_Tapped(object sender, TappedEventArgs e)
    {
        if (!BudgetToggleSetting.IsVisible)
        {
            BudgetToggleSetting.IsVisible = true;
            BudgetToggleIcon.Glyph = "\ue5cf";
        }
        else
        {
            BudgetToggleSetting.IsVisible = false;
            BudgetToggleIcon.Glyph = "\ue5ce";
        }
    }    
}