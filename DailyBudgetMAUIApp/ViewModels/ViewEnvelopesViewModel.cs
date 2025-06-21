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
        public partial ObservableCollection<Savings> Savings { get; set; } = new ObservableCollection<Savings>();

        [ObservableProperty]
        public partial Budgets Budget { get; set; }

        [ObservableProperty]
        public partial decimal EnvelopeBalance { get; set; }

        [ObservableProperty]
        public partial decimal EnvelopeTotal { get; set; }

        [ObservableProperty]
        public partial decimal RegularValue { get; set; }

        [ObservableProperty]
        public partial int DaysToPayDay { get; set; }

        [ObservableProperty]
        public partial double ScreenHeight { get; set; }

        [ObservableProperty]
        public partial double SignOutButtonWidth { get; set; }

        [ObservableProperty]
        public partial double MinHeight { get; set; }

        public ViewEnvelopesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check your envelopes {App.UserDetails.NickName}";
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            

        }
    }
}
 