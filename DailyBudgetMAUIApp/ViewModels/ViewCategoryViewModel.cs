using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using System.Collections.ObjectModel;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(HeaderCatId), nameof(HeaderCatId))]
    public partial class ViewCategoryViewModel : BaseViewModel
    {
        [ObservableProperty]
        private IndexableObservableCollection<Categories>  categories = new IndexableObservableCollection<Categories>();
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
        [ObservableProperty]
        private int  headerCatId;

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        
        public ViewCategoryViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
            ChartContentWidth = ScreenWidth - 20;
            TabContentWidth = ScreenWidth - 40;
            ChartContentHeight = ScreenHeight * 0.25;
            MaxChartContentHeight = ChartContentHeight + 10;
            ChartTitle = "Current period";
            
            ChartBrushes = App.ChartBrush;           
        }
    }

}
