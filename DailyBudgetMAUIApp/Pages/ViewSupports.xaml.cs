
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewSupports : BasePage
{
    private readonly ViewSupportsViewModel _vm;
    private readonly IPopupService _ps;
    public ViewSupports(ViewSupportsViewModel viewModel, IPopupService ps)
    {
        InitializeComponent();
        this.BindingContext = viewModel;
        _vm = viewModel;
        _ps = ps;

    }


    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        try
        {
            base.OnNavigatedTo(args);

            Shell.Current.ShowPopup(new PopUpPage(), options: new PopupOptions { CanBeDismissedByTappingOutsideOfPopup = false, PageOverlayColor = Color.FromArgb("#80000000") });

            await _vm.GetSupports();

            await _ps.ClosePopupAsync(Shell.Current);
        }
        catch (Exception ex)
        {
            await HandleException(ex, "ViewSupports", "OnNavigatedTo");
        }
    }

    async protected override void OnAppearing()
    {
        try
        {
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