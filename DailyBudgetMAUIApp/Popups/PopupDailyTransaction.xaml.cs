using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupDailyTransaction : Popup
{
    private readonly PopupDailyTransactionViewModel _vm;
    private readonly IProductTools _pt;

    public PopupDailyTransaction(Transactions Transaction, PopupDailyTransactionViewModel viewModel, IProductTools pt)
	{
        InitializeComponent();

        viewModel.Transaction = Transaction;

        BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;

        _vm.OriginalDate = _vm.Transaction.TransactionDate.GetValueOrDefault();
        _vm.OriginalAmount = _vm.Transaction.TransactionAmount.GetValueOrDefault();

        hslTransactionAmount.IsVisible = true;
        hslTargetDate.IsVisible = true;

        double TransactionAmount = (double?)_vm.Transaction.TransactionAmount ?? 0;
        lblTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);

        string GoalDate = _vm.Transaction.TransactionDate.GetValueOrDefault().ToShortDateString();
        lblTargetDate.Text = GoalDate;
   
    }

    void TransactionAmount_Changed(object sender, TextChangedEventArgs e)
    {
        decimal TransactionAmount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);
        //entTransactionAmount.CursorPosition = _pt.FindCurrencyCursorPosition(entTransactionAmount.Text);

        _vm.Transaction.TransactionAmount = TransactionAmount;
    }

    private void Close_Saving(object sender, EventArgs e)
    {
        this.Close("OK");
    }

    private void DeleteYes_Saving(object sender, EventArgs e)
    {
        this.Close("Delete");
    }

    private void DeleteNo_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = true;
        grdDeleteBtns.IsVisible = false;

        vslDeleteTransaction.IsVisible = false;
        vslTransactionComplete.IsVisible = true;
 
        lblTransactionAmount.IsVisible = false;
        lblTargetDate.IsVisible = false;

        entTransactionAmount.IsVisible = true;
        entTargetDate.IsVisible = true;

        double TransactionAmount = (double?)_vm.Transaction.TransactionAmount ?? 0;
        entTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);

        
    }

    private void Delete_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = false;
        grdDeleteBtns.IsVisible = true;

        vslDeleteTransaction.IsVisible = true;
        vslTransactionComplete.IsVisible = false;
    }

    private void Update_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = false;
        grdUpdateBtns.IsVisible = true;
        grdDeleteBtns.IsVisible = false;


        lblTransactionAmount.IsVisible = false;
        lblTargetDate.IsVisible = false;

        entTransactionAmount.IsVisible = true;
        entTargetDate.IsVisible = true;

        double TransactionAmount = (double?)_vm.Transaction.TransactionAmount ?? 0;
        entTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);

        
    }

    private bool ValidatePage()
    {
        bool IsValid = true;

        if(_vm.Transaction.TransactionAmount == 0)
        {
            IsValid = false;
            validatorTransactionAmount.IsVisible = true;
        }
        else
        {
            validatorTransactionAmount.IsVisible = false;
        }

        return IsValid;
    }

    private void AcceptUpdate_Saving(object sender, EventArgs e)
    {
        if(ValidatePage())
        {           
            this.Close(_vm.Transaction);
        }
    }

    private void Reset_Saving(object sender, EventArgs e)
    {
        grdFirstBtns.IsVisible = true;
        grdUpdateBtns.IsVisible = false;
        grdDeleteBtns.IsVisible = false;

        _vm.Transaction.TransactionAmount = _vm.OriginalAmount;
        _vm.Transaction.TransactionDate = _vm.OriginalDate;

        lblTransactionAmount.IsVisible = true;
        lblTargetDate.IsVisible = true;

        entTransactionAmount.IsVisible = false;
        entTargetDate.IsVisible = false;

        double TransactionAmount = (double?)_vm.Transaction.TransactionAmount ?? 0;
        lblTransactionAmount.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);

        string GoalDate = _vm.Transaction.TransactionDate.GetValueOrDefault().ToShortDateString();
        lblTargetDate.Text = GoalDate;
        

    }
}