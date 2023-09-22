using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Diagnostics;


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

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {       

        var popup = new PopUpPage();
        this.ShowPopup(popup);

        try
        {
            if (_vm.BudgetID == 0)
            {
                _vm.BudgetID = _ds.CreateNewBudget(App.UserDetails.Email).Result.BudgetID;

                if (_vm.BudgetID != 0)
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
                }
                else
                {
                    throw new Exception("Couldn't create a new budget in the database when trying to set one up");
                }
            }
            else
            {
                if(_vm.Buget == null)
                {
                    _vm.Budget = _ds.GetBudgetDetailsAsync(_vm.BudgetID, "Full").Result;
                }

            }
            
            //TODO: IF NO BUDGET NAME ASK FOR NAME ENETERED BY USING A POP UP.
            if(_vm.Budget.BudgetName == "" || _vm.Budget.BudgetName == null)
            {
                
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($" --> {ex.Message}");
            ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "Constructor").Result;
            popup.Close();
            Shell.Current.GoToAsync(nameof(ErrorPage),
                new Dictionary<string, object>
                {
                    ["Error"] = Error
                });
        }

        base.OnNavigatedTo(args);
        popup.Close();

    }
}
