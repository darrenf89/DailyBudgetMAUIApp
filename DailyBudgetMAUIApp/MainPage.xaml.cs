using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using System.Diagnostics;
using Newtonsoft.Json;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Alerts;

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

        ProcessSnackBar();

        base.OnAppearing();
    }

    private async void ProcessSnackBar()
    {
        var snackbarSuccessOptions = new SnackbarOptions
        {
            BackgroundColor = Colors.Red,
            TextColor = Colors.Green,
            ActionButtonTextColor = Colors.Yellow,
            CornerRadius = new CornerRadius(10),
            Font = Font.SystemFontOfSize(14),
            ActionButtonFont = Font.SystemFontOfSize(14),
            CharacterSpacing = 0.5
        };

        var snackbarWarningOptions = new SnackbarOptions
        {
            BackgroundColor = Colors.Red,
            TextColor = Colors.Green,
            ActionButtonTextColor = Colors.Yellow,
            CornerRadius = new CornerRadius(10),
            Font = Font.SystemFontOfSize(14),
            ActionButtonFont = Font.SystemFontOfSize(14),
            CharacterSpacing = 0.5
        };

        string text;
        string actionButtonText;
        Action action;
        TimeSpan duration;

        if(_vm.SnackBar == null || _vm.SnackBar == "")
        {

        }
        else
        {
            if(_vm.SnackBar == "Budget Created")
            {
                text=$"Horrrrah, you have created budget {App.DefaultBudget.BudgetName}!";
                actionButtonText="Undo";
                action = async () => await DisplayAlert("Snackbar ActionButton Tapped", "The user has tapped the Snackbar ActionButton", "OK");
                duration = TimeSpan.FromSeconds(10);

                await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();
            }
        }
    }

}

