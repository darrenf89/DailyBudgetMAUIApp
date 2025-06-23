using DailyBudgetMAUIApp.DataServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Transaction),nameof(Transaction))]
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(Bill), nameof(Bill))]
    [QueryProperty(nameof(PageType), nameof(PageType))]
    [QueryProperty(nameof(FamilyAccountID), nameof(FamilyAccountID))]
    public partial class SelectCategoryPageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        public partial int BudgetID { get; set; }

        [ObservableProperty]
        public partial int FamilyAccountID { get; set; }

        [ObservableProperty]
        public partial Transactions Transaction { get; set; }

        [ObservableProperty]
        public partial bool PayeeDoesntExists { get; set; }

        [ObservableProperty]
        public partial List<Categories>? CategoryList { get; set; } = new List<Categories>();

        [ObservableProperty]
        public partial List<Categories>? SubCategoryList { get; set; } = new List<Categories>();

        [ObservableProperty]
        public partial List<Categories>? GroupCategoryList { get; set; } = new List<Categories>();

        [ObservableProperty]
        public partial string NoCategoriesText { get; set; } = "You have not set up any Categories go ahead and do that!";

        [ObservableProperty]
        public partial bool IsFilterShown { get; set; } = false;

        [ObservableProperty]
        public partial double SortFilterHeight { get; set; } = 326;

        [ObservableProperty]
        public partial Bills Bill { get; set; }

        [ObservableProperty]
        public partial string PageType { get; set; }


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
