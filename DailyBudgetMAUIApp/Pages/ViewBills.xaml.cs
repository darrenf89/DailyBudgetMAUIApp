using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Popups;

namespace DailyBudgetMAUIApp.Pages;

public partial class ViewBills : BasePage
{
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
	private readonly ViewBillsViewModel _vm;
    public ViewBills(ViewBillsViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;


        InitializeComponent();

    }

    protected async override void OnAppearing()
    {
        try
        {
            _vm.Budget = _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited").Result;
            List<Bills> B = _ds.GetBudgetBills(App.DefaultBudgetID, "ViewBills").Result;

            _vm.TotalBills = 0;
            _vm.Budget.DailyBillOutgoing = 0;
            _vm.BillsPerPayPeriod = 0;
            _vm.BudgetAllocated = _vm.Budget.BankBalance.GetValueOrDefault() - _vm.Budget.MoneyAvailableBalance.GetValueOrDefault();
            _vm.Bills.Clear();

            int DaysToPayDay = (int)Math.Ceiling((_vm.Budget.NextIncomePayday.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);

            foreach (Bills bill in B)
            {
                _vm.TotalBills += bill.BillCurrentBalance;
                _vm.BillsPerPayPeriod += bill.BillCurrentBalance;
                _vm.BillsPerPayPeriod += bill.RegularBillValue.GetValueOrDefault() * DaysToPayDay;
                _vm.Budget.DailyBillOutgoing += bill.RegularBillValue.GetValueOrDefault();
                _vm.Bills.Add(bill);
            }

            double ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            _vm.SignOutButtonWidth = ScreenWidth - 60;

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewBills", "OnAppearing");
        }

    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        // listView.HeightRequest = _vm.ScreenHeight - BudgetDetailsGrid.Height - TitleView.Height - 150;
    }

    private async void HomeButton_Clicked(object sender, EventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.MainPage)}");
    }

    private async void EditBill_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Bill = (Bills)e.Parameter;

            bool result = await Shell.Current.DisplayAlert($"Edit {Bill.BillName}?", $"Are you sure you want to edit {Bill.BillName}?", "Yes", "Cancel");

            if(result)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"///{nameof(ViewBills)}/{nameof(AddBill)}?BudgetID={_vm.Budget.BudgetID}&BillID={Bill.BillID}&NavigatedFrom=ViewBills");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewBills", "EditBill_Tapped");
        }
    }
    private async void CloseBill_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Bill = (Bills)e.Parameter;
            bool result = await Shell.Current.DisplayAlert($"Close outgoing {Bill.BillName}?", $"Are you sure you want to close {Bill.BillName}?", "Yes", "Cancel");

            if (result)
            {
                List<PatchDoc> PatchDocs = new List<PatchDoc>();
                PatchDoc IsClosed = new PatchDoc
                {
                    op = "replace",
                    path = "/IsClosed",
                    value = true
                };

                PatchDocs.Add(IsClosed);

                await _ds.PatchBill(Bill.BillID, PatchDocs);
                _vm.Bills.Remove(Bill);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewBills", "CloseBill_Tapped");
        }
    }

    private async void UpdateDate_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Bill = (Bills)e.Parameter;

            string Description = "Update the outgoing due date!";
            string DescriptionSub = "Outoing not when you expected, you can update the outgoing due date to any date in the future. We will do the rest!";
            var popup = new PopUpPageVariableInput("Outgoing due date", Description, DescriptionSub, "", Bill.BillDueDate, "DateTime", new PopUpPageVariableInputViewModel());
            var result = await Application.Current.MainPage.ShowPopupAsync(popup);

            if(!string.IsNullOrEmpty(result.ToString()))
            {
                Bill.BillDueDate = (DateTime)result;

                List<PatchDoc> PatchDocs = new List<PatchDoc>();
                PatchDoc BillDueDate = new PatchDoc
                {
                    op = "replace",
                    path = "/BillDueDate",
                    value = Bill.BillDueDate
                };

                PatchDocs.Add(BillDueDate);

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

                PatchDoc RegularBillValue = new PatchDoc
                {
                    op = "replace",
                    path = "/RegularBillValue",
                    value = Bill.RegularBillValue
                };

                PatchDocs.Add(RegularBillValue);

                await _ds.PatchBill(Bill.BillID, PatchDocs);

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewBills", "UpdateDate_Tapped");
        }
    }

    private async void UpdateAmount_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var Bill = (Bills)e.Parameter;

            string Description = "Update the outgoing amount!";
            string DescriptionSub = "Outoing not as much as you expected, you can update the outgoing amount and we will do the rest!";
            var popup = new PopUpPageVariableInput("Outgoing amount", Description, DescriptionSub, "", Bill.BillAmount, "Currency", new PopUpPageVariableInputViewModel());
            var result = await Application.Current.MainPage.ShowPopupAsync(popup);

            if (!string.IsNullOrEmpty(result.ToString()))
            {
                Bill.BillAmount = (decimal)result;

                List<PatchDoc> PatchDocs = new List<PatchDoc>();
                PatchDoc BillAmount = new PatchDoc
                {
                    op = "replace",
                    path = "/BillAmount",
                    value = Bill.BillAmount
                };

                PatchDocs.Add(BillAmount);

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

                PatchDoc RegularBillValue = new PatchDoc
                {
                    op = "replace",
                    path = "/RegularBillValue",
                    value = Bill.RegularBillValue
                };

                PatchDocs.Add(RegularBillValue);

                await _ds.PatchBill(Bill.BillID, PatchDocs);

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewBills", "UpdateAmount_Tapped");
        }
    }    

    private async void AddNewBill_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool result = await Shell.Current.DisplayAlert($"Add a new outgoing?", $"Are you sure you want to add a new outgoing?", "Yes", "Cancel");

            if (result)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.MainPage.ShowPopup(PopUp);
                }

                await Shell.Current.GoToAsync($"///{nameof(ViewBills)}/{nameof(AddBill)}?BudgetID={_vm.Budget.BudgetID}&NavigatedFrom=ViewBills");
            }
        }
        catch (Exception ex)
        {

            await _pt.HandleException(ex, "ViewBills", "AddNewBill_Clicked");
        }
    }
}