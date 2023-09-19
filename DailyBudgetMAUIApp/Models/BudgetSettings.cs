using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyBudgetMAUIApp.Models
{
    public class BudgetSettings
    {
        public int SettingsID { get; set; }
        public int? BudgetID { get; set; }
        public Budgets? Budget { get; set; }
        public int? CurrencyPattern { get; set; } = 1;
        public int? CurrencySymbol { get; set; } = 1;
        public int? CurrencyDecimalDigits { get; set; } = 2;
        public int? CurrencyDecimalSeparator { get; set; } = 1;
        public int? CurrencyGroupSeparator { get; set; } = 1;
        public int? DateSeperator { get; set; } = 1;
        public int? ShortDatePattern { get; set; } = 1;

    }
}
