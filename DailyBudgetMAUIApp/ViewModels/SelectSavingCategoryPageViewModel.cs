using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class SelectSavingCategoryPageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private Transactions _transaction;
        [ObservableProperty]
        private List<Savings>? _envelopeSavingList = new List<Savings>();
        [ObservableProperty]
        private List<Savings>? _envelopeFilteredSavingList = new List<Savings>();
        [ObservableProperty]
        private string _noEnvelopeSavingText = "You have not set up any Envelope Savings!";
        [ObservableProperty]
        private bool _isFilterShown = false;
        [ObservableProperty]
        private double _sortFilterHeight = 236.190476190476;



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
