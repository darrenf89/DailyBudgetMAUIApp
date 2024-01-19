using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Handlers;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class ViewTransactionFilterBottomSheet : BottomSheet
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public FilterModel Filter { get; set; }


    public ViewTransactionFilterBottomSheet(FilterModel filter)
    {
        InitializeComponent();

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;

        lblBudgetName.Text = $"Filter";

        Filter = filter;

        LoadFilters();
        FillFilters();

    }

    private void LoadFilters()
    {

    }

    private void FillFilters()
    {

    }

    private async void ApplyFilter_Clicked(object sender, EventArgs e)
    {
        ViewTransactions CurrentPage = (ViewTransactions)Shell.Current.CurrentPage;

        FilterModel NewFilter = Filter;

        CurrentPage.Filters = NewFilter;

        App.CurrentPopUp = null;
        await this.DismissAsync();
    }
}