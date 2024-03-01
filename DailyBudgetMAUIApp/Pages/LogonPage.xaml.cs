using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class LogonPage : ContentPage
{
	public LogonPage(LogonPageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;

    }
}