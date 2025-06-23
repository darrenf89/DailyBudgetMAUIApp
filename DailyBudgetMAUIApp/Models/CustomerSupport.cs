using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DailyBudgetMAUIApp.Models
{
    public partial class CustomerSupport : ObservableObject
    {
        [ObservableProperty]
        public partial int SupportID { get; set; }

        [ObservableProperty]
        public partial string Type { get; set; }

        [ObservableProperty]
        public partial string Details { get; set; }

        [ObservableProperty]
        public partial string? PhoneNumber { get; set; }

        [ObservableProperty]
        public partial string? FileName { get; set; }

        [ObservableProperty]
        public partial string? FileLocation { get; set; }

        [ObservableProperty]
        public partial DateTime Whenadded { get; set; }

        [ObservableProperty]
        public partial bool IsClosed { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [ObservableProperty]
        public partial bool IsUnreadMessages { get; set; }

        [ObservableProperty]
        public partial List<CustomerSupportMessage>? Replys { get; set; }
    }
}
