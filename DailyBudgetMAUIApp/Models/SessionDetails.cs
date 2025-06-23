namespace DailyBudgetMAUIApp.Models
{
    public class SessionDetails
    {
        public int SessionID { get; set; } = 0;
        public string SessionToken { get; set; } 
        public DateTime SessionExpiry { get; set; }
        public int UserID { get; set; }
        public string SessionUser { get; set; }
        public string RefreshToken { get; set; }

    }
}
