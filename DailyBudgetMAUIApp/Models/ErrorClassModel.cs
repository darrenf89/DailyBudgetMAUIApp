using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.Models
{
    public class ErrorClass
    {
        public string? ErrorMessage { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
