using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;

using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class ShareBudget : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public double ButtonWidth;
    public double ScreenWidth;
    public ShareBudgetRequest ShareBudgetRequest;

    public ShareBudget(ShareBudgetRequest SBR, IRestDataService ds, IProductTools pt)
    {
        InitializeComponent();

        _ds = ds;
        _pt = pt;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 81;
        ShareBudgetRequest = SBR;

        btnShareBudgetButton.WidthRequest = ButtonWidth;
    }

    private void btnShareBudgetButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            entSharedWithUserEmail.IsEnabled = false;
            entSharedWithUserEmail.IsEnabled = true;

            if (entSharedWithUserEmail.Text == ShareBudgetRequest.SharedByUserEmail)
            {
                BudgetShareSuccess.IsVisible = false;
                validatorIsEmailFound.IsVisible = true;
                lblEmailValidator.Text = "Can not share a budget with yourself!";
            }
            else
            {
                ShareBudgetRequest.SharedWithUserEmail = entSharedWithUserEmail.Text;

                string Status = _ds.ShareBudgetRequest(ShareBudgetRequest).Result;

                if (Status == "OK")
                {
                    BudgetShareSuccess.IsVisible = true;
                    validatorIsEmailFound.IsVisible = false;
                    entSharedWithUserEmail.IsEnabled = false;
                    btnShareBudgetButton.IsEnabled = false;
                }
                else if (Status == "Budget Already Shared")
                {
                    BudgetShareSuccess.IsVisible = false;
                    validatorIsEmailFound.IsVisible = true;
                    lblEmailValidator.Text = "This Budget has already been shared. Please stop sharing the budget to share with someone else";
                    entSharedWithUserEmail.IsEnabled = false;
                    btnShareBudgetButton.IsEnabled = false;
                }
                else if (Status == "Share Request Active")
                {
                    BudgetShareSuccess.IsVisible = false;
                    validatorIsEmailFound.IsVisible = true;
                    lblEmailValidator.Text = "There is already a share request active, please cancel this request to share again!";
                    entSharedWithUserEmail.IsEnabled = false;
                    btnShareBudgetButton.IsEnabled = false;
                    btnCancelCurrentRequest.IsVisible = true;
                }
                else
                {
                    BudgetShareSuccess.IsVisible = false;
                    validatorIsEmailFound.IsVisible = true;
                    lblEmailValidator.Text = "No User with that email exists please check and try again!";
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ShareBudget", "btnShareBudgetButton_Clicked");
        }
    }

    private void entSharedWithUserEmail_Focused(object sender, FocusEventArgs e)
    {
        validatorIsEmailFound.IsVisible = false;
    }

    private async void btnCancelCurrentRequest_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            bool result = await Shell.Current.DisplayAlert("Cancel Current Requests", "Are you sure you want to cancel all currently acitve budget share requests", "Yes", "No, go back");
            if(result)
            {
                await _ds.CancelCurrentShareBudgetRequest(ShareBudgetRequest.SharedBudgetID);

                BudgetShareSuccess.IsVisible = false;
                validatorIsEmailFound.IsVisible = false;
                entSharedWithUserEmail.IsEnabled = true;
                btnShareBudgetButton.IsEnabled = true;
                btnCancelCurrentRequest.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ShareBudget", "btnCancelCurrentRequest_Tapped");
        }

    }
}