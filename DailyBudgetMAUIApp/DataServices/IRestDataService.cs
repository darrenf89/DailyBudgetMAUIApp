﻿using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.DataServices
{
    public interface IRestDataService
    {
        public string RegisterNewUserAsync(RegisterModel User);
        Task<string> GetUserSaltAsync(string UserEmail);
        public string LogoutUserAsync(RegisterModel User);
        Task<UserDetailsModel> GetUserDetailsAsync(string UserEmail);

    }
}