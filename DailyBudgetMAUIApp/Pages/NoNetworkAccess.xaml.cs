using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class NoNetworkAccess : BasePage
{
    private readonly NoNetworkAccessViewModel _vm;

    public NoNetworkAccess(NoNetworkAccessViewModel viewModel)
	{
        this.BindingContext = viewModel;
        InitializeComponent();

        _vm = viewModel;
    }

    protected async override void OnAppearing()
    {
        Application.Current.Resources.TryGetValue("Danger", out var Danger);

        _vm.TxtConnectionStatus = "Internet Connection Status: Disconnected";
        _vm.ColorConnectionStatus = (Color)Danger;
        _vm.BtnIsEnabled = false;

        base.OnAppearing(); 
    }

}