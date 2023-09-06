using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(Error), nameof(Error))]
    public partial class ErrorPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ErrorLog _error;

        public ErrorPageViewModel()
        {

        }
    }
}
