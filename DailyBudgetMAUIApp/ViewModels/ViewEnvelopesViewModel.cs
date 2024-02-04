using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewEnvelopesViewModel : BaseViewModel
    {

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        [ObservableProperty]
        private ObservableCollection<Savings> _savings = new ObservableCollection<Savings>();
        [ObservableProperty]
        private Budgets _budget;
        [ObservableProperty]
        private decimal _envelopeBalance;
        [ObservableProperty]
        private decimal _envelopeTotal;
        [ObservableProperty]
        private decimal _regularValue;
        [ObservableProperty]
        private int _daysToPayDay;
        [ObservableProperty]
        private double _screenHeight;
        [ObservableProperty]
        private double _signOutButtonWidth;
        [ObservableProperty]
        private double _minHeight;


        public ViewEnvelopesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check Your Savings {App.UserDetails.NickName}";
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            

        }
    }
}
 