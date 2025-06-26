using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewIncomes : BasePage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    private readonly IModalPopupService _ps;
	private readonly ViewIncomesViewModel _vm;
    public ViewIncomes(ViewIncomesViewModel viewModel, IProductTools pt, IRestDataService ds, IModalPopupService ps)
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

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        _vm.IsPageBusy = false;
        base.OnNavigatingFrom(args);
    }


    protected async override void OnAppearing()
    {
        try
        {
            if (_ps.CurrentPopup is not null)
                return;

            await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());

            _vm.IsPageBusy = true;
            _vm.Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited");
            List<IncomeEvents> I = await _ds.GetBudgetIncomes(App.DefaultBudgetID, "ViewIncomes");

            _vm.BalanceExtraPeriodIncome = _vm.Budget.BankBalance.GetValueOrDefault() + _vm.Budget.CurrentActiveIncome;

            _vm.Incomes.Clear();
        
            foreach (IncomeEvents income in I)
            {
                _vm.Incomes.Add(income);
            }

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            _vm.SignOutButtonWidth = ScreenWidth - 60;


            await _ps.CloseAsync<PopUpPage>();
            _vm.IsPageBusy = false;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewIncomes", "OnAppearing");
        }
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewIncomes", "HomeButton_Clicked");
        }
    }

    private async void EditIncome_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Income = (IncomeEvents)e.Parameter;

            bool result = await Shell.Current.DisplayAlert($"Edit {Income.IncomeName}?", $"Are you sure you want to edit {Income.IncomeName}?", "Yes", "Cancel");

            if(result)
            {
                await Shell.Current.GoToAsync($"///{nameof(ViewIncomes)}/{nameof(AddIncome)}?BudgetID={_vm.Budget.BudgetID}&IncomeID={Income.IncomeEventID}&NavigatedFrom=ViewIncomes");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewIncomes", "EditIncome_Tapped");
        }
    }
    private async void CloseIncome_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Income = (IncomeEvents)e.Parameter;
            bool result = await Shell.Current.DisplayAlert($"Close income {Income.IncomeName}?", $"Are you sure you want to close {Income.IncomeName}?", "Yes", "Cancel");

            if (result)
            {
                List<PatchDoc> PatchDocs = new List<PatchDoc>();
                PatchDoc IsClosed = new PatchDoc
                {
                    op = "replace",
                    path = "/IsClosed",
                    value = true
                };

                PatchDocs.Add(IsClosed);

                await _ds.PatchIncome(Income.IncomeEventID, PatchDocs);
                _vm.Incomes.Remove(Income);

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewIncomes", "CloseIncome_Tapped");
        }
    }

    private async void UpdateDate_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Income = (IncomeEvents)e.Parameter;

            string Description = "Update extra income due date!";
            string DescriptionSub = "extra income not due when you expected? You can update the due date to any date in the future!";

            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopUpPageVariableInputViewModel.Description)] = Description,
                [nameof(PopUpPageVariableInputViewModel.DescriptionSub)] = DescriptionSub,
                [nameof(PopUpPageVariableInputViewModel.TitleText)] = "Income due date",
                [nameof(PopUpPageVariableInputViewModel.Input)] = Income.DateOfIncomeEvent,
                [nameof(PopUpPageVariableInputViewModel.Type)] = "DateTime",
                [nameof(PopUpPageVariableInputViewModel.Placeholder)] = "",
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            IPopupResult<object> popupResult = await _ps.PopupService.ShowPopupAsync<PopUpPageVariableInput, object>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes,
                cancellationToken: CancellationToken.None
            );

            if (popupResult.Result is string && DateTime.TryParse((string)popupResult.Result, out DateTime result))
            {
                Income.DateOfIncomeEvent = result;

                List<PatchDoc> PatchDocs = new List<PatchDoc>();
                PatchDoc DateOfIncomeEvent = new PatchDoc
                {
                    op = "replace",
                    path = "/DateOfIncomeEvent",
                    value = Income.DateOfIncomeEvent
                };

                PatchDocs.Add(DateOfIncomeEvent);

                await _ds.PatchIncome(Income.IncomeEventID, PatchDocs);

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewIncomes", "UpdateDate_Tapped");
        }
    }

    private async void UpdateAmount_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Income = (IncomeEvents)e.Parameter;

            string Description = "Update income amount!";
            string DescriptionSub = "Income not as much as you expected, you can update the income amount and we will do the rest!";
            
            var queryAttributes = new Dictionary<string, object>
            {
                [nameof(PopUpPageVariableInputViewModel.Description)] = Description,
                [nameof(PopUpPageVariableInputViewModel.DescriptionSub)] = DescriptionSub,
                [nameof(PopUpPageVariableInputViewModel.TitleText)] = "income amount",
                [nameof(PopUpPageVariableInputViewModel.Input)] = Income.IncomeAmount,
                [nameof(PopUpPageVariableInputViewModel.Type)] = "Currency",
                [nameof(PopUpPageVariableInputViewModel.Placeholder)] = "",
            };

            var popupOptions = new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = false,
                PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
            };

            IPopupResult<object> popupResult = await _ps.PopupService.ShowPopupAsync<PopUpPageVariableInput, object>(
                Shell.Current,
                options: popupOptions,
                shellParameters: queryAttributes,
                cancellationToken: CancellationToken.None
            );

            if (popupResult.Result is string && decimal.TryParse((string)popupResult.Result, out decimal result))
            {
                Income.IncomeAmount = result;

                List<PatchDoc> PatchDocs = new List<PatchDoc>();
                PatchDoc IncomeAmount = new PatchDoc
                {
                    op = "replace",
                    path = "/IncomeAmount",
                    value = Income.IncomeAmount
                };

                PatchDocs.Add(IncomeAmount);

                await _ds.PatchIncome(Income.IncomeEventID, PatchDocs);

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewIncomes", "UpdateAmount_Tapped");
        }
    }    

    private async void AddNewIncome_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool result = await Shell.Current.DisplayAlert($"Add a new income?", $"Are you sure you want to add a new extra income?", "Yes", "Cancel");

            if (result)
            {
                await Shell.Current.GoToAsync($"///{nameof(ViewIncomes)}//{nameof(AddIncome)}?BudgetID={_vm.Budget.BudgetID}&NavigatedFrom=ViewIncomes");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewIncomes", "AddNewIncome_Clicked");
        }
    }
}