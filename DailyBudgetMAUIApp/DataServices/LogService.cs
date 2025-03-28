using System;
using System.IO;
using System.Threading.Tasks;
using DailyBudgetMAUIApp.DataServices;
using Microsoft.Maui.Storage;

internal class LogService : ILogService
{
    private static string logFilePath = "";
    private static string externalStoragePath = "";

    public LogService()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "app_logs.txt");
            var externalFilesDir = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments);
            externalStoragePath = Path.Combine(externalFilesDir.AbsolutePath, "app_logs.txt");
        }
    }

    public async Task LogErrorAsync(string errorMessage)
    {
        try
        {
            var directory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string logMessage = $"{DateTime.Now}: {errorMessage}\n";

            using (var streamWriter = new StreamWriter(logFilePath, append: true))
            {
                await streamWriter.WriteAsync(logMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write log: {ex.Message}");
        }
    }

    public async Task CopyLogFileToExternalAsync()
    {
        if (File.Exists(logFilePath))
        {
            try
            {
                File.Copy(logFilePath, externalStoragePath, overwrite: true);

                File.Delete(logFilePath);

                Console.WriteLine("Log file copied and original deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying or deleting the file: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Log file does not exist.");
        }
    }
}
