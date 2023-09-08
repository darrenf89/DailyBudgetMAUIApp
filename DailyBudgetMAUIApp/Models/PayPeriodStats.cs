using DailyBudgetMAUIApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyBudgetMAUIApp.Models
{
    public class PayPeriodStats
    {
        [Key]
        public int PayPeriodID { get; set; }
        public bool isCurrentPeriod { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DurationOfPeriod { get; set; }
        public decimal SavingsToDate { get; set; }
        public decimal BillsToDate { get; set; }
        public decimal IncomeToDate { get; set; }
        public decimal SpendToDate { get; set; }
        public decimal? StartLtSDailyAmount { get; set; }
        public decimal? StartLtSPeiordAmount { get; set; }
        public decimal? StartBBPeiordAmount { get; set; }
        public decimal? StartMaBPeiordAmount { get; set; }
        public decimal? EndLtSDailyAmount { get; set; }
        public decimal? EndLtSPeiordAmount { get; set; }
        public decimal? EndBBPeiordAmount { get; set; }
        public decimal? EndMaBPeiordAmount { get; set; }

    }
}
