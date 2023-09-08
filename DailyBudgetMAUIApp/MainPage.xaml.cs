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

        if (_vm.IsBudgetUpdate)
        {
            App.DefaultBudget = _vm.DefaultBudget;
            App.SessionLastUpdate = DateTime.UtcNow;

            string budgetString = JsonConvert.SerializeObject(_vm.DefaultBudget);
            Preferences.Set(nameof(App.DefaultBudget), budgetString);
            Preferences.Set(nameof(App.SessionLastUpdate), DateTime.UtcNow.ToString());
        }

        if (_vm.DefaultBudgetID == 1 || !_vm.DefaultBudget.IsCreated)
        {
            //TODO: Navigate to create Budget journey as no budget assinged!
        }
        

    }

}

