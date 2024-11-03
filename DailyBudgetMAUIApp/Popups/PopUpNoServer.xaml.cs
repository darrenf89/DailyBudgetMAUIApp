using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;


namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpNoServer : Popup
{
    private readonly PopUpNoServerViewModel _vm;
    public PopUpNoServer(PopUpNoServerViewModel viewModel)
	{
		InitializeComponent();
		MainGrid.HeightRequest = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
		MainGrid.WidthRequest = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        border.WidthRequest = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 80;

        BindingContext = viewModel;
        _vm = viewModel;
    }

}