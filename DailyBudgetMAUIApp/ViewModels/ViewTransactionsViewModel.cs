using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewTransactionsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private decimal  runningTotal;
        [ObservableProperty]
        private ChartClass  payPeriodTransactions;
        [ObservableProperty]
        private ObservableCollection<Transactions>  transactions = new ObservableCollection<Transactions>();
        [ObservableProperty]
        private int  currentOffset = 0;
        [ObservableProperty]
        private Budgets  budget;
        [ObservableProperty]
        private int  maxNumberOfTransactions;
        [ObservableProperty]
        private decimal  balanceAfterPending;
        [ObservableProperty]
        private double  chartContentHeight;
        [ObservableProperty]
        private double  chartContentWidth;
        [ObservableProperty]
        private double  maxChartContentHeight;
        [ObservableProperty]
        private double  sFListHeight;
        [ObservableProperty]
        private double  screenWidth;
        [ObservableProperty]
        private double  screenHeight;
        [ObservableProperty]
        private double  zeroAmount = 0;
        [ObservableProperty]
        private double  maxYValue = 0;
        [ObservableProperty]
        private double  yInterval = 0;
        [ObservableProperty]
        private string  scrollDirection;
        [ObservableProperty]
        private int  scrollCount;
        [ObservableProperty]
        private List<ChartClass>  transactionChart = new List<ChartClass>();
        [ObservableProperty]
        private List<ChartClass>  billChart = new List<ChartClass>();
        [ObservableProperty]
        private List<ChartClass>  savingsChart = new List<ChartClass>();
        [ObservableProperty]
        private List<ChartClass>  envelopeChart = new List<ChartClass>();
        [ObservableProperty]
        private List<Brush>  chartBrushes = new List<Brush>();

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
        }

        public async Task OnLoad()
        {
            Title = $"Check Your Transactions {App.UserDetails.NickName}";
            Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited");
            RunningTotal = Budget.BankBalance.GetValueOrDefault();
            BalanceAfterPending = Budget.BankBalance.GetValueOrDefault();
            MaxNumberOfTransactions = Budget.AccountInfo.NumberOfTransactions;

            List<Transactions> LoadTransactions = new List<Transactions>();
            LoadTransactions = await _ds.GetCurrentPayPeriodTransactions(App.DefaultBudgetID, "ViewTransactions");

            foreach (Transactions T in LoadTransactions)
            {
                if (!T.IsTransacted)
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
                    if (T.IsIncome)
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

            await LoadChartData(LoadTransactions);
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

        private async Task LoadChartData(List<Transactions> Transactions)
        {
            if(Transactions == null || Transactions.Count == 0)
            {
                List<Transactions> NewTransactions = await LoadMoreTransactions();
                Transactions.AddRange(NewTransactions);;
            }

            if (Transactions == null || Transactions.Count == 0)
            {

            }
            else
            {

                Transactions? EarliestTransaction = Transactions.OrderBy(t => t.TransactionDate).FirstOrDefault();
                DateTime FirstDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).AddDays(-12);

                while (EarliestTransaction.TransactionDate > FirstDate.AddDays(-1))
                {
                    List<Transactions> NewTransactions = await LoadMoreTransactions();
                    if (NewTransactions.Count() == 0)
                    {
                        break;
                    }
                    Transactions.AddRange(NewTransactions);
                    EarliestTransaction = Transactions.OrderBy(t => t.TransactionDate).FirstOrDefault();
                }

                if (EarliestTransaction != null)
                {

                    int NumberOfDays = Convert.ToInt32(Math.Ceiling((_pt.GetBudgetLocalTime(DateTime.UtcNow).Date - FirstDate).TotalDays));

                    for (int i = 0; i <= NumberOfDays; i++)
                    {
                        DateTime CurrentDate = FirstDate.AddDays(i).Date;
                        string CurrentDateString = CurrentDate.ToString("dd\\/MM");

                        decimal TransactionAmount = 0;
                        decimal BillAmount = 0;
                        decimal SavingAmount = 0;
                        decimal EnvelopeAmount = 0;

                        foreach (Transactions T in Transactions.Where(t => t.TransactionDate >= CurrentDate && t.TransactionDate < CurrentDate.AddDays(1) && !t.IsIncome && t.IsTransacted).ToList())
                        {
                            if (T.IsSpendFromSavings)
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
                            else if (T.EventType == "Bill")
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

                    List<double> MaxValues = new List<double>();
                    double Value = 0;

                    for (int i = 0; i <= TransactionChart.Count() - 1; i++)
                    {
                        Value = TransactionChart[i].YAxesDouble;
                        Value += BillChart[i].YAxesDouble;
                        Value += SavingsChart[i].YAxesDouble;
                        Value += EnvelopeChart[i].YAxesDouble;
                        MaxValues.Add(Value);
                    }

                    MaxYValue = (Math.Round(MaxValues.Max() / 10.0) * 10.0);
                    YInterval = MaxYValue / 5;
                    MaxYValue += YInterval;
                }
            }
        }

        [RelayCommand]
        async void LoadMoreItems(object obj)
        {
            try
            {
                if (Transactions.Count() < MaxNumberOfTransactions)
                {
                    var listView = obj as Syncfusion.Maui.ListView.SfListView;
                    listView.IsLazyLoading = true;
                    await Task.Delay(2000);

                    await LoadMoreTransactions();

                    listView.IsLazyLoading = false;
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewTransactions", "LoadMoreItems");
            }
        }

        private async Task<List<Transactions>> LoadMoreTransactions()
        {
            List<Transactions> NewTransactions = await _ds.GetRecentTransactionsOffset(App.DefaultBudgetID, 10, CurrentOffset, "ViewTransactions");
            CurrentOffset += 10;
            foreach (Transactions T in NewTransactions)
            {
                if (!T.IsTransacted)
                {
                    T.RunningTotal = 0;
                }
                else
                {
                    T.RunningTotal = RunningTotal;
                    if (T.IsIncome)
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

            return NewTransactions;
        }
    }
}
