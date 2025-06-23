using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Carousel;
using The49.Maui.BottomSheet;


namespace DailyBudgetMAUIApp.Pages;

public partial class ViewCategories : BasePage
{
    public List<Categories> _addCategoryList = new List<Categories>();
    public List<Categories> AddCategoryList
    {
        get => _addCategoryList;
        set
        {
            if (_addCategoryList != value)
            {
                try
                {
                    _addCategoryList = value;
                    _vm.Categories.Clear();
                    foreach (Categories c in AddCategoryList)
                    {
                        _vm.Categories.Add(c);
                    }
                }
                catch (Exception ex)
                {
                    _pt.HandleException(ex, "ViewCategories", "_addCategoryList");
                }
            }
        }
    }

    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewCategoriesViewModel _vm;
    private readonly IDispatcherTimer _timer;
    private readonly IPopupService _ps;

    public ViewCategories(ViewCategoriesViewModel viewModel, IProductTools pt, IRestDataService ds, IPopupService ps)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;
        _ps = ps;

        InitializeComponent();

        var timer = Application.Current.Dispatcher.CreateTimer();
        _timer = timer;
        timer.Interval = TimeSpan.FromSeconds(45);
        timer.Tick += async (s, e) =>
        {
            try
            {
                _vm.ChartUpdating = true;
                await CycleThroughChart();
                _vm.ChartUpdating = false;
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewCategories", "timer.Tick");
            }
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
        try
        {
            _vm.IsPageBusy = true;
            base.OnAppearing();

            await _vm.OnLoad();

            var size = _vm.ScreenWidth / 170;
            if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
            _vm.IsPageBusy = false;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewCategories", "OnAppearing");
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
        try
        {
            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewCategories", "HomeButton_Clicked");
        }
    }

    private async void ListViewTapped_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
            Categories Category = (Categories)e.Parameter;

            if(Category.CategoryID == -1)
            {
                AddNewCategoryBottomSheet page = new AddNewCategoryBottomSheet(_vm.Categories, _pt, _ds);

                page.Detents = new DetentsCollection()
                {
                    new FullscreenDetent()
                };

                page.HasBackdrop = true;
                page.CornerRadius = 0;

                App.CurrentBottomSheet = page;

                if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
                await page.ShowAsync();
            }
            else
            {
                await Task.Delay(1000);

                await Shell.Current.GoToAsync($"///{nameof(ViewCategories)}/{nameof(ViewCategory)}?HeaderCatId={Category.CategoryID}");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewCategories", "ListViewTapped_Tapped");
        }
    }

    private async void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {

        try
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
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewCategories", "SwipeGestureRecognizer_Swiped");
        }
    }

    private void btnPlayPause_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.IsPlaying)
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
        catch (Exception ex)
        {
             _pt.HandleException(ex, "ViewCategories", "btnPlayPause_Clicked");
        }
    }

    private async void TabCarousel_SwipeEnded(object sender, EventArgs e)
    {
        try
        {
            var Carousel = (SfCarousel)sender;
            int Index = Carousel.SelectedIndex;

            _vm.SelectedIndex = Index;
            await SwitchChart(Index);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewCategories", "TabCarousel_SwipeEnded");
        }


    }

    private async void TabCarousel_SelectionChanged(object sender, Syncfusion.Maui.Core.Carousel.SelectionChangedEventArgs e)
    {
        try
        {
            var Carousel = (SfCarousel)sender;
            int Index = Carousel.SelectedIndex;

            _vm.SelectedIndex = Index;
            await SwitchChart(Index);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewCategories", "TabCarousel_SelectionChanged");
        }
    }
}