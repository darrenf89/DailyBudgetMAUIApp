using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.ListView;
using System.ComponentModel;


namespace DailyBudgetMAUIApp.Pages;

public partial class ViewCategories : ContentPage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewCategoriesViewModel _vm;
    private readonly IDispatcherTimer _timer;
    public ViewCategories(ViewCategoriesViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        InitializeComponent();

        listView.PropertyChanged += listView_PropertyChanged;
        var timer = Application.Current.Dispatcher.CreateTimer();
        _timer = timer;
        timer.Interval = TimeSpan.FromSeconds(45);
        timer.Tick += async (s, e) => 
        {
            _vm.ChartUpdating = true;
            await CycleThroughChart();
            await Task.Delay(1000);
            _vm.ChartUpdating = false;
        };
        timer.Start();

    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        _timer.Stop();
    }
 
    protected async override void OnAppearing()
    {
        base.OnAppearing();

        _vm.Categories.Clear();

        List<Categories> CategoryList = _ds.GetAllHeaderCategoryDetailsFull(App.DefaultBudgetID).Result;

        foreach (Categories cat in CategoryList)
        {
            _vm.Categories.Add(cat);

            ChartClass Value = new ChartClass
            {
                XAxesString = cat.CategoryName,
                YAxesDouble = (double)cat.CategorySpendPayPeriod
            };

            _vm.CategoriesChart.Add(Value);
        }

        Categories AddCat = new Categories
        {
            CategoryName = "Add new category",
            CategoryID = -1,
            CategoryIcon = "Add"
        };

        _vm.Categories.Add(AddCat);

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private async Task SwitchChart()
    {
        
        if(_vm.CurrentChart == "PayPeriod")
        {
            _vm.CurrentChart = "AllTime";
            _vm.ChartTitle = "Spend per category all time";

            _vm.CategoriesChart.Clear();

            foreach (Categories cat in _vm.Categories)
            {
                if(cat.CategoryID != -1)
                {
                    ChartClass Value = new ChartClass
                    {
                        XAxesString = cat.CategoryName,
                        YAxesDouble = (double)cat.CategorySpendAllTime
                    };

                    _vm.CategoriesChart.Add(Value);
                }
            }
        }
        else if(_vm.CurrentChart == "AllTime")
        {
            _vm.CurrentChart = "PayPeriod";
            _vm.ChartTitle = "Spend per category this period";

            _vm.CategoriesChart.Clear();

            foreach (Categories cat in _vm.Categories)
            {
                if (cat.CategoryID != -1)
                {
                    ChartClass Value = new ChartClass
                    {
                        XAxesString = cat.CategoryName,
                        YAxesDouble = (double)cat.CategorySpendPayPeriod
                    };

                    _vm.CategoriesChart.Add(Value);
                }
            }
        }
    }

    private async Task CycleThroughChart()
    {
        MainThread.BeginInvokeOnMainThread(async () => await SwitchChart());
    }

    private void listView_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Width")
        {
            var size = _vm.ScreenWidth / listView.ItemSize;
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