using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewEnvelopes : BasePage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewEnvelopesViewModel _vm;
    public ViewEnvelopes(ViewEnvelopesViewModel viewModel, IProductTools pt, IRestDataService ds)
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
            _vm.IsPageBusy = true;
            base.OnAppearing();

            _vm.Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited");
            List<Savings> S = await _ds.GetBudgetEnvelopeSaving(App.DefaultBudgetID);

            _vm.EnvelopeBalance = 0;
            _vm.EnvelopeTotal = 0;
            _vm.RegularValue = 0;
            _vm.Savings.Clear();
        
            foreach (Savings saving in S)
            {
                _vm.EnvelopeBalance += saving.CurrentBalance.GetValueOrDefault();
                _vm.EnvelopeTotal += saving.PeriodSavingValue.GetValueOrDefault();
                _vm.RegularValue = saving.RegularSavingValue.GetValueOrDefault();
                _vm.Savings.Add(saving);
            }

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            _vm.SignOutButtonWidth = ScreenWidth - 60;

            _vm.DaysToPayDay = (int)Math.Ceiling((_vm.Budget.NextIncomePayday.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
            _vm.IsPageBusy = true;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewEnvelopes", "OnAppearing");
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
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewEnvelopes", "HomeButton_Clicked");
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
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"///{nameof(ViewEnvelopes)}//{nameof(AddSaving)}?BudgetID={_vm.Budget.BudgetID}&SavingID={Saving.SavingID}&NavigatedFrom=ViewEnvelopes");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewEnvelopes", "EditSaving_Tapped");
        }
    }

    private async void SpendSaving_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;
            bool result = await Shell.Current.DisplayAlert($"Spend from envelope?", $"Are you sure you want to take money out of your {Saving.SavingsName}?", "Yes", "Cancel");

            if (result)
            {
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
                    SavingID = Saving.SavingID,
                    SavingName = Saving.SavingsName,
                    SavingsSpendType = SpendType,
                    EventType = "Envelope",
                    TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow)
                };

                await Shell.Current.GoToAsync($"/{nameof(AddTransaction)}?BudgetID={_vm.Budget.BudgetID}&NavigatedFrom=ViewEnvelopes&TransactionID=0",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = T
                    });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewEnvelopes", "SpendSaving_Tapped");
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
            await _pt.HandleException(ex, "ViewEnvelopes", "DeleteSavings_Tapped");
        }
    }

    private async void MoveBalance_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;
            var popup = new PopupMoveBalance(App.DefaultBudget, "Saving", Saving.SavingID, false, new PopupMoveBalanceViewModel(), IPlatformApplication.Current.Services.GetService<IProductTools>(), IPlatformApplication.Current.Services.GetService<IRestDataService>());
            await Task.Delay(100);
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
            if (result.ToString() == "OK")
            {
                List<Savings> S = await _ds.GetBudgetEnvelopeSaving(App.DefaultBudgetID);

                _vm.EnvelopeBalance = 0;
                _vm.EnvelopeTotal = 0;
                _vm.RegularValue = 0;
                _vm.Savings.Clear();

                foreach (Savings saving in S)
                {
                    _vm.EnvelopeBalance += saving.CurrentBalance.GetValueOrDefault();
                    _vm.EnvelopeTotal += saving.PeriodSavingValue.GetValueOrDefault();
                    _vm.RegularValue = saving.RegularSavingValue.GetValueOrDefault();
                    _vm.Savings.Add(saving);
                }

                var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
                App.DefaultBudget = Budget;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewEnvelopes", "MoveBalance_Tapped");
        }
    }

    private async void AddNewSaving_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool result = await Shell.Current.DisplayAlert($"Add a new envelope?", $"Are you sure you want to add a new envelope?", "Yes", "Cancel");

            if (result)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"///{nameof(ViewEnvelopes)}//{nameof(AddSaving)}?BudgetID={_vm.Budget.BudgetID}&NavigatedFrom=ViewEnvelopes");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewEnvelopes", "AddNewSaving_Clicked");
        }
    }
}