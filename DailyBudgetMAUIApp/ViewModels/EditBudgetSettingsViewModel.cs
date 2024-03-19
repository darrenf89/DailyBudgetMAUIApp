using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class EditBudgetSettingsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private List<lut_CurrencySymbol> _currencySearchResults;
        [ObservableProperty]
        private lut_CurrencySymbol _selectedCurrencySymbol;
        [ObservableProperty]
        private bool _searchVisible = false;
        [ObservableProperty]
        private List<lut_CurrencyPlacement> _currencyPlacements;
        [ObservableProperty]
        private lut_CurrencyPlacement _selectedCurrencyPlacement;
        [ObservableProperty]
        private List<lut_DateFormat> _dateFormats;
        [ObservableProperty]
        private List<lut_BudgetTimeZone> _timeZones;
        [ObservableProperty]
        private lut_DateFormat _selectedDateFormats;
        [ObservableProperty]
        private List<lut_NumberFormat> _numberFormats;
        [ObservableProperty]
        private lut_NumberFormat _selectedNumberFormats;
        [ObservableProperty]
        private lut_BudgetTimeZone _selectedTimeZone;
        [ObservableProperty]
        private bool _isBorrowPay;
        [ObservableProperty]
        private BudgetSettings _budgetSettings;

        public EditBudgetSettingsViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            CurrencyPlacements = _ds.GetCurrencyPlacements("").Result;
            DateFormats = _ds.GetDateFormatsByString("").Result;
            NumberFormats = _ds.GetNumberFormats().Result;
            TimeZones = _ds.GetBudgetTimeZones("").Result;
        }      
        
        public async Task OnLoad()
        {

            BudgetSettings = _ds.GetBudgetSettings(App.DefaultBudgetID).Result;

            SelectedCurrencySymbol = _ds.GetCurrencySymbols(BudgetSettings.CurrencySymbol.ToString()).Result[0];
            SelectedCurrencyPlacement = _ds.GetCurrencyPlacements(BudgetSettings.CurrencyPattern.ToString()).Result[0];
            SelectedDateFormats = _ds.GetDateFormatsById(BudgetSettings.ShortDatePattern ?? 1, BudgetSettings.DateSeperator ?? 1).Result;
            SelectedNumberFormats = _ds.GetNumberFormatsById(BudgetSettings.CurrencyDecimalDigits ?? 2, BudgetSettings.CurrencyDecimalSeparator ?? 2, BudgetSettings.CurrencyGroupSeparator ?? 1).Result;
            SelectedTimeZone = _ds.GetTimeZoneById(BudgetSettings.TimeZone.GetValueOrDefault()).Result;
        }
        
        public void ChangeSelectedCurrency()
        {
            SearchVisible = true;
            CurrencySearchResults = _ds.GetCurrencySymbols("").Result;
        }

        [ICommand]
        async void CurrencySearch(string query)
        {
            try
            {
                CurrencySearchResults = _ds.GetCurrencySymbols(query).Result;
            }
            catch (Exception ex)
            {
                if (ex.Message == "One or more errors occurred. (No currencies found)")
                {
                    lut_CurrencySymbol cs = new lut_CurrencySymbol();
                    cs._code = "No results please, try again!";
                    CurrencySearchResults.Clear();
                    CurrencySearchResults.Add(cs);
                }
                else
                {
                    Debug.WriteLine($" --> {ex.Message}");
                    ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "CurrencySymbol").Result;
                    await Shell.Current.GoToAsync(nameof(ErrorPage),
                        new Dictionary<string, object>
                        {
                            ["Error"] = Error
                        });
                }
            }
        }

        [ICommand]
        private void CurrencySymbolSelected(lut_CurrencySymbol item)
        {
            SelectedCurrencySymbol = item;
            SearchVisible = false;
            CurrencySearchResults = null;
        }

        [ICommand]
        private async void CloseSettings(object obj)
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.MainPage.ShowPopup(PopUp);
            }

            await Task.Delay(500);

            await Shell.Current.GoToAsync($"..");
        }
    }
}
