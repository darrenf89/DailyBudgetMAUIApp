using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.ViewModels;
using Plugin.LocalNotification;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.Pages;

public partial class CreateNewFamilyAccounts : BasePage
{
    private readonly CreateNewFamilyAccountsViewModel _vm;
    private readonly IProductTools _pt;

    public CreateNewFamilyAccounts(CreateNewFamilyAccountsViewModel vm, IProductTools pt)
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
            await _vm.LoadFamilyAccount();

            pckrSymbolPlacement.SelectedIndex = _vm.SelectedCurrencyPlacement.Id - 1;
            pckrDateFormat.SelectedIndex = _vm.SelectedDateFormats.Id - 1;
            pckrNumberFormat.SelectedIndex = _vm.SelectedNumberFormats.Id - 1;
            pckrTimeZone.SelectedIndex = _vm.SelectedTimeZone.TimeZoneID - 1;

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "CreateNewFamilyAccounts", "OnAppearing");
        }
    }

    private void ChangeSelectedCurrency_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            _vm.ChangeCurrency();

            CurrencySearch.Text = "";
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "CreateNewFamilyAccounts", "ChangeSelectedCurrency_Tapped");
        }
    }


}