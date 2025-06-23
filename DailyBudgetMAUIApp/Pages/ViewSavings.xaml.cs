using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;


namespace DailyBudgetMAUIApp.Pages;

public partial class ViewSavings : BasePage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    private readonly IPopupService _ps;
	private readonly ViewSavingsViewModel _vm;
    public ViewSavings(ViewSavingsViewModel viewModel, IProductTools pt, IRestDataService ds, IPopupService ps)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;
        _ps = ps;

        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (Navigation.NavigationStack.Count > 1)
        {
            Shell.SetTabBarIsVisible(this, false);
        }
    }

    protected async override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
        _vm.IsPageBusy = false;

        await Task.Delay(500);
        base.OnNavigatingFrom(args);
    }


    protected async override void OnAppearing()
    {
        try
        {
            _vm.IsPageBusy = true;
            base.OnAppearing();
            await LoadPageData();

            if (App.IsPopupShowing){App.IsPopupShowing = false;await _ps.ClosePopupAsync(Shell.Current);}
            _vm.IsPageBusy = false;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "OnAppearing");
        }
    }

    private async Task LoadPageData()
    {
        _vm.Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited");
        List<Savings> S = await _ds.GetBudgetRegularSaving(App.DefaultBudgetID);

        _vm.TotalSavings = 0;
        _vm.Budget.DailySavingOutgoing = 0;
        _vm.Savings.Clear();

        foreach (Savings saving in S)
        {
            _vm.TotalSavings += saving.CurrentBalance.GetValueOrDefault();
            if (!saving.IsSavingsPaused)
            {
                _vm.Budget.DailySavingOutgoing += saving.RegularSavingValue.GetValueOrDefault();
            }
            _vm.Savings.Add(saving);
        }

        double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        _vm.SignOutButtonWidth = ScreenWidth - 60;

        int DaysToPayDay = (int)Math.Ceiling((_vm.Budget.NextIncomePayday.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);
        _vm.PayDaySavings = _vm.Budget.DailySavingOutgoing * DaysToPayDay;

    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
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
                if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
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
                if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
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

    private async void UnPause_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;
            bool result = await Shell.Current.DisplayAlert($"Pause {Saving.SavingsName}?", $"Are you sure you want to pause the saving {Saving.SavingsName}, we will recalculate your budget and stop putting money into this saving everyday!", "Yes", "Cancel");

            if (result)
            {
                if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                await Task.Delay(1);

                result = await _ds.UnPauseSaving(Saving.SavingID,App.DefaultBudgetID) == "OK" ? true : false;
                if (result)
                {
                    await _ds.ReCalculateBudget(App.DefaultBudgetID);
                    var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
                    App.DefaultBudget = Budget;
                    App.IsBudgetUpdated = true;
                }

                await LoadPageData();
                if (App.IsPopupShowing){App.IsPopupShowing = false;await _ps.ClosePopupAsync(Shell.Current);}
            }         

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "UnPause_Tapped");
        }
    }
    private async void Pause_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;
            bool result = await Shell.Current.DisplayAlert($"Restart Saving {Saving.SavingsName}?", $"Are you sure you want to restart saving for {Saving.SavingsName}, we will recalculate your budget and start putting money into this saving everyday!", "Yes", "Cancel");

            if (result)
            {
                if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                await Task.Delay(1);

                result = await _ds.PauseSaving(Saving.SavingID, App.DefaultBudgetID) == "OK" ? true : false;
                if (result)
                {
                    await _ds.ReCalculateBudget(App.DefaultBudgetID);
                    var Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
                    App.DefaultBudget = Budget;
                    App.IsBudgetUpdated = true;
                }

                await LoadPageData();

                if (App.IsPopupShowing){App.IsPopupShowing = false;await _ps.ClosePopupAsync(Shell.Current);}
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "Pause_Tapped");
        }
    }
    private async void MoveBalance_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Saving = (Savings)e.Parameter;
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopupMoveBalanceViewModel.Budget)] = App.DefaultBudget,
                [nameof(PopupMoveBalanceViewModel.Type)] = "Saving",
                [nameof(PopupMoveBalanceViewModel.Id)] = Saving.SavingID,
                [nameof(PopupMoveBalanceViewModel.IsCoverOverSpend)] = false
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            IPopupResult<string> popupResult = await _ps.ShowPopupAsync<PopupMoveBalance, string>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes,
                cancellationToken: CancellationToken.None

            ); 
            
            await Task.Delay(1);
            if (popupResult.Result.ToString() == "OK")
            {
                await LoadPageData();

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
                if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                await Shell.Current.GoToAsync($"///{nameof(ViewSavings)}//{nameof(AddSaving)}?BudgetID={_vm.Budget.BudgetID}&NavigatedFrom=ViewSavings");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewSavings", "AddNewSaving_Clicked");
        }
    }
}