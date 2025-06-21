using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class PopUpNoNetworkViewModel : BaseViewModel
    {

        [ObservableProperty]
        public partial int CountdownNumber { get; set; } = 29;

        [ObservableProperty]
        public partial string CountdownString { get; set; } = "Please wait another 30 seconds";

        private IDispatcherTimer _timer;

        public PopUpNoNetworkViewModel()
        {
            var timer = Application.Current.Dispatcher.CreateTimer();
            _timer = timer;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += async (s, e) =>
            {
                await UpdateCountDown();
            };
            timer.Start();
        }

        public async Task UpdateCountDown()
        {
            CountdownNumber--;
            if(CountdownNumber != 0)
            {
                if (CountdownNumber == 1) 
                {
                    CountdownString = $"Please wait another a second";
                }
                else
                {
                    CountdownString = $"Please wait another {CountdownNumber} seconds";
                }
                
            }
            else
            {
                CountdownString = "Capital A, Awkward we couldn't reconnect you!";
                _timer.Stop();
            }

        }
    }
}
