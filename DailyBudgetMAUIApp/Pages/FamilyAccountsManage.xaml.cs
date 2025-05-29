using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.ViewModels;
using Plugin.LocalNotification;

namespace DailyBudgetMAUIApp.Pages;

public partial class FamilyAccountsManage : BasePage
{
    private readonly FamilyAccountsManageViewModel _vm;
    private readonly IProductTools _pt;

    public FamilyAccountsManage(FamilyAccountsManageViewModel vm, IProductTools pt)
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
        base.OnAppearing();
        try
        {
            await _vm.LoadFamilyAccounts();

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "FamilyAccountsManage", "OnAppearing");
        }
    }


}