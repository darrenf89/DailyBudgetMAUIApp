using DailyBudgetMAUIApp.Models;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net;
using DailySpendWebApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Transactions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;


namespace DailyBudgetMAUIApp.DataServices
{
    //TODO: UPDATE ALL CALLS TO USE USING STREAM
    internal class RestDataService : IRestDataService
    {

        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;
        private readonly string _url;
        private readonly JsonSerializerOptions _jsonSerialiserOptions;
        private NetworkAccess PreviousNetworkAccess = NetworkAccess.Internet;
        private bool CheckNetworkChanged = true;

        public RestDataService()
        {
            _httpClient = new HttpClient();
            //_baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5074" : "https://localhost:7141";
            _baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "https://dailybudgetwebapi.azurewebsites.net/" : "https://dailybudgetwebapi.azurewebsites.net/";
            _url = $"{_baseAddress}api/v1";

            _jsonSerialiserOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            Connectivity.ConnectivityChanged +=  ConnectivityChangedHandler;
        }

        private async void ConnectivityChangedHandler(object sender, ConnectivityChangedEventArgs e)
        {
            PreviousNetworkAccess = e.NetworkAccess;
            if(CheckNetworkChanged)
            {
                if (e.NetworkAccess == NetworkAccess.Internet)
                {
                    //TODO: SHOW POPUP SAYING INTERNET RETURNED WOULD THEY LIKE TO RETURN TO THEIR BUDGETTING
                }
                else if (PreviousNetworkAccess == NetworkAccess.Internet)
                {
                    //TODO: TAKE THEM TO THE NO INTERNET PAGE
                    await Shell.Current.GoToAsync($"{nameof(NoNetworkAccess)}");
                }
            }
        }

        private async void HandleError(Exception ex, string Page, string Method)
        {
            if (ex.Message == "Connectivity")
            {
                //TODO: TAKE THEM TO THE NO INTERNET PAGE
                await Shell.Current.GoToAsync($"{nameof(NoNetworkAccess)}");
            }
            else
            {
                ErrorLog Error = new ErrorLog(ex, Page, Method);
                if(Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
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

        private async Task<bool> CheckNetworkConnection()
        {
            CheckNetworkChanged = false;

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                
                //TODO: SHOW POPUP
                if (App.CurrentPopUp != null)
                {
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                }

                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpNoNetwork();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Task.Delay(1000);

                int i = 0;
                while(Connectivity.Current.NetworkAccess != NetworkAccess.Internet && i < 30)
                {
                    await Task.Delay(1000);
                    i++;
                }

                if (App.CurrentPopUp != null)
                {
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                }

            }

            CheckNetworkChanged = true;

            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
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

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/error/adderrorlogentry", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

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

        public async Task<string> PatchUserAccount(int UserID, List<PatchDoc> PatchDoc)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PatchAsync($"{_url}/userAccounts/{UserID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "PatchUserAccount", "PatchUserAccount");
                throw new Exception(ex.Message);
            }

        }
        public async Task<string> GetUserSaltAsync(string UserEmail)
        {
            RegisterModel User = new RegisterModel();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/userAccounts/getsalt/{System.Web.HttpUtility.UrlEncode(UserEmail)}").Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    
                    User = System.Text.Json.JsonSerializer.Deserialize<RegisterModel>(content, _jsonSerialiserOptions);

                    return User.Salt;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
                
            }
            catch (Exception ex)
            {
                HandleError(ex, "GetUserSaltAsync", "GetUserSaltAsync");
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

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<RegisterModel>(User, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/userAccounts/registeruser", request).Result;
                string content =  response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    UserModel = System.Text.Json.JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);
                    UserModel.Error = null;
                    return UserModel;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }       
            }
            catch (Exception ex)
            {
                HandleError(ex, "RegisterNewUserAsync", "RegisterNewUserAsync");
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserDetailsModel> GetUserDetailsAsync(string UserEmail)
        {
            UserDetailsModel User = new();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/userAccounts/getLogonDetails/{System.Web.HttpUtility.UrlEncode(UserEmail)}").Result;
                string content = "";
                content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {

                    User = System.Text.Json.JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);
                    User.Error = null;
                    return User;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetUserDetailsAsync", "GetUserDetailsAsync");
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserAddDetails> GetUserAddDetails(int UserID)
        {
            UserAddDetails User = new UserAddDetails();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/userAccounts/getuseradddetails/{UserID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        User = serializer.Deserialize<UserAddDetails>(reader);
                    }

                    return User;
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        error = serializer.Deserialize<ErrorClass>(reader);
                        HandleError(new Exception(error.ErrorMessage), "GetUserAddDetails", "GetUserAddDetails");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "GetUserAddDetails", "GetUserAddDetails");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DowngradeUserAccount(int UserID)
        {
            UserAddDetails User = new UserAddDetails();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/userAccounts/downgrageuseraccount/{UserID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        error = serializer.Deserialize<ErrorClass>(reader);
                        HandleError(new Exception(error.ErrorMessage), "DowngradeUserAccount", "DowngradeUserAccount");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "DowngradeUserAccount", "DowngradeUserAccount");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> PostUserAddDetails(UserAddDetails User)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<UserAddDetails>(User, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/userAccounts/postuseradddetails", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "PostUserAddDetails", "PostUserAddDetails");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UploadUserProfilePicture(int UserID, FileResult File)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                var content = new MultipartFormDataContent
                {
                    { new StreamContent(await File.OpenReadAsync()), "file", File.FileName }
                };

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/userAccounts/uploaduserprofilepicture/{UserID}", content).Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        return "OK";
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "UploadProfilePicture", "UploadUserProfilePicture");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Stream> DownloadUserProfilePicture(int UserID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/userAccounts/downloaduserprofilepicture/{UserID}").Result;

                if (response.IsSuccessStatusCode)
                {
                     return await response.Content.ReadAsStreamAsync();        
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (Stream s = response.Content.ReadAsStreamAsync().Result)
                    using (StreamReader sr = new StreamReader(s))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DownloadProfilePicture", "DownloadProfilePicture");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Budgets> GetBudgetDetailsAsync(int BudgetID, string Mode)
        {
            Budgets Budget = new Budgets();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

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

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetBudgetDetailsAsync", "GetBudgetDetailsAsync");
                throw new Exception(ex.Message);
            }
        }

        public async Task<DateTime> GetBudgetNextIncomePayDayAsync(int BudgetID)
        {
            Budgets Budget = new Budgets();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/nextincomepayday/{BudgetID}").Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {

                    Budget = System.Text.Json.JsonSerializer.Deserialize<Budgets>(content, _jsonSerialiserOptions);

                    return Budget.NextIncomePayday ?? DateTime.MinValue;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetBudgetNextIncomePayDayAsync", "GetBudgetNextIncomePayDayAsync");
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GetBudgetDaysBetweenPayDay(int BudgetID)
        {
            Budgets Budget = new Budgets();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/daystopaydaynext/{BudgetID}").Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    
                    Budget = System.Text.Json.JsonSerializer.Deserialize<Budgets>(content, _jsonSerialiserOptions);

                    return Budget.AproxDaysBetweenPay.GetValueOrDefault();
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetBudgetDaysBetweenPayDay", "GetBudgetDaysBetweenPayDay");
                throw new Exception(ex.Message);
            }
        }

        public async Task<DateTime?> GetBudgetLastUpdatedAsync(int BudgetID)
        {
            Budgets Budget = new Budgets();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/getlastupdated/{BudgetID}").Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {

                    Budget = System.Text.Json.JsonSerializer.Deserialize<Budgets>(content, _jsonSerialiserOptions);

                    return Budget.LastUpdated;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetBudgetLastUpdatedAsync", "GetBudgetLastUpdatedAsync");
                throw new Exception(ex.Message);
            }
        }

        public async Task<DateTime> GetBudgetValuesLastUpdatedAsync(int BudgetID, string Page)
        {
            Budgets Budget = new Budgets();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/getbudgetvalueslastupdated/{BudgetID}").Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {

                    Budget = System.Text.Json.JsonSerializer.Deserialize<Budgets>(content, _jsonSerialiserOptions);

                    return Budget.BudgetValuesLastUpdated;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    HandleError(new Exception(error.ErrorMessage), Page, "GetBudgetValuesLastUpdatedAsync");

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, Page, "GetBudgetValuesLastUpdatedAsync");
                throw new Exception(ex.Message);
            }
        }

        public async Task<BudgetSettings> GetBudgetSettings(int BudgetID)
        {
            BudgetSettings BudgetSettings = new BudgetSettings();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getbudgetsettings/{BudgetID}").Result;
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
                HandleError(ex, "GetBudgetSettings", "GetBudgetSettings");
                throw new Exception(ex.Message);
            }
        }

        public async Task<BudgetSettingValues> GetBudgetSettingsValues(int BudgetID)
        {
            BudgetSettingValues BudgetSettings = new BudgetSettingValues();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getbudgetsettingsvalues/{BudgetID}").Result;
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
                HandleError(ex, "GetBudgetSettingsValues", "GetBudgetSettingsValues");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Budgets> CreateNewBudget(string UserEmail, string? BudgetType = "Basic")
        {
            Budgets Budget = new Budgets();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/createnewbudget/{UserEmail}/{BudgetType}").Result;
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
                HandleError(ex, "CreateNewBudget", "CreateNewBudget");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteBudget(int BudgetID, int UserID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/deletebudget/{BudgetID}/{UserID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Dictionary<string,string> result = serializer.Deserialize<Dictionary<string, string>>(reader);
                            string returnString = result["result"];
                            return returnString;
                        }

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteBudget", "DeleteBudget");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> ReCalculateBudget(int BudgetID)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/recalculateBudget/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            return "OK";
                        }

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteBudget", "DeleteBudget");
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<string> DeleteUserAccount(int UserID)
        {
 
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/userAccounts/deleteaccount/{UserID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Dictionary<string,string> result = serializer.Deserialize<Dictionary<string, string>>(reader);
                            string returnString = result["result"];
                            return returnString;
                        }

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteUserAccount", "DeleteUserAccount");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<lut_CurrencySymbol>> GetCurrencySymbols(string SearchQuery)
        {
            List<lut_CurrencySymbol> Currencies = new List<lut_CurrencySymbol>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getcurrencysymbols/{SearchQuery}").Result;
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

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetCurrencySymbols", "GetCurrencySymbols");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<lut_CurrencyPlacement>> GetCurrencyPlacements(string Query)
        {
            List<lut_CurrencyPlacement> Placements = new List<lut_CurrencyPlacement>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getcurrencyplcements/{Query}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            Placements = serializer.Deserialize<List<lut_CurrencyPlacement>>(reader);
                        }

                        return Placements;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetCurrencyPlacements", "GetCurrencyPlacements");
                throw new Exception(ex.Message);
            }

        }

        public async Task<List<lut_BudgetTimeZone>> GetBudgetTimeZones(string Query)
        {
            List<lut_BudgetTimeZone> TimeZones = new List<lut_BudgetTimeZone>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getbudgettimezones/{Query}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            TimeZones = serializer.Deserialize<List<lut_BudgetTimeZone>>(reader);
                        }

                        return TimeZones;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetBudgetTimeZones", "GetBudgetTimeZones");
                throw new Exception(ex.Message);
            }

        }

        public async Task<List<lut_DateFormat>> GetDateFormatsByString(string SearchQuery)
        {
            List<lut_DateFormat> DateFormats = new List<lut_DateFormat>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getdateformatsbystring/{SearchQuery}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            DateFormats = serializer.Deserialize<List<lut_DateFormat>>(reader);
                            
                        }

                        return DateFormats;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetDateFormatsByString", "GetDateFormatsByString");
                throw new Exception(ex.Message);
            }
        }

        public async Task<lut_DateFormat> GetDateFormatsById(int ShortDatePattern, int Seperator)
        {
            lut_DateFormat DateFormat = new lut_DateFormat();


            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getdateformatsbyid/{ShortDatePattern}/{Seperator}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            DateFormat = serializer.Deserialize<lut_DateFormat>(reader);
                            return DateFormat;
                        }                        

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetDateFormatsByString", "GetDateFormatsByString");
                throw new Exception(ex.Message);
            }
        }
        public async Task<lut_BudgetTimeZone> GetTimeZoneById(int TimeZoneID)
        {
            lut_BudgetTimeZone TimeZone = new lut_BudgetTimeZone();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/gettimezonebyid/{TimeZoneID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            TimeZone = serializer.Deserialize<lut_BudgetTimeZone>(reader);
                        }

                        return TimeZone;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetTimeZoneById", "GetTimeZoneById");
                throw new Exception(ex.Message);
            }
        }

        public async Task<lut_NumberFormat> GetNumberFormatsById(int CurrencyDecimalDigits, int CurrencyDecimalSeparator, int CurrencyGroupSeparator)
        {
            lut_NumberFormat NumberFormat = new lut_NumberFormat();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getnumberformatsbyid/{CurrencyDecimalDigits}/{CurrencyDecimalSeparator}/{CurrencyGroupSeparator}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            NumberFormat = serializer.Deserialize<lut_NumberFormat>(reader);
                        }

                        return NumberFormat;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetNumberFormatsById", "GetNumberFormatsById");
                throw new Exception(ex.Message);
            }
        }

        public async Task<lut_ShortDatePattern> GetShortDatePatternById(int ShortDatePatternID)
        {
            lut_ShortDatePattern ShaortDatePattern = new lut_ShortDatePattern();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getshortdatepatternbyid/{ShortDatePatternID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            ShaortDatePattern = serializer.Deserialize<lut_ShortDatePattern>(reader);
                        }

                        return ShaortDatePattern;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetShortDatePatternById", "GetShortDatePatternById");
                throw new Exception(ex.Message);
            }
        }

        public async Task<lut_DateSeperator> GetDateSeperatorById(int DateSeperatorID)
        {
            lut_DateSeperator DateSeperator = new lut_DateSeperator();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }


                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getdateseperatorbyid/{DateSeperatorID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            DateSeperator = serializer.Deserialize<lut_DateSeperator>(reader);
                        }

                        return DateSeperator;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetDateSeperatorById", "GetDateSeperatorById");
                throw new Exception(ex.Message);
            }
        }

        public async Task<lut_CurrencyGroupSeparator> GetCurrencyGroupSeparatorById(int CurrencyGroupSeparatorId)
        {
            lut_CurrencyGroupSeparator GroupSeparator = new lut_CurrencyGroupSeparator();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getcurrencygroupseparatorbyid/{CurrencyGroupSeparatorId}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            GroupSeparator = serializer.Deserialize<lut_CurrencyGroupSeparator>(reader);
                        }

                        return GroupSeparator;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetCurrencyGroupSeparatorById", "GetCurrencyGroupSeparatorById");
                throw new Exception(ex.Message);
            }
        }
        public async Task<lut_CurrencyDecimalSeparator> GetCurrencyDecimalSeparatorById(int CurrencyDecimalSeparatorId)
        {
            lut_CurrencyDecimalSeparator DecimalSeparator = new lut_CurrencyDecimalSeparator();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getcurrencydecimalseparatorbyid/{CurrencyDecimalSeparatorId}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            DecimalSeparator = serializer.Deserialize<lut_CurrencyDecimalSeparator>(reader);
                        }

                        return DecimalSeparator;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetCurrencyDecimalSeparatorById", "GetCurrencyDecimalSeparatorById");
                throw new Exception(ex.Message);
            }
        }
        public async Task<lut_CurrencyDecimalDigits> GetCurrencyDecimalDigitsById(int CurrencyDecimalDigitsId)
        {
            lut_CurrencyDecimalDigits DecimalDigits = new lut_CurrencyDecimalDigits();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getcurrencydecimaldigitsbyid/{CurrencyDecimalDigitsId}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            DecimalDigits = serializer.Deserialize<lut_CurrencyDecimalDigits>(reader);
                        }

                        return DecimalDigits;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetCurrencyDecimalDigitsById", "GetCurrencyDecimalDigitsById");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<lut_NumberFormat>> GetNumberFormats()
        {
            List<lut_NumberFormat> NumberFormat = new List<lut_NumberFormat>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgetsettings/getnumberformats").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            NumberFormat = serializer.Deserialize<List<lut_NumberFormat>>(reader);
                        }

                        return NumberFormat;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetNumberFormats", "GetNumberFormats");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> UpdatePayPeriodStats(PayPeriodStats Stats)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<PayPeriodStats>(Stats, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/budgets/updatepayperiodstats", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdatePayPeriodStats", "UpdatePayPeriodStats");
                throw new Exception(ex.Message);
            }
        }

        public async Task<PayPeriodStats> CreateNewPayPeriodStats(int BudgetID)
        {
            PayPeriodStats stats = new PayPeriodStats();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/createnewpayperiodstats/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            stats = serializer.Deserialize<PayPeriodStats>(reader);
                        }

                        return stats;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "CreateNewPayPeriodStats", "CreateNewPayPeriodStats");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchBudget(int BudgetID, List<PatchDoc> PatchDoc)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PatchAsync($"{_url}/budgets/updatebudget/{BudgetID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "PatchBudget", "PatchBudget");
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> PatchBudgetSettings(int BudgetID, List<PatchDoc> PatchDoc)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PatchAsync($"{_url}/budgetsettings/updatebudgetsettings/{BudgetID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "PatchBudgetSettings", "PatchBudgetSettings");
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> UpdateBudgetSettings(int BudgetID, BudgetSettings BS)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<BudgetSettings>(BS, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PutAsync($"{_url}/budgetsettings/{BudgetID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                HandleError(ex, "UpdateBudgetSettings", "UpdateBudgetSettings");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Bills> GetBillFromID(int BillID)
        {
            Bills Bill = new Bills();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/bills/getbillfromid/{BillID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {

                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        Bill = serializer.Deserialize<Bills>(reader);
                    }

                    return Bill;
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetBillFromID", "GetBillFromID");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string>SaveNewBill(Bills Bill, int BudgetID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Bills>(Bill, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/bills/savenewbill/{BudgetID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }       
            }
            catch (Exception ex)
            {
                HandleError(ex, "SaveNewBill", "SaveNewBill");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string>UpdateBill(Bills Bill)
        {

            try
            {
                if (!CheckNetworkConnection().Result)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Bills>(Bill, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/bills/updatebill", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }       
            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdateBill", "UpdateBill");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchBill(int BillID, List<PatchDoc> PatchDoc)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response =  _httpClient.PatchAsync($"{_url}/bills/patchbill/{BillID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "PatchBill", "PatchBill");
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> DeleteBill(int BillID)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/bills/deletebill/{BillID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            return "OK";
                        }
                                                
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteBill", "DeleteBill");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Bills>> GetBudgetBills(int BudgetID, string page)
        {
            List<Bills> Bills = new List<Bills>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/bills/getbudgetbills/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Bills = serializer.Deserialize<List<Bills>>(reader);
                        }

                        return Bills;
                                                
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "GetAllBudgetSavings");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Savings> GetSavingFromID(int SavingID)
        {
            Savings Saving = new Savings();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/savings/getsavingfromid/{SavingID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            Saving = serializer.Deserialize<Savings>(reader);
                        }

                        return Saving;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetSavingFromID", "GetSavingFromID");
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> SaveNewSaving(Savings Saving, int BudgetID)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Savings>(Saving, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/savings/savenewsaving/{BudgetID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    int SavingID = System.Text.Json.JsonSerializer.Deserialize<int>(content, _jsonSerialiserOptions);
                    return SavingID;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "SaveNewSaving", "SaveNewSaving");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UpdateSaving(Savings Saving)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Savings>(Saving, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/savings/updatesaving", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "SaveNewSaving", "SaveNewSaving");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchSaving(int SavingID, List<PatchDoc> PatchDoc)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PatchAsync($"{_url}/savings/patchsaving/{SavingID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "PatchSaving", "PatchSaving");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteSaving(int SavingID)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/savings/deletesaving/{SavingID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            return "OK";
                        }

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteSaving", "DeleteSaving");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Savings>> GetAllBudgetSavings(int BudgetID)
        {
            List<Savings> Savings = new List<Savings>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/savings/getallbudgetsavings/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Savings = serializer.Deserialize<List<Savings>>(reader);
                        }

                        return Savings;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetAllBudgetSavings", "GetAllBudgetSavings");
                throw new Exception(ex.Message);
            }
        }

        public async Task<IncomeEvents> GetIncomeFromID(int IncomeID)
        {
            IncomeEvents Income = new IncomeEvents();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/incomes/getincomefromid/{IncomeID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {

                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        Income = serializer.Deserialize<IncomeEvents>(reader);
                    }

                    return Income;
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetIncomeFromID", "GetIncomeFromID");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string>SaveNewIncome(IncomeEvents Income, int BudgetID)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<IncomeEvents>(Income, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/incomes/savenewincome/{BudgetID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }       
            }
            catch (Exception ex)
            {
                HandleError(ex, "SaveNewIncome", "SaveNewIncome");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string>UpdateIncome(IncomeEvents Income)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<IncomeEvents>(Income, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/incomes/updateincome", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }       
            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdateIncome", "UpdateIncome");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchIncome(int IncomeID, List<PatchDoc> PatchDoc)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response =  _httpClient.PatchAsync($"{_url}/incomes/patchincome/{IncomeID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdateIncome", "UpdateIncome");
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> DeleteIncome(int IncomeID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/incomes/deleteincome/{IncomeID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            return "OK";
                        }
                                                
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteIncome", "DeleteIncome");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IncomeEvents>> GetBudgetIncomes(int BudgetID, string page)
        {
            List<IncomeEvents> IncomeEvents = new List<IncomeEvents>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/incomes/getbudgetincomeevents/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            IncomeEvents = serializer.Deserialize<List<IncomeEvents>>(reader);
                        }

                        return IncomeEvents;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "GetBudgetIncomes");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> UpdateBudgetValues(int budgetID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/updatebudgetvalues/{budgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            return "OK";
                        }

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdateBudgetValues", "UpdateBudgetValues");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Transactions> SaveNewTransaction(Transactions Transaction, int BudgetID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Transactions>(Transaction, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/transactions/savenewtransaction/{BudgetID}", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    Transactions ReturnTransactions = System.Text.Json.JsonSerializer.Deserialize<Transactions>(content, _jsonSerialiserOptions);
                    return ReturnTransactions;
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "SaveNewTransaction", "SaveNewTransaction");
                throw new Exception(ex.Message);
            }
        }
        public async Task<Transactions> TransactTransaction(int TransactionID)
        {
            Transactions Transaction = new Transactions();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/transactions/transacttransaction/{TransactionID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))
                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            Transaction = serializer.Deserialize<Transactions>(reader);
                        }

                        return Transaction;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "TransactTransaction", "TransactTransaction");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> UpdateTransaction(Transactions Transaction)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Transactions>(Transaction, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/transactions/updatetransaction", request).Result;
                string content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdateTransaction", "UpdateTransaction");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteTransaction(int TransactionID)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/transactions/deletetransaction/{TransactionID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            return "OK";
                        }

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteTransaction", "DeleteTransaction");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetBudgetEventTypes(int BudgetID)
        {
            List<string> EventTypes = new List<string>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/transactions/getbudgeteventtypes/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            EventTypes = serializer.Deserialize<List<string>>(reader);
                        }
                        return EventTypes;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {

                HandleError(ex, "GetBudgetEventTypes", "GetBudgetEventTypes");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Transactions> GetTransactionFromID(int TransactionID)
        {
            Transactions Transaction = new Transactions();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/transactions/gettransactionfromid/{TransactionID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))
                if (response.IsSuccessStatusCode)
                {

                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        Transaction = serializer.Deserialize<Transactions>(reader);
                    }

                    return Transaction;
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetTransactionFromID", "GetTransactionFromID");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Budgets> GetAllBudgetTransactions(int BudgetID)
        {
            Budgets Budget = new Budgets();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/transactions/getallbudgettransactions/{BudgetID}").Result;
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

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetAllBudgetTransactions", "GetAllBudgetTransactions");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Transactions>> GetRecentTransactions(int BudgetID, int NumberOf, string page)
        {
            List<Transactions> transactions = new List<Transactions>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/transactions/getrecenttransactions/{BudgetID}/{NumberOf}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            transactions = serializer.Deserialize<List<Transactions>>(reader);
                        }

                        return transactions;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }


                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "GetRecentTransactions");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Transactions>> GetCurrentPayPeriodTransactions(int BudgetID, string page)
        {
            List<Transactions> transactions = new List<Transactions>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/transactions/getcurrentpayperiodtransactions/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            transactions = serializer.Deserialize<List<Transactions>>(reader);
                        }

                        return transactions;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }
                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "GetCurrentPayPeriodTransactions");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Transactions>> GetFilteredTransactions(int BudgetID, FilterModel Filters, string page)
        {
            List<Transactions> transactions = new List<Transactions>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<FilterModel>(Filters, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/transactions/getfilteredtransactions/{BudgetID}", request).Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            transactions = serializer.Deserialize<List<Transactions>>(reader);
                        }

                        return transactions;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "GetCurrentPayPeriodTransactions");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Transactions>> GetRecentTransactionsOffset(int BudgetID, int NumberOf, int Offset ,string page)
        {
            List<Transactions> transactions = new List<Transactions>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/transactions/getrecenttransactionsoffset/{BudgetID}/{NumberOf}/{Offset}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            transactions = serializer.Deserialize<List<Transactions>>(reader);
                        }

                        return transactions;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }
                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "GetRecentTransactionsOffset");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Budgets> SaveBudgetDailyCycle(Budgets budget)
        {
  
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Budgets>(budget, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/budgets/savebudgetdailycycle", request).Result;

                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        budget = serializer.Deserialize<Budgets>(reader);
                    }

                    return budget;
                }
                else
                {   
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "SaveBudgetDailyCycle", "SaveBudgetDailyCycle");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> CreateNewOtpCodeShareBudget(int UserID, int ShareBudgetID)
        {
            OTP UserOTP = new OTP();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/otp/createnewotpcodesharebudget/{UserID}/{ShareBudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return "MaxLimit";
                    }
                    else if (response.IsSuccessStatusCode)
                    {

                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            UserOTP = serializer.Deserialize<OTP>(reader);
                        }

                        if (UserOTP.OTPID == 0)
                        {
                            return "Error";
                        }
                        else
                        {
                            return "OK";
                        }

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "CreateNewOtpCodeShareBudget", "CreateNewOtpCodeShareBudget");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> CreateNewOtpCode(int UserID, string OTPType)
        {
            OTP UserOTP = new OTP();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/otp/createnewotpcode/{UserID}/{OTPType}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))
                if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    return "MaxLimit";
                }
                else if (response.IsSuccessStatusCode)
                {

                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        UserOTP = serializer.Deserialize<OTP>(reader);
                    }

                    if (UserOTP.OTPID == 0)
                    {
                        return "Error";
                    }
                    else
                    {
                        return "OK";
                    }
                        
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "CreateNewOtpCode", "CreateNewOtpCode");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> ValidateOTPCodeEmail(OTP UserOTP)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<OTP>(UserOTP, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/otp/validateotpcodeemail", request).Result;

                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return "Error";
                }
                else if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "ValidateOTPCodeEmail", "ValidateOTPCodeEmail");
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GetUserIdFromEmail(string UserEmail)
        {
            UserDetailsModel User = new UserDetailsModel();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/otp/getuseridfromemail/{UserEmail}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return 0;
                }
                else if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        User = serializer.Deserialize<UserDetailsModel>(reader);
                    }

                    return User.UserID;
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetUserIdFromEmail", "GetUserIdFromEmail");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetPayeeList(int BudgetID)
        {
            List<string>? Payee = new List<string>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/payee/getpayeelist/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Payee = serializer.Deserialize<List<string>>(reader);
                    }

                    return Payee;
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetPayeeList", "GetPayeeList");
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<List<Payees>> GetPayeeListFull(int BudgetID)
        {
            List<Payees>? Payee = new List<Payees>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/payee/getpayeelistfull/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Payee = serializer.Deserialize<List<Payees>>(reader);
                        }

                        return Payee;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                HandleError(ex, "GetPayeeListFull", "GetPayeeListFull");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Categories> GetPayeeLastCategory(int BudgetID, string PayeeName)
        {
            Categories Category = new Categories();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/payee/getpayeelastcategory/{BudgetID}/{PayeeName}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Category = serializer.Deserialize<Categories>(reader);
                        }

                        return Category;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetPayeeLastCategory", "GetPayeeLastCategory");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeletePayee(int BudgetID, string OldPayeeName, string NewPayeeName)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/payee/deletepayee/{BudgetID}/{OldPayeeName}/{NewPayeeName}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        return "OK";
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }
                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                HandleError(ex, "DeletePayee", "DeletePayee");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UpdatePayee(int BudgetID, string OldPayeeName, string NewPayeeName)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/payee/updatepayee/{BudgetID}/{OldPayeeName}/{NewPayeeName}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        return "OK";
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }
                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                HandleError(ex, "UpdatePayee", "UpdatePayee");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Categories>> GetCategories(int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();


            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/categories/getcategories/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            categories = serializer.Deserialize<List<Categories>>(reader);
                        }

                        return categories;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetCategories", "GetCategories");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Categories> GetCategoryFromID(int CategoryID)
        {
            Categories? Category = new Categories();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/categories/getcategoryfromid/{CategoryID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Category = serializer.Deserialize<Categories>(reader);
                        }

                        return Category;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetCategoryFromID", "GetCategoryFromID");
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> AddNewCategory(int BudgetID, DefaultCategories Category)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                int CategoryID;

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<DefaultCategories>(Category, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/categories/addnewcategory/{BudgetID}", request).Result;

                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            CategoryID = serializer.Deserialize<int>(reader);
                        }

                        return CategoryID;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "AddNewCategory", "AddNewCategory");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Categories> AddNewSubCategory(int BudgetID, Categories Category)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Categories>(Category, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/categories/addnewsubcategory/{BudgetID}", request).Result;

                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Category = serializer.Deserialize<Categories>(reader);
                        }

                        return Category;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "AddNewSubCategory", "AddNewSubCategory");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchCategory(int CategoryID, List<PatchDoc> PatchDoc)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PatchAsync($"{_url}/categories/patchcategory/{CategoryID}", request).Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))                    

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }
                        throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "PatchCategory", "PatchCategory");
                throw new Exception(ex.Message);
            }

        }
        public async Task<string> UpdateAllTransactionsCategoryName(int CategoryID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/categories/Updatealltransactionscategoryname/{CategoryID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        return "OK";
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }
                        throw new Exception(error.ErrorMessage);
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdateAllTransactionsCategoryName", "UpdateAllTransactionsCategoryName");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Categories>> GetAllHeaderCategoryDetailsFull(int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/categories/getallheadercategorydetailsfull/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            categories = serializer.Deserialize<List<Categories>>(reader);
                        }

                        return categories;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "ViewCategories", "GetAllHeaderCategoryDetailsFull");
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<Categories>> GetHeaderCategoryDetailsFull(int CategoryID, int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/categories/getheadercategorydetailsfull/{CategoryID}/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            categories = serializer.Deserialize<List<Categories>>(reader);
                        }

                        return categories;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "ViewCategories", "GetHeaderCategoryDetailsFull");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteCategory(int CategoryID, bool IsReassign, int ReAssignID)
        {
            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/categories/deletecategory/{CategoryID}/{IsReassign}/{ReAssignID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        return "OK";
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }
                        throw new Exception(error.ErrorMessage);
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteCategory", "DeleteCategory");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Dictionary<string, int>> GetAllCategoryNames(int BudgetID)
        {
            Dictionary<string, int> Categories = new Dictionary<string, int>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/categories/getallcategorynames/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Categories = serializer.Deserialize<Dictionary<string, int>>(reader);
                        }

                        return Categories;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }
                        throw new Exception(error.ErrorMessage);
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "GetAllCategoryNames", "GetAllCategoryNames");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Savings>> GetBudgetEnvelopeSaving(int BudgetID)
        {
            List<Savings>? Savings = new List<Savings>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/savings/getbudgetenvelopesaving/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Savings = serializer.Deserialize<List<Savings>>(reader);
                        }

                        return Savings;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetBudgetEnvelopeSaving", "GetBudgetEnvelopeSaving");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Savings>> GetBudgetRegularSaving(int BudgetID)
        {
            List<Savings>? Savings = new List<Savings>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/savings/getbudgetregularsaving/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            Savings = serializer.Deserialize<List<Savings>>(reader);
                        }

                        return Savings;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetBudgetRegularSaving", "GetBudgetRegularSaving");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> ShareBudgetRequest(ShareBudgetRequest BudgetShare)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<ShareBudgetRequest>(BudgetShare, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/budgets/sharebudgetrequest", request).Result;

                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            BudgetShare = serializer.Deserialize<ShareBudgetRequest>(reader);
                        }

                        if (BudgetShare.SharedBudgetID != 0)
                        {
                            return "OK";
                        }
                        else
                        {
                            return "Not Saved";
                        }                        
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        if (error.ErrorMessage == "User Not Found")
                        {
                            return "User Not Found";
                        }
                        else if(error.ErrorMessage == "Budget Already Shared")
                        {
                            return "Budget Already Shared";
                        }
                        else if (error.ErrorMessage == "Share Request Active")
                        {
                            return "Share Request Active";
                        }
                        else
                        {
                            throw new Exception(error.ErrorMessage);
                        }
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "ShareBudgetRequest", "ShareBudgetRequest");
                throw new Exception(ex.Message);
            }
        }

        public async Task<FirebaseDevices> RegisterNewFirebaseDevice(FirebaseDevices NewDevice)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<FirebaseDevices>(NewDevice, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/firebasedevices/registernewfirebasedevice", request).Result;

                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        NewDevice = serializer.Deserialize<FirebaseDevices>(reader);
                    }

                    return NewDevice;

                }  
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                    throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "RegisterNewFirebaseDevice", "RegisterNewFirebaseDevice");
                throw new Exception(ex.Message);
            }
        }

        public async Task<FirebaseDevices> UpdateDeviceUserDetails(FirebaseDevices NewDevice)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<FirebaseDevices>(NewDevice, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/firebasedevices/updatedeviceuserdetails", request).Result;

                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            NewDevice = serializer.Deserialize<FirebaseDevices>(reader);
                        }

                        return NewDevice;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdateDeviceUserDetails", "UpdateDeviceUserDetails");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> ValidateOTPCodeShareBudget(OTP UserOTP, int SharedBudgetRequestID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                string jsonRequest = System.Text.Json.JsonSerializer.Serialize<OTP>(UserOTP, _jsonSerialiserOptions);
                StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = _httpClient.PostAsync($"{_url}/otp/validateotpcodesharebudget/{SharedBudgetRequestID}", request).Result;

                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);
                    }

                        throw new Exception(error.ErrorMessage);
                }
            }
            catch (Exception ex)
            {

                HandleError(ex, "PopUpOTP", "ValidateOTPCodeShareBudget");
                throw new Exception(ex.Message);
            }

        }

        public async Task<ShareBudgetRequest> GetShareBudgetRequestByID(int SharedBudgetRequestID)
        {
            ShareBudgetRequest ShareRequest = new ShareBudgetRequest();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/getsharebudgetrequestbyid/{SharedBudgetRequestID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            ShareRequest = serializer.Deserialize<ShareBudgetRequest>(reader);
                        }

                        return ShareRequest;

                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetShareBudgetRequestByID", "GetShareBudgetRequestByID");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> CancelCurrentShareBudgetRequest(int BudgetID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/cancelcurrentsharebudgetrequest/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        return "OK";
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "CancelCurrentShareBudgetRequest", "CancelCurrentShareBudgetRequest");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> StopSharingBudget(int BudgetID)
        {

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/budgets/stopsharingbudget/{BudgetID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        return "OK";
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "StopSharingBudget", "StopSharingBudget");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Budgets>> GetUserAccountBudgets(int UserID, string page)
        {
            List<Budgets> budgets = new List<Budgets>();

            try
            {
                bool IsNetworkConnection = await CheckNetworkConnection();
                if (!IsNetworkConnection)
                {
                    throw new HttpRequestException("Connectivity");
                }

                HttpResponseMessage response = _httpClient.GetAsync($"{_url}/userAccounts/getuseraccountbudgets/{UserID}").Result;
                using (Stream s = response.Content.ReadAsStreamAsync().Result)
                using (StreamReader sr = new StreamReader(s))

                    if (response.IsSuccessStatusCode)
                    {
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            budgets = serializer.Deserialize<List<Budgets>>(reader);
                        }

                        return budgets;
                    }
                    else
                    {
                        ErrorClass error = new ErrorClass();
                        using (JsonReader reader = new JsonTextReader(sr))
                        {
                            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                            error = serializer.Deserialize<ErrorClass>(reader);
                        }

                        throw new Exception(error.ErrorMessage);
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "ValidateOTPCodeShareBudget");
                throw new Exception(ex.Message);
            }
        }

    }
}
