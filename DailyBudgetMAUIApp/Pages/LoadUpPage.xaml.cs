using DailyBudgetMAUIApp.ViewModels;


namespace DailyBudgetMAUIApp.Pages;

public partial class LoadUpPage : ContentPage
{
	public LoadUpPage(LoadUpPageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;

    }


}