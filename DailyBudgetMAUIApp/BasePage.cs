using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Text.Json;
using System.Text;


namespace DailyBudgetMAUIApp
{
    public class BasePage : ContentPage
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;
        private readonly string _url;
        private readonly JsonSerializerOptions _jsonSerialiserOptions;

        public BasePage()
        {
            _httpClient = new HttpClient();
            _baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "https://dailybudgetwebapi.azurewebsites.net/" : "https://dailybudgetwebapi.azurewebsites.net/";
            _url = $"{_baseAddress}api/v1";

            _jsonSerialiserOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        protected async override void OnAppearing()
        {
            try 
            {
                base.OnAppearing();
            }
            catch (Exception ex)
            {
                await HandleException(ex, "BasePage", "OnAppearing");
            }
        }

        protected async override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
            }
            catch (Exception ex)
            {
                await HandleException(ex, "BasePage", "OnDisappearing");
            }
        }

        protected async override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            try
            {
                base.OnNavigatedFrom(args);
            }
            catch (Exception ex)
            {
                await HandleException(ex, "BasePage", "OnNavigatedFrom");
            }
        }
        protected async override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            try
            {
                base.OnNavigatedTo(args);
            }
            catch (Exception ex)
            {
                await HandleException(ex, "BasePage", "OnNavigatedTo");
            }
        }
        protected async override void OnNavigatingFrom(NavigatingFromEventArgs args)
        {
            try
            {
                base.OnNavigatingFrom(args);
            }
            catch (Exception ex)
            {
                await HandleException(ex, "BasePage", "OnNavigatingFrom");
            }
        }

        public async Task HandleException(Exception ex, string Page, string Method)
        {
            if (ex.Message == "Connectivity")
            {
                await Shell.Current.GoToAsync($"{nameof(NoNetworkAccess)}");
            }
            else if (ex.Message == "Server Connectivity")
            {
                await Shell.Current.GoToAsync($"{nameof(NoServerAccess)}");
            }
            else
            {
                ErrorLog Error = new ErrorLog(ex, Page, Method);
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Error = await CreateNewErrorLog(Error);
                }
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        public async Task<ErrorLog> CreateNewErrorLog(ErrorLog NewLog)
        {
            ErrorLog ErrorLog = new();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                ErrorLog.ErrorMessage = "You have no Internet Connection, unfortunately you need that. Please try again when you are back in civilized society";
                return ErrorLog;
            }

            try
            {
                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<ErrorLog>(NewLog, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{_url}/error/adderrorlogentry", request);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    ErrorLog = System.Text.Json.JsonSerializer.Deserialize<ErrorLog>(content, _jsonSerialiserOptions);
                    if (ErrorLog.ErrorLogID != 0)
                    {
                        return ErrorLog;
                    }
                    else
                    {
                        ErrorLog.ErrorMessage = "Opps something went wrong. It was probably one of our graduate developers fault ... Sorry about that!";
                        return ErrorLog;
                    }

                }
                else
                {
                    ErrorLog.ErrorMessage = "Opps something went wrong. It was probably one of our graduate developers fault ... Sorry about that!";
                    return ErrorLog;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.ErrorMessage = "Opps something went wrong. It was probably one of our graduate developers fault ... Sorry about that!";
                return ErrorLog;
            }
        }
    }
}
