using System.Net;

namespace DailyBudgetMAUIApp.Models
{
    public class ErrorClass
    {

        public string? ErrorMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
