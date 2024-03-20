using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class lut_ShortDatePattern : ObservableObject
    {
        [ObservableProperty]
        public int  id;
        [ObservableProperty]        
        public string shortDatePattern = "";

    }
}
