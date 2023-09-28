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
        public Task<BudgetSettingValues> GetBudgetSettingsValues(int BudgetID);
        public Task<BudgetSettings> GetBudgetSettings(int BudgetID);
        public Task<Budgets> CreateNewBudget(string UserEmail);
        public Task<List<lut_CurrencySymbol>> GetCurrencySymbols(string SearchQuery);
        public Task<List<lut_CurrencyPlacement>> GetCurrencyPlacements(string SearchQuery);
        public Task<List<lut_DateFormat>> GetDateFormatsByString(string SearchQuery);
        public Task<lut_DateFormat> GetDateFormatsById(int ShortDatePattern, int Seperator);

    }
}
