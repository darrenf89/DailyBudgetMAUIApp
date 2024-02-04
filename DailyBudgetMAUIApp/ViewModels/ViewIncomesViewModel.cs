using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewIncomesViewModel : BaseViewModel
    {

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        [ObservableProperty]
        private ObservableCollection<IncomeEvents> _incomes = new ObservableCollection<IncomeEvents>();
        [ObservableProperty]
        private Budgets _budget;
        [ObservableProperty]
        private double _screenHeight;
        [ObservableProperty]
        private double _signOutButtonWidth;
        [ObservableProperty]
        private decimal _balanceExtraPeriodIncome;
        [ObservableProperty]
        private double _minHeight;


        public ViewIncomesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check Your Income Events {App.UserDetails.NickName}";
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
        }
    }
}
 