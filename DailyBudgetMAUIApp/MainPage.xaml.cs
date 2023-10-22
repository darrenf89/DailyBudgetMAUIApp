using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using System.Diagnostics;
using Newtonsoft.Json;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _vm;
    public MainPage(MainPageViewModel viewModel)
	{
        var popup = new PopUpPage();
        Application.Current.MainPage.ShowPopup(popup);
        
        InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;

        popup.Close();
        
    }

    protected override void OnAppearing()
    {        
        //TODO: Implement some kind of check that its the first time the page has loaded and only navigate if it is.
        //TODO: Show on the main page a warning that the budget set up isnt finished with a button to navigate to new budget journey
        if (!App.DefaultBudget.IsCreated && !App.HasVisitedCreatePage)
        {
            App.HasVisitedCreatePage = true;
            Shell.Current.GoToAsync($"{nameof(CreateNewBudget)}?BudgetID={App.DefaultBudgetID}&NavigatedFrom=Budget Settings");

            //TODO: Navigate to create Budget journey as no budget assinged!
        }

        base.OnAppearing();
    }

}

