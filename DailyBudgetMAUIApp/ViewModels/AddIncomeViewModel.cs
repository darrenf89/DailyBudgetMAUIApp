using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(IncomeID), nameof(IncomeID))]
    [QueryProperty(nameof(NavigatedFrom), nameof(NavigatedFrom))]
    public partial class AddIncomeViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private int  budgetID;
        [ObservableProperty]
        private int  incomeID;
        [ObservableProperty]
        private IncomeEvents  income;
        [ObservableProperty]
        private bool  isPageValid;
        [ObservableProperty]
        private DateTime  minimumDate = DateTime.UtcNow.Date.AddDays(1);
        [ObservableProperty]
        private string  navigatedFrom;

        public string IncomeTypeText { get; set; } = "";
        public string IncomeActiveText { get; set; } = "";
        public string RecurringIncomeTypeText { get; set; } = "";
        public AddIncomeViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Add a New Outgoing";
            Income = new IncomeEvents();

            MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date.AddDays(1);
        }

        [RelayCommand]
        public async void ChangeIncomeName()
        {
            try
            {
                string Description = "Every income needs a name, we will refer to it by the name you give it and this will make it easier to identify!";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";
                var popup = new PopUpPageSingleInput("Income Name", Description, DescriptionSub, "Enter an Income name!", Income.IncomeName, new PopUpPageSingleInputViewModel());
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if (result != null || (string)result != "")
                {
                    Income.IncomeName = (string)result;
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "AddIncome", "ChangeIncomeName");
            }
        }

        [RelayCommand]
        public async void BackButton()
        {
            try
            {
                if (NavigatedFrom == "CreateNewBudget")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Extra Income");
                }
                else if (NavigatedFrom == "ViewIncomes")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"//{nameof(ViewIncomes)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "AddIncome", "BackButton");
            }
        }

        public async void AddIncome()
        {

            string SuccessCheck = _ds.SaveNewIncome(Income, BudgetID).Result;
            if (SuccessCheck == "OK")
            {
                if (NavigatedFrom == "CreateNewBudget")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Extra Income");
                }
                else if (NavigatedFrom == "ViewIncomes")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"//{nameof(ViewIncomes)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                }
            }

        }

        public async void UpdateIncome()
        {

            string SuccessCheck = _ds.UpdateIncome(Income).Result;
            if (SuccessCheck == "OK")
            {
                if (NavigatedFrom == "CreateNewBudget")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Extra Income");
                }
                else if (NavigatedFrom == "ViewIncomes")
                {
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.MainPage.ShowPopup(PopUp);
                    }

                    await Shell.Current.GoToAsync($"//{nameof(ViewIncomes)}");
                }
                else
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}");
                }
            }                
        }

    }
}
