using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Carousel;
using Syncfusion.Maui.DataSource.Extensions;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewPayees : ContentPage
{
    public Payees _addPayee = new Payees();
    public Payees AddPayee
    {
        get => _addPayee;
        set
        {
            if (_addPayee != value)
            {
                _addPayee = value;
                LoadData();
            }
        }
    }

    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewPayeesViewModel _vm;
    private readonly IDispatcherTimer _timer;
    public ViewPayees(ViewPayeesViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;

        InitializeComponent();

        var timer = Application.Current.Dispatcher.CreateTimer();
        _timer = timer;
        timer.Interval = TimeSpan.FromSeconds(45);
        timer.Tick += async (s, e) => 
        {
            _vm.ChartUpdating = true;
            await CycleThroughChart();
            _vm.ChartUpdating = false;
        };
        timer.Start();
        _vm.IsPlaying = true;

    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        _timer.Stop();
    }
 
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await LoadData();
    }
    private async Task LoadData()
    {
        _vm.Payees.Clear();
        _vm.PayeesChart.Clear();

        List<Payees> PayeeList = await _ds.GetPayeeListFull( App.DefaultBudgetID);
        PayeeList = PayeeList.OrderByDescending(p => p.PayeeSpendPayPeriod).ToList();

        _vm.Title = $"Payee Details";

        foreach (Payees payee in PayeeList)
        {
            _vm.Payees.Add(payee);

            if(_vm.PayeesChart.Count() < 8)
            {
                ChartClass Value = new ChartClass
                {
                    XAxesString = payee.Payee,
                    YAxesDouble = (double)payee.PayeeSpendPayPeriod
                };

                _vm.PayeesChart.Add(Value);
            }
            
        }

        _vm.PayPeriods.Add("All Time");
        foreach (SpendPeriods SP in _vm.Payees[0].PayeeSpendPeriods)
        {
            if (SP.IsCurrentPeriod)
            {
                _vm.PayPeriods.Add("Current Period");
            }
            else
            {
                _vm.PayPeriods.Add($"{SP.FromDate: dd MMM} to {SP.ToDate: dd MMM}");
            }
        }

        listView.RefreshView();
        listView.RefreshItem();

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private async Task SwitchChart(int Index)
    {
        _vm.ChartUpdating = true;

        _vm.ChartTitle = _vm.PayPeriods[Index];

        if (Index == 0)
        {
            _vm.PayeesChart.Clear();

            foreach (Payees payee in _vm.Payees)
            {
                payee.PayeeSpendPayPeriod = payee.PayeeSpendPeriods[Index].SpendTotalAmount;                
            }

            List<Payees> PayeeList = _vm.Payees.OrderByDescending(p => p.PayeeSpendAllTime).ToList();
            _vm.Payees.Clear();
            
            foreach (Payees payee in PayeeList)
            {
                if (_vm.PayeesChart.Count() < 8)
                {
                    ChartClass Value = new ChartClass
                    {
                        XAxesString = payee.Payee,
                        YAxesDouble = (double)payee.PayeeSpendAllTime
                    };
                   
                    _vm.PayeesChart.Add(Value);
                }

                _vm.Payees.Add(payee);
            }
        }
        else
        {
            _vm.PayeesChart.Clear();

            foreach (Payees payee in _vm.Payees)
            {
                payee.PayeeSpendPayPeriod = payee.PayeeSpendPeriods[Index - 1].SpendTotalAmount;
            }

            List<Payees> PayeeList = _vm.Payees.OrderByDescending(p => p.PayeeSpendPayPeriod).ToList();
            _vm.Payees.Clear();

            foreach (Payees payee in PayeeList)
            {
                if (_vm.PayeesChart.Count() < 8)
                {
                    ChartClass Value = new ChartClass
                    {
                        XAxesString = payee.Payee,
                        YAxesDouble = (double)payee.PayeeSpendPeriods[Index - 1].SpendTotalAmount
                    };

                    _vm.PayeesChart.Add(Value);
                }
                _vm.Payees.Add(payee);
            }
        }

        await Task.Delay(1000);
        _timer.Stop();
        _timer.Start();

        _vm.ChartUpdating = false;

    }

    private async Task CycleThroughChart()
    {
        MainThread.BeginInvokeOnMainThread(async () => 
        {
            if (_vm.SelectedIndex == (_vm.PayPeriods.Count() - 1))
            {
                _vm.SelectedIndex = 0;                
            }
            else if(_vm.SelectedIndex == 0)
            {
                _vm.SelectedIndex = _vm.PayPeriods.Count() - 1;
            }
            else
            {
                _vm.SelectedIndex += 1;
            }
            await SwitchChart(_vm.SelectedIndex);
        });
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
    }

    private async void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
    {
        switch (e.Direction)
        {
            case SwipeDirection.Left:
                if(_vm.SelectedIndex < (_vm.PayPeriods.Count()-1))
                {
                    _vm.SelectedIndex += 1;
                    await SwitchChart(_vm.SelectedIndex);
                }
                break;
            case SwipeDirection.Right:
                if (_vm.SelectedIndex > 0)
                {
                    _vm.SelectedIndex -= 1;
                    await SwitchChart(_vm.SelectedIndex);
                }
                break;
        }       
    }

    private void btnPlayPause_Clicked(object sender, EventArgs e)
    {
        if(_vm.IsPlaying)
        {
            _vm.IsPlaying = false;
            _timer.Stop();
        }
        else
        {
            _vm.IsPlaying = true;
            _timer.Start();
        }
    }

    private async void TabCarousel_SwipeEnded(object sender, EventArgs e)
    {
        var Carousel = (SfCarousel)sender;
        int Index = Carousel.SelectedIndex;

        _vm.SelectedIndex = Index;
        await SwitchChart(Index);

    }

    private async void TabCarousel_SelectionChanged(object sender, Syncfusion.Maui.Core.Carousel.SelectionChangedEventArgs e)
    {
        var Carousel = (SfCarousel)sender;
        int Index = Carousel.SelectedIndex;

        _vm.SelectedIndex = Index;
        await SwitchChart(Index);
    }

    private async void DeleteCategory_Tapped(object sender, TappedEventArgs e)
    {
        Payees payee = (Payees)e.Parameter;

        bool Delete = await Application.Current.MainPage.DisplayAlert($"Delete payee?", $"Are you sure you want to Delete {payee.Payee}?", "Yes", "No!");
        if (Delete)
        {
            List<string> Payees = await _ds.GetPayeeList(App.DefaultBudgetID);
            string[] PayeeList = Payees.ToArray();
            var reassign = await Application.Current.MainPage.DisplayActionSheet($"Do you want to reassign this payees transactions?", "Cancel", "No", PayeeList);
            if(reassign == "Cancel")
            {

            }
            else if(reassign == "No")
            {
                await _ds.DeletePayee(App.DefaultBudgetID, payee.Payee, "");

                int index = _vm.Payees.IndexOf(payee);

                _vm.Payees.RemoveAt(index);
                _vm.PayeesChart.RemoveAt(index);
            }   
            else
            {
                await _ds.DeletePayee(App.DefaultBudgetID, payee.Payee, reassign);

                int index = _vm.Payees.IndexOf(payee);

                _vm.Payees.RemoveAt(index);
                _vm.PayeesChart.RemoveAt(index);
            }

            listView.RefreshView();
            listView.RefreshItem();
        }
    }

    private void EditCategory_Tapped(object sender, TappedEventArgs e)
    {
        if (!_vm.Payees.Where(p => p.IsEditMode).Any())
        {        
            Payees payee = (Payees)e.Parameter;
            payee.IsEditMode = true;

            listView.RefreshItem();

            _vm.OldPayeeName = payee.Payee;

            var Entries = listView.GetVisualTreeDescendants().Where(l => l.GetType() == typeof(BorderlessEntry));
            var EntryList = Entries.ToList();
            foreach(BorderlessEntry ent in EntryList)
            {
                if(ent.Text == payee.Payee)
                {
                    ent.Focus();
                    return;
                }
            }
        }
    }

    private void ApplyChanges_Clicked(object sender, EventArgs e)
    {
        var Button = (Button)sender;
        Payees payee = (Payees)Button.BindingContext;

        _ds.UpdatePayee(App.DefaultBudgetID, _vm.OldPayeeName, payee.Payee);

        payee.IsEditMode = false;

        listView.RefreshItem();

        var Entries = listView.GetVisualTreeDescendants().Where(l => l.GetType() == typeof(BorderlessEntry));
        var EntryList = Entries.ToList();
        foreach (BorderlessEntry ent in EntryList)
        {
            if (ent.Text == payee.Payee)
            {
                ent.IsEnabled = false;
                ent.IsEnabled = true;
                return;
            }
        }
    }

    private async void ViewTransactions_Tapped(object sender, TappedEventArgs e)
    {
        var Border = (Border)sender;
        Payees payee = (Payees)Border.BindingContext;

        if (!payee.IsEditMode)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);

            await Task.Delay(1000);

            FilterModel Filters = new FilterModel
            {
                PayeeFilter = new List<string>
                {
                    payee.Payee
                }
            };

            await Shell.Current.GoToAsync($"/{nameof(ViewFilteredTransactions)}",
                new Dictionary<string, object>
                {
                    ["Filters"] = Filters
                });
        }
    }
}