using Android.Accounts;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using Microsoft.Maui.ApplicationModel;
using Syncfusion.Maui.Picker;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Android.App.DownloadManager;
using static AndroidX.ConstraintLayout.Core.Motion.Utils.HyperSpline;

namespace DailyBudgetMAUIApp.ViewModels
{
    [QueryProperty(nameof(AccountID), nameof(AccountID))]
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
        private string infoTitle;
        [ObservableProperty]
        private string infoText;
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
        private string isNamelValidText;


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
            if(AccountID == 0)
            {
                Budget = new Budgets();
                Budget.Stage = 0;
                budgetSettings = new BudgetSettings();
                StageID = 1;
                await LoadStage(StageID);
            }
            else
            {
                //FamilyAccount = await _ds.GetFamilyUserAccount(AccountID);

                Budget = await _ds.GetBudgetDetailsAsync(FamilyAccount.BudgetID, "Full");
                BudgetSettings = await _ds.GetBudgetSettings(FamilyAccount.BudgetID);
                await LoadStage(Budget.Stage);
            }

            CurrencyPlacements = _ds.GetCurrencyPlacements("").Result;
            DateFormats = _ds.GetDateFormatsByString("").Result;
            NumberFormats = _ds.GetNumberFormats().Result;
            TimeZones = _ds.GetBudgetTimeZones("").Result;

            if (SelectedCurrencySymbol == null)
            {
                SelectedCurrencySymbol = _ds.GetCurrencySymbols(BudgetSettings.CurrencySymbol.ToString()).Result[0];
                SelectedCurrencyPlacement = _ds.GetCurrencyPlacements(BudgetSettings.CurrencyPattern.ToString()).Result[0];
                SelectedDateFormats = _ds.GetDateFormatsById(BudgetSettings.ShortDatePattern ?? 1, BudgetSettings.DateSeperator ?? 1).Result;
                SelectedNumberFormats = _ds.GetNumberFormatsById(BudgetSettings.CurrencyDecimalDigits ?? 2, BudgetSettings.CurrencyDecimalSeparator ?? 2, BudgetSettings.CurrencyGroupSeparator ?? 1).Result;
                SelectedTimeZone = _ds.GetTimeZoneById(BudgetSettings.TimeZone.GetValueOrDefault()).Result;
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
                        IsNamelValidText = "Please enter a name for the account";
                    }

                    break;
                case 2:

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
                        familyUserAccount.ProfilePicture = "Avatar1";
                        familyUserAccount.Password = "";

                        Random rnd = new();
                        int number = rnd.Next(2000);
                        familyUserAccount.Salt = _pt.GenerateSalt(number);

                        familyUserAccount = await _ds.SetUpNewFamilyAccount(familyUserAccount);
                        Budget = await _ds.GetBudgetDetailsAsync(familyUserAccount.BudgetID, "Limited");
                    }
                    else
                    {
                        //TODO: JUST PATCH NAME
                    }

                    break;
                case 2:

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

            Budget.Stage = stage + 1 > Budget.Stage ? stage + 1 : Budget.Stage;

            //TODO: UPDATE THE BUDGET STAGE
        }

        private async Task LoadStage(int stage)
        {
            StageID = stage;

            if (Budget.Stage <= stage)
            {
                IsShowInfo = true;
                IsContinueEnabled = false;
                IsBackEnabled = false;
            }

            switch (stage)
            {
                case 1:
                    Stage = "Set up account";
                    InfoText = "Please provide details for the family account you'd like to set up. We'll need a valid email address (this is required for logging in) and the name you'd like the account to be identified by. When you have finalised setting up the family account an email with a one-time passcode (OTP) will be sent. You'll then need to go to the Family Account login screen to complete the setup and start budgeting.";
                    InfoTitle = "Set up account";

                    IsBackButtonVisible = false;                    

                    if(Budget.Stage > 1)
                    {
                        IsEmailEnabled = false;
                    }

                    break;
                case 2:
                    Stage = "Allocate Anchor Budget";
                    InfoText = "You'll need to designate one of your budgets as the Anchor Budget for your family account. This budget will serve as the anchor budget for the new family account budget. The Allowance, any chore rewards, and any additional money requests will come to and be taken from this budget. A family account is linked to one of your Budgets directly not your account as a whole, but don't worry—you can update your Anchor Budget at any time if your financial setup changes.";
                    InfoTitle = "Allocate Anchor Budget";

                    IsBackButtonVisible = true;
                    break;
                case 3:
                    Stage = "";
                    InfoText = "";
                    InfoTitle = "";

                    IsBackButtonVisible = true;
                    break;
                case 4:
                    Stage = "";
                    InfoText = "";
                    InfoTitle = "";

                    IsBackButtonVisible = true;
                    break;
                case 5:
                    Stage = "";
                    InfoText = "";
                    InfoTitle = "";

                    IsBackButtonVisible = true;
                    break;
                case 6:
                    Stage = "";
                    InfoText = "";
                    InfoTitle = "";

                    IsBackButtonVisible = true;
                    break;
                case 7:
                    Stage = "";
                    InfoText = "";
                    InfoTitle = "";

                    IsBackButtonVisible = true;
                    break;
                case 8:
                    Stage = "";
                    InfoText = "";
                    InfoTitle = "";

                    IsBackButtonVisible = true;
                    break;
                case 9:
                    Stage = "";
                    InfoText = "";
                    InfoTitle = "";

                    IsBackButtonVisible = true;
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

                await Shell.Current.GoToAsync($"//{nameof(ManageFamilyAccounts)}");
                
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
                CurrencySearchResults = _ds.GetCurrencySymbols(value).Result;
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

                    await LoadStage(StageID);
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

                await LoadStage(StageID);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "CreateNewFamilyAccounts", "GoBackStage");
            }
        }
    }
}
