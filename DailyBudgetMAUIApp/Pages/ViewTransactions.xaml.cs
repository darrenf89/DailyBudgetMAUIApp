using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using Microsoft.Toolkit.Mvvm.ComponentModel;

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

        listview.ItemTapped += ListView_ItemTapped;
        listView.DataSource.SortDescriptors.Add(new SortDescriptor { PropertyName = "TransactionDate", Direction = ListSortDirection.Descending });
        listView.DataSource.GroupDescriptor.Comparer.Add(new CustomGroupComparer());
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
                
            }
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

    private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        var tappedItemData = (Transactions)sender;
        if (tappedItem != null && tappedItem.IsVisible)
        {
            tappedItem.IsVisible = false;
        }

        if (tappedItem == tappedItemData)
        {
            tappedItem = null;
            return;
        }

        tappedItem = tappedItemData;
        tappedItem.IsVisible = true;
    }

}

public class CustomGroupComparer : IComparer<GroupResult>
{
    public int Compare(GroupResult x, GroupResult y)
    {
        DateTime xDate = (DateTIme)x.Key;
        DateTime yDate = (DateTIme)y.Key;

        if (xDate > yDate)
        {
            return 1;
        }
        else if (xDate < yDate)
        {
            return -1;
        }

        return 0;
    }
}