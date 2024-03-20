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
    private IDispatcherTimer _timer;

    public EditBudgetSettings(EditBudgetSettingsViewModel viewModel)
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
        MainScrollView.MaximumHeightRequest = ScreenHeight - 280;
        TopBV.WidthRequest = ScreenWidth;
        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));
        MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));

        lblTitle.Text = $"Budget settings";

        await _vm.OnLoad();

        var timer = Application.Current.Dispatcher.CreateTimer();
        _timer = timer;
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += async (s, e) =>
        {
            await _vm.UpdateTime();
        };
        timer.Start();

        CurrencySettingValue.Text = 9000.01.ToString("c", CultureInfo.CurrentCulture);

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        _timer.Stop();
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

    private void ChangeSelectedCurrency_Tapped(object sender, TappedEventArgs e)
    {
        _vm.ChangeSelectedCurrency();
        CurrencySearch.Text = "";
    }    
}