using CommunityToolkit.Mvvm.ComponentModel;

using DailyBudgetMAUIApp.Handlers;

namespace DailyBudgetMAUIApp.Models
{
    public partial class Categories : ObservableObject, IIndexable
    {
        [ObservableProperty]
        public partial int  CategoryID { get; set; }
        [ObservableProperty]
        public partial string?  CategoryName { get; set; }
        [ObservableProperty]
        public partial bool  IsSubCategory { get; set; }
        [ObservableProperty]
        public partial int?  CategoryGroupID { get; set; }
        [ObservableProperty]
        public partial string  CategoryIcon { get; set; }
        [ObservableProperty]
        public partial decimal  CategorySpendAllTime { get; set; }
        [ObservableProperty]
        public partial decimal  CategorySpendPayPeriod { get; set; }
        [ObservableProperty]
        public partial List<SpendPeriods>  CategorySpendPeriods  { get; set; } = new List<SpendPeriods>();
        [ObservableProperty]
        public partial bool IsEditMode { get; set; } = false;
        public int Index { get; set; }
    }

    public partial class SpendPeriods: ObservableObject
    {
        [ObservableProperty]
        public partial DateTime  FromDate { get; set; }
        [ObservableProperty]
        public partial DateTime  ToDate { get; set; }
        [ObservableProperty]
        public partial decimal  SpendTotalAmount { get; set; }
        [ObservableProperty]
        public partial bool  IsCurrentPeriod { get; set; }
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
