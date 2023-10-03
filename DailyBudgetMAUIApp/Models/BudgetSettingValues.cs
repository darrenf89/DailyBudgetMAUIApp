using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.Models
{
    public class BudgetSettingValues
    {
        public string CurrencySymbol { get; set; }
        public string CurrencyDecimalSeparator { get; set; }
        public string CurrencyGroupSeparator { get; set; }
        public string ShortDatePattern { get; set; }
        public string DateSeparator { get; set; }
        public int CurrencyDecimalDigits { get; set; }
        public int CurrencyPositivePattern { get; set; }
        public bool IsUpdatedFlag { get; set; }
    }
}
