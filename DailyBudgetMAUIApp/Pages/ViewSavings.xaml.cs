using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewSavings : BasePage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewSavingsViewModel _vm;
    public ViewSavings(ViewSavingsViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        InitializeComponent();


    }

    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            _vm.Budget = _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited").Result;
            List<Savings> S = _ds.GetBudgetRegularSaving(App.DefaultBudgetID).Result;

            _vm.TotalSavings = 0;
            _vm.Budget.DailySavingOutgoing = 0;
            _vm.Savings.Clear();
        
            foreach (Savings saving in S)
            {
                _vm.TotalSavings += saving.CurrentBalance.GetValueOrDefault();
                _vm.Budget.DailySavingOutgoing += saving.RegularSavingValue.GetValueOrDefault();
                _vm.Savings.Add(saving);
            }

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            _vm.SignOutButtonWidth = ScreenWidth - 60;

            int DaysToPayDay = (int)Math.Ceiling((_vm.Budget.NextIncomePayday.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);
            _vm.PayDaySavings = _vm.Budget.DailySavingOutgoing * DaysToPayDay;

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "OnAppearing");
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        //listView.HeightRequest = _vm.ScreenHeight - BudgetDetailsGrid.Height - TitleView.Height - 110;
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "HomeButton_Clicked");
        }

    }

    private async void EditSaving_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;

            bool result = await Shell.Current.DisplayAlert($"Edit {Saving.SavingsName}?", $"Are you sure you want to edit {Saving.SavingsName}?", "Yes", "Cancel");

            if(result)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"///{nameof(ViewSavings)}//{nameof(AddSaving)}?BudgetID={_vm.Budget.BudgetID}&SavingID={Saving.SavingID}&NavigatedFrom=ViewSavings");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "EditSaving_Tapped");
        }

    }

    private async void SpendSaving_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

            var Saving = (Savings)e.Parameter;
            bool result = await Shell.Current.DisplayAlert($"Spend some savings?", $"Are you sure you want to spend some of the money you have saved for {Saving.SavingsName}?", "Yes", "Cancel");

            if (result)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

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

                await Shell.Current.GoToAsync($"/{nameof(AddTransaction)}?BudgetID={_vm.Budget.BudgetID}&NavigatedFrom=ViewSavings&TransactionID=0",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = T
                    });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "SpendSaving_Tapped");
        }
    }

    private async void DeleteSavings_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;
            bool result = await Shell.Current.DisplayAlert($"Delete {Saving.SavingsName}?", $"Are you sure you want to delete {Saving.SavingsName}?", "Yes", "Cancel");

            if (result)
            {
                result = await _ds.DeleteSaving(Saving.SavingID) == "OK" ? true: false;
                if (result)
                {
                    _vm.Savings.Remove(Saving);
                }

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "DeleteSavings_Tapped");
        }
    }

    private async void MoveBalance_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;
            var popup = new PopupMoveBalance(App.DefaultBudget, "Saving", Saving.SavingID, false, new PopupMoveBalanceViewModel(), new ProductTools(new RestDataService()), new RestDataService());
            await Task.Delay(100);
            var result = await Application.Current.MainPage.ShowPopupAsync(popup);
            if (result.ToString() == "OK")
            {
                List<Savings> S = _ds.GetBudgetRegularSaving(App.DefaultBudgetID).Result;

                _vm.TotalSavings = 0;
                _vm.Budget.DailySavingOutgoing = 0;
                _vm.Savings.Clear();

                foreach (Savings saving in S)
                {
                    _vm.TotalSavings += saving.CurrentBalance.GetValueOrDefault();
                    _vm.Budget.DailySavingOutgoing += saving.RegularSavingValue.GetValueOrDefault();
                    _vm.Savings.Add(saving);
                }

                App.DefaultBudget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "MoveBalance_Tapped");
        }
    }

    private async void AddNewSaving_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool result = await Shell.Current.DisplayAlert($"Add a new saving?", $"Are you sure you want to add a new saving?", "Yes", "Cancel");

            if (result)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"///{nameof(ViewSavings)}//{nameof(AddSaving)}?BudgetID={_vm.Budget.BudgetID}&NavigatedFrom=ViewSavings");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "AddNewSaving_Clicked");
        }
    }
}