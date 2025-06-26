using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class FamilyAccountsManage : BasePage
{
    private readonly FamilyAccountsManageViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IModalPopupService _ps;

    public FamilyAccountsManage(FamilyAccountsManageViewModel vm, IProductTools pt, IModalPopupService ps)
    {
        InitializeComponent();
        _vm = vm;
        _pt = pt;
        BindingContext = _vm;        
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

    }

    protected async override void OnAppearing()
    {
        try
        {
            if (_ps.CurrentPopup is not null)
                return;

            await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());

            base.OnAppearing();

            await _vm.LoadFamilyAccounts();
            await _ps.CloseAsync<PopUpPage>();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "FamilyAccountsManage", "OnAppearing");
        }
    }


}