using AndroidX.Core.View;
using DailyBudgetMAUIApp.ViewModels;


namespace DailyBudgetMAUIApp.Pages;

public partial class LoadUpPage : BasePage
{
	public LoadUpPage(LoadUpPageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;

    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }



}