using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.ListView;
using System.ComponentModel;


namespace DailyBudgetMAUIApp.Pages;

public partial class ViewSavings : ContentPage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewSavingsViewModel _vm;
    public ViewSavings(ViewSavingsViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        InitializeComponent();

    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
    }

    private void EditSaving_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void SpendSaving_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void DeleteSavings_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void MoveBalance_Tapped(object sender, TappedEventArgs e)
    {

    }
}