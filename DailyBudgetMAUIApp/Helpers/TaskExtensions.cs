namespace DailyBudgetMAUIApp.Helpers
{
    public static class TaskExtensions
    {
        public static async void FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                // Log or handle exceptions
                Console.WriteLine($"Unhandled exception: {ex.Message}");
            }
        }
    }
}
