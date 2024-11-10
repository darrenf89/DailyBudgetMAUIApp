using CommunityToolkit.Maui.Alerts;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DailyBudgetMAUIApp.DataServices
{
    public interface IProductTools
    {
        public string GenerateSalt(int nSalt);
        public string GenerateHashedPassword(string NonHasdedPassword, string Salt);
        public RegisterModel CreateUserSecurityDetails(RegisterModel obj);
        public RegisterModel ResetUserPassword(RegisterModel obj);
        public Task HandleException(Exception ex, string page, string Method);
        public DateTime? GetBudgetLastUpdated(int BudgetID);
        public void ShowPopup(PopUpPage popup);
        public double FormatCurrencyNumber(string input);
        public int FindCurrencyCursorPosition(string input);
        public int GetNumberOfDaysLastWorkingDay(int? NumberOfDaysBefore);
        public int GetNumberOfDaysLastDayOfWeek(int dayNumber);
        public string BudgetDailyCycleBudgetValuesUpdate(ref Budgets Budget);
        public string UpdateApproxDaysBetweenPay(ref Budgets Budget);
        public int CalculateBudgetDaysBetweenPay(Budgets Budget);
        public string UpdateBudgetCreateSavings(ref Budgets Budget);
        public string UpdateBudgetCreateBills(ref Budgets Budget);
        public string UpdateBudgetCreateIncome(ref Budgets Budget);
        public string UpdateBudgetCreateSavingsSpend(ref Budgets Budget);
        public string UpdateBudgetCreateBillsSpend(ref Budgets Budget);
        public string UpdateBudgetCreateBillsSpendBorrowPay(ref Budgets Budget);
        public DateTime CalculateNextDate(DateTime LastDate, string Type, int Value, string? Duration);
        public string CalculateNextDateEverynth(ref DateTime NextDate, DateTime LastDate, int Value, string? Duration);
        public string CalculateNextDateWorkingDays(ref DateTime NextDate, DateTime LastDate, int Value);
        public string CalculateNextDateOfEveryMonth(ref DateTime NextDate, DateTime LastDate, int Value);
        public string CalculateNextDateLastOfTheMonth(ref DateTime NextDate, DateTime LastDate, string? Duration);
        public void SetCultureInfo(BudgetSettingValues Settings);
        public string BudgetDailyEventsValuesUpdate(ref Budgets Budget);
        public Task<Budgets> BudgetDailyCycle(Budgets budget);
        public string UpdatePayPeriodStats(int? BudgetID);
        public string TransactSavingsTransaction(ref Transactions T, ref Budgets Budget);
        public string TransactTransaction(ref Transactions T, ref Budgets Budget);
        public DateTime GetBudgetLocalTime(DateTime UtcDate);
        public Task NavigateFromPendingIntent(string NavigationID);
        public Task<Picker> SwitchBudget(string page);
        public Task ChangeDefaultBudget(int UserID, int BudgetID, bool Navigate);
        public Task LoadTabBars(string UserSub, DateTime SubExpiry, string BudgetType);
        public Task<Dictionary<string, string>> GetIcons();
        public Task<string> GetIcon(string Name);
        public Task SetSubDetails();
        public Task MakeSnackBar(string text, Action? action, string? actionButtonText, TimeSpan duration, string snackBarType);
    }
}
