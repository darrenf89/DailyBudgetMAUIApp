using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewIncomesViewModel : BaseViewModel
    {

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        [ObservableProperty]
        private ObservableCollection<IncomeEvents>  incomes = new ObservableCollection<IncomeEvents>();
        [ObservableProperty]
        private Budgets  budget;
        [ObservableProperty]
        private double  screenHeight;
        [ObservableProperty]
        private double  signOutButtonWidth;
        [ObservableProperty]
        private decimal  balanceExtraPeriodIncome;
        [ObservableProperty]
        private double  minHeight;


        public ViewIncomesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check Your Income Events {App.UserDetails.NickName}";
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
        }
    }
}
 