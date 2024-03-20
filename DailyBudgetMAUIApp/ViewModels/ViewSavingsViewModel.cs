using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewSavingsViewModel : BaseViewModel
    {

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        [ObservableProperty]
        private ObservableCollection<Savings>  savings = new ObservableCollection<Savings>();
        [ObservableProperty]
        private Budgets  budget;
        [ObservableProperty]
        private decimal  totalSavings;
        [ObservableProperty]
        private decimal  payDaySavings;
        [ObservableProperty]
        private double  screenHeight;
        [ObservableProperty]
        private double  signOutButtonWidth;
        [ObservableProperty]
        private double  minHeight;


        public ViewSavingsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check your savings {App.UserDetails.NickName}";
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            

        }
    }
}
 