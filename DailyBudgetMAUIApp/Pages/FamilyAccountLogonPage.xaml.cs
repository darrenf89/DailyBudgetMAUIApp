using CommunityToolkit.Maui.Behaviors;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class FamilyAccountLogonPage : BasePage
{    
    public FamilyAccountLogonPage(FamilyAccountLogonPageViewModel viewModel)
	{
        InitializeComponent();
		this.BindingContext = viewModel;
    }
}