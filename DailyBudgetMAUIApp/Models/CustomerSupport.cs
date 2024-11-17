using CommunityToolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class CustomerSupport : ObservableObject
    {
        [ObservableProperty]
        public int supportID;
        [ObservableProperty]
        public string type;
        [ObservableProperty]
        public string details;
        [ObservableProperty]
        public string? phoneNumber;
        [ObservableProperty]
        public string? fileName;
        [ObservableProperty]
        public string? fileLocation;
        [ObservableProperty]
        public DateTime whenadded;
        [ObservableProperty]
        public bool isClosed;
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [ObservableProperty]
        public bool isUnreadMessages;
        [ObservableProperty]
        public List<CustomerSupportMessage>? replys;
    }  
}
