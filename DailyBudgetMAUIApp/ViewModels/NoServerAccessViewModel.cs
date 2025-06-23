using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;


namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class NoServerAccessViewModel : BaseViewModel
    {
        [ObservableProperty]
        public partial string TxtSubHeading { get; set; } = "";

        [ObservableProperty]
        public partial string TxtButton { get; set; } = "";

        [ObservableProperty]
        public partial string TxtConnectionStatus { get; set; } = "";

        [ObservableProperty]
        public partial Color ColorConnectionStatus { get; set; }

        [ObservableProperty]
        public partial bool BtnIsEnabled { get; set; }

        [ObservableProperty]
        public partial bool AIIsVisible { get; set; }

        [ObservableProperty]
        public partial int CountdownNumber { get; set; } = 14;

        [ObservableProperty]
        public partial string CountdownString { get; set; } = "Please wait another 30 seconds";


        private IDispatcherTimer Timer;
        private IDispatcherTimer CountDownTimer;
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        public NoServerAccessViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
        }

        [RelayCommand]
        async Task GoToLandingPage()
        {
            try
            {
                if (App.UserDetails is not null && App.UserDetails.SessionExpiry > DateTime.UtcNow)
                {
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(LandingPage)}");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "NoServerAccess", "GoToLandingPage");
            }
        
        }

        private async Task<bool> CheckServerConnection()
        {            
            return await _ds.CheckConnectionStrengthAsync();
        }
        public async Task StartTimer()
        {
            await Task.Delay(10);
            var countDownTimer = Application.Current.Dispatcher.CreateTimer();
            CountDownTimer = countDownTimer;
            countDownTimer.Interval = TimeSpan.FromSeconds(1);
            countDownTimer.Tick += async (s, e) =>
            {
                try
                {
                    await UpdateCountDown();
                }
                catch (Exception ex)
                {
                    await _pt.HandleException(ex, "NoServerAccess", "timer.Tick");
                }
            };

            var timer = Application.Current.Dispatcher.CreateTimer();
            Timer = timer;
            timer.Interval = TimeSpan.FromSeconds(15);
            timer.Tick += async (s, e) =>
            {
                try
                {
                    await CheckConnection(false);
                    CountdownNumber = 15;
                    CountdownString = $"Please wait another {CountdownNumber} seconds";
                    CountDownTimer.Start();
                }
                catch (Exception ex)
                {
                    await _pt.HandleException(ex, "NoServerAccess", "timer.Tick");
                }
            };
            timer.Start();
            countDownTimer.Start();
        }
        public async Task EndTimer()
        {
            await Task.Delay(10);
            Timer.Stop();
            CountDownTimer.Stop();
        }

        public async Task HandleException(Exception ex, string page, string Method)
        {
            await _pt.HandleException(ex, page, Method);
        }

        public async Task CheckConnection(bool IsPageLoad)
        {
            if (!IsPageLoad && await CheckServerConnection())
            {
                Application.Current.Resources.TryGetValue("Success", out var Success);

                TxtSubHeading = "Sorry about that, we have managed to regain connection.\nPlease go back and get budgeting!";
                AIIsVisible = false;
                BtnIsEnabled = true;
                TxtConnectionStatus = "Server Connection Status: Connected";
                ColorConnectionStatus = (Color)Success;
            }
            else
            {
                Application.Current.Resources.TryGetValue("Danger", out var Danger);

                TxtSubHeading = "Awkward we lost connection to the mother ship.\nWe are attempting to make contact again, please wait or come back later!";
                AIIsVisible = true;
                BtnIsEnabled = false;
                TxtConnectionStatus = "Server Connection Status: Disconnected";
                ColorConnectionStatus = (Color)Danger;
            }
        }

        public async Task UpdateCountDown()
        {
            CountdownNumber--;
            if (CountdownNumber != 0)
            {
                if (CountdownNumber == 1)
                {
                    CountdownString = $"Please wait one second";
                }
                else
                {
                    CountdownString = $"Please wait another {CountdownNumber} seconds";
                }

            }
            else
            {
                CountdownNumber = 15;
                CountdownString = $"Trying to reconnect";
                CountDownTimer.Stop();
            }

        }
    }
}
