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
        public partial IndexableObservableCollection<Payees> Payees { get; set; } = new IndexableObservableCollection<Payees>();

        [ObservableProperty]
        public partial ObservableCollection<ChartClass> PayeesChart { get; set; } = new ObservableCollection<ChartClass>();

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

        [ObservableProperty]
        public partial int HeaderCatId { get; set; }

        [ObservableProperty]
        public partial string? OldPayeeName { get; set; }


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
