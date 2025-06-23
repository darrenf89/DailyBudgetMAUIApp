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
        public partial ObservableCollection<IncomeEvents> Incomes { get; set; } = new ObservableCollection<IncomeEvents>();

        [ObservableProperty]
        public partial Budgets Budget { get; set; }

        [ObservableProperty]
        public partial double ScreenHeight { get; set; }

        [ObservableProperty]
        public partial double SignOutButtonWidth { get; set; }

        [ObservableProperty]
        public partial decimal BalanceExtraPeriodIncome { get; set; }

        [ObservableProperty]
        public partial double MinHeight { get; set; }

        public ViewIncomesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check Your Income Events {App.UserDetails.NickName}";
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
        }
    }
}
 