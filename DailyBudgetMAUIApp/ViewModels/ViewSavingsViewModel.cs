using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace DailyBudgetMAUIApp.ViewModels
{

    public partial class ViewSavingsViewModel : BaseViewModel
    {

        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        [ObservableProperty]
        private ObservableCollection<Savings> _savings = new ObservableCollection<Savings>();
        
        public ViewSavingsViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;

            List<Savings> S = _ds.GetBudgetRegularSaving(App.DefaultBudgetID).Result;
            foreach(Savings saving in S)
            {
                Savings.Add(saving);
            }

            Title = $"Check Your Savings {App.UserDetails.NickName}";
        }

    }

}
