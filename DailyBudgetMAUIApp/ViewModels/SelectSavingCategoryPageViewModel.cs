using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID),nameof(BudgetID))]
    [QueryProperty(nameof(Transaction), nameof(Transaction))]
    public partial class SelectSavingCategoryPageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        public partial int BudgetID { get; set; }

        [ObservableProperty]
        public partial Transactions Transaction { get; set; }

        [ObservableProperty]
        public partial List<Savings>? EnvelopeSavingList { get; set; } = new List<Savings>();

        [ObservableProperty]
        public partial List<Savings>? EnvelopeFilteredSavingList { get; set; } = new List<Savings>();

        [ObservableProperty]
        public partial string NoEnvelopeSavingText { get; set; } = "You have not set up any Envelope Bills!";

        [ObservableProperty]
        public partial bool IsFilterShown { get; set; } = false;

        [ObservableProperty]
        public partial double SortFilterHeight { get; set; } = 236.190476190476;




        public double ScreenWidth { get; }
        public double EntryWidth { get; }
        public double EntryButtonWidth { get; }
        public double PayeeBorderWidth { get; }
        public double MinHeight { get; }

        public SelectSavingCategoryPageViewModel(IProductTools pt, IRestDataService ds)
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
