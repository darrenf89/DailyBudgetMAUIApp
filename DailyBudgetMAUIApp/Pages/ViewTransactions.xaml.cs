using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewTransactions : ContentPage
{
    private readonly ViewTransactionsViewModel _vm;
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public ViewTransactions(ViewTransactionsViewModel viewModel, IRestDataService ds, IProductTools pt)
	{
		InitializeComponent();

        _ds = ds;
        _pt = pt;

        this.BindingContext = viewModel;
        _vm = viewModel;

    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        _vm.LVHeight = 580;

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        var PopUp = new PopUpPage();
        App.CurrentPopUp = PopUp;
        Application.Current.MainPage.ShowPopup(PopUp);

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
    }

}