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
        [ObservableProperty]
        private Budgets _budget;
        [ObserableProperty]
        private int _maxNumberOfTransactions;

        public ViewTransactionsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Title = $"Check Your Transactions {App.UserDetails.NickName}";

            Budget = _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited").Result;
            RunningTotal = Budget.BankBalance;
            MaxNumberOfTransactions = Budget.AccountInfo.NumberOfTransactions;

            Transactions = _ds.GetCurrentPayPeriodTransactions(App.DefaultBudgetID, "ViewTransactions").Result;
            CurrentOffset = Transactions.Count();
            foreach(Transactions T in Transactions)
            {
                if(!T.IsTransacted)
                {
                    T.RunningTotal = 0;
                }
                else
                {
                    T.RunningTotal = RunningTotal;
                    if(T.IsIncome)
                    {
                        RunningTotal += T.TransactionAmount;
                    }
                    else
                    {
                        RunningTotal -= T.TransactionAmount;
                    }
                }
            }
        }

        [ICommand]
        async void LoadMoreItems(object obj)
        {
            if(Transactions.Count() < MaxNumberOfTransactions)
            {
                var listView = obj as Syncfusion.Maui.ListView.SfListView;
                listView.IsLazyLoading = true;
                await Task.Delay(2500);

                List<Transactions> NewTransactions = await _ds.GetRecentTransactionsOffset(App.DefaultBudgetID, 10, CurrentOffset, "ViewTransactions");
                CurrentOffset += 100;
                foreach (Transactions T in NewTransactions)
                {
                    if(!T.IsTransacted)
                    {
                        T.RunningTotal = 0;
                    }
                    else
                    {
                        T.RunningTotal = RunningTotal;
                        if(T.IsIncome)
                        {
                            RunningTotal += T.TransactionAmount;
                        }
                        else
                        {
                            RunningTotal -= T.TransactionAmount;
                        }
                    }

                    Transactions.Add(T);
                }

                listView.IsLazyLoading = false;
            }
        }
    }
}
