using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(BillID), nameof(BillID))]
    [QueryProperty(nameof(Bill), nameof(Bill))]
    [QueryProperty(nameof(NavigatedFrom), nameof(NavigatedFrom))]
    [QueryProperty(nameof(FamilyAccountID), nameof(FamilyAccountID))]
    public partial class AddBillViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int  budgetID;
        [ObservableProperty]
        private int  familyAccountID;
        [ObservableProperty]
        private int  billID;
        [ObservableProperty]
        private Bills  bill;
        [ObservableProperty]
        private decimal  billOldBalance;
        [ObservableProperty]
        private bool  isPageValid;
        [ObservableProperty]
        private DateTime  minimumDate = DateTime.UtcNow.Date.AddDays(1);
        [ObservableProperty]
        private string  navigatedFrom;
        [ObservableProperty]
        private string  billName;
        [ObservableProperty]
        private string  billPayee;
        [ObservableProperty]
        private string billCategory;
        [ObservableProperty]
        private string redirectTo;
        [ObservableProperty]
        private bool isMultipleAccounts;
        [ObservableProperty]
        private List<BankAccounts> bankAccounts;
        [ObservableProperty]
        private BankAccounts? selectedBankAccount;


        public string BillTypeText { get; set; } = "";
        public string BillRecurringText { get; set; } = "";


        public AddBillViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Add a New Outgoing";

            MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date.AddDays(1);

        }

        public async void AddBill()
        {
            Bill.BillBalanceAtLastPayDay = Bill.BillCurrentBalance;
            string SuccessCheck = await _ds.SaveNewBill(Bill,BudgetID);
            if(SuccessCheck == "OK")
            {
                Bill.BillName = "";
                Bill.BillPayee = "";
                if (RedirectTo == "CreateNewBudget")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings", false);
                }
                else if (RedirectTo == "CreateNewFamilyAccount")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Outgoings", false);
                }
                else if (RedirectTo == "ViewBills")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"//{nameof(ViewBills)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                }
                    
            }
        }

        public async void UpdateBill()
        {
            if(Bill.BillCurrentBalance < Bill.BillBalanceAtLastPayDay)
            {
                Bill.BillBalanceAtLastPayDay = Bill.BillCurrentBalance;
            }
            else
            {
                Bill.BillBalanceAtLastPayDay += (Bill.BillCurrentBalance - BillOldBalance);
            }

            string SuccessCheck = await _ds.UpdateBill(Bill);
            if(SuccessCheck == "OK")
            {
                Bill.BillName = "";
                Bill.BillPayee = "";
                if (RedirectTo == "CreateNewBudget")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
                }
                else if (RedirectTo == "CreateNewFamilyAccount")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Outgoings", false);
                }
                else if (RedirectTo == "ViewBills")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"//{nameof(ViewBills)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                }
            }
        }

        [RelayCommand]
        public async Task BackButton()
        {
            try
            {
                Bill.BillName = "";
                Bill.BillPayee = "";
                if (RedirectTo == "CreateNewBudget")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
                }
                else if (RedirectTo == "CreateNewFamilyAccount")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Outgoings", false);
                }
                else if (RedirectTo == "ViewBills")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"//{nameof(ViewBills)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "AddBill", "BackButton");
            }
        }

        [RelayCommand]
        public async Task ChangeBillName()
        {
            try
            {
                string Description = "Every outgoing needs a name, we will refer to it by the name you give it and will make it easier to identify!";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Outgoing Name", Description, DescriptionSub, "Enter an outgoing name!", Bill.BillName, new PopUpPageSingleInputViewModel());
                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    Bill.BillName = (string)result;
                    BillName = Bill.BillName;
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "AddBill", "ChangeBillName");
            }
        }

        public string CalculateRegularBillValue()
        {
            if(Bill != null)
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
            else
            {
                return "Please update details!";
            }
        }
    }
}
