using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class NoNetworkAccess : ContentPage
{
	public NoNetworkAccess(NoNetworkAccessViewModel viewModel)
	{
        this.BindingContext = viewModel;
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }

        base.OnAppearing(); 
    }

}