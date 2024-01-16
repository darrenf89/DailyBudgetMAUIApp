using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using Syncfusion.Maui.ListView;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Syncfusion.Maui.DataSource;
using Syncfusion.Maui.DataSource.Extensions;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewTransactions : ContentPage
{
    private readonly ViewTransactionsViewModel _vm;
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;
    private Transactions tappedItem;

    public ViewTransactions(ViewTransactionsViewModel viewModel, IRestDataService ds, IProductTools pt)
	{
		InitializeComponent();

        _ds = ds;
        _pt = pt;

        this.BindingContext = viewModel;
        _vm = viewModel;

        listView.ItemTapped += ListView_ItemTapped;
        //listView.DataSource.SortDescriptors.Add(new SortDescriptor { PropertyName = "TransactionDate", Direction = ListSortDirection.Ascending });
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


    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        _vm.LVHeight = 580;

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        var PopUp = new PopUpPage();
        App.CurrentPopUp = PopUp;
        Application.Current.MainPage.ShowPopup(PopUp);

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
    }

    private async void ListView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    {

        var ListView = (SfListView)sender;
        Transactions tappedTransaction = (Transactions)e.DataItem;

        if (tappedItem == tappedTransaction)
        {            
            return;
        }

        if (tappedItem != null && tappedItem.IsVisible)
        {
            tappedItem.IsVisible = false;
        }

        tappedItem = tappedTransaction;
        tappedItem.IsVisible = true;

        ListView.RefreshView();
        ListView.RefreshItem();
    }

    private async void EditTransaction_Tapped(object sender, TappedEventArgs e)
    {
        Transactions T = (Transactions)e.Parameter;

        bool EditTransaction = await Application.Current.MainPage.DisplayAlert($"Are your sure?", $"Are you sure you want to Edit this transaction?", "Yes, continue", "No Thanks!");
        if (EditTransaction)
        {
            await Shell.Current.GoToAsync($"{nameof(AddTransaction)}?BudgetID={App.DefaultBudgetID}&TransactionID={T.TransactionID}",
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