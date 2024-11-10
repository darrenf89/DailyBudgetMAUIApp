
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewSupport : BasePage
{
    private readonly ViewSupportViewModel _vm;
    public ViewSupport(ViewSupportViewModel vm)
    {
        _vm = vm;
        this.BindingContext = _vm;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}