using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.ListView;
using System.ComponentModel;


namespace DailyBudgetMAUIApp.Pages;

public partial class ViewCategories : ContentPage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewCategoriesViewModel _vm;
    public ViewCategories(ViewCategoriesViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        InitializeComponent();

        listView.PropertyChanged += listView_PropertyChanged;

    }

    private void listView_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Width")
        {
            var size = Application.Current.MainPage.Width / listView.ItemSize;
            GridLayout gridLayout = new GridLayout();
            gridLayout.SpanCount = (int)size;
            listView.ItemsLayout = gridLayout;
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

    private void ListViewTapped_Tapped(object sender, TappedEventArgs e)
    {

    }
}