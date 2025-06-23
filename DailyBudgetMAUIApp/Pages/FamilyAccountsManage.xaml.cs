using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.ViewModels;
using Plugin.LocalNotification;

namespace DailyBudgetMAUIApp.Pages;

public partial class FamilyAccountsManage : BasePage
{
    private readonly FamilyAccountsManageViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IPopupService _ps;

    public FamilyAccountsManage(FamilyAccountsManageViewModel vm, IProductTools pt, IPopupService ps)
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
            if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "FamilyAccountsManage", "OnAppearing");
        }
    }


}