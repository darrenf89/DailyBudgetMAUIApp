using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using System.Diagnostics;
using Newtonsoft.Json;

namespace DailyBudgetMAUIApp;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _vm;
    public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (App.DefaultBudgetID == 1 || App.DefaultBudget.IsCreated)
        {
            //TODO: Navigate to create Budget journey as no budget assinged!
        }
    }

}

