using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;
using Syncfusion.Maui.Scheduler;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.Pages
{
    public partial class ViewCalendar : BasePage
    {
        private readonly ViewCalendarViewModel _vm;
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        public ViewCalendar(ViewCalendarViewModel viewModel, IProductTools pt, IRestDataService ds)        {

            InitializeComponent();

            this.BindingContext = viewModel;
            _vm = viewModel;
            _pt = pt;
            _ds = ds;
        }

        protected async override void OnAppearing()
        {
            try
            {
                Application.Current.Resources.TryGetValue("Primary", out var Primary);
                Application.Current.Resources.TryGetValue("Gray400", out var Tertiary);

                Scheduler.HeaderView.TextStyle = new SchedulerTextStyle
                {
                    TextColor = (Color)Primary,
                    FontSize = 25,
                    FontFamily = "OpenSansSemibold"
                };

                Scheduler.AgendaView.WeekHeaderSettings.TextStyle = new SchedulerTextStyle
                {
                    TextColor = (Color)Tertiary,
                    FontSize = 12              
                };

                base.OnAppearing();
                await _vm.LoadData();

                if (App.CurrentPopUp != null)
                {
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewCalendar", "OnAppearing");
            }

        }
        private async void HomeButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Task.Delay(500);

                await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewCalendar", "HomeButton_Clicked");
            }
        }

        private async void Scheduler_Tapped(object sender, SchedulerTappedEventArgs e)
        {
            try
            {
                var Appointments = e.Appointments;

                if (Appointments != null)
                {
                    SchedulerAppointment Appointment = (SchedulerAppointment)Appointments[0];
                    await _vm.LoadEventCard(Appointment.Notes, (int)Appointment.Id);
                    _vm.SelectedIndex = _vm.EventList.IndexOf(Appointment);
                    if (_vm.SelectedIndex == 0)
                    {
                        _vm.IsNextEnabled = true;
                        _vm.IsPreviousEnabled = false;
                    }
                    else if (_vm.SelectedIndex == _vm.EventList.Count() - 1)
                    {
                        _vm.IsNextEnabled = false;
                        _vm.IsPreviousEnabled = true;
                    }
                    else
                    {
                        _vm.IsNextEnabled = true;
                        _vm.IsPreviousEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "ViewCalendar", "Scheduler_Tapped");
            }
        }
        
    }
}

