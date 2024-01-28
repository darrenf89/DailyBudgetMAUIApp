using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewBills : ContentPage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewBillsViewModel _vm;
    public ViewBills(ViewBillsViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        InitializeComponent();

    }

    protected async override void OnAppearing()
    {

    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        listView.HeightRequest = _vm.ScreenHeight - BudgetDetailsGrid.Height - TitleView.Height - 150;
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
}