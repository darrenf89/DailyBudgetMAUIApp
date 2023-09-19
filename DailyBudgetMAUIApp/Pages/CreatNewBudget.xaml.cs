using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class CreatNewBudget : ContentPage
{
	public CreatNewBudget(CreatNewBudgetViewModel viewModel)
	{
		InitializeComponent();
        this.BindingContext = viewModel;
    }
}