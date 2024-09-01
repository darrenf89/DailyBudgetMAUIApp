using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpNoNetwork : Popup
{
	public PopUpNoNetwork()
	{
		InitializeComponent();
		MainGrid.HeightRequest = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
		MainGrid.WidthRequest = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        border.WidthRequest = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 80;
    }

}