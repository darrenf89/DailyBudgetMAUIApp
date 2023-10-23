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


        public string BillTypeText { get; set; }
        public string BillRecurringText { get; set; } = "";


        public AddBillViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

        }

        [ICommand]
        async void SaveBill()
        {
            var stack = Application.Current.MainPage.Navigation.NavigationStack;
            int count = Application.Current.MainPage.Navigation.NavigationStack.Count;
            if(stack[count - 2] == "")
            {
                await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
            }
            else
            {

            }
        }

        [ICommand]
        async void AddBill()
        {
            var stack = Application.Current.MainPage.Navigation.NavigationStack;
            int count = Application.Current.MainPage.Navigation.NavigationStack.Count;
            if(stack[count - 2] == "")
            {
                await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
            }
            else
            {

            }
        }

        [ICommand]
        async void UpdateBill()
        {
            var stack = Application.Current.MainPage.Navigation.NavigationStack;
            int count = Application.Current.MainPage.Navigation.NavigationStack.Count;
            if(stack[count - 2] == "")
            {
                await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
                //await Shell.Current.GoToAsync("..")
            }
            else
            {

            }
        }

        [ICommand]
        async void ChangeBillName()
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

        private string CalculateRegualarBillValue()
        {
            if(Bill.BillAmount == 0 || Bill.BillAmount == null || Bill.BillCurrentBalance >= Bill.BillCurrentBalance || Bill.BillDueDate == null || Bill.BillDueDate <= DateTime.Now)
            {
                return "Please update details!";
            }
            else
            {
                decimal DailySavingValue = new();
                TimeSpan Difference = (TimeSpan)(Bill.BillDueDate - DateTime.Now);
                int NumberOfDays = Difference.Days;
                decimal BillAmount = _Bill.BillAmount ?? 0;
                decimal RemainingBillAmount = BillAmount.BillAmount - Bill.BillCurrentBalance;
                DailySavingValue = RemainingBillAmount / NumberOfDays;
                DailySavingValue = Math.Round(DailySavingValue, 2);

                Bill.DailySavingValue = DailySavingValue;

                return DailySavingValue.ToString("c", CultureInfo.CurrentCulture);
            }

        }
    }
}
