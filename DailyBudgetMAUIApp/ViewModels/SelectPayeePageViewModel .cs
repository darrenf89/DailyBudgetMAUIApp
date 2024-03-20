using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class SelectPayeePageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int  budgetID;
        [ObservableProperty]
        private Transactions  transaction = new Transactions();
        [ObservableProperty]
        private Bills  bill = new Bills();
        [ObservableProperty]
        private bool  payeeDoesntExists;
        [ObservableProperty]
        private List<string>?  payeeList = new List<string>();
        [ObservableProperty]
        private string  filteredListEmptyText = "You have not set up any Payee's go ahead and do that!";
        [ObservableProperty]
        private string  selectedPayee;
        [ObservableProperty]
        private string  pageType;


        public double ScreenWidth { get; }
        public double EntryWidth { get; }
        public double EntryButtonWidth { get; }
        public double PayeeBorderWidth { get; }
        public double MinHeight { get; }

        public SelectPayeePageViewModel(IProductTools pt, IRestDataService ds)
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
