using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewCategoriesViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Categories> _categories = new ObservableCollection<Categories>();
        [ObservableProperty]
        private ObservableCollection<ChartClass> _categoriesChart = new ObservableCollection<ChartClass>();
        [ObservableProperty]
        private double _screenWidth;
        [ObservableProperty]
        private double _screenHeight;
        [ObservableProperty]
        private List<Brush> _chartBrushes = new List<Brush>();
        [ObservableProperty]
        private double _chartContentHeight;
        [ObservableProperty]
        private double _chartContentWidth;
        [ObservableProperty]
        private double _maxChartContentHeight;
        [ObservableProperty]
        private string _chartTitle;
        [ObservableProperty]
        private string _currentChart = "PayPeriod";
        [ObservableProperty]
        private bool _chartUpdating;
        [ObservableProperty]
        private bool _isPlaying;

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        
        public ViewCategoriesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
            ChartContentWidth = ScreenWidth - 20;
            ChartContentHeight = ScreenHeight * 0.25;
            MaxChartContentHeight = ChartContentHeight + 10;
            ChartTitle = "Spend per category this period";

            Title = $"Check Your Categories {App.UserDetails.NickName}";
            ChartBrushes = App.ChartBrush;

            Categories.Clear();
            CategoriesChart.Clear();

            List<Categories> CategoryList = _ds.GetAllHeaderCategoryDetailsFull(App.DefaultBudgetID).Result;

            foreach (Categories cat in CategoryList)
            {
                Categories.Add(cat);

                ChartClass Value = new ChartClass
                {
                    XAxesString = cat.CategoryName,
                    YAxesDouble = (double)cat.CategorySpendPayPeriod
                };

                CategoriesChart.Add(Value);
            }

            Categories AddCat = new Categories
            {
                CategoryName = "Add new category",
                CategoryID = -1,
                CategoryIcon = "Add"
            };

            Categories.Add(AddCat);
        }
    }

}
