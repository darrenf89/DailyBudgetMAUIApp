using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using The49.Maui.BottomSheet;

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
        private ObservableCollection<Transactions> _transactions = new ObservableCollection<Transactions>();
        [ObservableProperty]
        private int _currentOffset = 0;
        [ObservableProperty]
        private Budgets _budget;
        [ObservableProperty]
        private int _maxNumberOfTransactions;
        [ObservableProperty]
        private decimal _balanceAfterPending;
        [ObservableProperty]
        private double _chartContentHeight;
        [ObservableProperty]
        private double _maxChartContentHeight;
        [ObservableProperty]
        private double _sFListHeight;
        [ObservableProperty]
        private double _screenWidth;
        [ObservableProperty]
        private double _screenHeight;
        [ObservableProperty]
        private string _scrollDirection;
        [ObservableProperty]
        private int _scrollCount;

        public ViewTransactionsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
            ChartContentHeight = ScreenHeight * 0.25;
            MaxChartContentHeight = ChartContentHeight + 10;
            SFListHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 389;

            Title = $"Check Your Transactions {App.UserDetails.NickName}";

            Budget = _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited").Result;
            RunningTotal = Budget.BankBalance.GetValueOrDefault();
            BalanceAfterPending = Budget.BankBalance.GetValueOrDefault();
            MaxNumberOfTransactions = Budget.AccountInfo.NumberOfTransactions;

            List<Transactions> LoadTransactions = _ds.GetCurrentPayPeriodTransactions(App.DefaultBudgetID, "ViewTransactions").Result;    
            
            foreach(Transactions T in LoadTransactions)
            {
                if(!T.IsTransacted)
                {
                    T.RunningTotal = 0;
                    if (T.IsIncome)
                    {
                        BalanceAfterPending += T.TransactionAmount.GetValueOrDefault();
                    }
                    else
                    {
                        BalanceAfterPending -= T.TransactionAmount.GetValueOrDefault();
                    }
                }
                else
                {
                    T.RunningTotal = RunningTotal;
                    if(T.IsIncome)
                    {
                        RunningTotal -= T.TransactionAmount.GetValueOrDefault();
                    }
                    else
                    {
                        RunningTotal += T.TransactionAmount.GetValueOrDefault();
                    }
                }

                Transactions.Add(T);
            }

            CurrentOffset = Transactions.Count();
        }

        [ICommand]
        async void LoadMoreItems(object obj)
        {
            if(Transactions.Count() < MaxNumberOfTransactions)
            {
                var listView = obj as Syncfusion.Maui.ListView.SfListView;
                listView.IsLazyLoading = true;
                await Task.Delay(2000);

                List<Transactions> NewTransactions = await _ds.GetRecentTransactionsOffset(App.DefaultBudgetID, 10, CurrentOffset, "ViewTransactions");
                CurrentOffset += 10;
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
                            RunningTotal -= T.TransactionAmount.GetValueOrDefault();
                        }
                        else
                        {
                            RunningTotal += T.TransactionAmount.GetValueOrDefault();
                        }

                        Transactions.Add(T);
                    }                    
                }

                listView.IsLazyLoading = false;
            }
        }
    }
}
