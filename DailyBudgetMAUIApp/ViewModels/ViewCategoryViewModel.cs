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
        public partial IndexableObservableCollection<Categories> Categories { get; set; } = new IndexableObservableCollection<Categories>();

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

        [ObservableProperty]
        public partial int HeaderCatId { get; set; }


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
