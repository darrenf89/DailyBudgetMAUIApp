using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.DataServices
{
    public interface IProductTools
    {
        public string GenerateSalt(int nSalt);
        public string GenerateHashedPassword(string NonHasdedPassword, string Salt);
        public RegisterModel CreateUserSecurityDetails(RegisterModel obj);
        public Task<ErrorLog> HandleCatchedException(Exception ex, string page, string Method);
        public DateTime GetBudgetLastUpdated(int BudgetID);
        void ShowPopup(PopUpPage popup);
    }
}
