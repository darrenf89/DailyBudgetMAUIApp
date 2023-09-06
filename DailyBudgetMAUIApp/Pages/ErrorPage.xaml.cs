using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ErrorPage : ContentPage
{
	public ErrorPage(ErrorPageViewModel viewModel)
	{
		InitializeComponent();
        this.BindingContext = viewModel;
    }

}