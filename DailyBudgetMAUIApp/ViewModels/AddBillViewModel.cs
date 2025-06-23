using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;

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
        private readonly IPopupService _ps;

        [ObservableProperty]
        public partial int BudgetID { get; set; }

        [ObservableProperty]
        public partial int FamilyAccountID { get; set; }

        [ObservableProperty]
        public partial int BillID { get; set; }

        [ObservableProperty]
        public partial Bills Bill { get; set; }

        [ObservableProperty]
        public partial decimal BillOldBalance { get; set; }

        [ObservableProperty]
        public partial bool IsPageValid { get; set; }

        [ObservableProperty]
        public partial DateTime MinimumDate { get; set; } = DateTime.UtcNow.Date.AddDays(1);

        [ObservableProperty]
        public partial string NavigatedFrom { get; set; }

        [ObservableProperty]
        public partial string BillName { get; set; }

        [ObservableProperty]
        public partial string BillPayee { get; set; }

        [ObservableProperty]
        public partial string BillCategory { get; set; }

        [ObservableProperty]
        public partial string RedirectTo { get; set; }

        [ObservableProperty]
        public partial bool IsMultipleAccounts { get; set; }

        [ObservableProperty]
        public partial List<BankAccounts> BankAccounts { get; set; }

        [ObservableProperty]
        public partial BankAccounts? SelectedBankAccount { get; set; }



        public string BillTypeText { get; set; } = "";
        public string BillRecurringText { get; set; } = "";


        public AddBillViewModel(IProductTools pt, IRestDataService ds, IPopupService ps)
        {
            _pt = pt;
            _ds = ds;
            _ps = ps;

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
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings", false);
                }
                else if (RedirectTo == "CreateNewFamilyAccount")
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Outgoings", false);
                }
                else if (RedirectTo == "ViewBills")
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
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
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
                }
                else if (RedirectTo == "CreateNewFamilyAccount")
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Outgoings", false);
                }
                else if (RedirectTo == "ViewBills")
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

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
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
                }
                else if (RedirectTo == "CreateNewFamilyAccount")
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}
                    await Task.Delay(1);
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Outgoings", false);
                }
                else if (RedirectTo == "ViewBills")
                {
                    if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

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

                var queryAttributes = new Dictionary<string, object>
                {
                    [nameof(PopUpPageSingleInputViewModel.Description)] = Description,
                    [nameof(PopUpPageSingleInputViewModel.DescriptionSub)] = DescriptionSub,
                    [nameof(PopUpPageSingleInputViewModel.InputTitle)] = "Outgoing Name",
                    [nameof(PopUpPageSingleInputViewModel.Placeholder)] = "Enter an outgoing name!",
                    [nameof(PopUpPageSingleInputViewModel.Input)] = Bill.BillName
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
                    Bill.BillName = (string)popupResult.Result;
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
