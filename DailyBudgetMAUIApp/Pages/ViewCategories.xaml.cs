using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Charts;
using Syncfusion.Maui.ListView;
using System.Collections.ObjectModel;
using System.ComponentModel;
using The49.Maui.BottomSheet;


namespace DailyBudgetMAUIApp.Pages;

public partial class ViewCategories : ContentPage
{
    public List<Categories> _addCategoryList = new List<Categories>();
    public List<Categories> AddCategoryList
    {
        get => _addCategoryList;
        set
        {
            if (_addCategoryList != value)
            {
                _addCategoryList = value;
                _vm.Categories.Clear();
                foreach (Categories c in AddCategoryList)
                {
                    _vm.Categories.Add(c);
                }
            }
        }
    }

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
        _vm.IsPlaying = true;
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        _timer.Stop();
    }
 
    protected async override void OnAppearing()
    {
        base.OnAppearing();

        listView.RefreshView();
        listView.RefreshItem();

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
            _vm.ChartTitle = "All time";

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
            _vm.ChartTitle = "Current period";

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

    private async void ListViewTapped_Tapped(object sender, TappedEventArgs e)
    {
        Categories Category = (Categories)e.Parameter;

        if(Category.CategoryID == -1)
        {
            AddNewCategoryBottomSheet page = new AddNewCategoryBottomSheet(_vm.Categories, new ProductTools(new RestDataService()), new RestDataService());

            page.Detents = new DetentsCollection()
            {
                new FullscreenDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
        }
        else
        {

        }
    }

    private async void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        _vm.ChartUpdating = true;

        switch (e.Direction)
        {
            case SwipeDirection.Left:
                await SwitchChart();
                break;
            case SwipeDirection.Right:
                await SwitchChart();
                break;
        }

        await Task.Delay(2000);
        _timer.Stop();
        _timer.Start();
        _vm.ChartUpdating = false;
    }

    private void btnPlayPause_Clicked(object sender, EventArgs e)
    {
        if(_vm.IsPlaying)
        {
            _vm.IsPlaying = false;
            _timer.Stop();
        }
        else
        {
            _vm.IsPlaying = true;
            _timer.Start();
        }
    }


}