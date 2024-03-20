using CommunityToolkit.Mvvm.ComponentModel;

using DailyBudgetMAUIApp.Handlers;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Categories : ObservableObject, IIndexable
    {
        [ObservableProperty]
        public int  categoryID;
        [ObservableProperty]
        public string?  categoryName;
        [ObservableProperty]
        public bool  isSubCategory;
        [ObservableProperty]
        public int?  categoryGroupID;
        [ObservableProperty]
        public string  categoryIcon;
        [ObservableProperty]
        public decimal  categorySpendAllTime;
        [ObservableProperty]
        public decimal  categorySpendPayPeriod;
        [ObservableProperty]
        public List<SpendPeriods>  categorySpendPeriods = new List<SpendPeriods>();
        [ObservableProperty]
        public bool  isEditMode = false;
        public int Index { get; set; }
    }

    public partial class SpendPeriods: ObservableObject
    {
        [ObservableProperty]
        public DateTime  fromDate;
        [ObservableProperty]
        public DateTime  toDate;
        [ObservableProperty]
        public decimal  spendTotalAmount;
        [ObservableProperty]
        public bool  isCurrentPeriod;
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
