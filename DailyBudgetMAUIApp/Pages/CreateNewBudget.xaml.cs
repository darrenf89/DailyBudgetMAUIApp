using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;


namespace DailyBudgetMAUIApp.Pages;

public partial class CreateNewBudget : ContentPage
{
    private readonly CreateNewBudgetViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public CreateNewBudget(CreateNewBudgetViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
		InitializeComponent(); 

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

    }

    async protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {    
        try
        {
            if (_vm.BudgetID == 0)
            {
                _vm.BudgetID = _ds.CreateNewBudget(App.UserDetails.Email).Result.BudgetID;

                if (_vm.BudgetID != 0)
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
                    _vm.BudgetSettings = _ds.GetBudgetSettings(_vm.BudgetID).Result;
                    //TODO: SET THE BUDGET SETTINGS IN FRONT END
                }
                else
                {
                    throw new Exception("Couldn't create a new budget in the database when trying to set one up");
                }
            }
            else
            {
                if(_vm.Budget == null)
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
                    _vm.BudgetSettings = _ds.GetBudgetSettings(_vm.BudgetID).Result;
                    //TODO: SET THE BUDGET SETTINGS IN FRONT END

                }

            }

            UpdateStageDisplay();

            //TODO: IF NO BUDGET NAME ASK FOR NAME ENETERED BY USING A POP UP.
            if (_vm.Budget.BudgetName == "" || _vm.Budget.BudgetName == null)
            {

                try
                {
                    string Description = "Every budget needs a name, let us know how you'd like your budget to be known so we can use this to identify it for you in the future.";
                    string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                    var popup = new PopUpPageSingleInput("Budget Name", Description, DescriptionSub, "Enter a budget name!", _vm.Budget.BudgetName ,new PopUpPageSingleInputViewModel());
                    var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                    if (result != null || (string)result != "")
                    {
                        _vm.Budget.BudgetName = (string)result;
                    }

                }
                catch (Exception ex)
                {
                    ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "LoadingPopupBudgetName").Result;
                }

            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($" --> {ex.Message}");
            ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "Constructor").Result;
            await Shell.Current.GoToAsync(nameof(ErrorPage),
                new Dictionary<string, object>
                {
                    ["Error"] = Error
                });
        }

        base.OnNavigatedTo(args);

    }
    private void UpdateStageDisplay()
    {
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Gray300", out var Gray300);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);

        bvStage1.Color = (_vm.Stage == "Budget Settings" || _vm.Stage == "Budget Details" || _vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;
        bvStage2.Color = (_vm.Stage == "Budget Details" || _vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;
        bvStage3.Color = (_vm.Stage == "Budget Outgoings" || _vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;
        bvStage4.Color = (_vm.Stage == "Budget Savings" || _vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;
        bvStage5.Color = (_vm.Stage == "Budget Extra Income") ? (Color)Success : (Color)Gray300;

        SettingsDetails.IsVisible = (_vm.Stage == "Budget Settings");
        BudgetDetails.IsVisible = (_vm.Stage == "Budget Details");
        BillDetails.IsVisible = (_vm.Stage == "Budget Outgoings");
        SavingsDetails.IsVisible = (_vm.Stage == "Budget Savings");
        IncomeDetails.IsVisible = (_vm.Stage == "Budget Extra Income");

        lblSettingsHeader.TextColor = (_vm.Stage == "Budget Settings") ? (Color)Primary : (Color)Tertiary;
        lblBudgetHeader.TextColor = (_vm.Stage == "Budget Details") ? (Color)Primary : (Color)Tertiary;
        lblBillsHeader.TextColor = (_vm.Stage == "Budget Outgoings") ? (Color)Primary : (Color)Tertiary;
        lblSavingsHeader.TextColor = (_vm.Stage == "Budget Savings") ? (Color)Primary : (Color)Tertiary;
        lblIncomesHeader.TextColor = (_vm.Stage == "Budget Extra Income") ? (Color)Primary : (Color)Tertiary;
    }

    private void GoToStageSettings_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Settings";
        UpdateStageDisplay();
    }

    private void GoToStageBudget_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Details";
        UpdateStageDisplay();
    }

    private void GoToStageBills_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Outgoings";
        UpdateStageDisplay();
    }

    private void GoToStageSavings_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Savings";
        UpdateStageDisplay();
    }

    private void GoToStageIncomes_Tapped(object sender, TappedEventArgs e)
    {
        _vm.Stage = "Budget Extra Income";
        UpdateStageDisplay();
    }
}
