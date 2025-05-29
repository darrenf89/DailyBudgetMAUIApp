using DailyBudgetMAUIApp.DataServices;
using CommunityToolkit.Mvvm.ComponentModel;
using DailyBudgetMAUIApp.Models;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Pages;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewSupportsViewModel : BaseViewModel
    {
        private readonly IRestDataService _ds;
        private readonly IProductTools _pt;

        [ObservableProperty]
        private List<CustomerSupport> supports = new List<CustomerSupport>();
        [ObservableProperty]
        private ObservableCollection<CustomerSupport> filteredSupports = new ObservableCollection<CustomerSupport>();
        [ObservableProperty]
        private double signOutButtonWidth;
        [ObservableProperty]
        private string openClosedFilter = "none";
        [ObservableProperty]
        private string readUnreadFilter = "none";

        public ViewSupportsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

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
                var popup = new PopUpContactUs(new PopUpContactUsViewModel(_pt, _ds));
                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
                if (result is int)
                {

                    int SupportID = (int)result;
                    Action action = async () =>
                    {
                        await Task.Delay(500);
                        await Shell.Current.GoToAsync($"{nameof(ViewSupport)}?SupportID={SupportID}");
                        return;
                    };
                    await GetSupports();
                    await _pt.MakeSnackBar("We have received your support inquiry", action, "View", new TimeSpan(0, 0, 10), "Success");
                }
                else if ((string)result.ToString() == "Closed")
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
