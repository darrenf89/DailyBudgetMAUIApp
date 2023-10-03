using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Toolkit.Mvvm.ComponentModel;


namespace DailyBudgetMAUIApp.Models
{
    public partial class BudgetSettings : ObservableObject
    {
        [ObservableProperty]
        public int _settingsID; 
        [ObservableProperty]
        public int? _budgetID; 
        [ObservableProperty]
        public int? _currencyPattern  = 1;
        [ObservableProperty]
        public int? _currencySymbol  = 1;
        [ObservableProperty]
        public int? _currencyDecimalDigits  = 2;
        [ObservableProperty]
        public int? _currencyDecimalSeparator  = 1;
        [ObservableProperty]
        public int? _currencyGroupSeparator  = 1;
        [ObservableProperty]
        public int? _dateSeperator  = 1;
         [ObservableProperty]       
        public int? _shortDatePattern  = 1;

    }
}
