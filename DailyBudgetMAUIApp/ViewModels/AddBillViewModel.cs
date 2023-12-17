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
    [QueryProperty(nameof(BillID), nameof(BillID))]
    public partial class AddBillViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int _budgetID;
        [ObservableProperty]
        private int _billID;
        [ObservableProperty]
        private Bills _bill;
        [ObservableProperty]
        private bool _isPageValid;
        [ObservableProperty]
        private DateTime _minimumDate = DateTime.UtcNow.Date.AddDays(1);


        public string BillTypeText { get; set; } = "";
        public string BillRecurringText { get; set; } = "";


        public AddBillViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Add a New Outgoing";
            Bill = new Bills();

            MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date.AddDays(1);

        }

        public async void AddBill()
        {
            try
            {
                string SuccessCheck = _ds.SaveNewBill(Bill,BudgetID).Result;
                if(SuccessCheck == "OK")
                {
  
                    var stack = Application.Current.MainPage.Navigation.NavigationStack;
                    int count = Application.Current.MainPage.Navigation.NavigationStack.Count;
                    if (count >= 2)
                    {
                        if (stack[count - 2].ToString() == "DailyBudgetMAUIApp.Pages.CreateNewBudget")
                        {
                            await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
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
                ErrorLog Error = _pt.HandleCatchedException(ex, "AddBill", "AddBill").Result;
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        public async void UpdateBill()
        {
            try
            {
                string SuccessCheck = _ds.UpdateBill(Bill).Result;
                if(SuccessCheck == "OK")
                {
                    var stack = Application.Current.MainPage.Navigation.NavigationStack;
                    int count = Application.Current.MainPage.Navigation.NavigationStack.Count;
                    if (count >= 2)
                    {
                        if (stack[count - 2].ToString() == "DailyBudgetMAUIApp.Pages.CreateNewBudget")
                        {
                            await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
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
                ErrorLog Error = _pt.HandleCatchedException(ex, "AddBill", "UpdateBill").Result;
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        [ICommand]
        public async void ChangeBillName()
        {
            try
            {
                string Description = "Every outgoing needs a name, we will refer to it by the name you give it and will make it easier to identify!";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Outgoing Name", Description, DescriptionSub, "Enter an outgoing name!", Bill.BillName, new PopUpPageSingleInputViewModel());
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    Bill.BillName = (string)result;
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

        public string CalculateRegularBillValue()
        {
            if(Bill.BillAmount == 0 || Bill.BillAmount == null || Bill.BillCurrentBalance >= Bill.BillAmount || Bill.BillDueDate == null || Bill.BillDueDate.GetValueOrDefault().Date <= _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
            {
                return "Please update details!";
            }
            else
            {
                decimal DailySavingValue = new();
                TimeSpan Difference = (TimeSpan)(Bill.BillDueDate.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date);
                int NumberOfDays = (int)Difference.TotalDays;
                decimal RemainingBillAmount = Bill.BillAmount - Bill.BillCurrentBalance ?? 0;
                if(NumberOfDays != 0)
                {
                    DailySavingValue = RemainingBillAmount / NumberOfDays;
                    DailySavingValue = Math.Round(DailySavingValue, 2);
                }
                else
                {
                    DailySavingValue = 0;
                }

                Bill.RegularBillValue = DailySavingValue;

                return DailySavingValue.ToString("c", CultureInfo.CurrentCulture);
            }

        }
    }
}
