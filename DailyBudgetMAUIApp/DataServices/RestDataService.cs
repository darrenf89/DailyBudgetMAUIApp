using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Mail;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.DataServices
{
    internal class RestDataService : IRestDataService
    {

        private readonly HttpClient _httpClient;
        private readonly string _baseAddress;
        private readonly string _url;
        private readonly JsonSerializerOptions _jsonSerialiserOptions;

        public RestDataService()
        {
            _httpClient = new HttpClient();
            _baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5074" : "https://localhost:7141";
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
                    
                    User = JsonSerializer.Deserialize<RegisterModel>(content, _jsonSerialiserOptions);

                    return User.Salt;
                }
                else
                {
                    ErrorClass error = JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    return error.ErrorMessage;
                }
                
            }
            catch (Exception ex)
            {
                //TODO: Update to write to log instead
                Debug.WriteLine($"Error Trying to get Salt --> {ex.Message}");
                return ($"Error");
            }
        }

        public string LogoutUserAsync(RegisterModel User)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDetailsModel> RegisterNewUserAsync(RegisterModel User)
        {
            UserDetailsModel UserModel = null;

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                string jsonRequest = JsonSerializer.Serialize<RegisterModel>(User, _jsonSerialiserOptions);
                HttpRequestMessage request = new();

                HttpResponseMessage response = await _httpClient.SendAsync(request, $"{_url}/userAccounts/registerUser");
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    UserModel = JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);
                    return UserModel;
                }
                else
                {
                    UserModel = JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);
                    return UserModel;
                }       
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Trying to get Salt --> {ex.Message}");
                return UserModel;
            }
        }

        public async Task<UserDetailsModel> GetUserDetailsAsync(string UserEmail)
        {
            UserDetailsModel User = null;

            if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                HttpResponseMessage response = await _httpClient.GetAsync($"{_url}/userAccounts/getLogonDetails/{System.Web.HttpUtility.UrlEncode(UserEmail)}");
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {

                    User = JsonSerializer.Deserialize<UserDetailsModel>(content, _jsonSerialiserOptions);

                    return User;
                }
                else
                {
                    ErrorClass error = JsonSerializer.Deserialize<ErrorClass>(content, _jsonSerialiserOptions);
                    error.StatusCode = response.StatusCode;
                    User.Error = error;
                    return User;
                }

            }
            catch (Exception ex)
            {
                //Update to write to log instead
                Debug.WriteLine($"Error Trying to get Salt --> {ex.Message}");
                return User;
            }
        }
    }
}
