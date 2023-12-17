using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.Models
{
    public class UserDetailsModel
    {
        public int UserID { get; set; }
        public string NickName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool isEmailVerified { get; set; }
        public DateTime SessionExpiry { get; set; }
        public int DefaultBudgetID { get; set; }
        public ErrorClass? Error { get; set; } = null;
        public bool isDPAPermissions { get; set; }
        public bool isAgreedToTerms { get; set; }

    }
}
