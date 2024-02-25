using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Filters), nameof(Filters))]
    public partial class ViewFilteredTransactionsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private FilterModel _filters;
        [ObservableProperty]
        private ObservableCollection<Transactions> _transactions = new ObservableCollection<Transactions>();
        [ObservableProperty]
        private Budgets _budget;
        [ObservableProperty]
        private double _sFListHeight;
        [ObservableProperty]
        private double _screenWidth;
        [ObservableProperty]
        private double _screenHeight;
        [ObservableProperty]
        private string _totalSpendTypeHeader;
        [ObservableProperty]
        private decimal _totalSpend;

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


