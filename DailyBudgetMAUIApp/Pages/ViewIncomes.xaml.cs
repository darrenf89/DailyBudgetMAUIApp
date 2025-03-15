using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Popups;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewIncomes : BasePage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewIncomesViewModel _vm;
    public ViewIncomes(ViewIncomesViewModel viewModel, IProductTools pt, IRestDataService ds)
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
            _vm.Budget = _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited").Result;
            List<IncomeEvents> I = _ds.GetBudgetIncomes(App.DefaultBudgetID, "ViewIncomes").Result;

            _vm.BalanceExtraPeriodIncome = _vm.Budget.BankBalance.GetValueOrDefault() + _vm.Budget.CurrentActiveIncome;

            _vm.Incomes.Clear();
        
            foreach (IncomeEvents income in I)
            {
                _vm.Incomes.Add(income);
            }

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            _vm.SignOutButtonWidth = ScreenWidth - 60;


            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
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
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

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
            var popup = new PopUpPageVariableInput("Income due date", Description, DescriptionSub, "", Income.DateOfIncomeEvent, "DateTime", new PopUpPageVariableInputViewModel());
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

            if(!string.IsNullOrEmpty(result.ToString()))
            {
                Income.DateOfIncomeEvent = (DateTime)result;

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
            var popup = new PopUpPageVariableInput("Outgoing amount", Description, DescriptionSub, "", Income.IncomeAmount, "Currency", new PopUpPageVariableInputViewModel());
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

            if (!string.IsNullOrEmpty(result.ToString()))
            {
                Income.IncomeAmount = (decimal)result;

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
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"///{nameof(ViewIncomes)}//{nameof(AddIncome)}?BudgetID={_vm.Budget.BudgetID}&NavigatedFrom=ViewIncomes");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewIncomes", "AddNewIncome_Clicked");
        }
    }
}