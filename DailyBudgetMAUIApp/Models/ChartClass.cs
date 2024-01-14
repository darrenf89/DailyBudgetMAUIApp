using System.ComponentModel.DataAnnotations;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace DailyBudgetMAUIApp.Models
{
    public partial class ChartClass : ObservableObject
    {
        [ObservableProperty]
        private int _xaxis;
        [ObservableProperty]
        public string? _yaxis;
    }

}
