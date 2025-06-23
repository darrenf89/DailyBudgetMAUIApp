using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopupDailyTransactionViewModel : BaseViewModel, IQueryAttributable
    {
        public double ScreenWidth { get; }
        public double ScreenHeight { get; }
        public double PopupWidth { get; }
        public double EntryWidth { get; }
        public double ButtonOneWidth { get; }
        public double ButtonTwoWidth { get; }
        public double ButtonThreeWidth { get; }

        public decimal OriginalAmount { get; set; }
        public DateTime OriginalDate { get; set; }

        [ObservableProperty]
        public partial Transactions Transaction { get; set; }

        public PopupDailyTransactionViewModel()
        {
            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 30;
            EntryWidth = PopupWidth * 0.6;
            ButtonOneWidth = ((PopupWidth - 60) / 2);
            ButtonTwoWidth = ((PopupWidth - 140) / 3);
            ButtonThreeWidth = ((PopupWidth - 260) / 2);

        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(nameof(Transaction), out var transaction) && transaction is Transactions t)
            {
                Transaction = t;
            }

        }
    }
}
