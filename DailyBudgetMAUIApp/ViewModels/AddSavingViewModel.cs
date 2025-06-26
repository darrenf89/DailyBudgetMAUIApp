using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(SavingID), nameof(SavingID))]
    [QueryProperty(nameof(SavingType), nameof(SavingType))]
    [QueryProperty(nameof(NavigatedFrom), nameof(NavigatedFrom))]
    [QueryProperty(nameof(Saving), nameof(Saving))]
    [QueryProperty(nameof(FamilyAccountID), nameof(FamilyAccountID))]
    public partial class AddSavingViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        private readonly IPopupService _ps;

        [ObservableProperty]
        public partial int BudgetID { get; set; }

        [ObservableProperty]
        public partial int FamilyAccountID { get; set; }

        [ObservableProperty]
        public partial int SavingID { get; set; }

        [ObservableProperty]
        public partial int SelectedCalculatorIndex { get; set; } = 0;

        [ObservableProperty]
        public partial Savings Saving { get; set; }

        [ObservableProperty]
        public partial bool IsPageValid { get; set; }

        [ObservableProperty]
        public partial DateTime MinimumDate { get; set; } = DateTime.UtcNow.Date.AddDays(1);

        [ObservableProperty]
        public partial string SavingType { get; set; }

        [ObservableProperty]
        public partial string NavigatedFrom { get; set; }

        [ObservableProperty]
        public partial string IsTopUpLabelText { get; set; } = "Replenish";

        [ObservableProperty]
        public partial string IsTopUpParaText { get; set; } = "By replenishing the stash, every pay period we will reset the envelopes balance to the saving amount. Any balance not spent by the end of the period will effectively be added back to your budget for you to spend!";

        [ObservableProperty]
        public partial bool ShowCalculator { get; set; }


        public string SavingTypeText { get; set; } = "";
        public string SavingRecurringText { get; set; } = "";
        public DateTime BudgetNextPayDate { get; set; }
        public int BudgetDaysToNextPay { get; set; } = 0;
        public int BudgetDaysBetweenPay { get; set; } = 30;
        public List<PickerClass> DropDownSavingPeriod { get; set; } = new List<PickerClass>();


        public AddSavingViewModel(IProductTools pt, IRestDataService ds, IPopupService ps)
        {
            _pt = pt;
            _ds = ds;
            _ps = ps;   

            Saving = new Savings();

            PickerClass PerPayPeriod = new PickerClass("Pay Day", "PerPayPeriod");
            DropDownSavingPeriod.Add(PerPayPeriod);
            PickerClass PerDay = new PickerClass("Every Day", "PerDay");
            DropDownSavingPeriod.Add(PerDay);

            MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date.AddDays(1);

        }

        public async void AddSaving()
        {

            bool result = await Shell.Current.DisplayAlert($"Add a new saving {Saving.SavingsName}?", $"Are you sure you want to add a new saving {Saving.SavingsName}?", "Yes", "Cancel");
            if (result)
            {
                Saving.LastUpdatedValue = Saving.CurrentBalance;
                int SavingID = await _ds.SaveNewSaving(Saving, BudgetID);
                if (SavingID != 0)
                {
                    if (NavigatedFrom == "CreateNewBudget")
                    {
                        await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Bills");
                    }
                    else if (NavigatedFrom == "CreateNewFamilyAccountSaving")
                    {
                        await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Bills", false);
                    }
                    else if (NavigatedFrom == "CreateNewFamilyAccountEnvelope")
                    {
                        await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Envelopes", false);
                    }
                    else if (NavigatedFrom == "ViewSavings")
                    {
                        await Shell.Current.GoToAsync($"//{nameof(ViewSavings)}");
                    }
                    else if (NavigatedFrom == "ViewEnvelopes")
                    {
                        await Shell.Current.GoToAsync($"//{nameof(ViewEnvelopes)}");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"///{nameof(MainPage)}?SnackBar=Saving Added&SnackID={SavingID}");
                    }
                }
            }

        }

        [RelayCommand]
        public async Task BackButton()
        {
            try
            {
                if (NavigatedFrom == "CreateNewBudget")
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Bills");
                }
                else if (NavigatedFrom == "CreateNewFamilyAccountSaving")
                {
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Bills", false);
                }
                else if (NavigatedFrom == "CreateNewFamilyAccountEnvelope")
                {
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Envelopes", false);
                }
                else if (NavigatedFrom == "ViewSavings")
                {
                    await Shell.Current.GoToAsync($"//{nameof(ViewSavings)}");
                }
                else if (NavigatedFrom == "ViewEnvelopes")
                {
                    await Shell.Current.GoToAsync($"//{nameof(ViewEnvelopes)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "AddSaving", "BackButton");
            }
        }

        public async void UpdateSaving()
        {
 
            bool result = await Shell.Current.DisplayAlert($"Update {Saving.SavingsName}?", $"Are you sure you want to update {Saving.SavingsName}?", "Yes", "Cancel");
            if (result)
            {
                string SuccessCheck = await _ds.UpdateSaving(Saving);
                if (SuccessCheck == "OK")
                {
                    if (NavigatedFrom == "CreateNewBudget")
                    {
                        await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Bills");
                    }
                    else if (NavigatedFrom == "CreateNewFamilyAccountSaving")
                    {
                        await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Bills", false);
                    }
                    else if (NavigatedFrom == "CreateNewFamilyAccountEnvelope")
                    {
                        await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Envelopes", false);
                    }
                    else if (NavigatedFrom == "ViewSavings")
                    {
                        await Shell.Current.GoToAsync($"//{nameof(ViewSavings)}");
                    }
                    else if (NavigatedFrom == "ViewEnvelopes")
                    {
                        await Shell.Current.GoToAsync($"//{nameof(ViewEnvelopes)}");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                    }
                }
            }

        }

        [RelayCommand]
        public async Task ChangeSavingsName()
        {
            try
            {
                string Description = "Every savings needs a name, we will refer to it by the name you give it and this will make it easier to identify!";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";

                var queryAttributes = new Dictionary<string, object>
                {
                    [nameof(PopUpPageSingleInputViewModel.Description)] = Description,
                    [nameof(PopUpPageSingleInputViewModel.DescriptionSub)] = DescriptionSub,
                    [nameof(PopUpPageSingleInputViewModel.InputTitle)] = "Saving Name",
                    [nameof(PopUpPageSingleInputViewModel.Placeholder)] = "Enter a Saving name!",
                    [nameof(PopUpPageSingleInputViewModel.Input)] = Saving.SavingsName
                };

                var popupOptions = new PopupOptions
                {
                    CanBeDismissedByTappingOutsideOfPopup = false,
                    PageOverlayColor = Color.FromArgb("#800000").WithAlpha(0.5f),
                };

                IPopupResult<object> popupResult = await _ps.ShowPopupAsync<PopUpPageSingleInput, object>(
                    Shell.Current,
                    options: popupOptions,
                    shellParameters: queryAttributes,
                    cancellationToken: CancellationToken.None

                );

                if (popupResult.Result != null || (string)popupResult.Result != "")
                {
                    Saving.SavingsName = (string)popupResult.Result;
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "AddSaving", "ChangeSavingsName");
            }
        }

        public string CalculateSavingRegularValues()
        {
            if (Saving.SavingsGoal != 0 && Saving.GoalDate > DateTime.Today.Date)
            {
                int DaysToSavingDate = (int)Math.Ceiling((Saving.GoalDate.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);
                decimal? AmountOutstanding = Saving.SavingsGoal - Saving.CurrentBalance;

                if(DaysToSavingDate != 0)
                {
                    Saving.RegularSavingValue = AmountOutstanding / DaysToSavingDate;
                }
                else
                {
                    Saving.RegularSavingValue = 0;
                }                

                return Saving.RegularSavingValue.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
            }

            return "Please update values";
        }

        public DateTime CalculateSavingDate()
        {
            if(Saving.SavingsGoal != 0 && Saving.RegularSavingValue !=0)
            {
                if(Saving.DdlSavingsPeriod == "PerDay")
                {
                    decimal? BalanceLeft = Saving.SavingsGoal - (Saving.CurrentBalance ?? 0);
                    int NumberOfDays = (int)Math.Ceiling(BalanceLeft / Saving.RegularSavingValue ?? 0);

                    DateTime Today = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date;
                    Saving.GoalDate = Today.AddDays(NumberOfDays);
                }
                else if (Saving.DdlSavingsPeriod == "PerPayPeriod")
                {
                    decimal? BalanceLeft = Saving.SavingsGoal - (Saving.CurrentBalance ?? 0);
                    BalanceLeft = BalanceLeft - Saving.PeriodSavingValue;

                    decimal NumberOfPeriods = BalanceLeft / Saving.PeriodSavingValue ?? 0;
                    int NumberOfDays = (int)Math.Ceiling(NumberOfPeriods * BudgetDaysBetweenPay);

                    Saving.GoalDate = BudgetNextPayDate.AddDays(NumberOfDays);
                }
            }

            return Saving.GoalDate.GetValueOrDefault();
        }

    }
    public class PickerClass
    {        
        public PickerClass(string name, string key) 
        { 
            this.Name = name;
            this.Key = key;
        }
        public string Name { get; set; }
        public string Key { get; set; }
    }
}
