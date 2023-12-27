using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class SelectCategoryPageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private Transactions _transaction;
        [ObservableProperty]
        private bool _payeeDoesntExists;
        [ObservableProperty]
        private List<Categories>? _categoryList = new List<Categories>();
        [ObservableProperty]
        private List<Categories>? _subCategoryList = new List<Categories>();
        [ObservableProperty]
        private List<Categories>? _groupCategoryList = new List<Categories>();
        [ObservableProperty]
        private string _noCategoriesText = "You have not set up any Categories go ahead and do that!";
        [ObservableProperty]
        private bool _isFilterShown = false;
        [ObservableProperty]
        private double _sortFilterHeight = 326;



        public double ScreenWidth { get; }
        public double EntryWidth { get; }
        public double EntryButtonWidth { get; }
        public double PayeeBorderWidth { get; }
        public double MinHeight { get; }

        public SelectCategoryPageViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            double ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            EntryWidth = ScreenWidth - 40;
            EntryButtonWidth = EntryWidth - 60;
            PayeeBorderWidth = ScreenWidth - 60;
            MinHeight = ScreenHeight + 200;
        }

    }
}
