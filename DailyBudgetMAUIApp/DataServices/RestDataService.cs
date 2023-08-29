using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Mail;

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
            _baseAddress = DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5209" : "https://localhost:7209";
            _url = $"{_baseAddress}/api/v1";

            _jsonSerialiserOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public RegisterModel GetUserLoginInformation(string UserEmail)
        {
            RegisterModel User = new RegisterModel();

            if(Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            {
                throw new HttpRequestException("Connectivity");
            }

            try
            {

                string JsonString = JsonSerializer.Serialize<object>(value: new { User.Email, TransientPassword = User.Password }, options: _jsonSerialiserOptions);

                HttpRequestMessage webRequest = new HttpRequestMessage(HttpMethod.Post, $"{_url}/userAccounts/getLogonDetails")
                {
                    Content = new StringContent(JsonString, Encoding.UTF8, "applicaiton/json")
                };     

                HttpResponseMessage response = _httpClient.Send(webRequest);



                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    User = JsonSerializer.Deserialize<RegisterModel>(content, _jsonSerialiserOptions);

                    return User;
                }
                else
                {
                    throw new HttpRequestException();
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public string LogoutUser(RegisterModel User)
        {
            throw new NotImplementedException();
        }

        public string RegisterNewUser(RegisterModel User)
        {
            throw new NotImplementedException();
        }
    }
}
