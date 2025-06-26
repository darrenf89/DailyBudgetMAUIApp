using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using static Android.Telephony.CarrierConfigManager;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Transaction), nameof(Transaction))]
    [QueryProperty(nameof(Bill), nameof(Bill))]
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(PageType), nameof(PageType))]
    [QueryProperty(nameof(FamilyAccountID), nameof(FamilyAccountID))]
    public partial class SelectPayeePageViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        private readonly IPopupService _ps;

        [ObservableProperty]
        public partial int BudgetID { get; set; }

        [ObservableProperty]
        public partial int FamilyAccountID { get; set; }

        [ObservableProperty]
        public partial Transactions Transaction { get; set; } = new Transactions();

        [ObservableProperty]
        public partial Bills Bill { get; set; } = new Bills();

        [ObservableProperty]
        public partial bool PayeeDoesntExists { get; set; }

        [ObservableProperty]
        public partial List<string>? PayeeList { get; set; } = new List<string>();

        [ObservableProperty]
        public partial string FilteredListEmptyText { get; set; } = "You have not set up any Payee's go ahead and do that!";

        [ObservableProperty]
        public partial string SelectedPayee { get; set; }

        [ObservableProperty]
        public partial string PageType { get; set; }

        [ObservableProperty]
        public partial string PayeeName { get; set; }



        public double ScreenWidth { get; }
        public double EntryWidth { get; }
        public double EntryButtonWidth { get; }
        public double PayeeBorderWidth { get; }
        public double MinHeight { get; }

        public SelectPayeePageViewModel(IProductTools pt, IRestDataService ds, IPopupService ps)
        {
            _pt = pt;
            _ds = ds;
            _ps = ps;

            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            double ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
            EntryWidth = ScreenWidth - 40;
            EntryButtonWidth = EntryWidth - 60;
            PayeeBorderWidth = ScreenWidth - 60;
            MinHeight = ScreenHeight + 200;
        }

        [RelayCommand]
        private async Task ClosePayee(object obj)
        {
            try
            {
                await Shell.Current.GoToAsync($"..");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "SelectPayee", "ClosePayee");
            }
        }
    }
}
