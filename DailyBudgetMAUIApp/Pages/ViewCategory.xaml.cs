using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Carousel;
using Syncfusion.Maui.Charts;
using Syncfusion.Maui.ListView;
using System.Collections.ObjectModel;
using System.ComponentModel;
using The49.Maui.BottomSheet;


namespace DailyBudgetMAUIApp.Pages;

public partial class ViewCategory : ContentPage
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
	private readonly ViewCategoryViewModel _vm;
    private readonly IDispatcherTimer _timer;
    public ViewCategory(ViewCategoryViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        InitializeComponent();

        var timer = Application.Current.Dispatcher.CreateTimer();
        _timer = timer;
        timer.Interval = TimeSpan.FromSeconds(45);
        timer.Tick += async (s, e) => 
        {
            _vm.ChartUpdating = true;
            await CycleThroughChart();
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

        _vm.Categories.Clear();
        _vm.CategoriesChart.Clear();

        List<Categories> CategoryList = _ds.GetHeaderCategoryDetailsFull(_vm.HeaderCatId, App.DefaultBudgetID).Result;

        var CategoryName = CategoryList.Where(c => !c.IsSubCategory).Select(c => c.CategoryName).FirstOrDefault();

        _vm.Title = $"{CategoryName}";

        foreach (Categories cat in CategoryList.Where(c=>c.IsSubCategory))
        {
            _vm.Categories.Add(cat);

            ChartClass Value = new ChartClass
            {
                XAxesString = cat.CategoryName,
                YAxesDouble = (double)cat.CategorySpendPayPeriod
            };

            _vm.CategoriesChart.Add(Value);
        }

        _vm.PayPeriods.Add("All Time");
        foreach (SpendPeriods SP in _vm.Categories[0].CategorySpendPeriods)
        {
            if (SP.IsCurrentPeriod)
            {
                _vm.PayPeriods.Add("Current Period");
            }
            else
            {
                _vm.PayPeriods.Add($"{SP.FromDate: dd MMM} to {SP.ToDate: dd MMM}");
            }
        }

        listView.RefreshView();
        listView.RefreshItem();

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private async Task SwitchChart(int Index)
    {
        _vm.ChartUpdating = true;

        _vm.ChartTitle = _vm.PayPeriods[Index];

        if (Index == 0)
        {
            _vm.CategoriesChart.Clear();

            foreach (Categories cat in _vm.Categories)
            {
                if (cat.CategoryID != -1)
                {
                    ChartClass Value = new ChartClass
                    {
                        XAxesString = cat.CategoryName,
                        YAxesDouble = (double)cat.CategorySpendAllTime
                    };

                    _vm.CategoriesChart.Add(Value);

                    cat.CategorySpendPayPeriod = cat.CategorySpendPeriods[Index].SpendTotalAmount;
                }
            }
        }
        else
        {
            _vm.CategoriesChart.Clear();

            foreach (Categories cat in _vm.Categories)
            {
                if (cat.CategoryID != -1)
                {
                    ChartClass Value = new ChartClass
                    {
                        XAxesString = cat.CategoryName,
                        YAxesDouble = (double)cat.CategorySpendPeriods[Index - 1].SpendTotalAmount
                    };

                    _vm.CategoriesChart.Add(Value);

                    cat.CategorySpendPayPeriod = cat.CategorySpendPeriods[Index - 1].SpendTotalAmount;
                }
            }
        }

        await Task.Delay(1000);
        _timer.Stop();
        _timer.Start();

        _vm.ChartUpdating = false;

    }

    private async Task CycleThroughChart()
    {
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            if (_vm.SelectedIndex == (_vm.PayPeriods.Count() - 1))
            {
                _vm.SelectedIndex = 0;                
            }
            else if(_vm.SelectedIndex == 0)
            {
                _vm.SelectedIndex = _vm.PayPeriods.Count() - 1;
            }
            else
            {
                _vm.SelectedIndex += 1;
            }
            await SwitchChart(_vm.SelectedIndex);
        });
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

    private async void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        

        switch (e.Direction)
        {
            case SwipeDirection.Left:
                if(_vm.SelectedIndex < (_vm.PayPeriods.Count()-1))
                {
                    _vm.SelectedIndex += 1;
                    await SwitchChart(_vm.SelectedIndex);
                }
                break;
            case SwipeDirection.Right:
                if (_vm.SelectedIndex > 0)
                {
                    _vm.SelectedIndex -= 1;
                    await SwitchChart(_vm.SelectedIndex);
                }
                break;
        }       
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

    private async void TabCarousel_SwipeEnded(object sender, EventArgs e)
    {
        var Carousel = (SfCarousel)sender;
        int Index = Carousel.SelectedIndex;

        _vm.SelectedIndex = Index;
        await SwitchChart(Index);

    }

    private async void TabCarousel_SelectionChanged(object sender, Syncfusion.Maui.Core.Carousel.SelectionChangedEventArgs e)
    {
        var Carousel = (SfCarousel)sender;
        int Index = Carousel.SelectedIndex;

        _vm.SelectedIndex = Index;
        await SwitchChart(Index);
    }

    private void DeleteCategory_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void EditCategory_Tapped(object sender, TappedEventArgs e)
    {
        Categories cat = (Categories)e.Parameter;
        cat.IsEditMode = true;

        listView.RefreshItem();

    }

    private void ApplyChanges_Clicked(object sender, EventArgs e)
    {
        var Button = (Button)sender;
        Categories cat = (Categories)Button.BindingContext;
        cat.IsEditMode = false;

        listView.RefreshItem();
    }
}