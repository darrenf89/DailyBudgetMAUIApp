using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;
using Syncfusion.Maui.Scheduler;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;

namespace DailyBudgetMAUIApp.Pages
{
    public partial class ViewCalendar : ContentPage
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
        private async void HomeButton_Clicked(object sender, EventArgs e)
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.MainPage.ShowPopup(PopUp);
            }

            await Task.Delay(500);

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
        }

        private async void Scheduler_Tapped(object sender, SchedulerTappedEventArgs e)
        {
            var Appointments = e.Appointments;

            if(Appointments != null)
            {
                SchedulerAppointment Appointment = (SchedulerAppointment)Appointments[0];
                await _vm.LoadEventCard(Appointment.Notes, (int)Appointment.Id);
            }
        }
        
    }
}

