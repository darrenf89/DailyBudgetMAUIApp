
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewAccounts : BasePage
{
    private readonly ViewAccountsViewModel _vm;
    public ViewAccounts(ViewAccountsViewModel viewModel)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;

    }
    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        base.OnNavigatedFrom(args);
    }

    async protected override void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            await _vm.GetBankAccountDetails();

            WeakReferenceMessenger.Default.UnregisterAll(this);
            WeakReferenceMessenger.Default.Register<UpdateViewAccount>(this, (r, m) =>
            {
                try
                {
                    _vm.GetBankAccountDetails();
                }
                catch
                {

                }
            });

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch(Exception ex)
        {
            await _vm.HandleException(ex, "ViewAccounts", "OnAppearing");
        }
    }

    public class UpdateViewAccount
    {
        public bool _isSuccess;
        public UpdateViewAccount(bool IsSuccess)
        {
            _isSuccess = IsSuccess;
        }
    }
}