using System.ComponentModel.DataAnnotations;

namespace DailyBudgetMAUIApp.Models
{
    public class BudgetHstoryLastPeriod

    {
        [Key]
        public int BudgetHistoryID { get; set; }
        [Required]
        public DateTime LastUpdate { get; set; }
        [DataType(DataType.Currency)]
        public decimal? BankBalance { get; set; }
        [DataType(DataType.Currency)]
        public decimal? MoneyAvailableBalance { get; set; }
        [DataType(DataType.Currency)]
        public decimal? LeftToSPendBalance { get; set; }
        [DataType(DataType.Currency)]
        public decimal? TotalEarnedLastPeriod { get; set; }
        [DataType(DataType.Currency)]
        public decimal? TotalSpentLastPeriod { get; set; }
        [DataType(DataType.Currency)]
        public decimal? TotalSavedlastPeriod { get; set; }
        [DataType(DataType.Currency)]
        public decimal? TotalBillsLastPeriod { get; set; }
    }
}