using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopupDailyAllowanceViewModel : BaseViewModel, IQueryAttributable
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
        public partial Budgets Budget { get; set; }

        [ObservableProperty]
        public partial string NickName { get; set; }

        [ObservableProperty]
        public partial int UserID { get; set; }

        [ObservableProperty]
        public partial string Type { get; set; }

        [ObservableProperty]
        public partial string TextOne { get; set; }

        [ObservableProperty]
        public partial string TextTwo { get; set; }


        public PopupDailyAllowanceViewModel()
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
            if (query.TryGetValue("Budget", out var budget) && budget is Budgets b)
            {
                Budget = b;
            }
            if (query.TryGetValue("NickName", out var nickName) && nickName is string n)
            {
                NickName = n;
            }
            if (query.TryGetValue("UserID", out var userId) && userId is int id)
            {
                UserID = id;
            }
            if (query.TryGetValue("Type", out var type) && type is string t)
            {
                Type = t;
            }
        }
    }
}
