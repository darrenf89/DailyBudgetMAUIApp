using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewCategoriesViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Categories>  categories = new ObservableCollection<Categories>();
        [ObservableProperty]
        private ObservableCollection<ChartClass>  categoriesChart = new ObservableCollection<ChartClass>();
        [ObservableProperty]
        private double  screenWidth;
        [ObservableProperty]
        private double  screenHeight;
        [ObservableProperty]
        private List<Brush>  chartBrushes = new List<Brush>();
        [ObservableProperty]
        private double  chartContentHeight;
        [ObservableProperty]
        private double  chartContentWidth;
        [ObservableProperty]
        private double  tabContentWidth;
        [ObservableProperty]
        private double  maxChartContentHeight;
        [ObservableProperty]
        private string  chartTitle;
        [ObservableProperty]
        private bool  chartUpdating;
        [ObservableProperty]
        private bool  isPlaying;
        [ObservableProperty]
        private List<string>  payPeriods = new List<string>();
        [ObservableProperty]
        private int  selectedIndex = 1;

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        
        public ViewCategoriesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
            ChartContentWidth = ScreenWidth - 20;
            TabContentWidth = ScreenWidth - 40;
            ChartContentHeight = ScreenHeight * 0.25;
            MaxChartContentHeight = ChartContentHeight + 10;

        }

        public async Task OnLoad()
        {
            ChartTitle = "Current period";

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

            PayPeriods.Add("All Time");
            foreach (SpendPeriods SP in Categories[0].CategorySpendPeriods)
            {
                if (SP.IsCurrentPeriod)
                {
                    PayPeriods.Add("Current Period");
                }
                else
                {
                    PayPeriods.Add($"{SP.FromDate: dd MMM} to {SP.ToDate: dd MMM}");
                }
            }
        }
    }

}
