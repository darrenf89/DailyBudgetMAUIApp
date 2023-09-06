using Microsoft.Maui.Devices;

namespace DailyBudgetMAUIApp.Models
{
    public class ErrorLog
    {
        public ErrorLog() 
        { 

        }
        public ErrorLog(Exception ex, string ErrorPage, string ErrorMethod)
        {
            this.ErrorLogID = 0;
            this.ErrorMessage = ex.Message;
            this.ErrorPage = ErrorPage;
            this.ErrorMethod = ErrorMethod;
            this.DeviceName = DeviceInfo.Current.Name;
            this.DevicePlatform = DeviceInfo.Current.Platform.ToString();
            this.DeviceIdiom = DeviceInfo.Current.Idiom.ToString();
            this.DeviceOSVersion = DeviceInfo.Current.VersionString;
            this.DeviceModel = DeviceInfo.Current.Model;
            this.BudgetID = App.DefaultBudgetID;
            this.WhenAdded = DateTime.UtcNow;
            
        }
        public int ErrorLogID { get; set; }
        public string? ErrorMessage { get; set;}
        public string? ErrorPage { get; set; }
        public string? ErrorMethod { get; set; }
        public string? DeviceName { get; set; }
        public string? DevicePlatform { get; set; }
        public string? DeviceIdiom { get; set; }
        public string? DeviceOSVersion { get; set; }
        public string? DeviceModel { get; set; }
        public int BudgetID { get; set; }
        public DateTime? WhenAdded { get; set; }
    }
}