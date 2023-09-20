using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isPageBusy;

        [ObservableProperty]
        private bool _isButtonBusy;

        [ObservableProperty]
        private string _title;

        public BaseViewModel()
        {
            RestDataService _ds = new RestDataService();

            if(App.DefaultBudgetID != null && App.DefaultBudgetID != 0)
            {
                if(App.CurrentSettings == null)
                {
                    BudgetSettingValues Settings = _ds.GetBudgetSettings(App.DefaultBudgetID).Result;
                    App.CurrentSettings = Settings;
                }
                
                CultureInfo.DefaultThreadCurrentCulture.NumberFormat.CurrencySymbol = App.CurrentSettings.CurrencySymbol;
                CultureInfo.DefaultThreadCurrentCulture.NumberFormat.CurrencyDecimalSeparator = App.CurrentSettings.CurrencyDecimalSeparator;
                CultureInfo.DefaultThreadCurrentCulture.NumberFormat.CurrencyGroupSeparator = App.CurrentSettings.CurrencyGroupSeparator;
                CultureInfo.DefaultThreadCurrentCulture.NumberFormat.CurrencyDecimalDigits = App.CurrentSettings.CurrencyDecimalDigits;
                CultureInfo.DefaultThreadCurrentCulture.NumberFormat.CurrencyPositivePattern = App.CurrentSettings.CurrencyPositivePattern;
                CultureInfo.DefaultThreadCurrentCulture.DateTimeFormat.ShortDatePattern = App.CurrentSettings.ShortDatePattern;
                CultureInfo.DefaultThreadCurrentCulture.DateTimeFormat.DateSeparator = App.CurrentSettings.DateSeparator;
            }
            else
            {
                 CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-gb");
            }



        }


    }
}
