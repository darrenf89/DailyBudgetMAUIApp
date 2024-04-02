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
            _url = $"{_baseAddress}api/v1";

            _jsonSerialiserOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private async void HandleError(Exception ex, string Page, string Method)
        {
            if (ex.Message == "Connectivity")
            {

            }
            else
            {
                ErrorLog Error = new ErrorLog(ex, Page, Method);
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        public async Task<string> PatchUserAccount(int UserID, List<PatchDoc> PatchDoc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get patch User Account in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }

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
                    return error.ErrorMessage;
                }
                
            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get User salt in DataRestServices --> {ex.Message}");
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
                    error.StatusCode = response.StatusCode;
                    UserModel.Error = error;
                    return UserModel;
                }       
            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to register user in DataRestServices --> {ex.Message}");
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

        public async Task<string> UploadUserProfilePicture(int UserID, FileResult File)
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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

                        HandleError(new Exception(error.ErrorMessage), "UploadProfilePicture", "UploadUserProfilePicture");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "UploadProfilePicture", "UploadUserProfilePicture");
                return null;
            }
        }

        public async Task<Stream> DownloadUserProfilePicture(int UserID)
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                    HandleError(new Exception(error.ErrorMessage), "DownloadProfilePicture", "DownloadProfilePicture");
                    return null;
                }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DownloadProfilePicture", "DownloadProfilePicture");
                return null;
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

                HttpResponseMessage response =  _httpClient.PostAsync($"{_url}/error/adderrorlogentry", request).Result;
                string content =  response.Content.ReadAsStringAsync().Result;

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
        public async Task<DateTime> GetBudgetNextIncomePayDayAsync(int BudgetID)
        {
            Budgets Budget = new Budgets();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                    throw new Exception(response.StatusCode.ToString() + " - " + error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Budget next income pay day in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GetBudgetDaysBetweenPayDay(int BudgetID)
        {
            Budgets Budget = new Budgets();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                    throw new Exception(response.StatusCode.ToString() + " - " + error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Budget day between pay in DataRestServices --> {ex.Message}");
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
                    throw new Exception(response.StatusCode.ToString() + " - " + error.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Budget last updated date in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<DateTime> GetBudgetValuesLastUpdatedAsync(int BudgetID, string Page)
        {
            Budgets Budget = new Budgets();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                    
                    
                    return new DateTime();
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, Page, "GetBudgetValuesLastUpdatedAsync");

                return new DateTime();
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Budget Settings values in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Budgets> CreateNewBudget(string UserEmail, string? BudgetType = "Basic")
        {
            Budgets Budget = new Budgets();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to  Create new Budget in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteBudget(int BudgetID, int UserID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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

                        HandleError(new Exception(error.ErrorMessage), "DeleteBudget", "DeleteBudget");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteBudget", "DeleteBudget");
                return null;
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get currency symbols in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<lut_CurrencyPlacement>> GetCurrencyPlacements(string Query)
        {
            List<lut_CurrencyPlacement> Placements = new List<lut_CurrencyPlacement>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get curreny placements in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }

        }

        public async Task<List<lut_BudgetTimeZone>> GetBudgetTimeZones(string Query)
        {
            List<lut_BudgetTimeZone> TimeZones = new List<lut_BudgetTimeZone>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get TimeZones in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }

        }

        public async Task<List<lut_DateFormat>> GetDateFormatsByString(string SearchQuery)
        {
            List<lut_DateFormat> DateFormats = new List<lut_DateFormat>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get date format by string in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<lut_DateFormat> GetDateFormatsById(int ShortDatePattern, int Seperator)
        {
            lut_DateFormat DateFormat = new lut_DateFormat();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get date format by id in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<lut_BudgetTimeZone> GetTimeZoneById(int TimeZoneID)
        {
            lut_BudgetTimeZone TimeZone = new lut_BudgetTimeZone();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Time zone by id in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<lut_NumberFormat> GetNumberFormatsById(int CurrencyDecimalDigits, int CurrencyDecimalSeparator, int CurrencyGroupSeparator)
        {
            lut_NumberFormat NumberFormat = new lut_NumberFormat();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get get number format by id in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<lut_ShortDatePattern> GetShortDatePatternById(int ShortDatePatternID)
        {
            lut_ShortDatePattern ShaortDatePattern = new lut_ShortDatePattern();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "GetShortDatePatternById", "GetShortDatePatternById");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, "GetShortDatePatternById", "GetShortDatePatternById");
                return null;
            }
        }

        public async Task<lut_DateSeperator> GetDateSeperatorById(int DateSeperatorID)
        {
            lut_DateSeperator DateSeperator = new lut_DateSeperator();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "GetDateSeperatorById", "GetDateSeperatorById");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, "GetDateSeperatorById", "GetDateSeperatorById");
                return null;
            }
        }

        public async Task<lut_CurrencyGroupSeparator> GetCurrencyGroupSeparatorById(int CurrencyGroupSeparatorId)
        {
            lut_CurrencyGroupSeparator GroupSeparator = new lut_CurrencyGroupSeparator();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "GetCurrencyGroupSeparatorById", "GetCurrencyGroupSeparatorById");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, "GetCurrencyGroupSeparatorById", "GetCurrencyGroupSeparatorById");
                return null;
            }
        }
        public async Task<lut_CurrencyDecimalSeparator> GetCurrencyDecimalSeparatorById(int CurrencyDecimalSeparatorId)
        {
            lut_CurrencyDecimalSeparator DecimalSeparator = new lut_CurrencyDecimalSeparator();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "GetCurrencyDecimalSeparatorById", "GetCurrencyDecimalSeparatorById");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, "GetCurrencyDecimalSeparatorById", "GetCurrencyDecimalSeparatorById");
                return null;
            }
        }
        public async Task<lut_CurrencyDecimalDigits> GetCurrencyDecimalDigitsById(int CurrencyDecimalDigitsId)
        {
            lut_CurrencyDecimalDigits DecimalDigits = new lut_CurrencyDecimalDigits();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "GetCurrencyDecimalDigitsById", "GetCurrencyDecimalDigitsById");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, "GetCurrencyDecimalDigitsById", "GetCurrencyDecimalDigitsById");
                return null;
            }
        }

        public async Task<List<lut_NumberFormat>> GetNumberFormats()
        {
            List<lut_NumberFormat> NumberFormat = new List<lut_NumberFormat>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get get number format in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> UpdatePayPeriodStats(PayPeriodStats Stats)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to update payperiodstats in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<PayPeriodStats> CreateNewPayPeriodStats(int BudgetID)
        {
            PayPeriodStats stats = new PayPeriodStats();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get bill by id in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchBudget(int BudgetID, List<PatchDoc> PatchDoc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get patch budget in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> PatchBudgetSettings(int BudgetID, List<PatchDoc> PatchDoc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get patch budget in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> UpdateBudgetSettings(int BudgetID, BudgetSettings BS)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                Debug.WriteLine($"Error Trying to update budget settings in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Bills> GetBillFromID(int BillID)
        {
            Bills Bill = new Bills();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get bill by id in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string>SaveNewBill(Bills Bill, int BudgetID)
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to save new bill in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string>UpdateBill(Bills Bill)
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to update bill in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchBill(int BillID, List<PatchDoc> PatchDoc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to patch bill in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> DeleteBill(int BillID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to delete bill in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Bills>> GetBudgetBills(int BudgetID, string page)
        {
            List<Bills> Bills = new List<Bills>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                        
                        HandleError(new Exception(error.ErrorMessage), page, "GetAllBudgetSavings");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "GetAllBudgetSavings");
                return null;
            }
        }

        public async Task<Savings> GetSavingFromID(int SavingID)
        {
            Savings Saving = new Savings();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Saving by id in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> SaveNewSaving(Savings Saving, int BudgetID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to save a saving in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> UpdateSaving(Savings Saving)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to update a Saving in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchSaving(int SavingID, List<PatchDoc> PatchDoc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to Patch Saving in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteSaving(int SavingID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Delete Saving in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Savings>> GetAllBudgetSavings(int BudgetID)
        {
            List<Savings> Savings = new List<Savings>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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

                        HandleError(new Exception(error.ErrorMessage), "GetAllBudgetSavings", "GetAllBudgetSavings");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "GetAllBudgetSavings", "GetAllBudgetSavings");
                return null;
            }
        }

        public async Task<IncomeEvents> GetIncomeFromID(int IncomeID)
        {
            IncomeEvents Income = new IncomeEvents();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Income Event by ID in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string>SaveNewIncome(IncomeEvents Income, int BudgetID)
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to save new income in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string>UpdateIncome(IncomeEvents Income)
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying update income in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchIncome(int IncomeID, List<PatchDoc> PatchDoc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying patch income in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> DeleteIncome(int IncomeID)
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get delete income in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<IncomeEvents>> GetBudgetIncomes(int BudgetID, string page)
        {
            List<IncomeEvents> IncomeEvents = new List<IncomeEvents>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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

                        HandleError(new Exception(error.ErrorMessage), page, "GetBudgetIncomes");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, page, "GetBudgetIncomes");
                return null;
            }
        }
        public async Task<string> UpdateBudgetValues(int budgetID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get delete income in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Transactions> SaveNewTransaction(Transactions Transaction, int BudgetID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying update income in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<Transactions> TransactTransaction(int TransactionID)
        {
            Transactions Transaction = new Transactions();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Transaction by ID in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> UpdateTransaction(Transactions Transaction)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying update transaction in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeleteTransaction(int TransactionID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Delete Transaction in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetBudgetEventTypes(int BudgetID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            List<string> EventTypes = new List<string>();

            try
            {

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

                        HandleError(new Exception(error.ErrorMessage), "GetBudgetEventTypes", "GetBudgetEventTypes");
                        return null;
                    }

            }
            catch (Exception ex)
            {

                HandleError(ex, "GetBudgetEventTypes", "GetBudgetEventTypes");
                return null;
            }
        }

        public async Task<Transactions> GetTransactionFromID(int TransactionID)
        {
            Transactions Transaction = new Transactions();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get Transaction by ID in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Budgets> GetAllBudgetTransactions(int BudgetID)
        {
            Budgets Budget = new Budgets();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get All Transactions by ID in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Transactions>> GetRecentTransactions(int BudgetID, int NumberOf, string page)
        {
            List<Transactions> transactions = new List<Transactions>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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


                        HandleError(new Exception(error.ErrorMessage), page, "GetRecentTransactions");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, page, "GetRecentTransactions");
                return null;
            }
        }

        public async Task<List<Transactions>> GetCurrentPayPeriodTransactions(int BudgetID, string page)
        {
            List<Transactions> transactions = new List<Transactions>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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


                        HandleError(new Exception(error.ErrorMessage), page, "GetCurrentPayPeriodTransactions");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying get transactions in DataRestServices --> {ex.Message}");
                HandleError(ex, page, "GetCurrentPayPeriodTransactions");
                return null;
            }
        }

        public async Task<List<Transactions>> GetFilteredTransactions(int BudgetID, FilterModel Filters, string page)
        {
            List<Transactions> transactions = new List<Transactions>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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


                        HandleError(new Exception(error.ErrorMessage), page, "GetCurrentPayPeriodTransactions");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying get transactions in DataRestServices --> {ex.Message}");
                HandleError(ex, page, "GetCurrentPayPeriodTransactions");
                return null;
            }
        }

        public async Task<List<Transactions>> GetRecentTransactionsOffset(int BudgetID, int NumberOf, int Offset ,string page)
        {
            List<Transactions> transactions = new List<Transactions>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), page, "GetRecentTransactions");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying Get Recent Transactions in DataRestServices --> {ex.Message}");
                HandleError(ex, page, "GetRecentTransactionsOffset");
                return null;
            }
        }

        public async Task<Budgets> SaveBudgetDailyCycle(Budgets budget)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying update budget in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> CreateNewOtpCode(int UserID, string OTPType)
        {
            OTP UserOTP = new OTP();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to create new OTP code in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> ValidateOTPCodeEmail(OTP UserOTP)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying validate otp code in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> GetUserIdFromEmail(string UserEmail)
        {
            UserDetailsModel User = new UserDetailsModel();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to create new OTP code in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetPayeeList(int BudgetID)
        {
            List<string>? Payee = new List<string>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get payee list in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        
        public async Task<List<Payees>> GetPayeeListFull(int BudgetID)
        {
            List<Payees>? Payee = new List<Payees>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "GetPayeeListFull", "GetPayeeListFull");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                HandleError(ex, "GetPayeeListFull", "GetPayeeListFull");
                return null;
            }
        }

        public async Task<Categories> GetPayeeLastCategory(int BudgetID, string PayeeName)
        {
            Categories Category = new Categories();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get latest payee category in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> DeletePayee(int BudgetID, string OldPayeeName, string NewPayeeName)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "DeletePayee", "DeletePayee");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                HandleError(ex, "DeletePayee", "DeletePayee");
                return null;
            }
        }

        public async Task<string> UpdatePayee(int BudgetID, string OldPayeeName, string NewPayeeName)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "UpdatePayee", "UpdatePayee");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                HandleError(ex, "UpdatePayee", "UpdatePayee");
                return null;
            }
        }

        public async Task<List<Categories>> GetCategories(int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get categories in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<Categories> GetCategoryFromID(int CategoryID)
        {
            Categories? Category = new Categories();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get categories in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> AddNewCategory(int BudgetID, DefaultCategories Category)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "AddNewCategory", "AddNewCategory");
                        return 0;
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "AddNewCategory", "AddNewCategory");
                return 0;
            }
        }

        public async Task<Categories> AddNewSubCategory(int BudgetID, Categories Category)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying validate otp code in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> PatchCategory(int CategoryID, List<PatchDoc> PatchDoc)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                    HandleError(new Exception(error.ErrorMessage), "PatchCategory", "PatchCategory");
                    return "";
                }
            }
            catch (Exception ex)
            {
                HandleError(ex, "PatchCategory", "PatchCategory");
                return "";
            }

        }
        public async Task<string> UpdateAllTransactionsCategoryName(int CategoryID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                        HandleError(new Exception(error.ErrorMessage), "UpdateAllTransactionsCategoryName", "UpdateAllTransactionsCategoryName");
                        return "";
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "UpdateAllTransactionsCategoryName", "UpdateAllTransactionsCategoryName");
                return "";
            }
        }

        public async Task<List<Categories>> GetAllHeaderCategoryDetailsFull(int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "ViewCategories", "GetAllHeaderCategoryDetailsFull");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "ViewCategories", "GetAllHeaderCategoryDetailsFull");
                return null;
            }
        }
        public async Task<List<Categories>> GetHeaderCategoryDetailsFull(int CategoryID, int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), "ViewCategories", "GetHeaderCategoryDetailsFull");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                HandleError(ex, "ViewCategories", "GetHeaderCategoryDetailsFull");
                return null;
            }
        }

        public async Task<string> DeleteCategory(int CategoryID, bool IsReassign, int ReAssignID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                        HandleError(new Exception(error.ErrorMessage), "DeleteCategory", "DeleteCategory");
                        return "";
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "DeleteCategory", "DeleteCategory");
                return "";
            }
        }

        public async Task<Dictionary<string, int>> GetAllCategoryNames(int BudgetID)
        {
            Dictionary<string, int> Categories = new Dictionary<string, int>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

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
                        HandleError(new Exception(error.ErrorMessage), "GetAllCategoryNames", "GetAllCategoryNames");
                        return null;
                    }
            }
            catch (Exception ex)
            {
                HandleError(ex, "GetAllCategoryNames", "GetAllCategoryNames");
                return null;
            }
        }

        public async Task<List<Savings>> GetBudgetEnvelopeSaving(int BudgetID)
        {
            List<Savings>? Savings = new List<Savings>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get envelope savings in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Savings>> GetBudgetRegularSaving(int BudgetID)
        {
            List<Savings>? Savings = new List<Savings>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get envelope savings in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }
        public async Task<string> ShareBudgetRequest(ShareBudgetRequest BudgetShare)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying share budget in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<FirebaseDevices> RegisterNewFirebaseDevice(FirebaseDevices NewDevice)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying registering firebase token in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<FirebaseDevices> UpdateDeviceUserDetails(FirebaseDevices NewDevice)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying registering firebase token in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> ValidateOTPCodeShareBudget(OTP UserOTP, int SharedBudgetRequestID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                    HandleError(new Exception(error.ErrorMessage), "PopUpOTP", "ValidateOTPCodeShareBudget");
                    return "Error";
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, "PopUpOTP", "ValidateOTPCodeShareBudget");
                return "Error";
            }

        }

        public async Task<ShareBudgetRequest> GetShareBudgetRequestByID(int SharedBudgetRequestID)
        {
            ShareBudgetRequest ShareRequest = new ShareBudgetRequest();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to get sharebudgetrequest in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> CancelCurrentShareBudgetRequest(int BudgetID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to cancel current sharebudgetrequest in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> StopSharingBudget(int BudgetID)
        {
            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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
                //Write Debug Line and then throw the exception to the next level of the stack to be handled
                Debug.WriteLine($"Error Trying to cancel current sharebudgetrequest in DataRestServices --> {ex.Message}");
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Budgets>> GetUserAccountBudgets(int UserID, string page)
        {
            List<Budgets> budgets = new List<Budgets>();

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {
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

                        HandleError(new Exception(error.ErrorMessage), page, "ValidateOTPCodeShareBudget");
                        return null;
                    }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying validate share budget in DataRestServices --> {ex.Message}");
                HandleError(ex, page, "ValidateOTPCodeShareBudget");
                return null;
            }
        }

    }
}
