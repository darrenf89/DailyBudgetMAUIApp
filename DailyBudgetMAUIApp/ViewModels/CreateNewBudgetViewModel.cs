using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Xml.Linq;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    public partial class CreateNewBudgetViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private BudgetSettings _budgetSettings;
        [ObservableProperty]
        private Budgets _budget;
        [ObservableProperty]
        private string _budgetName;
        [ObservableProperty]
        private string _stage = "Budget Settings";
        public double StageWidth { get; }
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
        private lut_DateFormat _selectedDateFormats;
        [ObservableProperty]
        private List<lut_NumberFormat> _numberFormats;
        [ObservableProperty]
        private lut_NumberFormat _selectedNumberFormats;

        public CreateNewBudgetViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;
            StageWidth = (((DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 52) / 5);
            CurrencyPlacements = _ds.GetCurrencyPlacements("").Result;
            DateFormats = _ds.GetDateFormatsByString("").Result;
            NumberFormats = _ds.GetNumberFormats().Result;
        }

        [ICommand]
        async void ChangeBudgetName()
        {
            try
            {
                string Description = "Every budget needs a name, let us know how you'd like your budget to be known so we can use this to identify it for you in the future.";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Budget Name", Description, DescriptionSub, "Enter a budget name!", Budget.BudgetName , new PopUpPageSingleInputViewModel());
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    Budget.BudgetName = (string)result;
                }

                SaveStage("Budget Name");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($" --> {ex.Message}");
                ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "Constructor").Result;
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
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
                if(ex.Message == "One or more errors occurred. (No currencies found)")
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
        async void ContinueSettings()
        {
            SaveStage("Budget Settings");
        }

        private async void SaveStage(string CurrentStage)
        {
            try
            {
                List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                switch (CurrentStage)
                {
                case "Budget Name":

                    PatchDoc BudgetName = new PatchDoc
                    {
                        op = "replace",
                        path = "/BudgetName",
                        value = Budget.BudgetName
                    };

                    BudgetUpdate.Add(BudgetName);
                    await _ds.PatchBudget(BudgetID, BudgetUpdate);
                    break;

                case "Budget Settings":

                    BudgetSettings.CurrencyPattern = SelectedCurrencyPlacement.Id;
                    BudgetSettings.CurrencySymbol = SelectedCurrencySymbol.Id;
                    BudgetSettings.CurrencyDecimalDigits = SelectedNumberFormats.CurrencyDecimalDigitsID;
                    BudgetSettings.CurrencyDecimalSeparator = SelectedNumberFormats.CurrencyDecimalSeparatorID;
                    BudgetSettings.CurrencyGroupSeparator = SelectedNumberFormats.CurrencyGroupSeparatorID;
                    BudgetSettings.DateSeperator = SelectedDateFormats.DateSeperatorID;
                    BudgetSettings.ShortDatePattern = SelectedDateFormats.ShortDatePatternID;

                    await _ds.UpdateBudgetSettings(BudgetID, BudgetSettings);

                    App.CurrentSettings.IsUpdatedFlag = true;

                    break;
                case "Budget Details":

                    break;
                case "Budget Outgoings":

                    break;
                case "Budget Savings":

                    break;
                case "Budget Extra Income":

                    break;
                }                
            }
            catch (Exception ex)
            {
                ErrorLog Error = _pt.HandleCatchedException(ex, "CreateNewBudget", "UpdateBudget").Result;
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }

        }
    }
}
