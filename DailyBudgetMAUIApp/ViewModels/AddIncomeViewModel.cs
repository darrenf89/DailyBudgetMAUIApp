using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(BudgetID), nameof(BudgetID))]
    [QueryProperty(nameof(IncomeID), nameof(IncomeID))]
    [QueryProperty(nameof(NavigatedFrom), nameof(NavigatedFrom))]
    [QueryProperty(nameof(FamilyAccountID), nameof(FamilyAccountID))]
    public partial class AddIncomeViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;
        private readonly IPopupService _ps;

        [ObservableProperty]
        public partial int BudgetID { get; set; }

        [ObservableProperty]
        public partial int IncomeID { get; set; }

        [ObservableProperty]
        public partial int FamilyAccountID { get; set; }

        [ObservableProperty]
        public partial IncomeEvents Income { get; set; }

        [ObservableProperty]
        public partial bool IsPageValid { get; set; }

        [ObservableProperty]
        public partial DateTime MinimumDate { get; set; } = DateTime.UtcNow.Date.AddDays(1);

        [ObservableProperty]
        public partial string NavigatedFrom { get; set; }

        [ObservableProperty]
        public partial bool IsMultipleAccounts { get; set; }

        [ObservableProperty]
        public partial List<BankAccounts> BankAccounts { get; set; }

        [ObservableProperty]
        public partial BankAccounts? SelectedBankAccount { get; set; }


        public string IncomeTypeText { get; set; } = "";
        public string IncomeActiveText { get; set; } = "";
        public string RecurringIncomeTypeText { get; set; } = "";
        public AddIncomeViewModel(IProductTools pt, IRestDataService ds, IPopupService ps)
        {
            _pt = pt;
            _ds = ds;
            _ps = ps;

            Title = "Add a New Outgoing";
            Income = new IncomeEvents();

            MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date.AddDays(1);
        }

        [RelayCommand]
        public async Task ChangeIncomeName()
        {
            try
            {
                string Description = "Every income needs a name, we will refer to it by the name you give it and this will make it easier to identify!";
                string DescriptionSub = "Call it something useful or call it something silly up to you really!";

                var queryAttributes = new Dictionary<string, object>
                {
                    [nameof(PopUpPageSingleInputViewModel.Description)] = Description,
                    [nameof(PopUpPageSingleInputViewModel.DescriptionSub)] = DescriptionSub,
                    [nameof(PopUpPageSingleInputViewModel.InputTitle)] = "Income Name",
                    [nameof(PopUpPageSingleInputViewModel.Placeholder)] = "Enter an Income name!",
                    [nameof(PopUpPageSingleInputViewModel.Input)] = Income.IncomeName
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
                    Income.IncomeName = (string)popupResult.Result;
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "AddIncome", "ChangeIncomeName");
            }
        }

        [RelayCommand]
        public async Task BackButton()
        {
            try
            {
                if (NavigatedFrom == "CreateNewBudget")
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Extra Income");
                }
                else if (NavigatedFrom == "CreateNewFamilyAccount")
                {
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Incomes", false);
                }
                else if (NavigatedFrom == "ViewIncomes")
                {
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

            string SuccessCheck = await _ds.SaveNewIncome(Income, BudgetID);
            if (SuccessCheck == "OK")
            {
                if (NavigatedFrom == "CreateNewBudget")
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Extra Income");
                }
                else if (NavigatedFrom == "CreateNewFamilyAccount")
                {
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Incomes", false);
                }
                else if (NavigatedFrom == "ViewIncomes")
                {
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

            string SuccessCheck = await _ds.UpdateIncome(Income);
            if (SuccessCheck == "OK")
            {
                if (NavigatedFrom == "CreateNewBudget")
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Budget Extra Income");
                }
                else if (NavigatedFrom == "CreateNewFamilyAccount")
                {
                    await Shell.Current.GoToAsync($"../../{nameof(CreateNewFamilyAccounts)}?AccountID={FamilyAccountID}&NavigatedFrom=Budget Incomes", false);
                }
                else if (NavigatedFrom == "ViewIncomes")
                {
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
