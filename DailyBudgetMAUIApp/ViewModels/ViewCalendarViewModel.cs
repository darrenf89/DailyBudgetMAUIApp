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
        [ObservableProperty]
        private DateTime _today = DateTime.Today;
        [ObservableProperty]
        private double _schedulerHeight;

        public ViewCalendarViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;
            SchedulerHeight = ((DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density)) * 0.4;
        }

        public async Task LoadData()
        {
            Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
            await LoadPayDayEvents(Today.AddMonths(1));
            await LoadSavingsEvents();
        }

        [ICommand]
        private async void LoadMoreEvents(object obj)
        {
            ShowBusyIndicator = true;
            await Task.Delay(500);
            await GenerateSchedulerAppointments(((SchedulerQueryAppointmentsEventArgs)obj).VisibleDates.ToList());
            ShowBusyIndicator = false;
        }

        private async Task GenerateSchedulerAppointments(List<DateTime> visibleDates)
        {
            DateTime MaxDate = visibleDates.Max();
            await LoadPayDayEvents(MaxDate);
        }

        private async Task LoadPayDayEvents(DateTime MaxDate)
        {
            Application.Current.Resources.TryGetValue("White", out var White);

            while (Budget.NextIncomePayday.GetValueOrDefault() < MaxDate.AddDays(30))
            {
                SchedulerAppointment PayDay = new SchedulerAppointment
                {
                    StartTime = Budget.NextIncomePayday.GetValueOrDefault().Date,
                    EndTime = Budget.NextIncomePayday.GetValueOrDefault().Date.AddMinutes(1439),
                    Subject = "Pay Day!",
                    IsReadOnly = true,
                    Background = App.ChartColor[1],
                    TextColor = (Color)White
                };

                Events.Add(PayDay);

                foreach (Savings saving in Budget.Savings.Where(s => !s.IsRegularSaving))
                {
                    SchedulerAppointment EnvelopeEvent = new SchedulerAppointment
                    {
                        StartTime = Budget.NextIncomePayday.GetValueOrDefault().Date,
                        EndTime = Budget.NextIncomePayday.GetValueOrDefault().Date.AddMinutes(1439),
                        Subject = $"{saving.SavingsName}",
                        IsReadOnly = true,
                        Background = App.ChartColor[2],
                        TextColor = (Color)White,
                        Notes = $"Envelope for {string.Format("C", saving.PeriodSavingValue)}"
                    };

                    Events.Add(EnvelopeEvent);
                }

                Budget.NextIncomePayday = _pt.CalculateNextDate(Budget.NextIncomePayday.GetValueOrDefault(), Budget.PaydayType, Budget.PaydayValue.GetValueOrDefault(), Budget.PaydayDuration);
            }
        }

        private async Task LoadSavingsEvents()
        {
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach(Savings saving in Budget.Savings.Where(s => s.IsRegularSaving))
            {
                if(saving.SavingsType != "SavingsBuilder")
                {
                    SchedulerAppointment SavingEvent = new SchedulerAppointment
                    {
                        StartTime = saving.GoalDate.GetValueOrDefault().Date,
                        EndTime = saving.GoalDate.GetValueOrDefault().Date.AddMinutes(1439),
                        Subject = $"{saving.SavingsName}",
                        IsReadOnly = true,
                        Background = App.ChartColor[0],
                        TextColor = (Color)White,
                        Notes = $"Saving for {string.Format("C", saving.SavingsGoal)}"
                    };

                    Events.Add(SavingEvent);
                } 
            }            
        }
    }
}
