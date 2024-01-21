using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Microsoft.Maui.Handlers;
using System.Globalization;
using IeuanWalker.Maui.Switch;
using IeuanWalker.Maui.Switch.Events;
using IeuanWalker.Maui.Switch.Helpers;
using System.Windows.Input;
using CommunityToolkit.Maui.ApplicationModel;

namespace DailyBudgetMAUIApp.Pages;

public partial class AddTransaction : ContentPage
{
    private readonly AddTransactionViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    public AddTransaction(AddTransactionViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
		InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;
    }

    private async void Content_Loaded(object sender, EventArgs e)
    {
        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    async protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (_vm.BudgetID == 0)
        {
            _vm.BudgetID = App.DefaultBudgetID;
        }

        if (_vm.TransactionID == 0)
        {
            if(_vm.Transaction == null)
            {
                _vm.Transaction = new Transactions();
                _vm.Transaction.EventType = "Transaction";
                _vm.Title = "Add a New Transaction";
                btnAddTransaction.IsVisible = true;
                _vm.Transaction.TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date;
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
            }

        }
        else
        {
            if(_vm.Transaction == null)
            {
                _vm.Transaction = _ds.GetTransactionFromID(_vm.TransactionID).Result;
            }            
            _vm.Title = $"Update your transaction!";

            btnUpdateTransaction.IsVisible = true;

            _vm.IsFutureDatedTransaction = !(_vm.Transaction.TransactionDate.GetValueOrDefault().Date <= _pt.GetBudgetLocalTime(DateTime.UtcNow).Date);
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

        UpdateExpenseIncomeSelected();
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
        _vm.Transaction.IsIncome = false;
        UpdateExpenseIncomeSelected();
    }

    private void btnIncomeClicked_Clicked(object sender, EventArgs e)
    {
        _vm.Transaction.IsIncome = true;
        UpdateExpenseIncomeSelected();
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
        if(ValidateBillDetails())
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
                _vm.Transaction = _ds.SaveNewTransaction(_vm.Transaction, _vm.BudgetID).Result;
                if (_vm.Transaction.TransactionID != 0)
                {
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}?SnackBar=Transaction Added&SnackID={_vm.Transaction.TransactionID}");
                }
            }
        }

    }

    private async void btnUpdateTransaction_Clicked(object sender, EventArgs e)
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
                string status = _ds.UpdateTransaction(_vm.Transaction).Result;
                if (status == "OK")
                {
                    await Shell.Current.GoToAsync($"//{nameof(MainPage)}?SnackBar=Transaction Updated&SnackID={_vm.TransactionID}");

                }
            }
        }
    }

    private void ClearAllValidators()
    {
        validatorTransactionAmount.IsVisible = false;
    }

    void TransactionAmount_Changed(object sender, TextChangedEventArgs e)
    {
        ClearAllValidators();

        decimal TransactionAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);
        entTransactionAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entTransactionAmount.Text);
        _vm.Transaction.TransactionAmount = TransactionAmount;
    }

    private void swhTransactionDate_Toggled(object sender, ToggledEventArgs e)
    {
        if(!_vm.IsFutureDatedTransaction)
        {
            entTransactionDate.MinimumDate = new DateTime();
            _vm.Transaction.TransactionDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date;
        }
        else
        {
            entTransactionDate.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow).Date.AddDays(1);
#if ANDROID
            var handler = entTransactionDate.Handler as IDatePickerHandler;
            handler.PlatformView.PerformClick();
#endif
        }
    }

    private async void swhPayee_Toggled(object sender, ToggledEventArgs e)
    {
        entTransactionAmount.IsEnabled = false;
        entTransactionAmount.IsEnabled = true;
        edtNotes.IsEnabled = false;
        edtNotes.IsEnabled = true;

        if (!_vm.IsPayee)
        {            
            _vm.Transaction.Payee = "";
        }
        else
        {
            var page = new SelectPayeePage(_vm.BudgetID, _vm.Transaction, new RestDataService(), new ProductTools(new RestDataService()), new SelectPayeePageViewModel(new ProductTools(new RestDataService()), new RestDataService()));
            await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
        }
    }

    private async void swhSpendCategory_Toggled(object sender, ToggledEventArgs e)
    {


        entTransactionAmount.IsEnabled = false;
        entTransactionAmount.IsEnabled = true;
        edtNotes.IsEnabled = false;
        edtNotes.IsEnabled = true;

        if (!_vm.IsSpendCategory)
        {
            _vm.Transaction.Category = "";
            _vm.Transaction.CategoryID = 0;
        }
        else
        {
            var page = new SelectCategoryPage(_vm.BudgetID, _vm.Transaction, new RestDataService(), new ProductTools(new RestDataService()), new SelectCategoryPageViewModel(new ProductTools(new RestDataService()), new RestDataService()));
            await Navigation.PushModalAsync(page, true);
        }
    }

    async void SavingSpend_Toggled(object sender, ToggledEventArgs e)
    {
        entTransactionAmount.IsEnabled = false;
        entTransactionAmount.IsEnabled = true;
        edtNotes.IsEnabled = false;
        edtNotes.IsEnabled = true;

        if (!_vm.Transaction.IsSpendFromSavings)
        {
            _vm.Transaction.SavingName = "";
            _vm.Transaction.SavingID = 0;
            _vm.Transaction.SavingsSpendType = "";
            _vm.Transaction.EventType = "Transaction";
        }
        else
        {
            var page = new SelectSavingCategoryPage(_vm.BudgetID, _vm.Transaction, new RestDataService(), new ProductTools(new RestDataService()), new SelectSavingCategoryPageViewModel(new ProductTools(new RestDataService()), new RestDataService()));
            await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
        }
    }


    private async void swhNotes_Toggled(object sender, ToggledEventArgs e)
    {
        entTransactionAmount.IsEnabled = false;
        entTransactionAmount.IsEnabled = true;
        edtNotes.IsEnabled = false;
        edtNotes.IsEnabled = true;

        if (!_vm.IsNote)
        {
            _vm.Transaction.Notes = "";
        }
    }

    private void edtNotes_TextChanged(object sender, TextChangedEventArgs e)
    {
        int StringLength = edtNotes.Text.Length;
        lblNoteCharacterLeft.Text = $"{250 - StringLength} characters remaining";
    }

    static void CustomSwitch_SwitchPanUpdate(CustomSwitch customSwitch, SwitchPanUpdatedEventArgs e)
    {
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);

        //Switch Color Animation
        Color fromSwitchColor = e.IsToggled ? (Color)Primary : (Color)Gray400;
        Color toSwitchColor = e.IsToggled ? (Color)Gray400 : (Color)Primary;

        //BackGroundColor Animation
        Color fromColor = e.IsToggled ? (Color)Tertiary : (Color)PrimaryLight;
        Color toColor = e.IsToggled ? (Color)PrimaryLight : (Color)Tertiary;

        double t = e.Percentage * 0.01;

        customSwitch.KnobBackgroundColor = ColorAnimationUtil.ColorAnimation(fromSwitchColor, toSwitchColor, t);
        customSwitch.BackgroundColor = ColorAnimationUtil.ColorAnimation(fromColor, toColor, t);
    }

    private async void SaveTransaction_Clicked(object sender, EventArgs e)
    {
        if(btnAddTransaction.IsVisible)
        {
            btnAddTransaction_Clicked(sender, e);
        } 
        else if(btnUpdateTransaction.IsVisible)
        {
            btnUpdateTransaction_Clicked(sender, e);
        }
            
    }

    private async void ResetTransaction_Clicked(object sender, EventArgs e)
    {
        entTransactionAmount.IsEnabled = false;
        entTransactionAmount.IsEnabled = true;
        edtNotes.IsEnabled = false;
        edtNotes.IsEnabled = true;

        bool result = await DisplayAlert("Transaction Reset", "Are you sure you want to Reset the details of this transaction", "Yes, continue", "Cancel");
        if (result)
        {
            _vm.IsFutureDatedTransaction = false;
            _vm.IsPayee = false;
            _vm.IsSpendCategory = false;
            _vm.IsNote = false;
            _vm.Transaction.IsSpendFromSavings = false;

            double TransactionAmount = (double)0;
            entTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);

            _vm.Transaction.IsIncome = false;
            UpdateExpenseIncomeSelected();
        }
    }

    private async void ChangeSelectedPayee_Tapped(object sender, TappedEventArgs e)
    {
        entTransactionAmount.IsEnabled = false;
        entTransactionAmount.IsEnabled = true;
        edtNotes.IsEnabled = false;
        edtNotes.IsEnabled = true;

        var page = new SelectPayeePage(_vm.BudgetID, _vm.Transaction, new RestDataService(), new ProductTools(new RestDataService()), new SelectPayeePageViewModel(new ProductTools(new RestDataService()), new RestDataService()));
        await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
    }

    private async void ChangeSelectedCategory_Tapped(object sender, TappedEventArgs e)
    {
        entTransactionAmount.IsEnabled = false;
        entTransactionAmount.IsEnabled = true;
        edtNotes.IsEnabled = false;
        edtNotes.IsEnabled = true;

        var page = new SelectCategoryPage(_vm.BudgetID, _vm.Transaction, new RestDataService(), new ProductTools(new RestDataService()), new SelectCategoryPageViewModel(new ProductTools(new RestDataService()), new RestDataService()));
        await Navigation.PushModalAsync(page, true);
    }

    private async void ChangeSelectedSaving_Tapped(object sender, TappedEventArgs e)
    {
        entTransactionAmount.IsEnabled = false;
        entTransactionAmount.IsEnabled = true;
        edtNotes.IsEnabled = false;
        edtNotes.IsEnabled = true;

        var page = new SelectSavingCategoryPage(_vm.BudgetID, _vm.Transaction, new RestDataService(), new ProductTools(new RestDataService()), new SelectSavingCategoryPageViewModel(new ProductTools(new RestDataService()), new RestDataService()));
        await Application.Current.MainPage.Navigation.PushModalAsync(page, true);
    }
}