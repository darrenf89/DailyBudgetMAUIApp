
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewAccounts : BasePage
{
    private readonly ViewAccountsViewModel _vm;
    private readonly IModalPopupService _ps;
    public ViewAccounts(ViewAccountsViewModel viewModel, IModalPopupService ps)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;
        _ps = ps;

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

            if (_ps.CurrentPopup is not null)
                return;

            await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());

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

            await _ps.CloseAsync<PopUpPage>();
        }
        catch(Exception ex)
        {
            await _vm.HandleException(ex, "ViewAccounts", "OnAppearing");
        }
    }

    public class UpdateViewAccount
    {
        public bool _isSuccess;
        public bool _isBackground;
        public UpdateViewAccount(bool IsSuccess, bool isBackground)
        {
            _isSuccess = IsSuccess;
            _isBackground = isBackground;
        }
    }
}