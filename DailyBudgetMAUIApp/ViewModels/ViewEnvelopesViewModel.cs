using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewEnvelopesViewModel : BaseViewModel
    {

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        [ObservableProperty]
        private ObservableCollection<Savings>  savings = new ObservableCollection<Savings>();
        [ObservableProperty]
        private Budgets  budget;
        [ObservableProperty]
        private decimal  envelopeBalance;
        [ObservableProperty]
        private decimal  envelopeTotal;
        [ObservableProperty]
        private decimal  regularValue;
        [ObservableProperty]
        private int  daysToPayDay;
        [ObservableProperty]
        private double  screenHeight;
        [ObservableProperty]
        private double  signOutButtonWidth;
        [ObservableProperty]
        private double  minHeight;


        public ViewEnvelopesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check your envelopes {App.UserDetails.NickName}";
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            

        }
    }
}
 