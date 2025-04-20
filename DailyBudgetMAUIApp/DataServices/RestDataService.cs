using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailySpendWebApp.Models;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;
using System.Net;
using System.Text;
using System.Text.Json;
using static Android.Telecom.Call;
using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;


namespace DailyBudgetMAUIApp.DataServices
{
    //TODO: UPDATE ALL CALLS TO USE USING STREAM
    internal class RestDataService : IRestDataService
    {

        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;
        private readonly string _url;
        private readonly JsonSerializerOptions _jsonSerialiserOptions;

        private readonly int maxRetries = 5;
        private readonly int delayMilliseconds = 200;
        private readonly TimeSpan timeoutMilliseconds = TimeSpan.FromMilliseconds(8000);
        private DateTime LastServerHealthCheck;

        private bool IsRefreshingToken = false;

        private readonly ILogService _ls;

        public RestDataService(ILogService ls)
        {
            _httpClient = new HttpClient
            {
                Timeout = timeoutMilliseconds
            };
            //_baseAddress = "https://localhost:7141/";
            _baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "https://dailybudgetwebapi.azurewebsites.net/" : "https://dailybudgetwebapi.azurewebsites.net/";
            _url = $"{_baseAddress}api/v1";

            _jsonSerialiserOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _ls = ls;
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

                HttpResponseMessage response = PostHttpRequestAsync($"{_url}/error/adderrorlogentry", request).Result;
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

        public async Task<bool> CheckConnectionStrengthAsync()
        {
            return true;
            try
            {
                if (this.LastServerHealthCheck.AddMinutes(1) < DateTime.UtcNow)
                {
                    HttpClient pingClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromMilliseconds(60000)
                    };

                    var stopwatch = Stopwatch.StartNew();
                    //var request = new HttpRequestMessage(HttpMethod.Head, $"{_url}/healthCheck");
                    //HttpResponseMessage response = await pingClient.SendAsync(request);
                    HttpResponseMessage response = pingClient.GetAsync($"{_url}/healthCheck").Result;
                    stopwatch.Stop();

                    if (response.IsSuccessStatusCode)
                    {
                        double roundTripTime = stopwatch.Elapsed.TotalMilliseconds;
                        if (roundTripTime > 2000)
                        {
                            //TODO: SHOW A POPUP
                            if (App.CurrentPopUp != null)
                            {
                                await App.CurrentPopUp.CloseAsync();
                                App.CurrentPopUp = null;
                            }

                            if (App.CurrentPopUp == null)
                            {
                                var PopUp = new PopUpNoServer(new PopUpNoServerViewModel());
                                App.CurrentPopUp = PopUp;
                                Application.Current.Windows[0].Page.ShowPopup(PopUp);
                            }

                            int i = 0;
                            while (roundTripTime > 2000 && i < 2)
                            {
                                await Task.Delay(200);
                                stopwatch = Stopwatch.StartNew();
                                //request = new HttpRequestMessage(HttpMethod.Head, $"{_url}/healthCheck");
                                //response = await pingClient.SendAsync(request);
                                response = pingClient.GetAsync($"{_url}/healthCheck").Result;
                                stopwatch.Stop();

                                roundTripTime = stopwatch.Elapsed.TotalMilliseconds;
                                i++;
                            }

                            if (App.CurrentPopUp != null)
                            {
                                await App.CurrentPopUp.CloseAsync();
                                App.CurrentPopUp = null;
                            }

                            return true;
                        }
                        else
                        {
                            this.LastServerHealthCheck = DateTime.UtcNow;
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }

                //TODO: Use Azure Health Check when I pay for it.
                //using (var ping = new Ping())
                //{                    
                //    Send a ping request asynchronously
                //    PingReply reply = await ping.SendPingAsync($"{_url}/userAccounts/registeruser", 10000);

                //    if (reply.Status == IPStatus.Success)
                //    {
                //        long roundTripTime = reply.RoundtripTime;

                //        if (roundTripTime > 200)
                //        {
                //            //TODO: SHOW A POPUP
                //            if (App.CurrentPopUp != null)
                //            {
                //                await App.CurrentPopUp.CloseAsync();
                //                App.CurrentPopUp = null;
                //            }

                //            if (App.CurrentPopUp == null)
                //            {
                //                var PopUp = new PopUpNoNetwork(new PopUpNoNetworkViewModel());
                //                App.CurrentPopUp = PopUp;
                //                Application.Current.Windows[0].Page.ShowPopup(PopUp);
                //            }                        

                //            int i = 0;
                //            while (roundTripTime > 200 && i < 30)
                //            {
                //                await Task.Delay(200);
                //                reply = await ping.SendPingAsync(_baseAddress, 1000);
                //                roundTripTime = reply.RoundtripTime;
                //                i++;
                //            }

                //            if (App.CurrentPopUp != null)
                //            {
                //                await App.CurrentPopUp.CloseAsync();
                //                App.CurrentPopUp = null;
                //            }

                //            return roundTripTime < 200;
                //        }
                //        else
                //        {
                //            return true;
                //        }
                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<bool> CheckNetworkConnection()
        {

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                if (App.CurrentPopUp != null)
                {
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                }

                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpNoNetwork(new PopUpNoNetworkViewModel());
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Task.Delay(1);

                int i = 0;
                while (Connectivity.Current.NetworkAccess != NetworkAccess.Internet && i < 30)
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

            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        }

        public async Task CheckConnectivity()
        {
            bool IsNetworkConnection = await CheckNetworkConnection();
            if (!IsNetworkConnection)
            {
                throw new HttpRequestException("Connectivity");
            }

            bool IsServerConnection = await CheckConnectionStrengthAsync();
            if (!IsServerConnection)
            {
                throw new HttpRequestException("Server Connectivity");
            }
        }

        private async Task CheckAndUpdateSession()
        {
            if (IsRefreshingToken)
            {
                return;
            }

            string SessionString = SecureStorage.Default.GetAsync("Session").Result;
            if (string.IsNullOrEmpty(SessionString))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionToken"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionToken");

                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionClient"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionClient");

                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionUser"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionUser");

                return;
            }

            SessionDetails Sessions = JsonConvert.DeserializeObject<SessionDetails>(SessionString);

            if (Sessions.SessionExpiry.AddMinutes(-1) < DateTime.UtcNow)
            {
                IsRefreshingToken = true;

                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionToken"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionToken");

                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionClient"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionClient");

                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionUser"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionUser");

                Sessions = await RefreshSession(Sessions);
                SessionString = JsonConvert.SerializeObject(Sessions);

                if (SecureStorage.Default.GetAsync("Session").Result != null)
                {
                    SecureStorage.Default.Remove("Session");
                }
                SecureStorage.Default.SetAsync("Session", SessionString);

                _httpClient.DefaultRequestHeaders.Add("X-Custom-SessionToken", Sessions.SessionToken);
                _httpClient.DefaultRequestHeaders.Add("X-Custom-SessionClient", Sessions.SessionUser);
                _httpClient.DefaultRequestHeaders.Add("X-Custom-SessionUser", Sessions.UserID.ToString());

                IsRefreshingToken = false;
            }

            if(!_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionToken") ||!_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionClient") ||!_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionUser"))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionToken"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionToken");

                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionClient"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionClient");

                if (_httpClient.DefaultRequestHeaders.Contains("X-Custom-SessionUser"))
                    _httpClient.DefaultRequestHeaders.Remove("X-Custom-SessionUser");

                _httpClient.DefaultRequestHeaders.Add("X-Custom-SessionToken", Sessions.SessionToken);
                _httpClient.DefaultRequestHeaders.Add("X-Custom-SessionClient", Sessions.SessionUser);
                _httpClient.DefaultRequestHeaders.Add("X-Custom-SessionUser", Sessions.UserID.ToString());
            }
        }

        private async Task<HttpResponseMessage> GetHttpRequestAsync(string requestURL)
        {
            int attempt = 0;
            await CheckConnectivity();

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;                  
                    await CheckAndUpdateSession();
                    HttpResponseMessage response = _httpClient.GetAsync(requestURL).Result;
                    await HideServerConnectionPopup();

                    if(response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Invalid_Session");                        
                    }

                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    await _ls.LogErrorAsync($"GET REQUEST TIMED OUT - attempt {attempt}. Reason: {ex.Message}");
                }
                catch (Exception ex)
                {
                    if (ex.InnerException is TaskCanceledException || ex.InnerException is WebException)
                    {
                        await _ls.LogErrorAsync($"GET REQUEST TIMED OUT - attempt {attempt}. Reason: {ex.Message}");
                    }
                    else
                    {
                        await _ls.LogErrorAsync($"GET REQUEST ERROR - attempt {attempt}. Reason: {ex.Message}");

                        throw;
                    }
                }

                if (attempt == 1)
                {
                    await Task.Delay(10);
                    await ShowServerConnectionPopup();
                }

                if (attempt != maxRetries)
                {
                    int delay = delayMilliseconds * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay);
                }

            }

            // If all retries fail throw an exception
            await HideServerConnectionPopup();
            throw new HttpRequestException("Server Connectivity");
        }

        private async Task<HttpResponseMessage> PostHttpRequestAsync(string requestURL, HttpContent content)
        {
            int attempt = 0;
            await CheckConnectivity();

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;
                    await CheckAndUpdateSession();
                    HttpResponseMessage response = _httpClient.PostAsync(requestURL, content).Result;
                    await HideServerConnectionPopup();

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Invalid_Session");
                    }

                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    await _ls.LogErrorAsync($"POST REQUEST TIMED OUT - attempt {attempt}. Reason: {ex.Message}");

                }
                catch (Exception ex)
                {
                    if (ex.InnerException is TaskCanceledException || ex.InnerException is WebException)
                    {
                        await _ls.LogErrorAsync($"POST REQUEST TIMED OUT - attempt {attempt}. Reason: {ex.Message}");

                    }
                    else
                    {
                        await _ls.LogErrorAsync($"POST REQUEST ERROR - attempt {attempt}. Reason: {ex.Message}");
                        throw;
                    }
                }

                if (attempt == 1)
                {
                    await Task.Delay(10);
                    await ShowServerConnectionPopup();
                }

                if (attempt != maxRetries)
                {
                    int delay = delayMilliseconds * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay);
                }
            }

            // If all retries fail throw an exception
            await HideServerConnectionPopup();
            throw new HttpRequestException("Server Connectivity");
        }

        private async Task<HttpResponseMessage> PatchHttpRequestAsync(string requestURL, HttpContent content)
        {
            int attempt = 0;
            await CheckConnectivity();

            while (attempt < maxRetries)
            {
                try
                {

                    attempt++;
                    await CheckAndUpdateSession();
                    HttpResponseMessage response = _httpClient.PatchAsync(requestURL, content).Result;
                    await HideServerConnectionPopup();

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Invalid_Session");
                    }

                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    await _ls.LogErrorAsync($"PATCH REQUEST TIMED OUT - attempt {attempt}. Reason: {ex.Message}");

                }
                catch (Exception ex)
                {
                    if (ex.InnerException is TaskCanceledException || ex.InnerException is WebException)
                    {
                        await _ls.LogErrorAsync($"PATCH REQUEST TIMED OUT - attempt {attempt}. Reason: {ex.Message}");
                    }
                    else
                    {
                        await _ls.LogErrorAsync($"PATCH REQUEST ERROR - attempt {attempt}. Reason: {ex.Message}");
                        throw;
                    }
                }

                if (attempt == 1)
                {
                    await Task.Delay(10);
                    await ShowServerConnectionPopup();
                }

                if (attempt != maxRetries)
                {
                    int delay = delayMilliseconds * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay);
                }
            }

            // If all retries fail throw an exception
            await HideServerConnectionPopup();
            throw new HttpRequestException("Server Connectivity");
        }

        private async Task<HttpResponseMessage> PutHttpRequestAsync(string requestURL, HttpContent content)
        {
            int attempt = 0;
            await CheckConnectivity();

            while (attempt < maxRetries)
            {
                try
                {
                    attempt++;
                    await CheckAndUpdateSession();
                    HttpResponseMessage response = _httpClient.PutAsync(requestURL, content).Result;
                    await HideServerConnectionPopup();

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new Exception("Invalid_Session");
                    }

                    return response;
                }
                catch (TaskCanceledException ex)
                {
                    await _ls.LogErrorAsync($"PUT REQUEST TIMED OUT - attempt {attempt}. Reason: {ex.Message}");

                }
                catch (Exception ex)
                {
                    if (ex.InnerException is TaskCanceledException || ex.InnerException is WebException)
                    {
                        await _ls.LogErrorAsync($"PUT REQUEST TIMED OUT - attempt {attempt}. Reason: {ex.Message}");
                    }
                    else
                    {
                        await _ls.LogErrorAsync($"PATCH REQUEST ERROR - attempt {attempt}. Reason: {ex.Message}");
                        throw;
                    }
                }

                if (attempt == 1)
                {
                    await Task.Delay(10);
                    await ShowServerConnectionPopup();
                }

                if (attempt != maxRetries)
                {
                    int delay = delayMilliseconds * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay);
                }
            }

            // If all retries fail throw an exception
            await HideServerConnectionPopup();
            throw new HttpRequestException("Server Connectivity");
        }

        public async Task ShowServerConnectionPopup()
        {
            await Task.Delay(10);

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }

            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpNoServer(new PopUpNoServerViewModel());
                App.CurrentPopUp = PopUp;
                if (Application.Current.Windows[0].Page != null)
                {
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }
            }
        }
        public async Task HideServerConnectionPopup()
        {
            if (App.CurrentPopUp != null)
            {
                var type = App.CurrentPopUp.GetType();
                if (type.Name == "PopUpNoServer")
                {
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                }
            }
        }

        public async Task<string> PatchUserAccount(int UserID, List<PatchDoc> PatchDoc)
        {

            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PatchHttpRequestAsync($"{_url}/userAccounts/{UserID}", request);
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
        public async Task<string> GetUserSaltAsync(string UserEmail)
        {
            RegisterModel User = new RegisterModel();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/userAccounts/getsalt/{System.Web.HttpUtility.UrlEncode(UserEmail)}");
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

        public string LogoutUserAsync(RegisterModel User)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDetailsModel> RegisterNewUserAsync(RegisterModel User)
        {
            UserDetailsModel UserModel = new();

            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<RegisterModel>(User, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/userAccounts/registeruser", request);
            string content = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                UserModel = System.Text.Json.JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);
                UserModel.Error = null;
                return UserModel;
            }
            else
            {
                ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                if (error.ErrorMessage.ToLower() == "invalid email" || error.ErrorMessage.ToLower() == "email already in use")
                {
                    UserModel.Error = error;
                }
                throw new Exception(error.ErrorMessage);
            }
        }

        public async Task<UserDetailsModel> GetUserDetailsAsync(string UserEmail)
        {
            UserDetailsModel User = new();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/userAccounts/getLogonDetails/{System.Web.HttpUtility.UrlEncode(UserEmail)}");
            string content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                User = System.Text.Json.JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);
                User.Error = null;
                return User;
            }
            else
            {
                ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                if (error.ErrorMessage.ToLower() == "user not found")
                {
                    User.Error = error;
                    return User;
                }

                throw new Exception(error.ErrorMessage);
            }

        }

        public async Task<UserAddDetails> GetUserAddDetails(int UserID)
        {
            UserAddDetails User = new UserAddDetails();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/userAccounts/getuseradddetails/{UserID}");
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
                        throw new Exception(error.ErrorMessage);
                    }
                }
        }

        public async Task<string> DowngradeUserAccount(int UserID)
        {
            UserAddDetails User = new UserAddDetails();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/userAccounts/downgrageuseraccount/{UserID}");
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
                        throw new Exception(error.ErrorMessage);
                    }
                }
        }

        public async Task<string> PostUserAddDetails(UserAddDetails User)
        {

            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<UserAddDetails>(User, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/userAccounts/postuseradddetails", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> UploadUserProfilePicture(int UserID, FileResult File)
        {

            var content = new MultipartFormDataContent
            {
                { new StreamContent(await File.OpenReadAsync()), "file", File.FileName }
            };

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/userAccounts/uploaduserprofilepicture/{UserID}", content);
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

        public async Task<Stream> DownloadUserProfilePicture(int UserID)
        {

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/userAccounts/downloaduserprofilepicture/{UserID}");
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

        public async Task<Budgets> GetBudgetDetailsAsync(int BudgetID, string Mode)
        {
            Budgets Budget = new Budgets();

            string ApiMethod = "";
            if (Mode == "Full")
            {
                ApiMethod = "getbudgetdetailsfull";
            }
            else if (Mode == "Limited")
            {
                ApiMethod = "getbudgetdetailsonly";
            }


            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/{ApiMethod}/{BudgetID}");
            await HideServerConnectionPopup();
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

        public async Task<DateTime> GetBudgetNextIncomePayDayAsync(int BudgetID)
        {
            Budgets Budget = new Budgets();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/nextincomepayday/{BudgetID}");
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<int> GetBudgetDaysBetweenPayDay(int BudgetID)
        {
            Budgets Budget = new Budgets();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/daystopaydaynext/{BudgetID}");
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<DateTime?> GetBudgetLastUpdatedAsync(int BudgetID)
        {
            Budgets Budget = new Budgets();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/getlastupdated/{BudgetID}");
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<DateTime> GetBudgetValuesLastUpdatedAsync(int BudgetID, string Page)
        {
            Budgets Budget = new Budgets();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/getbudgetvalueslastupdated/{BudgetID}");
            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Budget = System.Text.Json.JsonSerializer.Deserialize<Budgets>(content, _jsonSerialiserOptions);
                return Budget.BudgetValuesLastUpdated;
            }
            else
            {
                ErrorClass error = System.Text.Json.JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                throw new Exception(error.ErrorMessage);
            }
        }

        public async Task<BudgetSettings> GetBudgetSettings(int BudgetID)
        {
            BudgetSettings BudgetSettings = new BudgetSettings();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getbudgetsettings/{BudgetID}");
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<BudgetSettingValues> GetBudgetSettingsValues(int BudgetID)
        {
            BudgetSettingValues BudgetSettings = new BudgetSettingValues();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getbudgetsettingsvalues/{BudgetID}");
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

        public async Task<Budgets> CreateNewBudget(string UserEmail, string? BudgetType = "Basic")
        {
            Budgets Budget = new Budgets();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/createnewbudget/{UserEmail}/{BudgetType}");
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> DeleteBudget(int BudgetID, int UserID)
        {

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/deletebudget/{BudgetID}/{UserID}");
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Dictionary<string, string> result = serializer.Deserialize<Dictionary<string, string>>(reader);
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
        public async Task<string> ReCalculateBudget(int BudgetID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/recalculateBudget/{BudgetID}");
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

        public async Task<string> DeleteUserAccount(int UserID)
        {

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/userAccounts/deleteaccount/{UserID}");
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Dictionary<string, string> result = serializer.Deserialize<Dictionary<string, string>>(reader);
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

        public async Task<List<lut_CurrencySymbol>> GetCurrencySymbols(string SearchQuery)
        {
            List<lut_CurrencySymbol> Currencies = new List<lut_CurrencySymbol>();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getcurrencysymbols/{SearchQuery}");
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

        public async Task<List<lut_CurrencyPlacement>> GetCurrencyPlacements(string Query)
        {
            List<lut_CurrencyPlacement> Placements = new List<lut_CurrencyPlacement>();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getcurrencyplcements/{Query}");
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

        public async Task<List<lut_BudgetTimeZone>> GetBudgetTimeZones(string Query)
        {
            List<lut_BudgetTimeZone> TimeZones = new List<lut_BudgetTimeZone>();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getbudgettimezones/{Query}");
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

        public async Task<List<lut_DateFormat>> GetDateFormatsByString(string SearchQuery)
        {
            List<lut_DateFormat> DateFormats = new List<lut_DateFormat>();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getdateformatsbystring/{SearchQuery}");
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

        public async Task<lut_DateFormat> GetDateFormatsById(int ShortDatePattern, int Seperator)
        {
            lut_DateFormat DateFormat = new lut_DateFormat();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getdateformatsbyid/{ShortDatePattern}/{Seperator}");
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

        public async Task<lut_BudgetTimeZone> GetTimeZoneById(int TimeZoneID)
        {
            lut_BudgetTimeZone TimeZone = new lut_BudgetTimeZone();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/gettimezonebyid/{TimeZoneID}");
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

        public async Task<lut_NumberFormat> GetNumberFormatsById(int CurrencyDecimalDigits, int CurrencyDecimalSeparator, int CurrencyGroupSeparator)
        {
            lut_NumberFormat NumberFormat = new lut_NumberFormat();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getnumberformatsbyid/{CurrencyDecimalDigits}/{CurrencyDecimalSeparator}/{CurrencyGroupSeparator}");
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

        public async Task<lut_ShortDatePattern> GetShortDatePatternById(int ShortDatePatternID)
        {
            lut_ShortDatePattern ShaortDatePattern = new lut_ShortDatePattern();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getshortdatepatternbyid/{ShortDatePatternID}");
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

        public async Task<lut_DateSeperator> GetDateSeperatorById(int DateSeperatorID)
        {
            lut_DateSeperator DateSeperator = new lut_DateSeperator();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getdateseperatorbyid/{DateSeperatorID}");
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

        public async Task<lut_CurrencyGroupSeparator> GetCurrencyGroupSeparatorById(int CurrencyGroupSeparatorId)
        {
            lut_CurrencyGroupSeparator GroupSeparator = new lut_CurrencyGroupSeparator();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getcurrencygroupseparatorbyid/{CurrencyGroupSeparatorId}");
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

        public async Task<lut_CurrencyDecimalSeparator> GetCurrencyDecimalSeparatorById(int CurrencyDecimalSeparatorId)
        {
            lut_CurrencyDecimalSeparator DecimalSeparator = new lut_CurrencyDecimalSeparator();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getcurrencydecimalseparatorbyid/{CurrencyDecimalSeparatorId}");
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

        public async Task<lut_CurrencyDecimalDigits> GetCurrencyDecimalDigitsById(int CurrencyDecimalDigitsId)
        {
            lut_CurrencyDecimalDigits DecimalDigits = new lut_CurrencyDecimalDigits();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getcurrencydecimaldigitsbyid/{CurrencyDecimalDigitsId}");
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

        public async Task<List<lut_NumberFormat>> GetNumberFormats()
        {
            List<lut_NumberFormat> NumberFormat = new List<lut_NumberFormat>();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgetsettings/getnumberformats");
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
        public async Task<string> UpdatePayPeriodStats(PayPeriodStats Stats)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<PayPeriodStats>(Stats, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/budgets/updatepayperiodstats", request);
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

        public async Task<PayPeriodStats> CreateNewPayPeriodStats(int BudgetID)
        {
            PayPeriodStats stats = new PayPeriodStats();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/createnewpayperiodstats/{BudgetID}");
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

        public async Task<string> PatchBudget(int BudgetID, List<PatchDoc> PatchDoc)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PatchHttpRequestAsync($"{_url}/budgets/updatebudget/{BudgetID}", request);
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

        public async Task<string> PatchBudgetSettings(int BudgetID, List<PatchDoc> PatchDoc)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PatchHttpRequestAsync($"{_url}/budgetsettings/updatebudgetsettings/{BudgetID}", request);
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

        public async Task<string> UpdateBudgetSettings(int BudgetID, BudgetSettings BS)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<BudgetSettings>(BS, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PutHttpRequestAsync($"{_url}/budgetsettings/{BudgetID}", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<Bills> GetBillFromID(int BillID)
        {
            Bills Bill = new Bills();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/bills/getbillfromid/{BillID}");
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

        public async Task<string> SaveNewBill(Bills Bill, int BudgetID)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Bills>(Bill, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/bills/savenewbill/{BudgetID}", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> UpdateBill(Bills Bill)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Bills>(Bill, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/bills/updatebill", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> PatchBill(int BillID, List<PatchDoc> PatchDoc)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PatchHttpRequestAsync($"{_url}/bills/patchbill/{BillID}", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> DeleteBill(int BillID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/bills/deletebill/{BillID}");
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

        public async Task<List<Bills>> GetBudgetBills(int BudgetID, string page)
        {
            List<Bills> Bills = new List<Bills>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/bills/getbudgetbills/{BudgetID}");
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

        public async Task<Savings> GetSavingFromID(int SavingID)
        {
            Savings Saving = new Savings();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/savings/getsavingfromid/{SavingID}");
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

        public async Task<int> SaveNewSaving(Savings Saving, int BudgetID)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Savings>(Saving, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/savings/savenewsaving/{BudgetID}", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> UpdateSaving(Savings Saving)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Savings>(Saving, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/savings/updatesaving", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> PatchSaving(int SavingID, List<PatchDoc> PatchDoc)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PatchHttpRequestAsync($"{_url}/savings/patchsaving/{SavingID}", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> DeleteSaving(int SavingID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/savings/deletesaving/{SavingID}");
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

        public async Task<string> UnPauseSaving(int SavingID, int BudgetID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/savings/unpausesaving/{SavingID}/{BudgetID}");
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

        public async Task<string> PauseSaving(int SavingID, int BudgetID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/savings/pausesaving/{SavingID}/{BudgetID}");
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

        public async Task<List<Savings>> GetAllBudgetSavings(int BudgetID)
        {
            List<Savings> Savings = new List<Savings>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/savings/getallbudgetsavings/{BudgetID}");
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

        public async Task<IncomeEvents> GetIncomeFromID(int IncomeID)
        {
            IncomeEvents Income = new IncomeEvents();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/incomes/getincomefromid/{IncomeID}");
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

        public async Task<string> SaveNewIncome(IncomeEvents Income, int BudgetID)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<IncomeEvents>(Income, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/incomes/savenewincome/{BudgetID}", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> UpdateIncome(IncomeEvents Income)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<IncomeEvents>(Income, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/incomes/updateincome", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> PatchIncome(int IncomeID, List<PatchDoc> PatchDoc)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PatchHttpRequestAsync($"{_url}/incomes/patchincome/{IncomeID}", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> DeleteIncome(int IncomeID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/incomes/deleteincome/{IncomeID}");
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

        public async Task<List<IncomeEvents>> GetBudgetIncomes(int BudgetID, string page)
        {
            List<IncomeEvents> IncomeEvents = new List<IncomeEvents>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/incomes/getbudgetincomeevents/{BudgetID}");
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
        public async Task<string> UpdateBudgetValues(int budgetID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/updatebudgetvalues/{budgetID}");
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

        public async Task<Transactions> SaveNewTransaction(Transactions Transaction, int BudgetID)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Transactions>(Transaction, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/transactions/savenewtransaction/{BudgetID}", request);
            string content = await response.Content.ReadAsStringAsync();

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
        public async Task<Transactions> TransactTransaction(int TransactionID)
        {
            Transactions Transaction = new Transactions();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/transactions/transacttransaction/{TransactionID}");
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
        public async Task<string> UpdateTransaction(Transactions Transaction)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Transactions>(Transaction, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/transactions/updatetransaction", request);
            string content = await response.Content.ReadAsStringAsync();

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

        public async Task<string> DeleteTransaction(int TransactionID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/transactions/deletetransaction/{TransactionID}");
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

        public async Task<List<string>> GetBudgetEventTypes(int BudgetID)
        {
            List<string> EventTypes = new List<string>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/transactions/getbudgeteventtypes/{BudgetID}");
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

        public async Task<Transactions> GetTransactionFromID(int TransactionID)
        {
            Transactions Transaction = new Transactions();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/transactions/gettransactionfromid/{TransactionID}");
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

        public async Task<Budgets> GetAllBudgetTransactions(int BudgetID)
        {
            Budgets Budget = new Budgets();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/transactions/getallbudgettransactions/{BudgetID}");
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

        public async Task<List<Transactions>> GetRecentTransactions(int BudgetID, int NumberOf, string page)
        {
            List<Transactions> transactions = new List<Transactions>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/transactions/getrecenttransactions/{BudgetID}/{NumberOf}");
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

        public async Task<List<Transactions>> GetCurrentPayPeriodTransactions(int BudgetID, string page)
        {
            List<Transactions> transactions = new List<Transactions>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/transactions/getcurrentpayperiodtransactions/{BudgetID}");
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

        public async Task<List<Transactions>> GetFilteredTransactions(int BudgetID, FilterModel Filters, string page)
        {
            List<Transactions> transactions = new List<Transactions>();
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<FilterModel>(Filters, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/transactions/getfilteredtransactions/{BudgetID}", request);
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

        public async Task<List<Transactions>> GetRecentTransactionsOffset(int BudgetID, int NumberOf, int Offset, string page)
        {
            List<Transactions> transactions = new List<Transactions>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/transactions/getrecenttransactionsoffset/{BudgetID}/{NumberOf}/{Offset}");
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

        public async Task<Budgets> SaveBudgetDailyCycle(Budgets budget)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Budgets>(budget, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/budgets/savebudgetdailycycle", request);

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

        public async Task<string> CreateNewOtpCodeShareBudget(int UserID, int ShareBudgetID)
        {
            OTP UserOTP = new OTP();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/otp/createnewotpcodesharebudget/{UserID}/{ShareBudgetID}");
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

        public async Task<string> CreateNewOtpCode(int UserID, string OTPType)
        {
            OTP UserOTP = new OTP();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/otp/createnewotpcode/{UserID}/{OTPType}");
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
        public async Task<string> ValidateOTPCodeEmail(OTP UserOTP)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<OTP>(UserOTP, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/otp/validateotpcodeemail", request);

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

        public async Task<string> ValidateOTPCodeFamilyAccount(OTP UserOTP)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<OTP>(UserOTP, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/otp/validateotpcodefamilyaccount", request);

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

        public async Task<int> GetUserIdFromEmail(string UserEmail)
        {
            UserDetailsModel User = new UserDetailsModel();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/otp/getuseridfromemail/{UserEmail}");
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

        public async Task<List<string>> GetPayeeList(int BudgetID)
        {
            List<string>? Payee = new List<string>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/payee/getpayeelist/{BudgetID}");
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

        public async Task<List<Payees>> GetPayeeListFull(int BudgetID)
        {
            List<Payees>? Payee = new List<Payees>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/payee/getpayeelistfull/{BudgetID}");
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

        public async Task<Categories> GetPayeeLastCategory(int BudgetID, string PayeeName)
        {
            Categories Category = new Categories();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/payee/getpayeelastcategory/{BudgetID}/{PayeeName}");
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

        public async Task<string> DeletePayee(int BudgetID, string OldPayeeName, string NewPayeeName)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/payee/deletepayee/{BudgetID}/{OldPayeeName}/{NewPayeeName}");
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

        public async Task<string> UpdatePayee(int BudgetID, string OldPayeeName, string NewPayeeName)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/payee/updatepayee/{BudgetID}/{OldPayeeName}/{NewPayeeName}");
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

        public async Task<List<Categories>> GetCategories(int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/categories/getcategories/{BudgetID}");
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

        public async Task<Categories> GetCategoryFromID(int CategoryID)
        {
            Categories? Category = new Categories();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/categories/getcategoryfromid/{CategoryID}");
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

        public async Task<int> AddNewCategory(int BudgetID, DefaultCategories Category)
        {
            int CategoryID;

            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<DefaultCategories>(Category, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/categories/addnewcategory/{BudgetID}", request);

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

        public async Task<Categories> AddNewSubCategory(int BudgetID, Categories Category)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<Categories>(Category, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/categories/addnewsubcategory/{BudgetID}", request);

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

        public async Task<string> PatchCategory(int CategoryID, List<PatchDoc> PatchDoc)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PatchHttpRequestAsync($"{_url}/categories/patchcategory/{CategoryID}", request);
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

        public async Task<string> UpdateAllTransactionsCategoryName(int CategoryID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/categories/Updatealltransactionscategoryname/{CategoryID}");
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

        public async Task<List<Categories>> GetAllHeaderCategoryDetailsFull(int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/categories/getallheadercategorydetailsfull/{BudgetID}");
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
        public async Task<List<Categories>> GetHeaderCategoryDetailsFull(int CategoryID, int BudgetID)
        {
            List<Categories>? categories = new List<Categories>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/categories/getheadercategorydetailsfull/{CategoryID}/{BudgetID}");
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

        public async Task<string> DeleteCategory(int CategoryID, bool IsReassign, int ReAssignID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/categories/deletecategory/{CategoryID}/{IsReassign}/{ReAssignID}");
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

        public async Task<Dictionary<string, int>> GetAllCategoryNames(int BudgetID)
        {
            Dictionary<string, int> Categories = new Dictionary<string, int>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/categories/getallcategorynames/{BudgetID}");
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

        public async Task<List<Savings>> GetBudgetEnvelopeSaving(int BudgetID)
        {
            List<Savings>? Savings = new List<Savings>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/savings/getbudgetenvelopesaving/{BudgetID}");
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

        public async Task<List<Savings>> GetBudgetRegularSaving(int BudgetID)
        {
            List<Savings>? Savings = new List<Savings>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/savings/getbudgetregularsaving/{BudgetID}");
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
        public async Task<string> ShareBudgetRequest(ShareBudgetRequest BudgetShare)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<ShareBudgetRequest>(BudgetShare, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/budgets/sharebudgetrequest", request);

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
                    else if (error.ErrorMessage == "Budget Already Shared")
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

        public async Task<FirebaseDevices> RegisterNewFirebaseDevice(FirebaseDevices NewDevice)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<FirebaseDevices>(NewDevice, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/firebasedevices/registernewfirebasedevice", request);

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

        public async Task<FirebaseDevices> UpdateDeviceUserDetails(FirebaseDevices NewDevice)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<FirebaseDevices>(NewDevice, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/firebasedevices/updatedeviceuserdetails", request);

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

        public async Task<string> ValidateOTPCodeShareBudget(OTP UserOTP, int SharedBudgetRequestID)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<OTP>(UserOTP, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/otp/validateotpcodesharebudget/{SharedBudgetRequestID}", request);

            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    return "OK";
                }
                else
                {
                    if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return "Incorrect Code";
                    }

                    ErrorClass error = new ErrorClass();
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                        error = serializer.Deserialize<ErrorClass>(reader);

                        if(error.ErrorMessage == "No budget share request")
                        {
                            return "Error";
                        }
                    }

                    throw new Exception(error.ErrorMessage);
                }
        }

        public async Task<ShareBudgetRequest> GetShareBudgetRequestByID(int SharedBudgetRequestID)
        {
            ShareBudgetRequest ShareRequest = new ShareBudgetRequest();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/getsharebudgetrequestbyid/{SharedBudgetRequestID}");
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

        public async Task<string> CancelCurrentShareBudgetRequest(int BudgetID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/cancelcurrentsharebudgetrequest/{BudgetID}");
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

        public async Task<string> StopSharingBudget(int BudgetID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/stopsharingbudget/{BudgetID}");
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

        public async Task<List<Budgets>> GetUserAccountBudgets(int UserID, string page)
        {
            List<Budgets> budgets = new List<Budgets>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/userAccounts/getuseraccountbudgets/{UserID}");
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

        public async Task<List<CustomerSupport>> GetSupports(int UserID, string page)
        {
            List<CustomerSupport> Support = new List<CustomerSupport>();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/supports/getsupports/{UserID}");
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Support = serializer.Deserialize<List<CustomerSupport>>(reader);
                    }

                    return Support;

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
        public async Task<CustomerSupport> GetSupport(int SupportID, string page)
        {
            CustomerSupport Support = new CustomerSupport();
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/supports/getsupport/{SupportID}");
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Support = serializer.Deserialize<CustomerSupport>(reader);
                    }

                    return Support;

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
        public async Task<CustomerSupport> CreateSupport(int UserID, CustomerSupport Support)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<CustomerSupport>(Support, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/supports/createsupport/{UserID}", request);
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Support = serializer.Deserialize<CustomerSupport>(reader);
                    }

                    return Support;
                }
                else
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        ErrorClass error = serializer.Deserialize<ErrorClass>(reader);
                        throw new Exception(error.ErrorMessage);
                    }
                }
        }
        public async Task<CustomerSupportMessage> AddReply(int SupportID, CustomerSupportMessage Reply)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<CustomerSupportMessage>(Reply, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/supports/addreply/{SupportID}", request);
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Reply = serializer.Deserialize<CustomerSupportMessage>(reader);
                    }

                    return Reply;
                }
                else
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        ErrorClass error = serializer.Deserialize<ErrorClass>(reader);
                        throw new Exception(error.ErrorMessage);
                    }
                }
        }

        public async Task<string> SaveSupportFile(FileResult File)
        {
            var content = new MultipartFormDataContent
            {
                { new StreamContent(await File.OpenReadAsync()), "file", File.FileName }
            };

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/supports/savefile", content);
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Dictionary<string, string> result = serializer.Deserialize<Dictionary<string, string>>(reader);
                        string returnString = result["fileName"];
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
        public async Task<Stream> DownloadFile(int SupportID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/supports/downloadfile/{SupportID}");
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

        public async Task<string> PatchSupport(int SupportID, List<PatchDoc> PatchDoc)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<List<PatchDoc>>(PatchDoc, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PatchHttpRequestAsync($"{_url}/supports/patchSupport/{SupportID}", request);
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

        public async Task<string> DeleteSupportFile(int SupportID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/supports/deletefile/{SupportID}");
            if (response.IsSuccessStatusCode)
            {
                return "OK";
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

        public async Task<string> SetAllMessagesRead(int SupportID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/supports/setmessagesread/{SupportID}");
            if (response.IsSuccessStatusCode)
            {
                return "OK";
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

        public async Task<List<BankAccounts>> GetBankAccounts(int BudgetID)
        {
            List<BankAccounts> Accounts = new List<BankAccounts>();

            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/getbankaccounts/{BudgetID}");
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Accounts = serializer.Deserialize<List<BankAccounts>>(reader);
                    }

                    return Accounts;
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

        public async Task<BankAccounts> AddBankAccounts(int BudgetID, BankAccounts Account)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<BankAccounts>(Account, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/budgets/addbankaccounts/{BudgetID}", request);
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        Account = serializer.Deserialize<BankAccounts>(reader);
                    }

                    return Account;
                }
                else
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        ErrorClass error = serializer.Deserialize<ErrorClass>(reader);
                        throw new Exception(error.ErrorMessage);
                    }
                }

        }
        public async Task<BankAccounts> UpdateBankAccounts(int BudgetID, BankAccounts Account)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<BankAccounts>(Account, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/budgets/updatebankaccounts/{BudgetID}", request);
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

            if (response.IsSuccessStatusCode)
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    Account = serializer.Deserialize<BankAccounts>(reader);
                }

                return Account;
            }
            else
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    ErrorClass error = serializer.Deserialize<ErrorClass>(reader);
                    throw new Exception(error.ErrorMessage);
                }
            }
        }

        public async Task<string> DeleteBankAccounts(int BudgetID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/deletebankaccounts/{BudgetID}");
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

        public async Task<string> DeleteBankAccount(int ID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/budgets/deletebankaccount/{ID}");
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

        public async Task<SessionDetails> RefreshSession(SessionDetails Details)
        {
            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<SessionDetails>(Details, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/auth/RefreshSession", request);
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

            if (response.IsSuccessStatusCode)
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    Details = serializer.Deserialize<SessionDetails>(reader);
                }

                return Details;
            }
            else
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    ErrorClass error = serializer.Deserialize<ErrorClass>(reader);
                    throw new Exception(error.ErrorMessage);
                }
            }
        }

        public async Task<SessionDetails> CreateSession(AuthDetails Details)
        {
            SessionDetails Session = new();

            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<AuthDetails>(Details, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/auth/CreateSession", request);
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

            if (response.IsSuccessStatusCode)
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    Session = serializer.Deserialize<SessionDetails>(reader);
                }

                return Session;
            }
            else
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    ErrorClass error = serializer.Deserialize<ErrorClass>(reader);
                    throw new Exception(error.ErrorMessage);
                }
            }
        }

        public async Task<string?> FamilyAccountEmailValid(string Email, int UserID)
        {
            HttpResponseMessage response = await GetHttpRequestAsync($"{_url}/userAccounts/familyaccountemailvalid/{Email}/{UserID}");
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        var Result = serializer.Deserialize<FamilyUserAccountValidEmailObject>(reader);

                        if (Result.IsValid.GetValueOrDefault())
                        {
                            return null;
                        }
                        else
                        {
                            switch (Result.InvalidReason)
                            {
                                case "EmailInUseByYou":
                                    return "Sorry this email is in use by another one of you family accounts. Please delete this account or use another valid email to set up a new budget for a family member.";

                                case "EmailInUseBySomeoneElse":
                                default:
                                    return "Sorry this email is in use by someone else. If you do not believe this is correct and are sure this is your email please contact us. Otherwise use another valid email.";
                            }                            
                        }
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

        public async Task<FamilyUserAccount> SetUpNewFamilyAccount(FamilyUserAccount User)
        {

            string jsonRequest = System.Text.Json.JsonSerializer.Serialize<FamilyUserAccount>(User, _jsonSerialiserOptions);
            StringContent request = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await PostHttpRequestAsync($"{_url}/userAccounts/setupnewfamilyaccount", request);
            using (Stream s = response.Content.ReadAsStreamAsync().Result)
            using (StreamReader sr = new StreamReader(s))

                if (response.IsSuccessStatusCode)
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        User = serializer.Deserialize<FamilyUserAccount>(reader);
                    }

                    return User;
                }
                else
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                        ErrorClass error = serializer.Deserialize<ErrorClass>(reader);
                        throw new Exception(error.ErrorMessage);
                    }
                }
        }
    }
}
