using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

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
        private double _chartContentWidth;
        [ObservableProperty]
        private double _maxChartContentHeight;
        [ObservableProperty]
        private double _sFListHeight;
        [ObservableProperty]
        private double _screenWidth;
        [ObservableProperty]
        private double _screenHeight;
        [ObservableProperty]
        private double _zeroAmount = 0;
        [ObservableProperty]
        private double _maxYValue = 0;
        [ObservableProperty]
        private double _yInterval = 0;
        [ObservableProperty]
        private string _scrollDirection;
        [ObservableProperty]
        private int _scrollCount;
        [ObservableProperty]
        private List<ChartClass> _transactionChart = new List<ChartClass>();
        [ObservableProperty]
        private List<ChartClass> _billChart = new List<ChartClass>();
        [ObservableProperty]
        private List<ChartClass> _savingsChart = new List<ChartClass>();
        [ObservableProperty]
        private List<ChartClass> _envelopeChart = new List<ChartClass>();
        [ObservableProperty]
        private List<Brush> _chartBrushes = new List<Brush>();

        public ViewTransactionsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density);
            ScreenWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density);
            ChartContentWidth = ScreenWidth - 20;
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

            LoadChartData(LoadTransactions);
            LoadChartBrushed();

        }

        private void LoadChartBrushed()
        {
            Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
            Application.Current.Resources.TryGetValue("PrimaryDarkBrush", out var PrimaryDarkBrush);
            Application.Current.Resources.TryGetValue("PrimaryLightBrush", out var PrimaryLightBrush);
            Application.Current.Resources.TryGetValue("PrimaryLightLightBrush", out var PrimaryLightLightBrush);


            ChartBrushes = App.ChartBrush;
        }

        private void LoadChartData(List<Transactions> Transactions)
        {

            Transactions? EarliestTransaction = Transactions.OrderBy(t => t.TransactionDate).FirstOrDefault();
            if(EarliestTransaction != null)
            {
                //DateTime FirstDate = EarliestTransaction.TransactionDate.GetValueOrDefault().Date;
                DateTime FirstDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).AddDays(-12);
                int NumberOfDays = Convert.ToInt32(Math.Ceiling((_pt.GetBudgetLocalTime(DateTime.UtcNow).Date - FirstDate).TotalDays));

                for (int i = 0; i <= NumberOfDays; i++) 
                {
                    DateTime CurrentDate = FirstDate.AddDays(i).Date;
                    string CurrentDateString = CurrentDate.ToString("dd\\/MM");

                    decimal TransactionAmount = 0;
                    decimal BillAmount = 0;
                    decimal SavingAmount = 0;
                    decimal EnvelopeAmount = 0;

                    foreach(Transactions T in Transactions.Where(t => t.TransactionDate >= CurrentDate && t.TransactionDate < CurrentDate.AddDays(1) && !t.IsIncome && t.IsTransacted).ToList())
                    {
                        if(T.IsSpendFromSavings)
                        {
                            if (T.SavingsSpendType == "EnvelopeSaving")
                            {
                                EnvelopeAmount += T.TransactionAmount.GetValueOrDefault();
                            }
                            else
                            {
                                SavingAmount += T.TransactionAmount.GetValueOrDefault();
                            }
                        }
                        else if(T.EventType == "Bill")
                        {
                            BillAmount += T.TransactionAmount.GetValueOrDefault();
                        }
                        else
                        {
                            TransactionAmount += T.TransactionAmount.GetValueOrDefault();
                        }
                    }

                    TransactionChart.Add(new ChartClass
                    {
                        XAxesString = CurrentDateString,
                        YAxesDouble = Convert.ToInt32(TransactionAmount)
                    });
                    BillChart.Add(new ChartClass
                    {
                        XAxesString = CurrentDateString,
                        YAxesDouble = Convert.ToInt32(BillAmount)
                    });
                    SavingsChart.Add(new ChartClass
                    {
                        XAxesString = CurrentDateString,
                        YAxesDouble = Convert.ToInt32(SavingAmount)
                    });
                    EnvelopeChart.Add(new ChartClass
                    {
                        XAxesString = CurrentDateString,
                        YAxesDouble = Convert.ToInt32(EnvelopeAmount)
                    });
                }

                MaxYValue += TransactionChart.Max(t => t.YAxesDouble);
                MaxYValue += BillChart.Max(t => t.YAxesDouble);
                MaxYValue += SavingsChart.Max(t => t.YAxesDouble);
                MaxYValue += EnvelopeChart.Max(t => t.YAxesDouble);
                MaxYValue = (Math.Round(MaxYValue / 10.0)) * 10;

                YInterval = MaxYValue / 5;
            }
            
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
