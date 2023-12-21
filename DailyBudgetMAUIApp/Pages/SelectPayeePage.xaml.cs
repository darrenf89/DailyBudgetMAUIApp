using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class SelectPayeePage : ContentPage
{
	private readonly IRestDataService _ds;
	private readonly IProductTools _pt;
	private readonly SelectPayeePageViewModel _vm;

	public SelectPayeePage(int BudgetID, Transactions Transaction, IRestDataService ds, IProductTools pt, SelectPayeePageViewModel viewModel)
	{
        if(Transaction.Payee == null)
        {
            Transaction.Payee = "";
        }

        InitializeComponent();

        _ds = ds;
		_pt = pt;

        this.BindingContext = viewModel;
        _vm = viewModel;

        _vm.Transaction = Transaction;
        _vm.BudgetID = BudgetID;

    }



    async protected override void OnAppearing()
    {
        base.OnAppearing();
                
        _vm.PayeeList = _ds.GetPayeeList(_vm.BudgetID).Result;

        LoadHeader();
        LoadPayeeList(_vm.Transaction.Payee);
    }


    private void LoadHeader()
    {
        if(string.IsNullOrEmpty(_vm.Transaction.Payee))
        {
            entPayee.WidthRequest = _vm.EntryWidth;
            bvHeader.WidthRequest = _vm.EntryWidth;
            btnClearPayee.IsVisible = false;
        }
        else
        {
            entPayee.WidthRequest = _vm.EntryButtonWidth;
            bvHeader.WidthRequest = _vm.EntryButtonWidth;
            btnClearPayee.IsVisible = true;
        }

        _vm.PayeeDoesntExists = !_vm.PayeeList.Contains(_vm.Transaction.Payee) && _vm.Transaction.Payee != "";

        if(_vm.PayeeDoesntExists)
        {
            hslAddNewPayee.IsVisible = true;
        }
        else
        {
            hslAddNewPayee.IsVisible = false;
        }

    }

    private void LoadPayeeList(string SearchQuery)
    {
       List<string> FilteredList = new List<string>();

        if (!(_vm.PayeeList.Count == 0 && _vm.PayeeList == null))
        {
            
            FilteredList = _vm.PayeeList.Where(x => x.Contains(SearchQuery)).ToList();
            string StartLetter = "*";

            foreach (string s in FilteredList)
            {
                if (!s.StartsWith(StartLetter))
                {
                    StartLetter = s[0].ToString();
                }
            }
        }

        if(FilteredList.Count == 0)
        {
            brdPayeeNotSetUp.IsVisible = true;
            _vm.FilteredListEmptyText = "No Payee's matches that name, to create one go ahead and hit the create button!";
        }
        else
        {
            brdPayeeNotSetUp.IsVisible = false;
        }
 
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        _vm.Transaction.Payee = "";

        await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}", 
            new Dictionary<string,object>
            {
                ["Transaction"] = _vm.Transaction
            });
    }

    private void ClearEntPayee_Clicked(object sender, EventArgs e)
    {
        _vm.Transaction.Payee = "";
        LoadHeader();
        LoadPayeeList(_vm.Transaction.Payee);

        entPayee.IsEnabled = false;
        entPayee.IsEnabled = true;
        entPayee.Unfocus();

    }

    private async void AddNewPayee_Clicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Add New Payee", $"Are you sure you want to add {_vm.Transaction.Payee} as a new Payee?", "Yes, continue", "No, go back!");
        if (result)
        {

        }
    }

    private void entPayee_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadHeader();
        LoadPayeeList(_vm.Transaction.Payee);
    }
}