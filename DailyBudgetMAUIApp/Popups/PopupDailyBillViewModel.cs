using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;



namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopupDailyBillViewModel : BaseViewModel, IQueryAttributable
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
        public partial Bills Bill { get; set; }

        [ObservableProperty]
        public partial bool IsAcceptOnly { get; set; }

        public PopupDailyBillViewModel()
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
            if (query.TryGetValue(nameof(Bill), out var bill) && bill is Bills b)
            {
                Bill = b;
            }

            if (query.TryGetValue(nameof(IsAcceptOnly), out var acceptOnly) && acceptOnly is bool accept)
            {
                IsAcceptOnly = accept;
            }

        }

    }
}
