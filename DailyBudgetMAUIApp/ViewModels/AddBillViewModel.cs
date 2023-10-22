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
        private DateTime _minimumDate = DateTime.UtcNow.Date.AddDays(1);

        public string BillTypeText { get; set; }


        public AddBillViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

        }

        [ICommand]
        async void AddBill()
        {
            await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
        }

        [ICommand]
        async void UpdateBill()
        {
            await Shell.Current.GoToAsync($"../../{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Outgoings");
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
    }
}
