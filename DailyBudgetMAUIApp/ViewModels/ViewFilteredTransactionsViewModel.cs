using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Filters), nameof(Filters))]
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
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
        [ObservableProperty]
        private int budgetID = 0;

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
            var FilterBudgetID = BudgetID == 0 ? App.DefaultBudgetID : BudgetID;

            Budget = await _ds.GetBudgetDetailsAsync(FilterBudgetID, "Limited");
            List<Transactions> LoadTransactions = await _ds.GetFilteredTransactions(FilterBudgetID, Filters, "ViewFilterTransactions");

            if (Filters.SavingFilter != null)
            {
                TotalSpendTypeHeader = "Total saving/s spend";
                Savings Saving = await _ds.GetSavingFromID(Filters.SavingFilter[0]);
                Title = $"{Saving.SavingsName}";
            }
            else if(Filters.CategoryFilter != null)
            {
                TotalSpendTypeHeader = "Total category/s spend";
                Categories Category = await _ds.GetCategoryFromID(Filters.CategoryFilter[0]);
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
            else
            {
                TotalSpendTypeHeader = $"Total Spend";
                Title = $"{Budget.BudgetName}";
            }

            foreach (Transactions T in LoadTransactions)
            {
                TotalSpend += T.TransactionAmount.GetValueOrDefault();
                Transactions.Add(T);
            }

        }

    }
}


