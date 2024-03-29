using DailyBudgetMAUIApp.DataServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Transaction),nameof(Transaction))]
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(Bill), nameof(Bill))]
    [QueryProperty(nameof(PageType), nameof(PageType))]
    public partial class SelectCategoryPageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int  budgetID;
        [ObservableProperty]
        private Transactions transaction;
        [ObservableProperty]
        private bool  payeeDoesntExists;
        [ObservableProperty]
        private List<Categories>?  categoryList = new List<Categories>();
        [ObservableProperty]
        private List<Categories>?  subCategoryList = new List<Categories>();
        [ObservableProperty]
        private List<Categories>?  groupCategoryList = new List<Categories>();
        [ObservableProperty]
        private string  noCategoriesText = "You have not set up any Categories go ahead and do that!";
        [ObservableProperty]
        private bool  isFilterShown = false;
        [ObservableProperty]
        private double  sortFilterHeight = 326;
        [ObservableProperty]
        private Bills bill;
        [ObservableProperty]
        private string pageType;

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
