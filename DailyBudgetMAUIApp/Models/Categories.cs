using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Categories: ObservableObject
    {
        [ObservableProperty]
        public int _categoryID;
        [ObservableProperty]
        public string? _categoryName;
        [ObservableProperty]        
        public bool _isSubCategory;
        [ObservableProperty]
        public int? _categoryGroupID;
        [ObservableProperty]
        public string _categoryIcon;
        [ObservableProperty]
        public double _categorySpendAllTime;
        [ObservableProperty]
        public double _categorySpendPayPeriod;


    }
}
