using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.Converters;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using Syncfusion.Maui.Core;
using System.Text.RegularExpressions;
using The49.Maui.BottomSheet;


namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(AccountID), nameof(AccountID))]
    [QueryProperty(nameof(NavigatedFrom), nameof(NavigatedFrom))]
    public partial class CreateNewFamilyAccountsViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        public double ScreenWidth { get; }
        public double PopupWidth { get; }
        public double StageWidth { get; }


        [ObservableProperty]
        private FamilyUserAccount familyAccount;
        [ObservableProperty]
        private Budgets budget;
        [ObservableProperty]
        private int accountID;
        [ObservableProperty]
        private string? navigatedFrom;
        [ObservableProperty]
        private string infoTitle;
        [ObservableProperty]
        private string stage;
        [ObservableProperty]
        private string accountEmail;
        [ObservableProperty]
        private string accountName;
        [ObservableProperty]
        private string currencySearchText;
        [ObservableProperty]
        private int stageID;
        [ObservableProperty]
        private bool isShowInfo;
        [ObservableProperty]
        private bool isContinueEnabled;
        [ObservableProperty]
        private bool isBackEnabled;
        [ObservableProperty]
        private bool isEmailEnabled = true;
        [ObservableProperty]
        private bool isBackButtonVisible;

        [ObservableProperty]
        private Color bvStage1;
        [ObservableProperty]
        private Color bvStage2;
        [ObservableProperty]
        private Color bvStage3;
        [ObservableProperty]
        private Color bvStage4;
        [ObservableProperty]
        private Color bvStage5;
        [ObservableProperty]
        private Color bvStage6;
        [ObservableProperty]
        private Color bvStage7;
        [ObservableProperty]
        private Color bvStage8;
        [ObservableProperty]
        private Color bvStage9;

        [ObservableProperty]
        private bool isStageVisible1;
        [ObservableProperty]
        private bool isStageVisible2;
        [ObservableProperty]
        private bool isStageVisible3;
        [ObservableProperty]
        private bool isStageVisible4;
        [ObservableProperty]
        private bool isStageVisible5;
        [ObservableProperty]
        private bool isStageVisible6;
        [ObservableProperty]
        private bool isStageVisible7;
        [ObservableProperty]
        private bool isStageVisible8;
        [ObservableProperty]
        private bool isStageVisible9;

        [ObservableProperty]
        private List<lut_CurrencySymbol> currencySearchResults;
        [ObservableProperty]
        private lut_CurrencySymbol selectedCurrencySymbol;
        [ObservableProperty]
        private bool searchVisible = false;
        [ObservableProperty]
        private List<lut_CurrencyPlacement> currencyPlacements;
        [ObservableProperty]
        private lut_CurrencyPlacement selectedCurrencyPlacement;
        [ObservableProperty]
        private List<lut_DateFormat> dateFormats;
        [ObservableProperty]
        private List<lut_BudgetTimeZone> timeZones;
        [ObservableProperty]
        private lut_DateFormat selectedDateFormats;
        [ObservableProperty]
        private List<lut_NumberFormat> numberFormats;
        [ObservableProperty]
        private lut_NumberFormat selectedNumberFormats;
        [ObservableProperty]
        private lut_BudgetTimeZone selectedTimeZone;
        [ObservableProperty]
        private bool isBorrowPay;
        [ObservableProperty]
        private BudgetSettings budgetSettings;

        [ObservableProperty]
        private bool isEmailExistsVisible = true;
        [ObservableProperty]
        private string isEmailExistsText;
        [ObservableProperty]
        private bool isEmailValid = true;
        [ObservableProperty]
        private string isEmailValidText;
        [ObservableProperty]
        private bool isNameValid = true;
        [ObservableProperty]
        private string isNameValidText;
        [ObservableProperty]
        private bool isBudgetNameValid = true;
        [ObservableProperty]
        private string isBudgetNameValidText;

        [ObservableProperty]
        private SfAvatarView profilePicture = new SfAvatarView();
        [ObservableProperty]
        public BorderlessPicker switchBudgetPicker;
        [ObservableProperty]
        private List<Budgets> userBudgets;

        [ObservableProperty]
        private string profilePictureString;
        [ObservableProperty]
        private string everynthDuration;
        [ObservableProperty]
        private string everynthValue;
        [ObservableProperty]
        private string payDayTypeText;
        [ObservableProperty]
        private string workingDaysValue;
        [ObservableProperty]
        private string ofEveryMonthValue;
        [ObservableProperty]
        private string lastOfTheMonthDuration;

        [ObservableProperty]
        private List<Bills> bills;
        [ObservableProperty]
        private List<Savings> savings;
        [ObservableProperty]
        private List<Savings> envelopes;
        [ObservableProperty]
        private List<IncomeEvents> incomes;



        public CreateNewFamilyAccountsViewModel(IProductTools pt, IRestDataService ds)
        {
            _pt = pt;
            _ds = ds;

            Title = "Create Family Account";

            ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
            PopupWidth = ScreenWidth - 40;
            StageWidth = ((ScreenWidth - 44) / 9);
        }

        public async Task LoadFamilyAccount()
        {
            UserBudgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "EditAccountSettings");
            UserBudgets = UserBudgets.Where(b => b.SharedUserID != App.UserDetails.UserID).ToList();

            Application.Current.Resources.TryGetValue("White", out var White);
            Application.Current.Resources.TryGetValue("Info", out var Info);
            Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

            BorderlessPicker picker = new BorderlessPicker
            {
                Title = "Select a budget",
                ItemsSource = UserBudgets,
                TitleColor = (Color)Gray900,
                BackgroundColor = (Color)White,
                TextColor = (Color)Info,
                Margin = new Thickness(20, 0, 0, 0),
            };

            picker.ItemDisplayBinding = new Binding(".", BindingMode.Default, new ChangeBudgetStringConvertor());
            picker.SelectedIndexChanged += (s, e) =>
            {
                var picker = (Picker)s;
                var SelectedBudget = (Budgets)picker.SelectedItem;
                FamilyAccount.AssignedBudgetID = SelectedBudget.BudgetID;
            };

            SwitchBudgetPicker = picker;

            if (AccountID == 0)
            {
                Budget = new Budgets();
                Budget.BudgetName = "";
                Budget.Stage = 0;
                BudgetSettings = new BudgetSettings();
                StageID = 1;

                ProfilePicture = new SfAvatarView
                {
                    HeightRequest = 75,
                    WidthRequest = 75,
                    CornerRadius = 5,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    StrokeThickness = 0,
                };

                Random random = new Random();
                int randomNumber = random.Next(1, 21);

                ProfilePictureString = $"Avatar{randomNumber}";
                bool Success = Enum.TryParse(ProfilePictureString, out AvatarCharacter Avatar);
                if (Success)
                {
                    ProfilePicture.AvatarCharacter = Avatar;
                    int Number = Convert.ToInt32(ProfilePictureString[ProfilePictureString.Length - 1]);
                    Math.DivRem(Number, 8, out int index);
                    ProfilePicture.Background = App.ChartColor[index];
                }
                else
                {
                    ProfilePicture.AvatarCharacter = AvatarCharacter.Avatar1;
                    ProfilePicture.Background = App.ChartColor[1];
                }

                await LoadStage(StageID, true);
            }
            else
            {
                FamilyAccount = await _ds.GetFamilyUserAccount(AccountID);

                Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");
                BudgetSettings = await _ds.GetBudgetSettings(FamilyAccount.BudgetID);

                IsBorrowPay = Budget.IsBorrowPay;

                if (Budget.Bills != null)
                {
                    Bills = Budget.Bills;
                }    

                if (Budget.Savings != null)
                {
                    Savings = Budget.Savings.Where(s => s.IsRegularSaving).ToList();
                }    

                if (Budget.Savings != null)
                {
                    Envelopes = Budget.Savings.Where(s => !s.IsRegularSaving).ToList();
                }      

                if (Budget.IncomeEvents != null)
                {
                    Incomes = Budget.IncomeEvents;
                }    

                AccountEmail = FamilyAccount.Email;
                AccountName = FamilyAccount.NickName;

                ProfilePicture = new SfAvatarView
                {
                    HeightRequest = 75,
                    WidthRequest = 75,
                    CornerRadius = 5,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    StrokeThickness = 0
                };

                ProfilePictureString = FamilyAccount.ProfilePicture;
                if (FamilyAccount.ProfilePicture.Contains("Avatar"))
                {
                    ProfilePicture.ContentType = ContentType.AvatarCharacter;
                    bool Success = Enum.TryParse(FamilyAccount.ProfilePicture, out AvatarCharacter Avatar);
                    if (Success)
                    {
                        ProfilePicture.AvatarCharacter = Avatar;
                        int Number = Convert.ToInt32(FamilyAccount.ProfilePicture[FamilyAccount.ProfilePicture.Length - 1]);
                        Math.DivRem(Number, 8, out int index);
                        ProfilePicture.Background = App.ChartColor[index];
                        
                    }
                    else
                    {
                        ProfilePicture.AvatarCharacter = AvatarCharacter.Avatar1;
                        ProfilePicture.Background = App.ChartColor[1];
                    }
                }
                else
                {
                    ProfilePicture.ContentType = ContentType.Custom;
                    Stream profilePictureStream = await GetUserProfilePictureStream(FamilyAccount.UniqueUserID);
                    ProfilePicture.ImageSource = ImageSource.FromStream(() => profilePictureStream);
                }

                for (int i = 0; i < UserBudgets.Count; i++)
                {
                    if (UserBudgets[i].BudgetID == FamilyAccount.AssignedBudgetID)
                    {
                        picker.SelectedItem = UserBudgets[i];
                    }
                }

                if(string.IsNullOrWhiteSpace(NavigatedFrom))
                {
                    await LoadStage(Budget.Stage, true);
                }
                else
                {
                    switch(NavigatedFrom)
                    {
                        case "Budget Outgoings":
                            await LoadStage(5, false);
                            break;
                        case "Budget Bills":
                            await LoadStage(6, false);
                            break;
                        case "Budget Envelopes":
                            await LoadStage(7, false);
                            break;
                        case "Budget Incomes":
                            await LoadStage(8, false);
                            break;
                        default:
                            break;
                    }
                }
            }

            CurrencyPlacements = await _ds.GetCurrencyPlacements("");
            DateFormats = await _ds.GetDateFormatsByString("");
            NumberFormats = await _ds.GetNumberFormats();
            TimeZones = await _ds.GetBudgetTimeZones("");

            if (SelectedCurrencySymbol == null)
            {
                SelectedCurrencySymbol = (await _ds.GetCurrencySymbols(BudgetSettings.CurrencySymbol.ToString()))[0];
                SelectedCurrencyPlacement = (await _ds.GetCurrencyPlacements(BudgetSettings.CurrencyPattern.ToString()))[0];
                SelectedDateFormats = await _ds.GetDateFormatsById(BudgetSettings.ShortDatePattern ?? 1, BudgetSettings.DateSeperator ?? 1);
                SelectedNumberFormats = await _ds.GetNumberFormatsById(
                    BudgetSettings.CurrencyDecimalDigits ?? 2,
                    BudgetSettings.CurrencyDecimalSeparator ?? 2,
                    BudgetSettings.CurrencyGroupSeparator ?? 1
                );
                SelectedTimeZone = await _ds.GetTimeZoneById(BudgetSettings.TimeZone.GetValueOrDefault());
            }
        }

        public async Task ChangeCurrency()
        {
            SearchVisible = true;
            CurrencySearchResults = await _ds.GetCurrencySymbols("");
        }

        private void ClearValidation()
        {
            IsEmailExistsVisible = true;
            IsEmailValid = true;
            IsNameValid = true;
            IsBudgetNameValid = true;
        }

        private async Task<bool> ValidateStage(int stage)
        {
            bool IsValid = true;

            switch (stage)
            {
                case 1:

                    if (Budget.Stage < 1 && !string.IsNullOrEmpty(AccountEmail))
                    {
                        string FamilyAccountEmail = await _ds.FamilyAccountEmailValid(AccountEmail, App.UserDetails.UserID);
                        if (!string.IsNullOrEmpty(FamilyAccountEmail))
                        {
                            IsValid = false;
                            IsEmailExistsVisible = false;
                            IsEmailExistsText = FamilyAccountEmail;
                        }
                    }

                    if (string.IsNullOrEmpty(AccountEmail))
                    {
                        IsValid = false;
                        IsEmailValid = false;
                        IsEmailValidText = "Please enter a email address";
                    }

                    Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");

                    if (!regex.IsMatch(AccountEmail))
                    {
                        IsValid = false;
                        IsEmailValid = false;
                        IsEmailValidText = "Please enter a valid email address";
                    }

                    if (string.IsNullOrEmpty(AccountEmail))
                    {
                        IsValid = false;
                        IsNameValid = false;
                        IsNameValidText = "Please enter a name for the account";
                    }

                    break;
                case 2:

                    if (string.IsNullOrEmpty(Budget.BudgetName))
                    {
                        IsValid = false;
                        IsBudgetNameValid = false;
                        IsBudgetNameValidText = "Please enter a name for the budget";
                    }

                    break;
                case 3:

                    break;
                case 4:

                    break;
                case 5:

                    break;
                case 6:

                    break;
                case 7:

                    break;
                case 8:

                    break;
                case 9:

                    break;

            }

            return IsValid;
        }
        private async Task SaveStage(int stage)
        {

            List<PatchDoc> BudgetUpdate = new List<PatchDoc>();
            List<PatchDoc> UpdateUserDetails = new List<PatchDoc>();
            bool IsNavigateManage = false;
            switch (stage)
            {
                case 1:

                    if(Budget.Stage < 1)
                    {
                        //TODO: SET UP USER ACCOUNT FOR THE FIRST TIME
                        FamilyUserAccount familyUserAccount = new FamilyUserAccount();
                        familyUserAccount.ParentUserID = App.UserDetails.UserID;
                        familyUserAccount.Email = AccountEmail;
                        familyUserAccount.NickName = AccountName;

                        familyUserAccount.ProfilePicture = ProfilePictureString;
                        familyUserAccount.Password = "";

                        Random rnd = new();
                        int number = rnd.Next(2000);
                        familyUserAccount.Salt = _pt.GenerateSalt(number);

                        FamilyAccount = await _ds.SetUpNewFamilyAccount(familyUserAccount);
                        AccountID = FamilyAccount.UserID;
                        Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Limited");
                    }
                    else
                    {
                        PatchDoc NickName = new PatchDoc
                        {
                            op = "replace",
                            path = "/NickName",
                            value = AccountName
                        };

                        UpdateUserDetails.Add(NickName);

                        PatchDoc ProfilePick = new PatchDoc
                        {
                            op = "replace",
                            path = "/ProfilePicture",
                            value = ProfilePictureString
                        };

                        UpdateUserDetails.Add(ProfilePick);

                        await _ds.PatchFamilyUserAccount(FamilyAccount.UserID, UpdateUserDetails);
                    }

                    break;
                case 2:

                    PatchDoc AssignedBudgetID = new PatchDoc
                    {
                        op = "replace",
                        path = "/AssignedBudgetID",
                        value = FamilyAccount.AssignedBudgetID
                    };

                    UpdateUserDetails.Add(AssignedBudgetID);

                    PatchDoc IsBudgetHidden = new PatchDoc
                    {
                        op = "replace",
                        path = "/IsBudgetHidden",
                        value = FamilyAccount.IsBudgetHidden
                    };

                    UpdateUserDetails.Add(IsBudgetHidden);

                    PatchDoc BudgetName = new PatchDoc
                    {
                        op = "replace",
                        path = "/BudgetName",
                        value = Budget.BudgetName
                    };

                    BudgetUpdate.Add(BudgetName);

                    await _ds.PatchFamilyUserAccount(FamilyAccount.UserID, UpdateUserDetails);

                    break;
                case 3:
                        
                    PatchDoc BankBalancePatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/BankBalance",
                        value = Budget.BankBalance
                    };
                    BudgetUpdate.Add(BankBalancePatch);

                    PatchDoc NextIncomePaydayPatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/NextIncomePayday",
                        value = Budget.NextIncomePayday
                    };
                    BudgetUpdate.Add(NextIncomePaydayPatch);

                    PatchDoc NextIncomePaydayCalculatedPatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/NextIncomePaydayCalculated",
                        value = Budget.NextIncomePayday
                    };
                    BudgetUpdate.Add(NextIncomePaydayCalculatedPatch);

                    Budget.PaydayType = PayDayTypeText;
                    PatchDoc PayDayTypePatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/PayDayType",
                        value = Budget.PaydayType
                    };
                    BudgetUpdate.Add(PayDayTypePatch);

                    string PayDayDuration = "";
                    int PayDayValue = 0;
                    int AproxDaysBetweenPay = 0;

                    if (PayDayTypeText == "Everynth")
                    {
                        PayDayValue = Convert.ToInt32(EverynthValue ?? "1");
                        PayDayDuration = EverynthDuration ?? "days";

                        int Duration = new int();
                        if (PayDayDuration == "days")
                        {
                            Duration = 1;
                        }
                        else if (PayDayDuration == "weeks")
                        {
                            Duration = 7;
                        }
                        else if (PayDayDuration == "years")
                        {
                            Duration = 365;
                        }
                    }
                    else if (PayDayTypeText == "WorkingDays")
                    {
                        PayDayValue = Convert.ToInt32(WorkingDaysValue ?? "1");
                        PayDayDuration = "";
                    }
                    else if (PayDayTypeText == "OfEveryMonth")
                    {
                        PayDayValue = Convert.ToInt32(OfEveryMonthValue ?? "1");
                        PayDayDuration = "";
                    }
                    else if (PayDayTypeText == "LastOfTheMonth")
                    {
                        PayDayValue = 0;
                        PayDayDuration = LastOfTheMonthDuration ?? "Monday";
                    }

                    Budget.PaydayDuration = PayDayDuration;
                    PatchDoc PayDayDurationPatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/PayDayDuration",
                        value = Budget.PaydayDuration
                    };
                    BudgetUpdate.Add(PayDayDurationPatch);

                    Budget.PaydayValue = PayDayValue;
                    PatchDoc PayDayValuePatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/PayDayValue",
                        value = Budget.PaydayValue
                    };
                    BudgetUpdate.Add(PayDayValuePatch);

                    Budget.AproxDaysBetweenPay = _pt.CalculateBudgetDaysBetweenPay(Budget);

                    PatchDoc AproxDaysBetweenPayPatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/AproxDaysBetweenPay",
                        value = Budget.AproxDaysBetweenPay
                    };
                    BudgetUpdate.Add(AproxDaysBetweenPayPatch);

                    PatchDoc PaydayAmountPatch = new PatchDoc
                    {
                        op = "replace",
                        path = "/PaydayAmount",
                        value = Budget.PaydayAmount
                    };
                    BudgetUpdate.Add(PaydayAmountPatch);                    

                    break;
                case 4:
                    BudgetSettings = await _ds.GetBudgetSettings(FamilyAccount.BudgetID);
                    BudgetSettings.CurrencyPattern = SelectedCurrencyPlacement.Id;
                    BudgetSettings.CurrencySymbol = SelectedCurrencySymbol.Id;
                    BudgetSettings.CurrencyDecimalDigits = SelectedNumberFormats.CurrencyDecimalDigitsID;
                    BudgetSettings.CurrencyDecimalSeparator = SelectedNumberFormats.CurrencyDecimalSeparatorID;
                    BudgetSettings.CurrencyGroupSeparator = SelectedNumberFormats.CurrencyGroupSeparatorID;
                    BudgetSettings.DateSeperator = SelectedDateFormats.DateSeperatorID;
                    BudgetSettings.ShortDatePattern = SelectedDateFormats.ShortDatePatternID;
                    BudgetSettings.TimeZone = SelectedTimeZone.TimeZoneID;

                    await _ds.UpdateBudgetSettings(FamilyAccount.BudgetID, BudgetSettings);

                    break;
                case 5:
                    Budget.IsBorrowPay = IsBorrowPay;
                    PatchDoc IsBorrow = new PatchDoc
                    {
                        op = "replace",
                        path = "/IsBorrowPay",
                        value = Budget.IsBorrowPay
                    };
                    BudgetUpdate.Add(IsBorrow);

                    break;
                case 6:

                    break;
                case 7:

                    break;
                case 8:

                    break;
                case 9:

                    bool result = await Shell.Current.DisplayAlert("Finalise set up?", "Are you sure you want to finalise the set up? This will active the new family account and send the new member an email so they can complete their account. It will also start taking the allowance from your budget when it is due.", "Yes", "No");
                    if(result)
                    {
                        IsNavigateManage = true;

                        PatchDoc IsCreated = new PatchDoc
                        {
                            op = "replace",
                            path = "/IsCreated",
                            value = true
                        };

                        BudgetUpdate.Add(IsCreated);

                        PatchDoc BudgetLastUpdated = new PatchDoc
                        {
                            op = "replace",
                            path = "/BudgetValuesLastUpdated",
                            value = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date
                        };

                        BudgetUpdate.Add(BudgetLastUpdated);

                        FamilyAccount = await _ds.FinaliseFamilyAccount(FamilyAccount);
                    }


                    break;

            }            

            Budget.Stage = stage + 1 > Budget.Stage ? stage + 1 : Budget.Stage;
            Budget.Stage = Budget.Stage > 9 ? 9 : Budget.Stage;
            Budget.Stage = Budget.Stage < 1 ? 1 : Budget.Stage;

            PatchDoc Stage = new PatchDoc
            {
                op = "replace",
                path = "/Stage",
                value = Budget.Stage
            };

            BudgetUpdate.Add(Stage);

            await _ds.PatchBudget(FamilyAccount.BudgetID, BudgetUpdate);

            if(IsNavigateManage)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }
                await Task.Delay(1);
                await Shell.Current.GoToAsync($"//{nameof(FamilyAccountsManage)}");
            }
        }

        private async Task LoadStage(int stage, bool ShowPopup)
        {
            StageID = stage;

            if (Budget.Stage <= stage && ShowPopup)
            {
                IsShowInfo = true;
                IsContinueEnabled = false;
                IsBackEnabled = false;
            }

            switch (stage)
            {
                case 1:
                    Stage = "Set up account";
                    InfoTitle = "Set up account";

                    IsBackButtonVisible = false;                    

                    if(Budget.Stage > 1)
                    {
                        IsEmailEnabled = false;
                    }

                    break;
                case 2:
                    FamilyAccount = await _ds.GetFamilyUserAccount(AccountID);
                    Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");
                    

                    Stage = "Allocate Anchor Budget";
                    InfoTitle = "Allocate Anchor Budget";

                    IsBackButtonVisible = true;
                    break;
                case 3:
                    FamilyAccount = await _ds.GetFamilyUserAccount(AccountID);
                    Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");

                    Stage = "Budget Details";
                    InfoTitle = "Enter Allowance details";

                    IsBackButtonVisible = true;
                    break;
                case 4:
                    Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");

                    Stage = "Budget Settings";
                    InfoTitle = "Update budget settings";

                    IsBackButtonVisible = true;
                    break;
                case 5:
                    BudgetSettings = await _ds.GetBudgetSettings(FamilyAccount.BudgetID);

                    Stage = "Budget Outgoings";
                    InfoTitle = "Add bills to the Budget";

                    IsBackButtonVisible = true;
                    break;
                case 6:
                    Stage = "Budget Bills";
                    InfoTitle = "Add savings to the Budget";

                    IsBackButtonVisible = true;
                    break;
                case 7:
                    Stage = "Budget Envelopes";
                    InfoTitle = "Add envelopes to the Budget";

                    IsBackButtonVisible = true;
                    break;
                case 8:
                    Stage = "Budget Extra Income";
                    InfoTitle = "Add income to the Budget";

                    IsBackButtonVisible = true;
                    break;
                case >= 9:
                    Stage = "Finalise Budget";
                    InfoTitle = "Finalise Set up";

                    IsBackButtonVisible = true;

                    await _ds.UpdateBudgetValues(FamilyAccount.BudgetID);

                    break;

            }

            UpdateStageDisplay(stage);
        }

        private void UpdateStageDisplay(int stage)
        {
            Application.Current.Resources.TryGetValue("Success", out var Success);
            Application.Current.Resources.TryGetValue("Gray300", out var Gray300);

            BvStage1 = stage > 0 ? (Color)Success : (Color)Gray300;
            BvStage2 = stage > 1 ? (Color)Success : (Color)Gray300;
            BvStage3 = stage > 2 ? (Color)Success : (Color)Gray300;
            BvStage4 = stage > 3 ? (Color)Success : (Color)Gray300;
            BvStage5 = stage > 4 ? (Color)Success : (Color)Gray300;
            BvStage6 = stage > 5 ? (Color)Success : (Color)Gray300;
            BvStage7 = stage > 6 ? (Color)Success : (Color)Gray300;
            BvStage8 = stage > 7 ? (Color)Success : (Color)Gray300;
            BvStage9 = stage > 8 ? (Color)Success : (Color)Gray300;

            IsStageVisible1 = stage == 1;
            IsStageVisible2 = stage == 2;
            IsStageVisible3 = stage == 3;
            IsStageVisible4 = stage == 4;
            IsStageVisible5 = stage == 5;
            IsStageVisible6 = stage == 6;
            IsStageVisible7 = stage == 7;
            IsStageVisible8 = stage == 8;
            IsStageVisible9 = stage == 9;
        }


        [RelayCommand]
        public async Task CloseInfoBox()
        {            
            try
            {
                IsShowInfo = false;
                IsContinueEnabled = true;
                IsBackEnabled = true;
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "CloseInfoBox");
            }
        }


        [RelayCommand]
        public async Task ShowInfoBox()
        {            
            try
            {
                IsShowInfo = true;
                IsContinueEnabled = false;
                IsBackEnabled = false;
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "ShowInfoBox");
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

                await Shell.Current.GoToAsync($"//{nameof(FamilyAccountsManage)}");
                
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "BackButton");
            }
        }

        async partial void OnSelectedCurrencySymbolChanged(lut_CurrencySymbol oldValue, lut_CurrencySymbol newValue)
        {
            try
            {
                SearchVisible = false;
                CurrencySearchResults = null;
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "CurrencySymbolSelected");
            }
        }

        async partial void OnCurrencySearchTextChanged(string value)
        {
            try
            {
                CurrencySearchResults = await _ds.GetCurrencySymbols(value);
            }
            catch (Exception ex)
            {
                if (ex.Message == "One or more errors occurred. (No currencies found)")
                {
                    lut_CurrencySymbol cs = new lut_CurrencySymbol();
                    cs.Code = "No results please, try again!";
                    CurrencySearchResults.Clear();
                    CurrencySearchResults.Add(cs);
                }
                else
                {
                    await _pt.HandleException(ex, "CreateNewFamilyAccounts", "CurrencySearch");
                }
            }
        }

        [RelayCommand]
        public async Task SaveStage()
        {
            try
            {
                ClearValidation();

                if (await ValidateStage(StageID))
                {                
                    await SaveStage(StageID);

                    StageID = StageID >= 8 ? 9 : StageID += 1;

                    await LoadStage(StageID, true);
                }

            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "SaveStage");
            }
        }

        [RelayCommand]
        public async Task GoBackStage()
        {
            try
            {
                ClearValidation();

                StageID = StageID <= 1 ? 1 : StageID -= 1;

                await LoadStage(StageID, true);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "GoBackStage");
            }
        }

        [RelayCommand]
        public async Task AddNewOutgoing()
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
                await Shell.Current.GoToAsync($"/{nameof(AddBill)}?FamilyAccountID={AccountID}&BudgetID={FamilyAccount.BudgetID}&BillID={0}&NavigatedFrom=CreateNewFamilyAccount");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "AddNewOutgoing");
            }
        }

        [RelayCommand]
        public async Task DeleteBudgetOutgoings(Bills bill)
        {
            try
            {
                bool result = await Shell.Current.DisplayAlert("Bill", "Are you sure you want to delete your Outgoing " + bill.BillName.ToString(), "Yes, continue", "Cancel");
                if (result)
                {
                    string Result = await _ds.DeleteBill(bill.BillID);
                    if (Result == "OK")
                    {
                        Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");
                        if (Budget.Bills != null)
                        {
                            Bills = Budget.Bills;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "DeleteBudgetOutgoings");
            }
        }

        [RelayCommand]
        public async Task EditBudgetOutgoings(Bills bill)
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
                await Shell.Current.GoToAsync($"/{nameof(AddBill)}?FamilyAccountID={AccountID}&BudgetID={FamilyAccount.BudgetID}&BillID={bill.BillID}&NavigatedFrom=CreateNewFamilyAccount");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "EditBudgetOutgoings");
            }
        }

        [RelayCommand]
        public async Task AddNewSaving()
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
                await Shell.Current.GoToAsync($"/{nameof(AddSaving)}?FamilyAccountID={AccountID}&BudgetID={FamilyAccount.BudgetID}&SavingID={0}&NavigatedFrom=CreateNewFamilyAccountSaving");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "AddNewSaving");
            }
        }

        [RelayCommand]
        public async Task DeleteBudgetSavings(Savings saving)
        {
            try
            {
                bool result = await Shell.Current.DisplayAlert("Delete Saving", "Are you sure you want to delete your Saving " + saving.SavingsName.ToString(), "Yes, continue", "Cancel");
                if (result)
                {
                    string Result = await _ds.DeleteSaving(saving.SavingID);
                    if (Result == "OK")
                    {
                        Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");
                        if (Budget.Savings != null)
                        {
                            Savings = Budget.Savings.Where(s => s.IsRegularSaving).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "DeleteBudgetSavings");
            }
        }

        [RelayCommand]
        public async Task EditBudgetSavings(Savings saving)
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
                await Shell.Current.GoToAsync($"/{nameof(AddSaving)}?FamilyAccountID={AccountID}&BudgetID={FamilyAccount.BudgetID}&SavingID={saving.SavingID}&NavigatedFrom=CreateNewFamilyAccountSaving");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "EditBudgetSavings");
            }
        }

        [RelayCommand]
        public async Task AddNewIncome()
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
                await Shell.Current.GoToAsync($"/{nameof(AddIncome)}?FamilyAccountID={AccountID}&BudgetID={FamilyAccount.BudgetID}&IncomeID={0}&NavigatedFrom=CreateNewFamilyAccount");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "AddNewIncome");
            }
        }

        [RelayCommand]
        public async Task DeleteBudgetIncomes(IncomeEvents income)
        {
            try
            {
                bool result = await Shell.Current.DisplayAlert("Delete Income", "Are you sure you want to delete your Income " + income.IncomeName.ToString(), "Yes, continue", "Cancel");
                if (result)
                {
                    string Result = await _ds.DeleteIncome(income.IncomeEventID);
                    if (Result == "OK")
                    {
                        Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");
                        if (Budget.IncomeEvents != null)
                        {
                            Incomes = Budget.IncomeEvents;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "DeleteBudgetIncomes");
            }
        }

        [RelayCommand]
        public async Task EditBudgetIncomes(IncomeEvents income)
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
                await Shell.Current.GoToAsync($"/{nameof(AddIncome)}?FamilyAccountID={AccountID}&BudgetID={FamilyAccount.BudgetID}&IncomeID={income.IncomeEventID}&NavigatedFrom=CreateNewFamilyAccount");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "EditBudgetIncomes");
            }
        }

        [RelayCommand]
        public async Task AddNewEnvelope()
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
                await Shell.Current.GoToAsync($"/{nameof(AddSaving)}?FamilyAccountID={AccountID}&BudgetID={FamilyAccount.BudgetID}&SavingID={0}&NavigatedFrom=CreateNewFamilyAccountEnvelope");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "AddNewEnvelope");
            }
        }

        [RelayCommand]
        public async Task DeleteBudgetEnvelopes(Savings saving)
        {
            try
            {
                bool result = await Shell.Current.DisplayAlert("Delete Envelope", "Are you sure you want to delete your Envelope " + saving.SavingsName.ToString(), "Yes, continue", "Cancel");
                if (result)
                {
                    string Result = await _ds.DeleteSaving(saving.SavingID);
                    if (Result == "OK")
                    {
                        Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");
                        if (Budget.Savings != null)
                        {
                            Envelopes = Budget.Savings.Where(s => !s.IsRegularSaving).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "DeleteBudgetEnvelopes");
            }
        }

        [RelayCommand]
        public async Task EditBudgetEnvelopes(Savings saving)
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
                await Shell.Current.GoToAsync($"/{nameof(AddSaving)}?FamilyAccountID={AccountID}&BudgetID={FamilyAccount.BudgetID}&SavingID={saving.SavingID}&NavigatedFrom=CreateNewFamilyAccountEnvelope");
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "EditBudgetEnvelopes");
            }
        }

        [RelayCommand]
        public async Task UpdateAvatar()
        {
            try
            {
                EditProfilePictureBottomSheet page = new EditProfilePictureBottomSheet(IPlatformApplication.Current.Services.GetService<IProductTools>(), IPlatformApplication.Current.Services.GetService<IRestDataService>());

                page.Detents = new DetentsCollection()
                {
                    new FixedContentDetent
                    {
                        IsDefault = true
                    },
                    new MediumDetent(),
                    new FullscreenDetent()
                };

                page.HasBackdrop = true;
                page.CornerRadius = 0;

                App.CurrentBottomSheet = page;

                await page.ShowAsync();
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "UpdateAvatar");
            }
        }

        public async Task<Stream> GetUserProfilePictureStream(int UserID)
        {
            return await _ds.DownloadUserProfilePicture(UserID);
        }

        partial void OnIsBorrowPayChanged(bool oldValue, bool newValue)
        {
            if (oldValue)
            {
                CheckIsBorrowPay();
            }
        }

        private async Task CheckIsBorrowPay()
        {
            bool result = await Shell.Current.DisplayAlert("Start paying outgoings each day?", "\nAre you sure you want to change the setting and start \"paying\" your outgoings each day?\n \nCareful! If you don't have the money put aside you might end up with no money left to spend.", "Yes", "Cancel");
            if (!result)
            {
                IsBorrowPay = true;
            }
        }

        [RelayCommand]
        private async Task GoToBorrowPayVideos()
        {
            try
            {
                List<string> SubTitle = new List<string>{
                    "",
                    "",
                    "",
                    "",
                };

                List<string> Info = new List<string>{
                    "Effectively managing your bill payments is essential for maintaining financial stability. dBudget offers two distinct methods for handling bill payments, each designed to accommodate different budgeting preferences and financial situations.",
                    "\"Cover Bills When Paid\" method aligns with the common practice of paying bills as income is received. When you get paid, dBudget allocates funds to cover bills up to that payday and any bills due within the current budget cycle. Bills due outside the budget cycle accrue their balance daily from your bank balance, but the funds don't transfer until the next payday. This approach mirrors real-life bill payment habits and helps prevent your daily spending allowance from appearing negative. However, careful if the next pay day doesn't come on time you might not have the money in your account to cover all your bills.",
                    "\"Cover Bills From Balance Every Day\" method involves allocating funds to fully cover each bill for the upcoming period upon receiving your paycheck. Daily, the allocated amounts are deducted from your available balance and transferred into the bill balance THEN AND THERE! This strategy ensures that bills are entirely covered before their due dates, providing clarity on financial obligations. However, it necessitates having sufficient funds available upfront, which may not be feasible for everyone. Additionally, this approach can make your daily spending allowance appear lower than it actually is, potentially affecting budgeting flexibility.",
                    "If accumulating the necessary funds to cover bills upfront is challenging, it's advisable to use the \"Cover Bills When Paid\" option. This method aligns with typical financial practices and provides a more accurate reflection of your daily spending capacity. As you work towards building savings, you might consider transitioning to the \"Cover Bills From Balance Every Day\" method for enhanced financial predictability.",
                };

                var popup = new PopupInfo("Bill accrual", SubTitle, Info);
                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "GoToBorrowPayVideos");
            }
        }
    }
}
