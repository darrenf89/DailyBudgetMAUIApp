using DailyBudgetMAUIApp.ViewModels;


namespace DailyBudgetMAUIApp.Pages;

public partial class LoadUpPage : BasePage
{
	public LoadUpPage(LoadUpPageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;

    }


}