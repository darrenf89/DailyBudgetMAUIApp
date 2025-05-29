using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.DataSource;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView.Helpers;
using System.Globalization;
using The49.Maui.BottomSheet;


namespace DailyBudgetMAUIApp.Pages;

public partial class ViewTransactions : BasePage
{
    private readonly ViewTransactionsViewModel _vm;
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;
    private Transactions tappedItem;
    private double CurrentScrollY;

    public FilterModel _filters;
    public FilterModel Filters
    {
        get => _filters;
        set
        {
            if(_filters != value)
            {
                try
                {
                    _filters = value;
                    listView.DataSource.Filter = FilterContacts;
                    listView.DataSource.RefreshFilter();
                }
                catch (Exception ex)
                {
                    _pt.HandleException(ex, "ViewTransactions", "FilterModel");
                }
            }
        }
    }

    public ViewTransactions(ViewTransactionsViewModel viewModel, IRestDataService ds, IProductTools pt)
	{
		InitializeComponent();

        _ds = ds;
        _pt = pt;

        this.BindingContext = viewModel;
        _vm = viewModel;

        listView.ItemTapped += ListView_ItemTapped;
        listView.DataSource.SortDescriptors.Add(new SortDescriptor { PropertyName = "TransactionDate", Direction = ListSortDirection.Descending });
        listView.DataSource.GroupDescriptors.Add(new GroupDescriptor()
        {
            PropertyName = "TransactionDate",
            KeySelector = (object obj1) =>
            {
                var item = (obj1 as Transactions);
                if(item.IsTransacted)
                {
                    return item.TransactionDate.GetValueOrDefault().Date;
                }
                else
                {
                    return item.TransactionDate.GetValueOrDefault().Date;
                }
                
            },
            Comparer = new CustomGroupComparer()           
        });

        ListViewScrollView ListViewScrollBar = listView.GetScrollView();
        ListViewScrollBar.Scrolled += ListViewScrollView_Scrolled;
    }

    protected async override void OnAppearing()
    {
        try
        {
            await _vm.OnLoad();
            base.OnAppearing();

            AbsMain.SetLayoutBounds(vslChart, new Rect(0, 0, _vm.ScreenWidth, _vm.ChartContentHeight + 10));
            AbsMain.SetLayoutBounds(vslTransactionData, new Rect(0, _vm.ChartContentHeight + 10, _vm.ScreenWidth, _vm.ScreenHeight));            

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactions", "OnAppearing");
        }
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactions", "HomeButton_Clicked");
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        _vm.SFListHeight = _vm.ScreenHeight - vslHeader.Height - App.NavBarHeight - App.StatusBarHeight - TitleView.Height - 49;

    }

    private async void ListView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {
        try
        {
            var ListView = (SfListView)sender;
            Transactions tappedTransaction = (Transactions)e.DataItem;

            if (tappedItem == tappedTransaction)
            {
                tappedItem.IsVisible = false;
                tappedItem = null;

                ListView.RefreshItem();
                ListView.RefreshView();

                return;
            }

            if (tappedItem != null && tappedItem.IsVisible)
            {
                tappedItem.IsVisible = false;
            }

            tappedItem = tappedTransaction;
            tappedItem.IsVisible = true;

            ListView.RefreshItem();
            ListView.RefreshView();

            int index = listView.DataSource.DisplayItems.IndexOf(tappedTransaction); 
            listView.ItemsLayout.ScrollToRowIndex(index, Microsoft.Maui.Controls.ScrollToPosition.Center, false);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactions", "ListView_ItemTapped");
        }
    }

    private async void EditTransaction_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Transactions T = (Transactions)e.Parameter;

            bool EditTransaction = await Application.Current.Windows[0].Page.DisplayAlert($"Are your sure?", $"Are you sure you want to Edit this transaction?", "Yes, continue", "No Thanks!");
            if (EditTransaction)
            {
                await Shell.Current.GoToAsync($"{nameof(ViewTransactions)}/{nameof(AddTransaction)}?BudgetID={App.DefaultBudgetID}&TransactionID={T.TransactionID}&NavigatedFrom=ViewTransactions",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = T
                    });
                }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactions", "EditTransaction_Tapped");
        }
    }

    private async void DeleteTransaction_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Transactions T = (Transactions)e.Parameter;

            bool DeleteTransaction = await Application.Current.Windows[0].Page.DisplayAlert($"Are your sure?", $"Are you sure you want to Delete this transaction?", "Yes", "No Thanks!");
            if (DeleteTransaction)
            {
                await _ds.DeleteTransaction(T.TransactionID);
            }

            if(_vm.Transactions.Contains(T))
            {
                _vm.Transactions.Remove(T);
            }

            listView.RefreshItem();
            listView.RefreshView();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactions", "DeleteTransaction_Tapped");
        }
    }

    private async void ListViewScrollView_Scrolled(object sender, ScrolledEventArgs e)
    {
        try
        {
            _vm.SFListHeight = _vm.ScreenHeight - vslHeader.Height - App.NavBarHeight - App.StatusBarHeight - TitleView.Height - 49;

        double HeightDifference = CurrentScrollY - (double)e.ScrollY;
        double YChangeAmount = HeightDifference / 2;
        double vslYCoord = vslTransactionData.Y;

        if(HeightDifference > 1 || HeightDifference < -1)
        {
            if (HeightDifference < 0)
            {
                if (_vm.ScrollDirection == "DOWN")
                {
                    if (vslYCoord > 0)
                    {
                        double NewYCoord = vslYCoord + YChangeAmount < 0 ? 0 : (vslYCoord + YChangeAmount);
                        AbsMain.SetLayoutBounds(vslTransactionData, new Rect(0, NewYCoord, _vm.ScreenWidth, _vm.ScreenHeight));

                        if (NewYCoord == 0)
                        {
                            FilterOption.IsVisible = true;                            
                        }
                    }

                }
                else
                {
                    _vm.ScrollDirection = "DOWN";
                }
            }
            else if (HeightDifference > 0 && (double)e.ScrollY < (_vm.MaxChartContentHeight - 10) * 2)
            {
                int DisplayCount = listView.DataSource.Items.Count;
                int ItemCount = _vm.Transactions.Count;

                if (DisplayCount == ItemCount)
                {
                    if(_vm.ScrollDirection == "UP")
                    { 
                        if((double)e.ScrollY < 40)
                        {
                            double NewYCoord = (_vm.MaxChartContentHeight);
                            AbsMain.SetLayoutBounds(vslTransactionData, new Rect(0, NewYCoord, _vm.ScreenWidth, _vm.ScreenHeight));
                            FilterOption.IsVisible = false;
                        }
                        else if (vslYCoord < (_vm.MaxChartContentHeight))
                        {
                            double NewYCoord = vslYCoord + YChangeAmount > (_vm.MaxChartContentHeight) ? (_vm.MaxChartContentHeight) : (vslYCoord + YChangeAmount);
                            AbsMain.SetLayoutBounds(vslTransactionData, new Rect(0, NewYCoord, _vm.ScreenWidth, _vm.ScreenHeight));
                            if (NewYCoord >= (_vm.MaxChartContentHeight - 20))
                            {
                                FilterOption.IsVisible = false;
                            }
                        }
                        
                    }
                    else
                    {
                        _vm.ScrollDirection = "UP";
                    }
                }
            }            
        }

        CurrentScrollY = (double)e.ScrollY;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactions", "ListViewScrollView_Scrolled");
        }
    }

    private async void Content_Loaded(object sender, EventArgs e)
    {
        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private void SearchAmount_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            decimal Amount = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entSearchAmount);

            if (listView.DataSource != null)
            {
                listView.DataSource.Filter = FilterContacts;
                listView.DataSource.RefreshFilter();
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactions", "SearchAmount_TextChanged");
        }
    }

    private bool FilterContacts(object obj)
    {
        Transactions T = (Transactions)obj;
        bool IsFilter = false;
        bool IsFilterApplied = false;

        if(Filters != null)
        {
            if(Filters.CategoryFilter != null)
            {
                IsFilterApplied = true;
                if (Filters.CategoryFilter.Contains(T.CategoryID.GetValueOrDefault()))
                {
                    IsFilter = true;
                }
            }

            if(Filters.PayeeFilter != null)
            {
                IsFilterApplied = true;
                if (Filters.PayeeFilter.Contains(T.Payee))
                {
                    IsFilter = true;
                }
            }

            if(Filters.SavingFilter != null)
            {
                IsFilterApplied = true;
                if (Filters.SavingFilter.Contains(T.SavingID.GetValueOrDefault()))
                {
                    IsFilter = true;
                }
            }

            if(Filters.TransactionEventTypeFilter != null)
            {
                IsFilterApplied = true;
                if (Filters.TransactionEventTypeFilter.Contains(T.EventType))
                {
                    IsFilter = true;
                }
            }

            if(IsFilterApplied && !IsFilter)
            {
                return false;
            }

            if (Filters.DateFilter != null)
            {
                IsFilterApplied = true;
                if (T.TransactionDate < Filters.DateFilter.DateFrom || T.TransactionDate > Filters.DateFilter.DateTo)
                {
                    return false;
                }
                else
                {
                    IsFilter = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(entSearchAmount.Text))
        {
            decimal Amount = (decimal)_pt.FormatCurrencyNumber(entSearchAmount.Text);
            if (Amount != 0)
            {
                IsFilterApplied = true;
                if (Amount != T.TransactionAmount)
                {
                    return false;
                }
                else
                {
                    IsFilter = true;
                }
            }
            else
            {
                IsFilter = true;
            }
        }

        if(!IsFilterApplied)
        {
            return true;
        }

        return IsFilter;
    }

    private void SearchAmount_Tapped(object sender, TappedEventArgs e)
    {
        entSearchAmount.Focus();
    }

    private async void FilterItems_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            ViewTransactionFilterBottomSheet page = new ViewTransactionFilterBottomSheet(Filters, IPlatformApplication.Current.Services.GetService<IProductTools>());

            page.Detents = new DetentsCollection()
            {            
                new FullscreenDetent(),
                new MediumDetent(),
                new FixedContentDetent
                {
                    IsDefault = true
                }
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactions", "FilterItems_Tapped");
        }
    }
}

public class CustomGroupComparer : IComparer<GroupResult>
{
    public int Compare(GroupResult x, GroupResult y)
    {
        DateTime xDate = (DateTime)x.Key;
        DateTime yDate = (DateTime)y.Key;

        if (xDate > yDate)
        {
            return -1;
        }
        else if (xDate < yDate)
        {
            return 1;
        }

        return 0;
    }
}