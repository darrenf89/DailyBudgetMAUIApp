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
        public decimal _categorySpendAllTime;
        [ObservableProperty]
        public decimal _categorySpendPayPeriod;
        [ObservableProperty]
        public List<SpendPeriods> _categorySpendPeriods = new List<SpendPeriods>();
    }

    public partial class SpendPeriods: ObservableObject
    {
        [ObservableProperty]
        public DateTime _fromDate;
        [ObservableProperty]
        public DateTime _toDate;
        [ObservableProperty]
        public decimal _spendTotalAmount;
        [ObservableProperty]
        public bool _isCurrentPeriod;
    }

    public class DefaultCategories
    {
        public string CatName { get; set; }
        public string CategoryIcon { get; set; }
        public List<SubCategories> SubCategories { get; set; }
     
    }

    public class SubCategories
    {
        public string SubCatName { get; set; }
    }
}
