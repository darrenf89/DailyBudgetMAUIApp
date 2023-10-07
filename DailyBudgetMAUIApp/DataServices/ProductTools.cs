using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Mail;
using System.Diagnostics;
using System.Security.Cryptography;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;

namespace DailyBudgetMAUIApp.DataServices
{
    internal class ProductTools : IProductTools
    {
        private readonly IRestDataService _ds;

        public ProductTools(IRestDataService ds)
        {
            _ds = ds;
        }

        public RegisterModel CreateUserSecurityDetails(RegisterModel obj)
        {
            Random rnd = new();
            int number = rnd.Next(2000);
            string Salt = GenerateSalt(number);

            string HashedPassword = GenerateHashedPassword(obj.Password, Salt);

            obj.Password = HashedPassword;
            obj.Salt = Salt;

            return obj;
        }

        public string GenerateSalt(int nSalt)
        {
            Byte[] saltBytes = new Byte[nSalt];
            RandomNumberGenerator.Create().GetNonZeroBytes(saltBytes);

            return Convert.ToBase64String(saltBytes);

        }

        public string GenerateHashedPassword(string NonHasdedPassword, string Salt)
        {
            int nHash = 70;
            int nIteraitons = 10101;

            Byte[] saltBytes = Convert.FromBase64String(Salt);

            Rfc2898DeriveBytes obj = new(NonHasdedPassword, saltBytes, nIteraitons);

            using (obj)
            {
                return Convert.ToBase64String(obj.GetBytes(nHash));
            }

        }

        public async Task<ErrorLog> HandleCatchedException(Exception ex, string Page, string Method)
        {
            try
            {
                ErrorLog NewLog = new ErrorLog(ex, Page, Method);


                ErrorLog Response = await _ds.CreateNewErrorLog(NewLog);

                return Response;
            }
            catch (Exception EndExcption)
            {
                Debug.WriteLine($"Error Trying to Log the Error --> {EndExcption.Message}");
                //TODO: Write the error to a physical file

                throw new Exception("Fatal Error Trying to Log an Error");
            }



        }

        public DateTime GetBudgetLastUpdated(int BudgetID)
        {
            DateTime LastUpdated = _ds.GetBudgetLastUpdatedAsync(BudgetID).Result;

            return LastUpdated;
        }

        public void ShowPopup(PopUpPage popup)
        {
            Page page = Application.Current.MainPage ?? throw new NullReferenceException();
            page.ShowPopup(popup);
        }

        public double FormatCurrencyNumber(string input)
        {
            input = input.Replace(App.CurrentSettings.CurrencySymbol,"").Replace(App.CurrentSettings.CurrencyGroupSeparator,"").Replace(App.CurrentSettings.CurrencyDecimalSeparator, "");
            input = input.Trim();

            //TODO: GET THE NUMBER OF DIGITS - CHECK THAT IT IS GREATER THAN 2
            try
            {
                double Number = Convert.ToDouble(input);
                Number = Number / 100;
                
                return Number;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int FindCurrencyCursorPosition(string input)
        {
            try
            {
                int position = input.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
                position = position + 3;

                return position;
            }
            catch (Exception ex)
            {
                return input.Length;
            }
        }


    }
}
