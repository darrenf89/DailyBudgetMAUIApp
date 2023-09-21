using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.DataServices
{
    public interface IRestDataService
    {
        public Task<UserDetailsModel> RegisterNewUserAsync(RegisterModel User);
        public Task<string> GetUserSaltAsync(string UserEmail);
        public string LogoutUserAsync(RegisterModel User);
        public Task<UserDetailsModel> GetUserDetailsAsync(string UserEmail);
        public Task<ErrorLog> CreateNewErrorLog(ErrorLog NewLog);
        public Task<Budgets> GetBudgetDetailsAsync(int BudgetID, string Mode);
        public Task<DateTime> GetBudgetLastUpdatedAsync(int BudgetID);
        public Task<BudgetSettingValues> GetBudgetSettings(int BudgetID);
        public Task<Budgets> CreateNewBudget(string UserEmail);

    }
}
