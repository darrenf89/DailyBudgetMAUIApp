using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using System.Globalization;
using DailyBudgetMAUIApp.Handlers;
using Plugin.MauiMTAdmob;
using CommunityToolkit.Maui;


#if ANDROID
using Microsoft.Maui.Handlers;
#endif

namespace DailyBudgetMAUIApp.Pages;

public partial class AddTransaction : BasePage
{
    private readonly AddTransactionViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    private readonly IModalPopupService _ps;
    public AddTransaction(AddTransactionViewModel viewModel, IProductTools pt, IRestDataService ds, IModalPopupService ps)
	{
		InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;
        _ps = ps;

    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        _vm.IsPageBusy = false;
        base.OnNavigatingFrom(args);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (Navigation.NavigationStack.Count > 1)
        {
            Shell.SetTabBarIsVisible(this, false);
        }
    }

    async protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _vm.NavigatedFrom = "";
    }

    async protected override void OnAppearing()
    {
        try
        {
            if (_ps.CurrentPopup is not null)
                return;

            await _ps.ShowAsync<PopUpPage>(() => new PopUpPage());

            base.OnAppearing();
            _vm.IsAppearing = true;
            CrossMauiMTAdmob.Current.OnInterstitialClosed += async (sender, args) => {
                UserAddDetails User = await _ds.GetUserAddDetails(App.UserDetails.UserID);
                User.LastViewed = DateTime.Now;
                User.NumberOfViews++;
                await _ds.PostUserAddDetails(User);
            };

            CrossMauiMTAdmob.Current.LoadInterstitial("ca-app-pub-3940256099942544/1033173712");

            if (string.IsNullOrEmpty(_vm.NavigatedFrom))
            {
                _vm.Transaction = null;
                _vm.TransactionID = 0;
                _vm.BudgetID = App.DefaultBudgetID;
                _vm.IsFutureDatedTransaction = false;
                _vm.IsPayee = false;
                _vm.IsSpendCategory = false;
                _vm.IsNote = false;
                _vm.RedirectTo = "";
            }
            else if(string.Equals(_vm.NavigatedFrom, "ViewMainPage", StringComparison.OrdinalIgnoreCase) ||string.Equals(_vm.NavigatedFrom, "ViewTransactions", StringComparison.OrdinalIgnoreCase) || string.Equals(_vm.NavigatedFrom, "ViewSavings", StringComparison.OrdinalIgnoreCase) || string.Equals(_vm.NavigatedFrom, "ViewEnvelopes", StringComparison.OrdinalIgnoreCase)|| string.Equals(_vm.NavigatedFrom, "ViewAccounts", StringComparison.OrdinalIgnoreCase))
            {
                _vm.RedirectTo = _vm.NavigatedFrom;
            }

            if (_vm.TransactionID == 0)
            {
                if(_vm.Transaction == null)
                {
                    _vm.Transaction = new Transactions();
                    _vm.Transaction.Payee = "";
                    _vm.Transaction.IsIncome = false;
                    _vm.Transaction.EventType = "Transaction";
                    _vm.Title = "Add a New Transaction";
                    btnAddTransaction.IsVisible = true;
                    _vm.Transaction.TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow);

                }
                else
                {
                    _vm.Title = "Add a New Transaction";
                    btnAddTransaction.IsVisible = true;

                    _vm.IsFutureDatedTransaction = !(_vm.Transaction.TransactionDate.GetValueOrDefault().Date <= _pt.GetBudgetLocalTime(DateTime.UtcNow).Date);
                    _vm.IsPayee = !string.IsNullOrEmpty(_vm.Transaction.Payee);
                    _vm.IsSpendCategory = !string.IsNullOrEmpty(_vm.Transaction.Category);
                    _vm.IsNote = !string.IsNullOrEmpty(_vm.Transaction.Notes);
                    _vm.Transaction.IsSpendFromSavings = !string.IsNullOrEmpty(_vm.Transaction.SavingName);

                    if(_vm.Transaction.SavingsSpendType == "BuildingSaving" || _vm.Transaction.SavingsSpendType == "MaintainValues" || _vm.Transaction.SavingsSpendType == "UpdateValues")
                    {
                        SavingSavingHeader.IsVisible = true;
                        EnvelopeSavingHeader.IsVisible = false;
                        swhSavingSpend.IsEnabled = false;
                        lblSavingsName.IsEnabled = false;
                    }
                }
            }
            else
            {
                if(_vm.Transaction == null)
                {
                    _vm.Transaction = await _ds.GetTransactionFromID(_vm.TransactionID);
                }            
                _vm.Title = $"Update your transaction!";

                btnUpdateTransaction.IsVisible = true;
                btnExpenseClicked.IsEnabled = false;
                btnIncomeClicked.IsEnabled = false;
                btnResetTransaction.IsVisible = false;

                if(_vm.Transaction.TransactionDate.GetValueOrDefault().Date == _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
                {
                    _vm.IsFutureDatedTransaction = false;
                }
                else
                {
                    _vm.IsFutureDatedTransaction = true;
                    swhTransactionDate.IsEnabled = false;
                    entTransactionDate.MinimumDate = default(DateTime);
                }

                _vm.IsPayee = !string.IsNullOrEmpty(_vm.Transaction.Payee);
                _vm.IsSpendCategory = !string.IsNullOrEmpty(_vm.Transaction.Category);
                _vm.IsNote = !string.IsNullOrEmpty(_vm.Transaction.Notes);
                _vm.Transaction.IsSpendFromSavings = !string.IsNullOrEmpty(_vm.Transaction.SavingName);
            }

            double TransactionAmount = (double?)_vm.Transaction.TransactionAmount ?? 0;
            entTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);

            if(_vm.IsNote)
            {
                int StringLength = edtNotes.Text.Length;
                lblNoteCharacterLeft.Text = $"{250 - StringLength} characters remaining";
            }

            if (_vm.Transaction.SavingsSpendType == "BuildingSaving" || _vm.Transaction.SavingsSpendType == "MaintainValues" || _vm.Transaction.SavingsSpendType == "UpdateValues")
            {
                SavingSavingHeader.IsVisible = true;
                EnvelopeSavingHeader.IsVisible = false;
                swhSavingSpend.IsEnabled = false;
                lblSavingsName.IsEnabled = false;
            }

            if(_vm.Transaction.EventType == "IncomeEvent")
            {
                swhSavingSpend.IsEnabled = false;
                SavingSwitch.IsVisible = false;
                SavingHeader.IsVisible = false;
            }

            _vm.IsMultipleAccounts = App.DefaultBudget.IsMultipleAccounts && _vm.IsPremiumAccount;
            if (_vm.IsMultipleAccounts) 
            {
                _vm.BankAccounts = await _ds.GetBankAccounts(_vm.BudgetID);
                if(_vm.Transaction.AccountID.GetValueOrDefault() != 0)
                {
                    _vm.SelectedBankAccount = _vm.BankAccounts.Where(b => b.ID == _vm.Transaction.AccountID.GetValueOrDefault()).FirstOrDefault();
                    if(_vm.SelectedBankAccount == null)
                    {
                        BankAccounts? B = _vm.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                        _vm.SelectedBankAccount = B;
                        _vm.DefaultAccountName = B.BankAccountName;
                        _vm.IsAccount = false;
                    }
                    else
                    {
                        _vm.IsAccount = true;
                    }
                }
                else
                {
                    BankAccounts? B = _vm.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                    _vm.SelectedBankAccount = B;
                    _vm.DefaultAccountName = B.BankAccountName;
                    _vm.IsAccount = false;
                }
            }

            UpdateExpenseIncomeSelected();
            _vm.IsAppearing = false;
            await _ps.CloseAsync<PopUpPage>();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "OnAppearing");
        }
    }

    private void UpdateExpenseIncomeSelected()
    {

        Application.Current.Resources.TryGetValue("Gray100", out var Gray100);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Danger", out var Danger);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);

        if (_vm.Transaction.IsIncome)
        {
            btnExpenseClicked.BackgroundColor = (Color)Gray100;
            btnExpenseClicked.TextColor = (Color)Gray400;
            btnIncomeClicked.BackgroundColor = (Color)Success;
            btnIncomeClicked.TextColor = (Color)White;

            entTransactionAmount.TextColor = (Color)Success;
            
        }
        else
        {
            btnExpenseClicked.BackgroundColor = (Color)Danger;
            btnExpenseClicked.TextColor = (Color)White;
            btnIncomeClicked.BackgroundColor = (Color)Gray100;
            btnIncomeClicked.TextColor = (Color)Gray400;

            entTransactionAmount.TextColor = (Color)Danger;
        }


    }
    private void btnExpenseClicked_Clicked(object sender, EventArgs e)
    {
        try
        {
            _vm.Transaction.IsIncome = false;
            UpdateExpenseIncomeSelected();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddTransaction", "btnExpenseClicked_Clicked");
        }
    }

    private void btnIncomeClicked_Clicked(object sender, EventArgs e)
    {
        try
        {
            _vm.Transaction.IsIncome = true;
            UpdateExpenseIncomeSelected();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddTransaction", "btnIncomeClicked_Clicked");
        }
    }

    private bool ValidateBillDetails()
    {
        bool IsValid = true;

        if (_vm.Transaction.TransactionAmount == 0)
        {
            IsValid = false;
            validatorTransactionAmount.IsVisible = true;
        }
        else
        {
            validatorTransactionAmount.IsVisible = false;
        }

        _vm.IsPageValid = IsValid;
        return IsValid;
    }

    private async void btnAddTransaction_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (ValidateBillDetails())
            {
                string TransactionAmount = _vm.Transaction.TransactionAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
                string TransactionType = "";
                if (_vm.Transaction.IsIncome)
                {
                    TransactionType = "an Income";
                }
                else
                {
                    TransactionType = "an Expenditure";
                }

                bool result = await DisplayAlert("Add New Transaction", $"You are adding {TransactionType} for {TransactionAmount}, are you sure you want to continue?", "Yes, continue", "Cancel");
                if (result)
                {

                    if (!_vm.IsMultipleAccounts)
                    {
                        _vm.Transaction.AccountID = null;
                    }

                    _vm.Transaction.IsQuickTransaction = false;
                    _vm.Transaction = await _ds.SaveNewTransaction(_vm.Transaction, _vm.BudgetID);
                    if (_vm.Transaction.TransactionID != 0)
                    {
                        if(!_vm.IsPremiumAccount)
                        {
                            if (CrossMauiMTAdmob.Current.IsInterstitialLoaded())
                            {
                                UserAddDetails User = await _ds.GetUserAddDetails(App.UserDetails.UserID);
                                if (User.NumberOfViews <= 3)
                                {
                                    CrossMauiMTAdmob.Current.ShowInterstitial();
                                }
                            }
                            else
                            {
                                CrossMauiMTAdmob.Current.LoadInterstitial("ca-app-pub-3940256099942544/1033173712");
                            }
                        }

                        if (_vm.RedirectTo == "ViewSavings")
                        {
                            await Shell.Current.GoToAsync($"///{nameof(ViewSavings)}");
                        }
                        else if (_vm.RedirectTo == "ViewEnvelopes")
                        {
                            await Shell.Current.GoToAsync($"///{nameof(ViewEnvelopes)}");
                        }
                        else
                        {
                            await Shell.Current.GoToAsync($"///{nameof(MainPage)}?SnackBar=Transaction Updated&SnackID={_vm.TransactionID}");
                        }
                    }
                }

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "btnAddTransaction_Clicked");
        }

    }

    private async void btnUpdateTransaction_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (ValidateBillDetails())
            {
                string TransactionAmount = _vm.Transaction.TransactionAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
                string TransactionType = "";
                if (_vm.Transaction.IsIncome)
                {
                    TransactionType = "an Income";
                }
                else
                {
                    TransactionType = "an Expenditure";
                }

                bool result = await DisplayAlert("Update A Transaction", $"You are updating {TransactionType} to {TransactionAmount}, are you sure you want to?", "Yes, continue", "Cancel");
                if (result)
                {
                    if (!_vm.IsMultipleAccounts)
                    {
                        _vm.Transaction.AccountID = null;
                    }

                    _vm.Transaction.IsQuickTransaction = false;
                    string status = await _ds.UpdateTransaction(_vm.Transaction);
                    if (status == "OK")
                    {
                        if (_vm.RedirectTo == "ViewTransactions")
                        {
                            await Shell.Current.GoToAsync($"///{nameof(ViewTransactions)}");
                        }
                        else if (_vm.RedirectTo == "ViewSavings")
                        {
                            await Shell.Current.GoToAsync($"///{nameof(ViewSavings)}");
                        }
                        else if (_vm.RedirectTo == "ViewEnvelopes")
                        {
                            await Shell.Current.GoToAsync($"///{nameof(ViewEnvelopes)}");
                        }
                        else
                        {
                            await Shell.Current.GoToAsync($"///{nameof(MainPage)}?ID=1&SnackBar=Transaction Updated&SnackID={_vm.TransactionID}");
                        }
                    }
                }            
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "btnUpdateTransaction_Clicked");
        }
    }

    private void ClearAllValidators()
    {
        validatorTransactionAmount.IsVisible = false;
    }

    void TransactionAmount_Changed(object sender, TextChangedEventArgs e)
    {
        try
        {
            ClearAllValidators();

            decimal TransactionAmount = (decimal)_pt.FormatBorderlessEntryNumber(sender, e, entTransactionAmount);

            _vm.Transaction.TransactionAmount = TransactionAmount;
            
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddTransaction", "TransactionAmount_Changed");
        }
    }


    private void swhTransactionDate_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            if (!_vm.IsFutureDatedTransaction)
            {
                entTransactionDate.MinimumDate = new DateTime();
                if(_vm.Transaction != null)
                {
                    _vm.Transaction.TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow);
                }
                
            }
            else
            {
                if(swhTransactionDate.IsEnabled)
                {
                    entTransactionDate.MinimumDate = default(DateTime);
                }
#if ANDROID
                if(!_vm.IsAppearing)
                {
                    var handler = entTransactionDate.Handler as IDatePickerHandler;
                    handler.PlatformView.PerformClick();
                }
#endif
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddTransaction", "swhTransactionDate_Toggled");
        }
    }

    private async void swhPayee_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            entTransactionAmount.IsEnabled = false;
            entTransactionAmount.IsEnabled = true;
            edtNotes.IsEnabled = false;
            edtNotes.IsEnabled = true;

            if (!_vm.IsPayee)
            {        
                if(_vm.Transaction is not null)
                {
                    _vm.Transaction.Payee = "";
                }            
            }
            else
            {
                if (_vm.Transaction != null && _vm.Transaction.Payee is null)
                {
                    _vm.Transaction.Payee = "";
                }

                if (!_vm.IsAppearing)
                {
                    await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.SelectPayeePage)}?BudgetID={_vm.BudgetID}&PageType=Transaction",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = _vm.Transaction
                    });
                }
            }

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "swhPayee_Toggled");
        }
    }

    private async void swhSpendCategory_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            entTransactionAmount.IsEnabled = false;
            entTransactionAmount.IsEnabled = true;
            edtNotes.IsEnabled = false;
            edtNotes.IsEnabled = true;

            if (!_vm.IsSpendCategory)
            {
                if (_vm.Transaction is not null)
                {
                    _vm.Transaction.Category = "";
                    _vm.Transaction.CategoryID = 0;
                }
            }
            else
            {
                if (_vm.Transaction != null && _vm.Transaction.Category == null)
                {
                    _vm.Transaction.Category = "";
                    _vm.Transaction.CategoryID = 0;
                }

                if(!_vm.IsAppearing)
                {
                    await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.SelectCategoryPage)}?BudgetID={_vm.BudgetID}&PageType=Transaction",
                        new Dictionary<string, object>
                        {
                            ["Transaction"] = _vm.Transaction
                        });
                }

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "swhSpendCategory_Toggled");
        }
    }

    async void SavingSpend_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {

            entTransactionAmount.IsEnabled = false;
            entTransactionAmount.IsEnabled = true;
            edtNotes.IsEnabled = false;
            edtNotes.IsEnabled = true;

            if (_vm.Transaction is null)
            {
                _vm.Transaction.SavingName = "";
                _vm.Transaction.SavingID = 0;
                _vm.Transaction.SavingsSpendType = "";
                _vm.Transaction.EventType = "Transaction";
            }
            else
            {
                if (!_vm.Transaction.IsSpendFromSavings)
                {
                    if (_vm.Transaction is not null)
                    {
                        _vm.Transaction.SavingName = "";
                        _vm.Transaction.SavingID = 0;
                        _vm.Transaction.SavingsSpendType = "";
                        _vm.Transaction.EventType = "Transaction";
                    }

                }
                else
                {
                    if (!_vm.IsAppearing)
                    {
                        await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.SelectSavingCategoryPage)}?BudgetID={_vm.BudgetID}",
                        new Dictionary<string, object>
                        {
                            ["Transaction"] = _vm.Transaction
                        });
                    }

                }
            }
        }
        catch (Exception ex)
        {

            await _pt.HandleException(ex, "AddTransaction", "SavingSpend_Toggled");
        }

    }


    private async void swhNotes_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            if (entTransactionAmount != null)
            {
                entTransactionAmount.IsEnabled = false;
                entTransactionAmount.IsEnabled = true;
            }    
        
            if(edtNotes != null)
            {
                edtNotes.IsEnabled = false;
                edtNotes.IsEnabled = true;
            }

            if (!_vm.IsNote)
            {
                if(_vm.Transaction!= null)
                {
                    _vm.Transaction.Notes = "";
                }            
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "swhNotes_Toggled");

        }
    }

    private void edtNotes_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            int StringLength;
            if (!string.IsNullOrEmpty(edtNotes.Text))
            {
                StringLength = edtNotes.Text.Length;
            }
            else
            {
                StringLength = 0;
            }
            
            lblNoteCharacterLeft.Text = $"{250 - StringLength} characters remaining";
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddTransaction", "edtNotes_TextChanged");
        }
    }

    private async void SaveTransaction_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (btnAddTransaction.IsVisible)
            {
                btnAddTransaction_Clicked(sender, e);
            } 
            else if(btnUpdateTransaction.IsVisible)
            {
                btnUpdateTransaction_Clicked(sender, e);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "SaveTransaction_Clicked");
        }

    }

    private async void ResetTransaction_Clicked(object sender, EventArgs e)
    {
        try
        {
            entTransactionAmount.IsEnabled = false;
            entTransactionAmount.IsEnabled = true;
            edtNotes.IsEnabled = false;
            edtNotes.IsEnabled = true;

            bool result = await DisplayAlert("Transaction Reset", "Are you sure you want to Reset the details of this transaction", "Yes, continue", "Cancel");
            if (result)
            {

                if (_vm.Transaction.SavingsSpendType == "BuildingSaving" || _vm.Transaction.SavingsSpendType == "MaintainValues" || _vm.Transaction.SavingsSpendType == "UpdateValues")
                {
                    swhSavingSpend.IsEnabled = false;
                    _vm.IsFutureDatedTransaction = false;
                    _vm.IsPayee = false;
                    _vm.IsSpendCategory = false;
                    _vm.IsNote = false;
                    _vm.IsAccount = false;
                }
                else
                {            
                    _vm.IsFutureDatedTransaction = false;
                    _vm.IsPayee = false;
                    _vm.IsSpendCategory = false;
                    _vm.IsNote = false;
                    _vm.Transaction.IsSpendFromSavings = false;
                    _vm.IsAccount = false;
                }

                double TransactionAmount = (double)0;
                entTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);

                _vm.Transaction.IsIncome = false;
                UpdateExpenseIncomeSelected();
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "ResetTransaction_Clicked");
        }
    }

    private async void ChangeSelectedPayee_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            entTransactionAmount.IsEnabled = false;
            entTransactionAmount.IsEnabled = true;
            edtNotes.IsEnabled = false;
            edtNotes.IsEnabled = true;

            await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.SelectPayeePage)}?BudgetID={_vm.BudgetID}&PageType=Transaction",
                new Dictionary<string, object>
                {
                    ["Transaction"] = _vm.Transaction
                });
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "ChangeSelectedPayee_Tapped");
        }
    }

    private async void ChangeSelectedCategory_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            entTransactionAmount.IsEnabled = false;
            entTransactionAmount.IsEnabled = true;
            edtNotes.IsEnabled = false;
            edtNotes.IsEnabled = true;

            await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.SelectCategoryPage)}?BudgetID={_vm.BudgetID}&PageType=Transaction",
                new Dictionary<string, object>
                {
                    ["Transaction"] = _vm.Transaction
                });
        }
        catch (Exception ex)
        {

            await _pt.HandleException(ex, "AddTransaction", "ChangeSelectedCategory_Tapped");
        }
    }

    private async void ChangeSelectedSaving_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            entTransactionAmount.IsEnabled = false;
            entTransactionAmount.IsEnabled = true;
            edtNotes.IsEnabled = false;
            edtNotes.IsEnabled = true;

            if (_vm.Transaction.SavingsSpendType == "BuildingSaving" || _vm.Transaction.SavingsSpendType == "MaintainValues" || _vm.Transaction.SavingsSpendType == "UpdateValues")
            {
                bool result = await Shell.Current.DisplayAlert("Sorry can't do that", "To change the savings that you want to spend you need to go back and start again!", "Ok", "Cancel");
                if (result)
                {
                    await Shell.Current.GoToAsync($"///{nameof(DailyBudgetMAUIApp.Pages.ViewSavings)}");
                }
            }
            else
            {
                await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.SelectSavingCategoryPage)}?BudgetID={_vm.BudgetID}",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = _vm.Transaction
                    });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "ChangeSelectedSaving_Tapped");
        }
    }

    private void swhIsAccount_Toggled(object sender, ToggledEventArgs e)
    {
        try
        {
            if(!_vm.IsAppearing)
            {
                if (_vm.SelectedBankAccount != null && _vm.BankAccounts != null && _vm.Transaction != null)
                {
                    if (!_vm.IsAccount)
                    {
                        BankAccounts? B = _vm.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                        _vm.SelectedBankAccount = B;
                        _vm.DefaultAccountName = B.BankAccountName;
                        _vm.Transaction.AccountID = B.ID;
                    }
                    else
                    {
                        entIsAccount.Focus();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddTransaction", "swhIsAccount_Toggled");
        }

    }

    private void entIsAccount_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            if (_vm.SelectedBankAccount != null && _vm.Transaction != null)
            {
                _vm.Transaction.AccountID = _vm.SelectedBankAccount.ID;
            }            
        }
        catch (Exception ex)
        {

            _pt.HandleException(ex, "AddTransaction", "entIsAccount_SelectedIndexChanged");
        }
    }
}