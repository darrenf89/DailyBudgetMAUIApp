using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class LoadUpPage : BasePage
{
    private readonly IPopupService _ps;
    public LoadUpPage(LoadUpPageViewModel viewModel, IPopupService ps)
	{
        _ps = ps;
        InitializeComponent();
		this.BindingContext = viewModel;

    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
    }



}