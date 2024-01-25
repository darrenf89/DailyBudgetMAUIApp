using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewCategoriesViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Categories> _categories;
        [ObservableProperty]
        private ObservableCollection<Categories> _groupCategories;

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        
        public ViewCategoriesViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            //Categories = (ObservableCollection)_ds.GetCategoriesCalculated(App.DefaultBudgetID).Result;

            foreach (Categories cat in Categories.Where(c =>!c.IsSubCategory).ToList())
            {
                GroupCategories.Add(cat);
            }
        }
    }

}
