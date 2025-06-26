
using CommunityToolkit.Maui;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewSupports : BasePage
{
    private readonly ViewSupportsViewModel _vm;
    private readonly IModalPopupService _mps;

    public ViewSupports(ViewSupportsViewModel viewModel, IModalPopupService mps)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;
        _mps = mps;
    }

    async protected override void OnAppearing()
    {
        try
        {
            if(_mps.CurrentPopup is not null)
                return;

            await _mps.ShowAsync<PopUpPage>(() => new PopUpPage());

            await _vm.GetSupports();

            await _mps.CloseAsync<PopUpPage>();

            base.OnAppearing();

        }
        catch(Exception ex)
        {
            await _vm.HandleException(ex, "ViewSupports", "OnAppearing");
        }

    }

    private async void filterOpen_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (_vm.OpenClosedFilter == "open")
            {
                _vm.OpenClosedFilter = "none";
                filterClosed.FontAttributes = FontAttributes.None;
                filterOpen.FontAttributes = FontAttributes.None;
            }
            else
            {
                _vm.OpenClosedFilter = "open";
                filterClosed.FontAttributes = FontAttributes.None;
                filterOpen.FontAttributes = FontAttributes.Bold;
            }

            await _vm.FilterSupports();
        }
        catch(Exception ex)
        {
            await _vm.HandleException(ex, "ViewSupports", "filterOpen_Tapped");
        }


    }
    private async void filterClosed_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (_vm.OpenClosedFilter == "closed")
            {
                _vm.OpenClosedFilter = "none";
                filterClosed.FontAttributes = FontAttributes.None;
                filterOpen.FontAttributes = FontAttributes.None;
            }
            else
            {
                _vm.OpenClosedFilter = "closed";
                filterClosed.FontAttributes = FontAttributes.Bold;
                filterOpen.FontAttributes = FontAttributes.None;
            }

            await _vm.FilterSupports();
        }
        catch (Exception ex)
        {
            await _vm.HandleException(ex, "ViewSupports", "filterClosed_Tapped");
        }
    }
    private async void filterRead_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (_vm.ReadUnreadFilter == "read")
            {
                _vm.ReadUnreadFilter = "none";
                filterRead.FontAttributes = FontAttributes.None;
                filterUnread.FontAttributes = FontAttributes.None;
            }
            else
            {
                _vm.ReadUnreadFilter = "read";
                filterRead.FontAttributes = FontAttributes.Bold;
                filterUnread.FontAttributes = FontAttributes.None;
            }

            await _vm.FilterSupports();
        }
        catch (Exception ex)
        {
            await _vm.HandleException(ex, "ViewSupports", "filterRead_Tapped");
        }
    }
    private async void filterUnread_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (_vm.ReadUnreadFilter == "unread")
            {
                _vm.ReadUnreadFilter = "none";
                filterRead.FontAttributes = FontAttributes.None;
                filterUnread.FontAttributes = FontAttributes.None;
            }
            else
            {
                _vm.ReadUnreadFilter = "unread";
                filterRead.FontAttributes = FontAttributes.None;
                filterUnread.FontAttributes = FontAttributes.Bold;
            }

            await _vm.FilterSupports();
        }
        catch (Exception ex)
        {
            await _vm.HandleException(ex, "ViewSupports", "filterUnread_Tapped");
        }
    }

    private async void NavigateViewSupport(object sender, TappedEventArgs e)
    {        
        try
        {
            int SupportID = (int)e.Parameter;
            await Shell.Current.GoToAsync($"{nameof(ViewSupport)}?SupportID={SupportID}");
        }
        catch (Exception ex)
        {
            await _vm.HandleException(ex, "ViewSupports", "NavigateViewSupport");
        }
    }
}