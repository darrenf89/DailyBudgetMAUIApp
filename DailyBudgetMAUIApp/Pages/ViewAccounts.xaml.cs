
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewAccounts : BasePage
{
    private readonly ViewAccountsViewModel _vm;
    private readonly IPopupService _ps;
    public ViewAccounts(ViewAccountsViewModel viewModel, IPopupService ps)
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

            if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
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