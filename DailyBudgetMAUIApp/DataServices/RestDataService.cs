using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Mail;
using System.Diagnostics;
using Newtonsoft.Json;
using CommunityToolkit.Maui.ApplicationModel;

namespace DailyBudgetMAUIApp.DataServices
{
    //TODO: UPDATE ALL CALLS TO USE USING STREAM
    internal class RestDataService : IRestDataService
    {

        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;
        private readonly string _url;
        private readonly JsonSerializerOptions _jsonSerialiserOptions;

        public RestDataService()
        {
            _httpClient = new HttpClient();
            //_baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5074" : "https://localhost:7141";
            _baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "https://dailybudgetwebapi.azurewebsites.net/" : "https://dailybudgetwebapi.azurewebsites.net/";
            _url = $"{_baseAddress}/api/v1";

            _jsonSerialiserOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<string> GetUserSaltAsync(string UserEmail)
        {
            RegisterModel User = new RegisterModel();

            if(Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync($"{_url}/userAccounts/getsalt/{System.Web.HttpUtility.UrlEncode(UserEmail)}");
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    
                    User = System.Text.Json.JsonSerializer.Deserialize<RegisterModel>(content, _jsonSerialiserOptions);

                    return User.Salt;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    return error.ErrorMessage;
                }
                
            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get User Details in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public string LogoutUserAsync(RegisterModel User)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDetailsModel> RegisterNewUserAsync(RegisterModel User)
        {
            UserDetailsModel UserModel = new();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<RegisterModel>(User, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{_url}/userAccounts/registeruser", request);
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    UserModel = System.Text.Json.JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);
                    UserModel.Error = null;
                    return UserModel;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    error.StatusCode = response.StatusCode;
                    UserModel.Error = error;
                    return UserModel;
                }       
            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get User Details in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserDetailsModel> GetUserDetailsAsync(string UserEmail)
        {
            UserDetailsModel User = new();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync($"{_url}/userAccounts/getLogonDetails/{System.Web.HttpUtility.UrlEncode(UserEmail)}");
                string content = "";
                content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {

                    User = System.Text.Json.JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);
                    User.Error = null;
                    return User;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    error.StatusCode = response.StatusCode;
                    User.Error = error;
                    return User;
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get User Details in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<ErrorLog> CreateNewErrorLog(ErrorLog NewLog)
        {
            ErrorLog ErrorLog = new();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                ErrorLog.ErrorMessage = "You have no Internet Connection, unfortunately you need that. Please try again when you are back in civilised society";
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
                    if(ErrorLog.ErrorLogID != 0)
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
                Debug.WriteLine($"Error Trying to Log the Error --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Budgets> GetBudgetDetailsAsync(int BudgetID, string Mode)
        {
            Budgets Budget = new Budgets();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
                string ApiMethod = "";
                if (Mode == "Full")
                {
                    ApiMethod = "getbudgetdetailsfull";
                }
                else if(Mode == "Limited")
                {
                    ApiMethod = "getbudgetdetailsonly";
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/{ApiMethod}/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {

                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        Budget = serializer.Deserialize<Budgets>(reader);
                    }

                    return Budget;
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    Budget.Error = error;
                    return Budget;
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Budget Details in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<DateTime> GetBudgetLastUpdatedAsync(int BudgetID)
        {
            Budgets Budget = new Budgets();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync($"{_url}/budgets/getlastupdated/{BudgetID}");
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {

                    Budget = System.Text.Json.JsonSerializer.Deserialize<Budgets>(content, _jsonSerialiserOptions);

                    return Budget.LastUpdated;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(response.StatusCode.ToString() + " - " + error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Budget Details in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<BudgetSettings> GetBudgetSettings(int BudgetID)
        {
            BudgetSettings BudgetSettings = new BudgetSettings();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/getbudgetsettingsvalues/{BudgetID}").Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {

                    BudgetSettings = System.Text.Json.JsonSerializer.Deserialize<BudgetSettings>(content, _jsonSerialiserOptions);

                    return BudgetSettings;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(response.StatusCode.ToString() + " - " + error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Budget Settings in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<BudgetSettingValues> GetBudgetSettingsValues(int BudgetID)
        {
            BudgetSettingValues BudgetSettings = new BudgetSettingValues();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/getbudgetsettingsvalues/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {

                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        BudgetSettings = serializer.Deserialize<BudgetSettingValues>(reader);
                    }

                    return BudgetSettings;
                }
                else
                {

                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(response.StatusCode.ToString() + " - " + error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Budget Settings in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Budgets> CreateNewBudget(string UserEmail)
        {
            Budgets Budget = new Budgets();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/createnewbudget/{UserEmail}").Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {

                    Budget = System.Text.Json.JsonSerializer.Deserialize<Budgets>(content, _jsonSerialiserOptions);

                    return Budget;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(response.StatusCode.ToString() + " - " + error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Create new Budget in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<lut_CurrencySymbol>> GetCurrencySymbols(string SearchQuery)
        {
            List<lut_CurrencySymbol> Currencies = new List<lut_CurrencySymbol>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/getcurrencysymbols/{SearchQuery}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {

                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        Currencies = serializer.Deserialize<List<lut_CurrencySymbol>>(reader);
                    }

                    return Currencies;
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(response.StatusCode.ToString() + " - " + error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Create new Budget in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
    }
}
