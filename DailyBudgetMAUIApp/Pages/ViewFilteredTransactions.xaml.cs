using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.DataSource;
using DailyBudgetMAUIApp.Popups;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewFilteredTransactions : ContentPage
{
    private readonly ViewFilteredTransactionsViewModel _vm;
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;
    private Transactions tappedItem;

    public ViewFilteredTransactions(ViewFilteredTransactionsViewModel viewModel, IRestDataService ds, IProductTools pt)
	{
		InitializeComponent();

        _ds = ds;
        _pt = pt;

        this.BindingContext = viewModel;
        _vm = viewModel;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        listView.ItemTapped += ListView_ItemTapped;
        listView.DataSource.SortDescriptors.Add(new SortDescriptor { PropertyName = "TransactionDate", Direction = ListSortDirection.Descending });
        listView.DataSource.GroupDescriptors.Add(new GroupDescriptor()
        {
            PropertyName = "TransactionDate",
            KeySelector = (object obj1) =>
            {
                var item = (obj1 as Transactions);
                if (item.IsTransacted)
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

        _vm.OnAppearing();

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private async void ListView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
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

    private async void EditTransaction_Tapped(object sender, TappedEventArgs e)
    {
        Transactions T = (Transactions)e.Parameter;

        bool EditTransaction = await Application.Current.MainPage.DisplayAlert($"Are your sure?", $"Are you sure you want to Edit this transaction?", "Yes, continue", "No Thanks!");
        if (EditTransaction)
        {
            await Shell.Current.GoToAsync($"{nameof(AddTransaction)}?BudgetID={App.DefaultBudgetID}&TransactionID={T.TransactionID}&NavigatedFrom=ViewTransactions",
                new Dictionary<string, object>
                {
                    ["Transaction"] = T
                });
        }
    }

    private async void DeleteTransaction_Tapped(object sender, TappedEventArgs e)
    {
        Transactions T = (Transactions)e.Parameter;

        bool DeleteTransaction = await Application.Current.MainPage.DisplayAlert($"Are your sure?", $"Are you sure you want to Delete this transaction?", "Yes", "No Thanks!");
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
    private async void Content_Loaded(object sender, EventArgs e)
    {
        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }
}