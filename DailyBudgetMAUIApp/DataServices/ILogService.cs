using DailyBudgetMAUIApp.Models;
using DailySpendWebApp.Models;


namespace DailyBudgetMAUIApp.DataServices
{
    public interface ILogService
    {
        public Task LogErrorAsync(string errorMessage);
        public Task CopyLogFileToExternalAsync();
    }
}
