using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyBudgetMAUIApp.Models
{
    public class Savings
    {
        [Key]
        public int SavingID { get; set; }
        public string? SavingsType { get; set; }
        [Required]
        [MaxLength(15)]
        public string? SavingsName { get; set; }
        [DataType(DataType.Currency)]
        [DisplayName("Current Balance")]
        public decimal? CurrentBalance { get; set; } = 0;
        public DateTime LastUpdatedDate { get; set; } = DateTime.Now;
        [DisplayName("When?")]
        public DateTime? GoalDate { get; set; } = null;
        [DataType(DataType.Currency)]
        public decimal? LastUpdatedValue { get; set; }
        public bool isSavingsClosed { get; set; } = false;
        [DataType(DataType.Currency)]
        [DisplayName("Saving Target")]
        public decimal? SavingsGoal { get; set; } = 0;
        public bool canExceedGoal { get; set; }
        public bool isDailySaving { get; set; }
        public bool isRegularSaving { get; set; }
        [DisplayName("Savings Amount")]
        public decimal? RegularSavingValue { get; set; }
        public decimal? PeriodSavingValue { get; set; }
        public bool isAutoComplete { get; set; }
        public string? ddlSavingsPeriod { get; set; }

    }
}
