using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;

using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.Pages;

public partial class BudgetOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }

    public BudgetOptionsBottomSheet()
    {
        InitializeComponent();

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;

        DismissButton.WidthRequest = ButtonWidth;

    }
    
}