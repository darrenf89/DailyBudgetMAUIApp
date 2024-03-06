using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;
using Syncfusion.Maui.Scheduler;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;

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

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
        }


    }
}

