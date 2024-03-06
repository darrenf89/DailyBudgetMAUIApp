using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Syncfusion.Maui.Scheduler;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewCalendarViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private bool _showBusyIndicator;
        [ObservableProperty]
        private Budgets _budget;
        [ObservableProperty]
        private ObservableCollection<SchedulerAppointment> _events = new ObservableCollection<SchedulerAppointment>();

        public ViewCalendarViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;
        }

        public async Task LoadData()
        {
            Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
        }

        [ICommand]
        private async void LoadMoreEvents(object obj)
        {
            ShowBusyIndicator = true;
            await Task.Delay(500);
            Events = GenerateSchedulerAppointments(((SchedulerQueryAppointmentsEventArgs)obj).VisibleDates.ToList());
            ShowBusyIndicator = false;
        }

        private ObservableCollection<SchedulerAppointment> GenerateSchedulerAppointments(List<DateTime> visibleDates)
        {

            return new ObservableCollection<SchedulerAppointment>();
        }
    }
}
