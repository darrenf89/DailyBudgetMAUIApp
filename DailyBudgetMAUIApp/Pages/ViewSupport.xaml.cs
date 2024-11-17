
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewSupport : BasePage
{
    private readonly ViewSupportViewModel _vm;
    public ViewSupport(ViewSupportViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;

    }
    async protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _vm.GetSupport();
        }
        catch(Exception ex)
        {
            await _vm.HandleException(ex, "ViewSupport", "OnAppearing");
        }

    }

}