using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DailyBudgetMAUIApp.Models
{
    public class Transactions
    {
        [Key]
        public int TransactionID { get; set; }
        public bool isSpendFromSavings { get; set; } = false;
        [ForeignKey("Savings")]
        public int? SavingID { get; set; }
        public string? SavingName { get; set; } = "";
        public DateTime? TransactionDate { get; set; }
        public DateTime? WhenAdded { get; set; } = DateTime.UtcNow;
        public bool isIncome { get; set; } = false;
        public decimal? TransactionAmount { get; set; }
        public string? Category { get; set; } = "";
        public string? Payee { get; set; } = "";
        public string? Notes { get; set; }
        [ForeignKey("Categories")]
        public int? CategoryID { get; set; }
        public bool isTransacted { get; set; } = false;
        public string? SavingsSpendType { get; set; }
        [NotMapped]
        public List<string> PayeeList { get; set; } = new List<string>();
        public string stage { get; set; } = "Create";
    }
}
