using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ErrorPage : BasePage
{
    private readonly ErrorPageViewModel _vm;
    public ErrorPage(ErrorPageViewModel viewModel)
	{
		InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;
    }

    protected async override void OnAppearing()
    {
        try 
        {
            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }

            if(_vm.Error is not null && _vm.Error.ErrorLogID != 0)
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