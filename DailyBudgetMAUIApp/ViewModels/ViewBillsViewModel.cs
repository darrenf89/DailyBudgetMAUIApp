using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewBillsViewModel : BaseViewModel
    {

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        [ObservableProperty]
        private ObservableCollection<Bills>  bills = new ObservableCollection<Bills>();
        [ObservableProperty]
        private Budgets  budget;
        [ObservableProperty]
        private double  screenHeight;
        [ObservableProperty]
        private double  signOutButtonWidth;
        [ObservableProperty]
        private decimal  totalBills;
        [ObservableProperty]
        private decimal  billsPerPayPeriod;        
        [ObservableProperty]
        private decimal budgetAllocated;
        [ObservableProperty]
        private double  minHeight;


        public ViewBillsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check Your Outgoings {App.UserDetails.NickName}";
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
        }
    }
}
 