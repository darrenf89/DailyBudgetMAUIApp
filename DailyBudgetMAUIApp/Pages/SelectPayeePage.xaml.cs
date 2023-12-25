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

        _ds = ds;
        _pt = pt;

        InitializeComponent();

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
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("brdPrimary", out var brdPrimary);
        Application.Current.Resources.TryGetValue("PrimaryDark", out var PrimaryDark);

        List<string> FilteredList = new List<string>();

        if (!(_vm.PayeeList.Count == 0 && _vm.PayeeList == null))
        {
            vslPayeeList.Children.Clear();

            FilteredList = _vm.PayeeList.Where(x => x.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            string StartLetter = "*";
            VerticalStackLayout LetterVSL = null;

            foreach (string Payee in FilteredList)
            {
                if (!Payee.StartsWith(StartLetter))
                {
                    if (LetterVSL != null)
                    {
                        vslPayeeList.Children.Add(LetterVSL);
                    }

                    StartLetter = Payee[0].ToString();

                    LetterVSL = new VerticalStackLayout
                    {
                        HorizontalOptions = LayoutOptions.End,
                        WidthRequest = _vm.PayeeBorderWidth,
                        Margin = new Thickness(0, 0, 0, 10)
                    };

                    Label LetterLabel = new Label
                    {
                        Text = StartLetter.ToUpper(),
                        FontFamily = "ManoloMono",
                        HorizontalOptions = LayoutOptions.End,
                        FontSize = 40,
                        TextColor = (Color)Primary
                    };

                    BoxView LetterBoxView = new BoxView
                    {
                        WidthRequest = 60,
                        HeightRequest = 4,
                        Color = (Color)Tertiary,
                        HorizontalOptions = LayoutOptions.End,
                        Margin = new Thickness(0, 0, 0, 10)

                    };
                    LetterVSL.Children.Add(LetterLabel);
                    LetterVSL.Children.Add(LetterBoxView);
                }

                Border PayeeBorder = new Border
                {
                    Style = (Style)brdPrimary,
                    Margin = new Thickness(5,0,5,4),                    
                };

                TapGestureRecognizer PayeeTapGesture = new TapGestureRecognizer {
                    CommandParameter = Payee
                };
                PayeeTapGesture.Tapped += (s, e) => SelectExistingPayee_Tapped(s, e);
                PayeeBorder.GestureRecognizers.Add(PayeeTapGesture);

                Label PayeeLabel = new Label
                {
                    Text = Payee,
                    TextColor = (Color)Gray900,
                    FontSize = 14,
                    Padding = new Thickness(2,2,2,2)
                };

                PayeeBorder.Content = PayeeLabel;
                LetterVSL.Children.Add(PayeeBorder);
            }

            vslPayeeList.Children.Add(LetterVSL);
        }

        if(FilteredList.Count == 0)
        {
            brdPayeeNotSetUp.IsVisible = true;
            PayeeList.IsVisible = false;
            _vm.FilteredListEmptyText = "No Payee matches that name, to create one go ahead and hit the create button!";
        }
        else
        {
            brdPayeeNotSetUp.IsVisible = false;
            PayeeList.IsVisible = true;
        }
 
    }

    private async void SelectExistingPayee_Tapped(object sender, TappedEventArgs e)
    {
        string Payee = (string)e.Parameter;

        bool result = await DisplayAlert($"Select {Payee}", $"Are you sure you want to Select {Payee} as the Payee?", "Yes, continue", "No, go back!");
        if (result)
        {
            entPayee.IsEnabled = false;
            entPayee.IsEnabled = true;
            entPayee.Unfocus();

            if(string.IsNullOrEmpty(_vm.Transaction.Category))
            {
                Categories LastCategory = await _ds.GetPayeeLastCategory(_vm.BudgetID, Payee);
                _vm.Transaction.Category = LastCategory.CategoryName;
                _vm.Transaction.CategoryID = LastCategory.CategoryID;
            }

            _vm.Transaction.Payee = Payee;
            await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}",
            new Dictionary<string, object>
            {
                ["Transaction"] = _vm.Transaction
            });
        }


    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        _vm.Transaction.Payee = "";

        entPayee.IsEnabled = false;
        entPayee.IsEnabled = true;
        entPayee.Unfocus();

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
            entPayee.IsEnabled = false;
            entPayee.IsEnabled = true;
            entPayee.Unfocus();

            await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}",
            new Dictionary<string, object>
            {
                ["Transaction"] = _vm.Transaction
            });
        }
    }

    private void entPayee_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadHeader();
        LoadPayeeList(_vm.Transaction.Payee);
    }
}