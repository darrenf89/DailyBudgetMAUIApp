using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Diagnostics;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class EditBudgetSettingsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private List<lut_CurrencySymbol> currencySearchResults;
        [ObservableProperty]
        private lut_CurrencySymbol selectedCurrencySymbol;
        [ObservableProperty]
        private bool searchVisible = false;
        [ObservableProperty]
        private List<lut_CurrencyPlacement> currencyPlacements;
        [ObservableProperty]
        private lut_CurrencyPlacement selectedCurrencyPlacement;
        [ObservableProperty]
        private List<lut_DateFormat> dateFormats;
        [ObservableProperty]
        private List<lut_BudgetTimeZone> timeZones;
        [ObservableProperty]
        private lut_DateFormat selectedDateFormats;
        [ObservableProperty]
        private List<lut_NumberFormat> numberFormats;
        [ObservableProperty]
        private lut_NumberFormat selectedNumberFormats;
        [ObservableProperty]
        private lut_BudgetTimeZone selectedTimeZone;
        [ObservableProperty]
        private bool isBorrowPay;
        [ObservableProperty]
        private BudgetSettings budgetSettings;
        [ObservableProperty]
        private string currentTime;
        [ObservableProperty]
        private CultureInfo timeCultureInfo = new CultureInfo("en-gb");

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

            TimeCultureInfo.DateTimeFormat.ShortDatePattern = _ds.GetShortDatePatternById(SelectedDateFormats.ShortDatePatternID).Result.ShortDatePattern;
            TimeCultureInfo.DateTimeFormat.DateSeparator = _ds.GetDateSeperatorById(SelectedDateFormats.DateSeperatorID).Result.DateSeperator;

        }

        partial void OnSelectedDateFormatsChanged(lut_DateFormat value)
        {
            if(SelectedDateFormats != null)
            {
                TimeCultureInfo.DateTimeFormat.ShortDatePattern = SelectedDateFormats.DateFormat;
                TimeCultureInfo.DateTimeFormat.LongDatePattern = SelectedDateFormats.DateFormat + "HH:mm:ss";
            }

        }
        
        public void ChangeSelectedCurrency()
        {
            SearchVisible = true;
            CurrencySearchResults = _ds.GetCurrencySymbols("").Result;
        }

        [RelayCommand]
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
                    cs.Code = "No results please, try again!";
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

        [RelayCommand]
        private void CurrencySymbolSelected(lut_CurrencySymbol item)
        {
            SelectedCurrencySymbol = item;
            SearchVisible = false;
            CurrencySearchResults = null;
        }

        [RelayCommand]
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

        public async Task UpdateTime()
        {
            DateTime CurrentDateTime = DateTime.UtcNow.AddHours(SelectedTimeZone.TimeZoneUTCOffset);
            CurrentTime = CurrentDateTime.ToString(TimeCultureInfo);
        }
    }
}
