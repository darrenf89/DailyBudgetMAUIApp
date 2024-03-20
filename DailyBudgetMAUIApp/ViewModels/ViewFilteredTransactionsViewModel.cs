using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Filters), nameof(Filters))]
    public partial class ViewFilteredTransactionsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private FilterModel  filters;
        [ObservableProperty]
        private ObservableCollection<Transactions>  transactions = new ObservableCollection<Transactions>();
        [ObservableProperty]
        private Budgets  budget;
        [ObservableProperty]
        private double  sFListHeight;
        [ObservableProperty]
        private double  screenWidth;
        [ObservableProperty]
        private double  screenHeight;
        [ObservableProperty]
        private string  totalSpendTypeHeader;
        [ObservableProperty]
        private decimal  totalSpend;

        public ViewFilteredTransactionsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
            SFListHeight = ScreenHeight - App.NavBarHeight - App.StatusBarHeight - 37;
        }

        public async void OnAppearing()
        {
            
            if(Filters.SavingFilter != null)
            {
                TotalSpendTypeHeader = "Total saving/s spend";
                Savings Saving = _ds.GetSavingFromID(Filters.SavingFilter[0]).Result;
                Title = $"{Saving.SavingsName}";
            }
            else if(Filters.CategoryFilter != null)
            {
                TotalSpendTypeHeader = "Total category/s spend";
                Categories Category = _ds.GetCategoryFromID(Filters.CategoryFilter[0]).Result;
                Title = $"{Category.CategoryName}";
            }
            else if (Filters.TransactionEventTypeFilter != null)
            {
                TotalSpendTypeHeader = "Total event type/s spend";
                Title = $"{Filters.TransactionEventTypeFilter[0]}";
            }
            else if (Filters.PayeeFilter != null)
            {
                TotalSpendTypeHeader = "Total payee/s spend";
                Title = $"{Filters.PayeeFilter[0]}";
            }            

            Budget = _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited").Result;
            List<Transactions> LoadTransactions = _ds.GetFilteredTransactions(App.DefaultBudgetID, Filters, "ViewFilterTransactions").Result;

            foreach (Transactions T in LoadTransactions)
            {
                TotalSpend += T.TransactionAmount.GetValueOrDefault();
                Transactions.Add(T);
            }

        }

    }
}


