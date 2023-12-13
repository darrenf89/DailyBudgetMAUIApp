using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(SavingID), nameof(SavingID))]
    public partial class AddSavingViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private int _savingID;
        [ObservableProperty]
        private Savings _saving;
        [ObservableProperty]
        private bool _isPageValid;
        [ObservableProperty]
        private DateTime _minimumDate = DateTime.UtcNow.Date.AddDays(1);

        public string SavingTypeText { get; set; } = "";
        public string SavingRecurringText { get; set; } = "";
        public DateTime BudgetNextPayDate { get; set; }
        public int BudgetDaysToNextPay { get; set; } = 0;
        public int BudgetDaysBetweenPay { get; set; } = 30;
        public List<PickerClass> DropDownSavingPeriod { get; set; } = new List<PickerClass>();


        public AddSavingViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Saving = new Savings();

            PickerClass PerPayPeriod = new PickerClass("Pay Day", "PerPayPeriod");
            DropDownSavingPeriod.Add(PerPayPeriod);
            PickerClass PerDay = new PickerClass("Every Day", "PerDay");
            DropDownSavingPeriod.Add(PerDay);

            MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date.AddDays(1);

        }

        public async void AddSaving()
        {
            try
            {
                Saving.LastUpdatedValue = Saving.CurrentBalance;
                string SuccessCheck = _ds.SaveNewSaving(Saving, BudgetID).Result;
                if (SuccessCheck == "OK")
                {
                    var stack = Application.Current.MainPage.Navigation.NavigationStack;
                    int count = Application.Current.MainPage.Navigation.NavigationStack.Count;
                    if (count >= 2)
                    {
                        if (stack[count - 2].ToString() == "DailyBudgetMAUIApp.Pages.CreateNewBudget")
                        {
                            await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Savings");
                        }
                        else
                        {
                            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                        }
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                    }

                }
            }
            catch (Exception ex)
            {
                ErrorLog Error = _pt.HandleCatchedException(ex, "AddSaving", "AddSaving").Result;
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        public async void UpdateSaving()
        {
            try
            {
                string SuccessCheck = _ds.UpdateSaving(Saving).Result;
                if (SuccessCheck == "OK")
                {
                    var stack = Application.Current.MainPage.Navigation.NavigationStack;
                    int count = Application.Current.MainPage.Navigation.NavigationStack.Count;
                    if (count >= 2)
                    {
                        if (stack[count - 2].ToString() == "DailyBudgetMAUIApp.Pages.CreateNewBudget")
                        {
                            await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Savings");
                        }
                        else
                        {
                            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                        }
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog Error = _pt.HandleCatchedException(ex, "AddSaving", "UpdateSaving").Result;
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        [ICommand]
        public async void ChangeSavingsName()
        {
            try
            {
                string Description = "Every savings needs a name, we will refer to it by the name you give it and this will make it easier to identify!";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Saving Name", Description, DescriptionSub, "Enter an Saving name!", Saving.SavingsName, new PopUpPageSingleInputViewModel());
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    Saving.SavingsName = (string)result;
                }

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

        public string CalculateSavingRegularValues()
        {
            if (Saving.SavingsGoal != 0 && Saving.GoalDate > DateTime.Today.Date)
            {
                int DaysToSavingDate = (Saving.GoalDate.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).Days;
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
