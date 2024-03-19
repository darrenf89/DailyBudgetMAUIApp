using DailyBudgetMAUIApp.ViewModels;
using The49.Maui.BottomSheet;
using Microsoft.Maui.Layouts;
using System.Globalization;

namespace DailyBudgetMAUIApp.Pages;

public partial class EditBudgetSettings : ContentPage
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }

    private readonly EditBudgetSettingsViewModel _vm;

    public EditBudgetSettings(EditBudgetSettingsViewModel viewModel)
	{
		InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 60;
        ButtonWidth = ScreenWidth - 40;
        MainScrollView.MaximumHeightRequest = ScreenHeight - 280;

        TopBV.WidthRequest = ScreenWidth;
        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));
        MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));

        lblTitle.Text = $"Budget settings";

    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        await _vm.OnLoad();

        CurrencySettingValue.Text = 9000.01.ToString("c", CultureInfo.CurrentCulture);

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
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

    private void ChangeSelectedCurrency_Tapped(object sender, TappedEventArgs e)
    {
        _vm.ChangeSelectedCurrency();
        CurrencySearch.Text = "";
    }    
}