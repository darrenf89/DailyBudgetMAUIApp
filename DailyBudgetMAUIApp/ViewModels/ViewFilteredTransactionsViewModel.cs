using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
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
        private List<Transactions>  loadedTransactions = new List<Transactions>();
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
        [ObservableProperty]
        private int currentOffset = 0;
        [ObservableProperty]
        private int maxNumberOfTransactions;

        public ViewFilteredTransactionsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
            SFListHeight = ScreenHeight - App.NavBarHeight - App.StatusBarHeight - 113;
        }

        public async void OnAppearing()
        {
            CurrentOffset = 0;
            var FilterBudgetID = BudgetID == 0 ? App.DefaultBudgetID : BudgetID;

            Budget = await _ds.GetBudgetDetailsAsync(FilterBudgetID, "Limited");
            LoadedTransactions = await _ds.GetFilteredTransactions(FilterBudgetID, Filters, "ViewFilterTransactions");
            LoadedTransactions = LoadedTransactions.OrderByDescending(t => t.TransactionDate).ToList();
            MaxNumberOfTransactions = LoadedTransactions.Count;

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

            foreach (Transactions T in LoadedTransactions)
            {
                TotalSpend += T.TransactionAmount.GetValueOrDefault();
            }

            var NextStep = CurrentOffset + 5 > MaxNumberOfTransactions ? MaxNumberOfTransactions : CurrentOffset + 5;
            for (int i = CurrentOffset; i <= NextStep - 1; i ++)
            {
                Transactions.Add(LoadedTransactions[i]);
            }
            CurrentOffset = NextStep;          
        }

        [RelayCommand]
        async Task LoadMoreItems(object obj)
        {
            try
            {
                if (Transactions.Count() < MaxNumberOfTransactions)
                {
                    var listView = obj as Syncfusion.Maui.ListView.SfListView;
                    listView.IsLazyLoading = true;                    

                    await LoadMoreTransactions();

                    listView.IsLazyLoading = false;
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewFilteredTransactions", "LoadMoreItems");
            }
        }

        private async Task LoadMoreTransactions()
        {
            await Task.Delay(1000);
            var NextStep = CurrentOffset + 5 > MaxNumberOfTransactions ? MaxNumberOfTransactions : CurrentOffset + 5;
            for (int i = CurrentOffset; i <= NextStep - 1; i++)
            {
                Transactions.Add(LoadedTransactions[i]);
            }
            CurrentOffset = NextStep;
        }
    }
}


