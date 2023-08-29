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
        public string RegisterNewUser(RegisterModel User);
        public RegisterModel GetUserLoginInformation(string UserEmail);
        public string LogoutUser(RegisterModel User);

    }
}
