using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class NoNetworkAccess : BasePage
{
    private readonly NoNetworkAccessViewModel _vm;
    private readonly IPopupService _ps;
    public NoNetworkAccess(NoNetworkAccessViewModel viewModel, IPopupService ps)
	{
        this.BindingContext = viewModel;
        InitializeComponent();

        _vm = viewModel;
        _ps = ps;
    }

    protected async override void OnAppearing()
    {
        if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
        Application.Current.Resources.TryGetValue("Danger", out var Danger);

        _vm.TxtConnectionStatus = "Internet Connection Status: Disconnected";
        _vm.ColorConnectionStatus = (Color)Danger;
        _vm.BtnIsEnabled = false;

        base.OnAppearing(); 
    }

}