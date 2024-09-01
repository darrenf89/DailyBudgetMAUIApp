using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Handlers;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Transaction), nameof(Transaction))]
    [QueryProperty(nameof(Bill), nameof(Bill))]
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(PageType), nameof(PageType))]
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
        [ObservableProperty]
        private string payeeName;


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

        [RelayCommand]
        private async void ClosePayee(object obj)
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Task.Delay(500);

                await Shell.Current.GoToAsync($"..");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "SelectPayee", "ClosePayee");
            }
        }
    }
}
