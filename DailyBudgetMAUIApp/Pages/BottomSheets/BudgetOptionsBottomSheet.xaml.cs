using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Handlers;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class BudgetOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public Budgets Budget { get; set; }
    public Picker SwitchBudgetPicker { get; set; }

    public BudgetOptionsBottomSheet(Budgets InputBudget, IProductTools pt, IRestDataService ds)
    {
        InitializeComponent();

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        var ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;

        btnDismiss.WidthRequest = ButtonWidth;

        Budget = InputBudget;

        lblBudgetName.Text = $"{Budget.BudgetName} Budget Details";
        MainScrollView.MaximumHeightRequest = ScreenHeight - 280;
        _pt = pt;
        _ds = ds;
    }

    private void btnDismiss_Clicked(object sender, EventArgs e)
    {
        this.DismissAsync();
    }

    private async void ViewTransactions_Tapped(object sender, TappedEventArgs e)
    {
        var PopUp = new PopUpPage();
        App.CurrentPopUp = PopUp;
        Application.Current.MainPage.ShowPopup(PopUp);

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }
        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewTransactions)}");
    }

    private void ViewBills_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void ViewSavings_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void ViewIncomes_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void EditPayInfo_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void EditBudgetSettings_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void SyncBankBalance_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void UserSettings_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void CreateNewBudget_Tapped(object sender, TappedEventArgs e)
    {
        bool result = await Shell.Current.DisplayAlert("Create a new budget?", "Are you sure you want to create a new budget?", "Yes", "No");
        if(result)
        {
            try
            {
                if (App.CurrentBottomSheet != null)
                {
                    await this.DismissAsync();
                    App.CurrentBottomSheet = null;
                }
            }
            catch(Exception)
            {

            }

            Budgets NewBudget = await _ds.CreateNewBudget(App.UserDetails.Email);
            await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.CreateNewBudget)}?BudgetID={NewBudget.BudgetID}&NavigatedFrom=Budget Settings");

        }
       
    }

    private void EditShareBudget_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void ShareBudget_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void SwitchBudget_Tapped(object sender, TappedEventArgs e)
    {
        SwitchBudgetPicker = await _pt.SwitchBudget("Budget Options");
        MainVSL.Children.Add(SwitchBudgetPicker);
        SwitchBudgetPicker.Focus();
    }

    private void ViewEnvelopes_Tapped(object sender, TappedEventArgs e)
    {

    }
}