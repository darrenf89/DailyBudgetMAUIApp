using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewCategoriesPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObserableObject<Categories> _categories;
                [ObservableProperty]
        private ObserableObject<Categories> _groupCategories;
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        
        public ViewCategoriesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            Categories = (ObservableObject)_ds.GetCategoriesCalculated(App.DefaultBudgetID).Result;

            foreach(Categories cat in Categories.Where(c=>c.!isSubCategory).ToList())
            {
                GroupCategories.Add(cat);
            }
        }
    }

}
