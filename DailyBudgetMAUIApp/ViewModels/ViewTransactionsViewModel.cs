using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewTransactionsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private decimal _runningTotal;
        [ObservableProperty]
        private ChartClass _payPeriodTransactions;
        [ObservableProperty]
        private List<Transactions> _transactions;
        [ObservableProperty]
        private int _currentOffset = 0;

        public ViewTransactionsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check Your Transactions {App.UserDetails.NickName}";

            Transactions = _ds.GetRecentTransactionsOffset(App.DefaultBudgetID, 100, CurrentOffset, "ViewTransactions").Result;
        }

        [ICommand]
        async void LoadMoreItems(object obj)
        {
            CurrentOffset += 1;

            var listView = obj as Syncfusion.Maui.ListView.SfListView;
            listView.IsLazyLoading = true;
            await Task.Delay(2500);

            List<Transactions> NewTransactions = await _ds.GetRecentTransactionsOffset(App.DefaultBudgetID, 100, CurrentOffset, "ViewTransactions");

            foreach (Transactions T in NewTransactions)
            {
                Transactions.Add(T);
            }

            listView.IsLazyLoading = false;
        }
    }
}
