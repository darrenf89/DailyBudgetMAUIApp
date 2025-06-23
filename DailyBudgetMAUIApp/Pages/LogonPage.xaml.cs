using CommunityToolkit.Maui.Behaviors;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class LogonPage : BasePage
{    
    public LogonPage(LogonPageViewModel viewModel)
	{
        InitializeComponent();
		this.BindingContext = viewModel;
    }
}