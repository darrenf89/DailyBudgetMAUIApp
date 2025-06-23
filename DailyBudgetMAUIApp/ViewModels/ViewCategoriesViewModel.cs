using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewCategoriesViewModel : BaseViewModel
    {
        [ObservableProperty]
        public partial ObservableCollection<Categories> Categories { get; set; } = new ObservableCollection<Categories>();

        [ObservableProperty]
        public partial ObservableCollection<ChartClass> CategoriesChart { get; set; } = new ObservableCollection<ChartClass>();

        [ObservableProperty]
        public partial double ScreenWidth { get; set; }

        [ObservableProperty]
        public partial double ScreenHeight { get; set; }

        [ObservableProperty]
        public partial List<Brush> ChartBrushes { get; set; } = new List<Brush>();

        [ObservableProperty]
        public partial double ChartContentHeight { get; set; }

        [ObservableProperty]
        public partial double ChartContentWidth { get; set; }

        [ObservableProperty]
        public partial double TabContentWidth { get; set; }

        [ObservableProperty]
        public partial double MaxChartContentHeight { get; set; }

        [ObservableProperty]
        public partial string ChartTitle { get; set; }

        [ObservableProperty]
        public partial bool ChartUpdating { get; set; }

        [ObservableProperty]
        public partial bool IsPlaying { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<string> PayPeriods { get; set; } = new ObservableCollection<string>();

        [ObservableProperty]
        public partial int SelectedIndex { get; set; } = 1;


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

            List<Categories> CategoryList = await _ds.GetAllHeaderCategoryDetailsFull(App.DefaultBudgetID);

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
