using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class RegisterPage : BasePage
{
	public RegisterPage(RegisterPageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}
}