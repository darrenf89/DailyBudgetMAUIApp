using DailyBudgetMAUIApp.DataServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(SupportID), nameof(SupportID))]
    public partial class ViewSupportViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int supportID = 0;
       
        public ViewSupportViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;                       
        }       
    }
}
