using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using System.Collections.ObjectModel;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewPayeesViewModel : BaseViewModel
    {
        [ObservableProperty]
        private IndexableObservableCollection<Payees>  payees = new IndexableObservableCollection<Payees>();
        [ObservableProperty]
        private ObservableCollection<ChartClass>  payeesChart = new ObservableCollection<ChartClass>();
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
        private ObservableCollection<string>  payPeriods = new ObservableCollection<string>();
        [ObservableProperty]
        private int  selectedIndex = 1;
        [ObservableProperty]
        private int  headerCatId;
        [ObservableProperty]
        private string?  oldPayeeName;

        public ViewPayeesViewModel()
        {

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
