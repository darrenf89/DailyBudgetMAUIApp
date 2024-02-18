using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(HeaderCatId), nameof(HeaderCatId))]
    public partial class ViewCategoryViewModel : BaseViewModel
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
        private double _tabContentWidth;
        [ObservableProperty]
        private double _maxChartContentHeight;
        [ObservableProperty]
        private string _chartTitle;
        [ObservableProperty]
        private bool _chartUpdating;
        [ObservableProperty]
        private bool _isPlaying;
        [ObservableProperty]
        private List<string> _payPeriods = new List<string>();
        [ObservableProperty]
        private int _selectedIndex = 1;
        [ObservableProperty]
        private int _headerCatId;

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
