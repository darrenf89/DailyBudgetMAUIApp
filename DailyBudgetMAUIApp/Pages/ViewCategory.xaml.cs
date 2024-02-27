using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Carousel;



namespace DailyBudgetMAUIApp.Pages;

public partial class ViewCategory : ContentPage
{

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

    private async void DeleteCategory_Tapped(object sender, TappedEventArgs e)
    {
        Categories cat = (Categories)e.Parameter;

        bool Delete = await Application.Current.MainPage.DisplayAlert($"Delete category?", $"Are you sure you want to Delete this category?", "Yes", "No!");
        if (Delete)
        {
            Dictionary<string, int> Categories = await _ds.GetAllCategoryNames(App.DefaultBudgetID);
            string[] CategoryList = Categories.Keys.ToArray();
            var reassign = await Application.Current.MainPage.DisplayActionSheet($"Do you want to reassign this categories transactions?", "Cancel", "No", CategoryList);
            if(reassign == "Cancel")
            {

            }
            else if(reassign == "No")
            {
                await _ds.DeleteCategory(cat.CategoryID, false, 0);
            }   
            else
            {
                int ReplacementID = Categories[reassign];
                await _ds.DeleteCategory(cat.CategoryID, true, ReplacementID);
            }
            
        }
    }

    private void EditCategory_Tapped(object sender, TappedEventArgs e)
    {
        Categories cat = (Categories)e.Parameter;
        cat.IsEditMode = true;

        listView.RefreshItem();

        var Entries = listView.GetVisualTreeDescendants().Where(l => l.GetType() == typeof(BorderlessEntry));
        var EntryList = Entries.ToList();
        BorderlessEntry Entry = (BorderlessEntry)EntryList[cat.Index];
        Entry.Focus();

    }

    private void ApplyChanges_Clicked(object sender, EventArgs e)
    {
        var Button = (Button)sender;
        Categories cat = (Categories)Button.BindingContext;

        List<PatchDoc> CategoryDetails = new List<PatchDoc>();

        PatchDoc NewName = new PatchDoc
        {
            op = "replace",
            path = "/CategoryName",
            value = cat.CategoryName
        };

        CategoryDetails.Add(NewName);

        _ds.PatchCategory(cat.CategoryID, CategoryDetails);
        _ds.UpdateAllTransactionsCategoryName(cat.CategoryID);

        cat.IsEditMode = false;

        listView.RefreshItem();

        var Entries = listView.GetVisualTreeDescendants().Where(l => l.GetType() == typeof(BorderlessEntry));
        var EntryList = Entries.ToList();
        BorderlessEntry Entry = (BorderlessEntry)EntryList[cat.Index];
        Entry.IsEnabled = false;
        Entry.IsEnabled = true;
    }

    private async void ViewTransactions_Tapped(object sender, TappedEventArgs e)
    {
        var PopUp = new PopUpPage();
        App.CurrentPopUp = PopUp;
        Application.Current.MainPage.ShowPopup(PopUp);

        await Task.Delay(1000);

        var Border = (Border)sender;
        Categories Cat = (Categories)Border.BindingContext;

        FilterModel Filters = new FilterModel
        {
            CategoryFilter = new List<int>
                {
                    Cat.CategoryID
                }
        };

        await Shell.Current.GoToAsync($"{nameof(ViewFilteredTransactions)}",
            new Dictionary<string, object>
            {
                ["Filters"] = Filters
            });
    }

    private void AddSubCat_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void EditHeaderCategory_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void DeleteHeaderCat_Tapped(object sender, TappedEventArgs e)
    {
        Categories cat = (Categories)e.Parameter;

        bool Delete = await Application.Current.MainPage.DisplayAlert($"Delete category group?", $"Are you sure you want to Delete the category group?", "Yes", "No!");
        if (Delete)
        {
            Dictionary<string, int> Categories = await _ds.GetAllCategoryNames(App.DefaultBudgetID);
            string[] CategoryList = Categories.Keys.ToArray();

            var Popup = new PopupReassignCategories(new PopupReassignCategoriesViewModel(Categories, _vm.HeaderCatId, _vm.Categories.ToList(), new RestDataService()));
            var result = await Shell.Current.ShowPopupAsync(Popup);
            if (result.ToString() == "Cancel")
            {

            }
            else if (result.ToString() == "Ok")
            {
                await _ds.DeleteCategory(_vm.HeaderCatId, false, 0);
                await Shell.Current.GoToAsync($"..");
            }

        }
    }
}