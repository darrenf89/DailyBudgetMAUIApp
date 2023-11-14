using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;

namespace DailyBudgetMAUIApp.Pages;

public partial class LogonPage : ContentPage
{
	public LogonPage(LogonPageViewModel viewModel)
	{
		InitializeComponent();
		this.BindingContext = viewModel;


    }
}