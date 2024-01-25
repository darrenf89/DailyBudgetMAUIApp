using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewSavingsViewModel : BaseViewModel
    {

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        
        public ViewSavingsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;


        }
    }

}
