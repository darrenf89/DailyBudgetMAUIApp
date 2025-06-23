using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class TransactionOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }

    public TransactionOptionsBottomSheet(IRestDataService ds, IProductTools pt)
    {
        InitializeComponent();

        _ds = ds;
        _pt = pt;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        var ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;
        btnDismiss.WidthRequest = ButtonWidth;
        if(!App.IsPremiumAccount)
        {
            Category.IsVisible = false;
            Payee.IsVisible = false;
            IsPremiumAccount.IsVisible = true;
            IsPremiumAccountTwo.IsVisible = true;
        }
        else
        {
            IsPremiumAccount.IsVisible = false;
            IsPremiumAccountTwo.IsVisible = false;
        }

        //MainScrollView.MaximumHeightRequest = ScreenHeight - 280;

    }

    private void btnDismiss_Clicked(object sender, EventArgs e)
    {
        this.DismissAsync();
    }

    private async void CreateNewTransaction_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            bool result = await Shell.Current.DisplayAlert("Create new Transaction?", "Are you sure you want to create a new Transaction?", "Yes", "No");
            if (result)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }


                if (App.CurrentBottomSheet != null)
                {
                    await this.DismissAsync();
                    App.CurrentBottomSheet = null;
                }


                Transactions transaction = new Transactions();
                await Shell.Current.GoToAsync($"{nameof(MainPage)}/{nameof(AddTransaction)}?BudgetID={App.DefaultBudgetID}&TransactionID={transaction.TransactionID}&NavigatedFrom=ViewMainPage",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = transaction
                    });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "TransactionOptionsBottomSheet", "CreateNewTransaction_Tapped");
        }
    }

    private async void ViewAllTransactions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewTransactions)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "TransactionOptionsBottomSheet", "ViewAllTransactions_Tapped");
        }
    }

    private async void ViewCategories_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewCategories)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "TransactionOptionsBottomSheet", "ViewCategories_Tapped");
        }
    }

    private async void ViewPayees_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewPayees)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "TransactionOptionsBottomSheet", "ViewPayees_Tapped");
        }
    }

    private async void SpendMoney_Tapped(object sender, TappedEventArgs e)
    {
        try
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
            var SelectEnvelope = await Application.Current.Windows[0].Page.DisplayActionSheet($"Select which stash you want to pay from!", "Cancel", null, EnvelopeList);
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
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
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
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "TransactionOptionsBottomSheet", "SpendMoney_Tapped");
        }

    }

    private async void SpendSavingMoney_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await this.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            List<Savings>? savings = await _ds.GetAllBudgetSavings(App.DefaultBudgetID);
            Dictionary<string, int> Savings = new Dictionary<string, int>();
            foreach (var s in savings)
            {
                Savings.Add(s.SavingsName, s.SavingID);
            }

            string[] EnvelopeList = Savings.Keys.ToArray();
            var SelectedSaving = await Application.Current.Windows[0].Page.DisplayActionSheet($"Select which saving you want to pay from!", "Cancel", null, EnvelopeList);
            if (SelectedSaving == "Cancel")
            {

            }
            else
            {
                int SavingsID = Savings[SelectedSaving];
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                Savings Saving = await _ds.GetSavingFromID(SavingsID);
                string SpendType = Saving.SavingsType == "SavingsBuilder" ? "BuildingSaving" : "MaintainValues";
                Transactions T = new Transactions
                {
                    IsSpendFromSavings = true,
                    SavingID = Saving.SavingID,
                    SavingName = Saving.SavingsName,
                    SavingsSpendType = SpendType,
                    EventType = "Saving",
                    TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow)
                };

                await Shell.Current.GoToAsync($"/{nameof(AddTransaction)}?BudgetID={App.DefaultBudget.BudgetID}&NavigatedFrom=ViewMainPage&TransactionID=0",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = T
                    });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "TransactionOptionsBottomSheet", "SpendSavingMoney_Tapped");
        }

    }
}