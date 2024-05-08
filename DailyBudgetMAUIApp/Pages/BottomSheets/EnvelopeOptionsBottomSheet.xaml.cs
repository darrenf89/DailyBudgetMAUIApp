using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class EnvelopeOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }

    public EnvelopeOptionsBottomSheet(IRestDataService ds, IProductTools pt)
    {
        InitializeComponent();

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        var ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;
        btnDismiss.WidthRequest = ButtonWidth;
        //MainScrollView.MaximumHeightRequest = ScreenHeight - 280;

        _ds = ds;
        _pt = pt;
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

            await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(AddSaving)}?SavingType=Envelope");
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

    private async void SpendMoney_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentBottomSheet != null)
        {
            await this.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        List<Savings>? savings = await _ds.GetBudgetEnvelopeSaving(App.DefaultBudgetID);
        Dictionary<string, int> EnvelopeSavings = new Dictionary<string, int>();
        foreach (var s in savings)
        {
            EnvelopeSavings.Add(s.SavingsName, s.SavingID);
        }

        string[] EnvelopeList = EnvelopeSavings.Keys.ToArray();
        var SelectEnvelope = await Application.Current.MainPage.DisplayActionSheet($"Select which stash you want to pay from!", "Cancel", null, EnvelopeList);
        if (SelectEnvelope == "Cancel")
        {

        }
        else
        {
            int SavingsID = EnvelopeSavings[SelectEnvelope];
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.MainPage.ShowPopup(PopUp);
            }

            string SpendType = "EnvelopeSaving";
            Transactions T = new Transactions
            {
                IsSpendFromSavings = true,
                SavingID = SavingsID,
                SavingName = SelectEnvelope,
                SavingsSpendType = SpendType,
                EventType = "Envelope",
                TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow)
            };

            await Shell.Current.GoToAsync($"/{nameof(AddTransaction)}?BudgetID={App.DefaultBudget.BudgetID}&NavigatedFrom=ViewMainPage&TransactionID=0",
                new Dictionary<string, object>
                {
                    ["Transaction"] = T
                });
        }
    }
}