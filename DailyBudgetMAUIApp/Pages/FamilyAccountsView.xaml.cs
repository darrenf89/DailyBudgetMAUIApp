using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class FamilyAccountsView : BasePage
{
    private readonly FamilyAccountsViewViewModel _vm;
    private readonly IProductTools _pt;

    public FamilyAccountsView(FamilyAccountsViewViewModel vm, IProductTools pt)
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
            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }

            _vm.IsPageBusy = true;
            await _vm.OnLoad();
            if(_vm.familyUserAccounts.Count > 0)
            {
                vslPckrSwitchBudget.Content = _vm.SwitchBudgetPicker;
                _vm.IsPageBusy = false;
            }
            else
            {
                NoFamilyAccounts.IsVisible = true;
                _vm.IsBudgetVisible = false;
            }


        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "FamilyAccountsView", "OnAppearing");
        }
    }

    private async void SeeMoreTransactions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!vslRecentTransactions.IsVisible)
            {
                vslRecentTransactions.IsVisible = true;
                fisRecentTransactionHideShow.Glyph = "\ue5cf";
                vslRecentTransactions.HeightRequest = 0;

                var animation = new Animation(v => vslRecentTransactions.HeightRequest = v, 0, _vm.RecentTransactionsHeight);
                animation.Commit(this, "ShowRecentTran", 16, 100, Easing.CubicIn);

            }
            else
            {
                var animation = new Animation(v => vslRecentTransactions.HeightRequest = v, _vm.RecentTransactionsHeight, 0);
                animation.Commit(this, "HideRecentTran", 16, 1000, Easing.CubicIn, async (v, c) =>
                {
                    fisRecentTransactionHideShow.Glyph = "\ue5ce";
                    vslRecentTransactions.IsVisible = false;

                });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "FamilyAccountsView", "SeeMoreTransactions_Tapped");
        }
    }
}