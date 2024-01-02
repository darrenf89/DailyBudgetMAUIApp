using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;

using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.Pages;

public partial class ShareBudget : BottomSheet
{
    private readonly IRestDataService _ds;

    public double ButtonWidth;
    public double ScreenWidth;
    public ShareBudgetRequest ShareBudgetRequest;

    public ShareBudget(ShareBudgetRequest SBR, IRestDataService ds)
    {
        InitializeComponent();

        _ds = ds;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 81;
        ShareBudgetRequest = SBR;

        btnShareBudgetButton.WidthRequest = ButtonWidth;
    }

    private void btnShareBudgetButton_Clicked(object sender, EventArgs e)
    {
        entSharedWithUserEmail.IsEnabled = false;
        entSharedWithUserEmail.IsEnabled = true;

        ShareBudgetRequest.SharedWithUserEmail = entSharedWithUserEmail.Text;

        string Status = _ds.ShareBudgetRequest(ShareBudgetRequest).Result;

        if(Status == "OK")
        {
            grdShareBudget.IsVisible = false;
            grdBudgetShared.IsVisible = true;
        }
        else
        {
            validatorIsEmailFound.IsEnabled = true;
        }
    }

    private void entSharedWithUserEmail_Focused(object sender, FocusEventArgs e)
    {
        validatorIsEmailFound.IsEnabled = false;
    }

    private void Dismiss_Clicked(object sender, EventArgs e)
    {
        entSharedWithUserEmail.IsEnabled = false;
        entSharedWithUserEmail.IsEnabled = true;

        this.DismissAsync();
    }
}