using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class NoServerAccess : BasePage
{
    private readonly NoServerAccessViewModel _vm;
    private readonly IPopupService _ps;
    public NoServerAccess(NoServerAccessViewModel viewModel, IPopupService ps)
	{
        this.BindingContext = viewModel;
        InitializeComponent();

        _vm = viewModel;
        _ps = ps;
    }

    protected async override void OnAppearing()
    {
        try
        {
            if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
            if (App.UserDetails is not null && App.UserDetails.SessionExpiry > DateTime.UtcNow)
            {
                _vm.TxtButton = "Get back to budgeting!";
            }
            else
            {
                _vm.TxtButton = "Try again!";
            }

            await _vm.CheckConnection(true);
            await _vm.StartTimer();

            base.OnAppearing();
        }
        catch (Exception ex) 
        {
            await _vm.HandleException(ex, "NoServerAccess", "OnAppearing");
        }
 
    }

    protected async override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        try
        {
            await _vm.EndTimer();
        }
        catch 
        { 

        }
        
        base.OnNavigatedFrom(args);

    }
}