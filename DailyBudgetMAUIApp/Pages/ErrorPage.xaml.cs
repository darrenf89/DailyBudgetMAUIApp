using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ErrorPage : BasePage
{
    private readonly ErrorPageViewModel _vm;
    private readonly IPopupService _ps;
    public ErrorPage(ErrorPageViewModel viewModel, IPopupService ps)
	{
		InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;
        _ps = ps;
    }

    protected async override void OnAppearing()
    {
        try 
        {
            if (_vm.Error is not null && _vm.Error.ErrorLogID != 0)
            {
                 _vm.TxtErrorMessage = _vm.Error.ErrorMessage;
            }
            else
            {
                _vm.TxtErrorMessage = "If you continue to experience problems please contact us so we can help.";
            }

            base.OnAppearing();
        } 
        catch (Exception ex)
        {
            await _vm.HandleException(ex, "ErrorPage", "OnAppearing");
        }
    }

}