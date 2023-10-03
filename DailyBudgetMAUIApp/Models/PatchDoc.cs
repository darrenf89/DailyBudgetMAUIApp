namespace DailyBudgetMAUIApp.Models
{
    public class PatchDoc
    {
        public string op { get; set; }
        public string path { get; set;}
        public object value { get; set; }
    }
}
