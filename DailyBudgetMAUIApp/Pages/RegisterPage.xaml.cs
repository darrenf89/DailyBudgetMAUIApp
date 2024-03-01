using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Popups;

namespace DailyBudgetMAUIApp.Pages;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterPageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;
	}
}