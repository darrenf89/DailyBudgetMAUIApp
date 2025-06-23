using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewSupportsViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;
        private readonly IPopupService _ps;

        [ObservableProperty]
        public partial List<CustomerSupport> Supports { get; set; } = new List<CustomerSupport>();

        [ObservableProperty]
        public partial ObservableCollection<CustomerSupport> FilteredSupports { get; set; } = new ObservableCollection<CustomerSupport>();

        [ObservableProperty]
        public partial double SignOutButtonWidth { get; set; }

        [ObservableProperty]
        public partial string OpenClosedFilter { get; set; } = "none";

        [ObservableProperty]
        public partial string ReadUnreadFilter { get; set; } = "none";


        public ViewSupportsViewModel(IProductTools pt, IRestDataService ds, IPopupService ps)
        {
            _ds = ds;
            _pt = pt;
            _ps = ps;

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            SignOutButtonWidth = ScreenWidth - 60;

        }

        public async Task GetSupports()
        {
            this.Supports = await _ds.GetSupports(App.IsFamilyAccount ? App.FamilyUserDetails.UniqueUserID : App.UserDetails.UniqueUserID, "ViewSupports");
            foreach(CustomerSupport C in Supports)
            {
                C.IsUnreadMessages = C.Replys.Any(c => !c.IsRead);
            }

            await FilterSupports();

            return;
        }

        public async Task HandleException(Exception ex, string Page, string Method)
        {
            await _pt.HandleException(ex, Page, Method);
        }

        public async Task FilterSupports()
        {
            await Task.Delay(1);
            FilteredSupports.Clear();

            foreach (CustomerSupport C in Supports)
            {
                this.FilteredSupports.Add(C);
            }            

            if(OpenClosedFilter == "open")
            {
                for (int i = FilteredSupports.Count - 1; i >= 0; i--)
                {
                    if(FilteredSupports[i].IsClosed)
                    {
                        FilteredSupports.RemoveAt(i);
                    }
                }
            }
            else if(OpenClosedFilter == "closed")
            {
                for (int i = FilteredSupports.Count - 1; i >= 0; i--)
                {
                    if (!FilteredSupports[i].IsClosed)
                    {
                        FilteredSupports.RemoveAt(i);
                    }
                }
            }

            if(ReadUnreadFilter == "read")
            {
                for (int i = FilteredSupports.Count - 1; i >= 0; i--)
                {
                    if (!FilteredSupports[i].IsUnreadMessages)
                    {
                        FilteredSupports.RemoveAt(i);
                    }
                }
            }
            else if(ReadUnreadFilter == "unread")
            {
                for (int i = FilteredSupports.Count - 1; i >= 0; i--)
                {
                    if (FilteredSupports[i].IsUnreadMessages)
                    {
                        FilteredSupports.RemoveAt(i);
                    }
                }
            }
        }

        [RelayCommand]
        public async Task AddNewSupport()
        {
            try
            {
                var popup = new PopUpContactUs(new PopUpContactUsViewModel(_pt,_ds));
                var popupOptions = new PopupOptions
                {
                    CanBeDismissedByTappingOutsideOfPopup = false,
                    PageOverlayColor = Color.FromArgb("#80000000")
                };

                IPopupResult<string> popupResult = await Shell.Current.ShowPopupAsync<string>(
                    popup,
                    options: popupOptions,
                    shellParameters: null,
                    token: CancellationToken.None
                );

                if (int.TryParse(popupResult.Result, out int SupportID))
                {
                    Action action = async () =>
                    {
                        await Task.Delay(500);
                        await Shell.Current.GoToAsync($"{nameof(ViewSupport)}?SupportID={SupportID}");
                        return;
                    };
                    await GetSupports();
                    await _pt.MakeSnackBar("We have received your support inquiry", action, "View", new TimeSpan(0, 0, 10), "Success");
                }
                else if ((string)popupResult.Result.ToString() == "Closed")
                {

                }
                else
                {
                    await _pt.MakeSnackBar("Sorry something went wrong, inquiry not received.", null, null, new TimeSpan(0, 0, 10), "Danger");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewSupports", "AddNewSupport");
            }
        }
    }

}
