using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Converters;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using System.Collections.ObjectModel;
using System.Globalization;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(FamilyAccountID), nameof(FamilyAccountID))]

    public partial class FamilyAccountsEditViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        public BorderlessPicker switchBudgetPicker;
        [ObservableProperty]
        public BorderlessPicker switchParentBudgetPicker;
        [ObservableProperty]
        public List<FamilyUserAccount> familyUserAccounts = new List<FamilyUserAccount>();
        [ObservableProperty]
        public FamilyUserAccount familyUserAccount;
        [ObservableProperty]
        public Budgets budget;
        [ObservableProperty]
        public int familyAccountID;

        [ObservableProperty]
        private ObservableCollection<Transactions> recentTransactions = new ObservableCollection<Transactions>();

        [ObservableProperty]
        private double borderWidth;
        [ObservableProperty]
        private double progressBarWidthRequest;
        [ObservableProperty]
        private double signOutButtonWidth;
        [ObservableProperty]
        private double screenWidth;

        [ObservableProperty]
        private bool validatorPayDay;
        [ObservableProperty]
        private bool validatorPayDayAmount;
        [ObservableProperty]
        private string payDayTypeText;
        [ObservableProperty]
        private bool validatorPayType;
        [ObservableProperty]
        private bool isMultipleAccounts;
        [ObservableProperty]
        private bool hasPayDayTypeTextChanged;
        [ObservableProperty]
        private bool hasPayDayOptionsChanged;
        [ObservableProperty]
        private string everyNthDuration;
        [ObservableProperty]
        private string everyNthValue;
        [ObservableProperty]
        private bool validatorEveryNthDuration;
        [ObservableProperty]
        private string workingDaysValue;
        [ObservableProperty]
        private bool validatorWorkingDayDuration;
        [ObservableProperty]
        private string everyMonthValue;
        [ObservableProperty]
        private bool validatorOfEveryMonthDuration;
        [ObservableProperty]
        private string lastOfTheMonthDuration;
        [ObservableProperty]
        private decimal payAmount;
        [ObservableProperty]
        private string payAmountString;
        [ObservableProperty]
        private int payAmountCursorPosition;
        [ObservableProperty]
        private bool hasPayAmountChanged;
        [ObservableProperty]
        private bool hasAssignedBudgetChanged;
        [ObservableProperty]
        private DateTime payDayDate;
        [ObservableProperty]
        private bool hasPayDayDateChanged;
        [ObservableProperty]
        private List<Budgets> userBudgets;

        public FamilyAccountsEditViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Allowance Details";
            borderWidth = (DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density) - 20;
            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            ProgressBarWidthRequest = ScreenWidth - 85;
            SignOutButtonWidth = ScreenWidth - 60;
        }

        public async Task OnLoad()
        {
            var Accounts = await _ds.GetUserFamilyAccounts(App.UserDetails.UserID);
            FamilyUserAccounts = Accounts.Where(a => a.IsActive).ToList();

            if (FamilyUserAccounts.Count == 0)
            {
                return;
            }

            if (FamilyAccountID == 0)
            {
                Budget = await _ds.GetBudgetDetailsAsync(FamilyUserAccounts[0].BudgetID, "Full");
                FamilyUserAccount = FamilyUserAccounts[0];
                FamilyAccountID = FamilyUserAccounts[0].UserID;
            }

            Application.Current.Resources.TryGetValue("White", out var White);
            Application.Current.Resources.TryGetValue("PrimaryDark", out var PrimaryDark);
            Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

            BorderlessPicker picker = new BorderlessPicker
            {
                Title = "Select a user",
                ItemsSource = FamilyUserAccounts,
                TitleColor = (Color)Gray900,
                BackgroundColor = (Color)White,
                TextColor = (Color)PrimaryDark,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 16
            };

            picker.ItemDisplayBinding = new Binding("NickName", BindingMode.Default);

            SwitchBudgetPicker = picker;

            UserBudgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "EditAccountSettings");
            UserBudgets = UserBudgets.Where(b => b.SharedUserID != App.UserDetails.UserID).ToList();

            Application.Current.Resources.TryGetValue("Info", out var Info);

            BorderlessPicker ParentPicker = new BorderlessPicker
            {
                Title = "Select a budget",
                ItemsSource = UserBudgets,
                TitleColor = (Color)Gray900,
                BackgroundColor = (Color)White,
                TextColor = (Color)Gray900,
                Margin = new Thickness(20, 0, 0, 0),
            };

            ParentPicker.ItemDisplayBinding = new Binding(".", BindingMode.Default, new ChangeBudgetStringConvertor());
            ParentPicker.SelectedIndexChanged += (s, e) =>
            {
                var picker = (Picker)s;
                var SelectedBudget = (Budgets)picker.SelectedItem;
                if(SelectedBudget.BudgetID != FamilyUserAccount.AssignedBudgetID)
                {
                    HasAssignedBudgetChanged = true;
                }
                FamilyUserAccount.AssignedBudgetID = SelectedBudget.BudgetID;
            };

            SwitchParentBudgetPicker = ParentPicker;

            await LoadBudgetDetails();
        }

        public async Task LoadBudgetDetails()
        {
            PayAmount = Budget.PaydayAmount ?? 0;
            PayAmountString = PayAmount.ToString("c", CultureInfo.CurrentCulture);

            PayDayDate = Budget.NextIncomePayday.GetValueOrDefault();

            if (Budget.PaydayType == "Everynth")
            {
                PayDayTypeText = "Everynth";
                EveryNthDuration = Budget.PaydayDuration;
                EveryNthValue = Budget.PaydayValue.ToString();

            }
            else if (Budget.PaydayType == "WorkingDays")
            {
                PayDayTypeText = "WorkingDays";
                WorkingDaysValue = Budget.PaydayValue.ToString();
            }
            else if (Budget.PaydayType == "OfEveryMonth")
            {
                PayDayTypeText = "OfEveryMonth";
                EveryMonthValue = Budget.PaydayValue.ToString();
            }
            else if (Budget.PaydayType == "LastOfTheMonth")
            {
                PayDayTypeText = "LastOfTheMonth";
                LastOfTheMonthDuration = Budget.PaydayDuration;
            }

            IsMultipleAccounts = Budget.IsMultipleAccounts;

            for (int i = 0; i < UserBudgets.Count; i++)
            {
                if (UserBudgets[i].BudgetID == FamilyUserAccount.AssignedBudgetID)
                {
                    SwitchParentBudgetPicker.SelectedItem = UserBudgets[i];
                }
            }

            for (int i = 0; i < FamilyUserAccounts.Count; i++)
            {
                if (FamilyUserAccounts[i].UserID == FamilyAccountID)
                {
                    SwitchBudgetPicker.SelectedItem = FamilyUserAccounts[i];
                    FamilyUserAccount = FamilyUserAccounts[i];
                }
            }
        }    

        partial void OnEveryNthDurationChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "FamilyAccountsEdit", "OnEveryNthDurationChanged");
            }
        }

        partial void OnEveryNthValueChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "FamilyAccountsEdit", "OnEveryNthValueChanged");
            }
        }

        partial void OnWorkingDaysValueChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "FamilyAccountsEdit", "OnWorkingDaysValueChanged");
            }
        }

        partial void OnEveryMonthValueChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "FamilyAccountsEdit", "OnEveryMonthValueChanged");
            }
        }
        partial void OnLastOfTheMonthDurationChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayOptionsChanged = true;
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "FamilyAccountsEdit", "OnLastOfTheMonthDurationChanged");
            }
        }

        partial void OnPayDayTypeTextChanged(string oldValue, string newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayTypeTextChanged = true;
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "FamilyAccountsEdit", "OnPayDayTypeTextChanged");
            }
        }

        partial void OnPayAmountStringChanged(string value)
        {
            try
            {
                decimal ValueNumber = (decimal)_pt.FormatCurrencyNumber(value);
                if (PayAmount != ValueNumber)
                {
                    PayAmount = ValueNumber;
                    PayAmountString = PayAmount.ToString("c", CultureInfo.CurrentCulture);
                    HasPayAmountChanged = true;
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "FamilyAccountsEdit", "OnPayAmountStringChanged");
            }
        }

        partial void OnPayDayDateChanged(DateTime oldValue, DateTime newValue)
        {
            try
            {
                if (oldValue != newValue)
                {
                    HasPayDayDateChanged = true;
                }
            }
            catch (Exception ex)
            {
                _pt.HandleException(ex, "FamilyAccountsEdit", "OnPayDayDateChanged");
            }
        }

        [RelayCommand]
        private async Task SaveBudgetDetails()
        {
            try
            {
                bool EditBudget = await Application.Current.Windows[0].Page.DisplayAlert($"Update allowance?", $"Are you sure you want to update the accounts allowance details?", "Yes", "No");
                if (EditBudget)
                {

                    List<PatchDoc> BudgetUpdate = new List<PatchDoc>();  
                    List<PatchDoc> AccountUpdate = new List<PatchDoc>(); 
                    
                    if(HasAssignedBudgetChanged)
                    {
                        PatchDoc AssignedBudget = new PatchDoc
                        {
                            op = "replace",
                            path = "/AssignedBudgetID",
                            value = FamilyUserAccount.AssignedBudgetID
                        };
                        AccountUpdate.Add(AssignedBudget);
                    }

                    if (HasPayAmountChanged)
                    {
                        PatchDoc PayDayAmount = new PatchDoc
                        {
                            op = "replace",
                            path = "/PayDayAmount",
                            value = PayAmount
                        };

                        BudgetUpdate.Add(PayDayAmount);
                    }

                    if (HasPayDayDateChanged)
                    {
                        PatchDoc NextIncomePayday = new PatchDoc
                        {
                            op = "replace",
                            path = "/NextIncomePayday",
                            value = PayDayDate
                        };

                        BudgetUpdate.Add(NextIncomePayday);
                    }


                    if (HasPayDayTypeTextChanged)
                    {
                        PatchDoc PayType = new PatchDoc
                        {
                            op = "replace",
                            path = "/PaydayType",
                            value = PayDayTypeText
                        };

                        BudgetUpdate.Add(PayType);
                    }

                    if (HasPayDayOptionsChanged)
                    {
                        if (PayDayTypeText == "Everynth")
                        {
                            PatchDoc PaydayValue = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayValue",
                                value = EveryNthValue
                            };

                            BudgetUpdate.Add(PaydayValue);

                            PatchDoc PaydayDuration = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayDuration",
                                value = EveryNthDuration
                            };

                            BudgetUpdate.Add(PaydayDuration);

                        }
                        else if (PayDayTypeText == "WorkingDays")
                        {
                            PatchDoc PaydayValue = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayValue",
                                value = WorkingDaysValue
                            };

                            BudgetUpdate.Add(PaydayValue);

                            PatchDoc PaydayDuration = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayDuration",
                                value = ""
                            };

                            BudgetUpdate.Add(PaydayDuration);
                        }
                        else if (PayDayTypeText == "OfEveryMonth")
                        {
                            PatchDoc PaydayValue = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayValue",
                                value = EveryMonthValue
                            };

                            BudgetUpdate.Add(PaydayValue);

                            PatchDoc PaydayDuration = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayDuration",
                                value = ""
                            };

                            BudgetUpdate.Add(PaydayDuration);
                        }
                        else if (PayDayTypeText == "LastOfTheMonth")
                        {
                            PatchDoc PaydayValue = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayValue",
                                value = ""
                            };

                            BudgetUpdate.Add(PaydayValue);

                            PatchDoc PaydayDuration = new PatchDoc
                            {
                                op = "replace",
                                path = "/PaydayDuration",
                                value = LastOfTheMonthDuration
                            };

                            BudgetUpdate.Add(PaydayDuration);
                        }
                    }

                    if (BudgetUpdate.Count() > 0)
                    {
                        await _ds.PatchBudget(Budget.BudgetID, BudgetUpdate);
                    }

                    if (AccountUpdate.Count() > 0)
                    {
                        await _ds.PatchFamilyUserAccount(FamilyUserAccount.UserID, AccountUpdate);
                    }

                    await _pt.MakeSnackBar("Well done, Allowance Details Updated", null, null, new TimeSpan(0,0,10), "Success");


                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountsEdit", "SaveBudgetSettings");
            }
        }

        [RelayCommand]
        public async Task BackButton()
        {
            try
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }
                await Task.Delay(1);

                await Shell.Current.GoToAsync($"//{nameof(MainPage)}");

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "FamilyAccountsEdit", "BackButton");
            }
        }
    }
}
