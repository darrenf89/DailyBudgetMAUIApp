using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class EnvelopeOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }

    public EnvelopeOptionsBottomSheet()
    {
        InitializeComponent();

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        var ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;
        btnDismiss.WidthRequest = ButtonWidth;
        //MainScrollView.MaximumHeightRequest = ScreenHeight - 280;
  
    }

    private void btnDismiss_Clicked(object sender, EventArgs e)
    {
        this.DismissAsync();
    }

    private async void CreateNewEnvelope_Tapped(object sender, TappedEventArgs e)
    {
        bool result = await Shell.Current.DisplayAlert("Create new Envelope?", "Are you sure you want to create a new envelope?", "Yes", "No");
        if (result)
        {
            try
            {
                if (App.CurrentBottomSheet != null)
                {
                    await this.DismissAsync();
                    App.CurrentBottomSheet = null;
                }
            }
            catch (Exception)
            {

            }

            await Shell.Current.GoToAsync($"{nameof(AddSaving)}?SavingType=Envelope");
        }
    }

    private async void ViewAllEnvelopes_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        await Shell.Current.GoToAsync($"///{nameof(DailyBudgetMAUIApp.Pages.ViewEnvelopes)}");
    }
}