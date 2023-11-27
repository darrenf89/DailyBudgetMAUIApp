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
        public void ShowPopup(PopUpPage popup);
        public double FormatCurrencyNumber(string input);
        public int FindCurrencyCursorPosition(string input);
        public int GetNumberOfDaysLastWorkingDay(int? NumberOfDaysBefore);
        public int GetNumberOfDaysLastDayOfWeek(int dayNumber);
        public void UpdateBudget(ref Budgets Budget);
        public void UpdateBudgetRecalculateSavings(ref Budgets Budget);
        public void UpdateBudgetIncomes(ref Budgets Budget);
        public void UpdateBudgetSavings(ref Budgets Budget);  
        public void UpdateBudgetBills(ref Budgets Budget);
        public DateTime CalculateNextDate(DateTime LastDate, string Type, int Value, string? Duration);
        public string CalculateNextDateEverynth(ref DateTime NextDate, DateTime LastDate, int Value, string? Duration);
        public string CalculateNextDateWorkingDays(ref DateTime NextDate, DateTime LastDate, int Value);
        public string CalculateNextDateOfEveryMonth(ref DateTime NextDate, DateTime LastDate, int Value);
        public string CalculateNextDateLastOfTheMonth(ref DateTime NextDate, DateTime LastDate, string? Duration);
        public void SetCultureInfo(BudgetSettingValues Settings);
        public void BudgetDailyLoadCheck(Budgets budget);
        
    }
}
