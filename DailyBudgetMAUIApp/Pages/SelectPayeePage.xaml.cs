using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace DailyBudgetMAUIApp.Pages;

public partial class SelectPayeePage : BasePage
{
	private readonly IRestDataService _ds;
	private readonly IProductTools _pt;
	private readonly SelectPayeePageViewModel _vm;
    private IDispatcherTimer _payeeSearchTimer;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }
    public string LastSearchPayee { get; set; } = "";

    public SelectPayeePage(IRestDataService ds, IProductTools pt, SelectPayeePageViewModel viewModel)
    {
        _ds = ds;
        _pt = pt;

        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;

        var timer = Application.Current.Dispatcher.CreateTimer();
        _payeeSearchTimer = timer;

        timer.Interval = TimeSpan.FromMilliseconds(800);
        timer.Tick += async (s, e) =>
        {
            try
            {
                await UpdateAfterPayeeChanged();
                _payeeSearchTimer.Stop();
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "SelectPayee", "timer.Tick");
            }
        };

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 60;
        ButtonWidth = ScreenWidth - 40;

    }

    public SelectPayeePage(int BudgetID, Transactions Transaction, IRestDataService ds, IProductTools pt, SelectPayeePageViewModel viewModel)
	{
        if (Transaction.Payee == null)
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
        _vm.SelectedPayee = Transaction.Payee;

        _vm.PageType = "Transaction";
        entTransactionPayee.IsVisible = true;

    }

    public SelectPayeePage(int BudgetID, Bills Bill, IRestDataService ds, IProductTools pt, SelectPayeePageViewModel viewModel)
    {
        if (Bill.BillPayee == null)
        {
            Bill.BillPayee = "";
        }

        _ds = ds;
        _pt = pt;

        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        
        _vm.Bill = Bill;
        _vm.BudgetID = BudgetID;
        _vm.SelectedPayee = Bill.BillPayee;

        _vm.PageType = "Bill";
        entBillPayee.IsVisible = true;

    }

    async protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {

        base.OnNavigatedTo(args);

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }
    }

    private void acrPayeeName_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!PayeeName.IsVisible)
            {
                PayeeName.IsVisible = true;
                PayeeNameIcon.Glyph = "\ue5cf";
            }
            else
            {
                PayeeName.IsVisible = false;
                PayeeNameIcon.Glyph = "\ue5ce";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectPayee", "acrPayeeName_Tapped");
        }
    }

    private void acrPayeeList_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!PayeeList.IsVisible)
            {
                PayeeList.IsVisible = true;
                PayeeListIcon.Glyph = "\ue5cf";
            }
            else
            {
                PayeeList.IsVisible = false;
                PayeeListIcon.Glyph = "\ue5ce";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectPayee", "acrPayeeList_Tapped");
        }
    }

    async protected override void OnAppearing()
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            if (_vm.PageType == "ViewList")
            {
                _vm.Transaction = new Transactions
                {
                    Payee = ""
                };
            }

            await Task.Delay(10);

            TopBV.WidthRequest = ScreenWidth;
            MainAbs.WidthRequest = ScreenWidth;
            MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
            MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));
            base.OnAppearing();

            _vm.SelectedPayee = _vm.Transaction.Payee;
            if (string.Equals(_vm.PageType, "Bill", StringComparison.OrdinalIgnoreCase))
            {
                entBillPayee.IsVisible = true;
            }
            else
            {
                entTransactionPayee.IsVisible = true;
            }

            _vm.PayeeList = await _ds.GetPayeeList(_vm.BudgetID);

            LoadHeader();
            if (_vm.PageType == "Bill")
            {
                LoadPayeeList(_vm.Bill.BillPayee);
            }
            else if (_vm.PageType == "Transaction")
            {
                LoadPayeeList(_vm.Transaction.Payee);
            }
            else
            {
                LoadPayeeList(_vm.Transaction.Payee);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "SelectPayee", "OnAppearing");
        }
    }


    private void LoadHeader()
    {
        if (_vm.PageType == "Bill")
        {
            if (string.IsNullOrEmpty(_vm.Bill.BillPayee))
            {
                entBillPayee.WidthRequest = _vm.EntryWidth;
                bvHeader.WidthRequest = _vm.EntryWidth;
                btnClearPayee.IsVisible = false;
            }
            else
            {
                entBillPayee.WidthRequest = _vm.EntryButtonWidth;
                bvHeader.WidthRequest = _vm.EntryButtonWidth;
                btnClearPayee.IsVisible = true;
            }
        }
        else if (_vm.PageType == "Transaction")
        {
            if (string.IsNullOrEmpty(_vm.Transaction.Payee))
            {
                entTransactionPayee.WidthRequest = _vm.EntryWidth;
                bvHeader.WidthRequest = _vm.EntryWidth;
                btnClearPayee.IsVisible = false;
            }
            else
            {
                entTransactionPayee.WidthRequest = _vm.EntryButtonWidth;
                bvHeader.WidthRequest = _vm.EntryButtonWidth;
                btnClearPayee.IsVisible = true;
            }
        }
        else
        {
            if (string.IsNullOrEmpty(_vm.Transaction.Payee))
            {
                entTransactionPayee.WidthRequest = _vm.EntryWidth;
                bvHeader.WidthRequest = _vm.EntryWidth;
                btnClearPayee.IsVisible = false;
            }
            else
            {
                entTransactionPayee.WidthRequest = _vm.EntryButtonWidth;
                bvHeader.WidthRequest = _vm.EntryButtonWidth;
                btnClearPayee.IsVisible = true;
            }
        }

        if(_vm.PageType == "Transaction")
        {
            _vm.PayeeDoesntExists = !_vm.PayeeList.Contains(_vm.Transaction.Payee) && _vm.Transaction.Payee != "";
        }
        else if(_vm.PageType == "Bill")
        {
            _vm.PayeeDoesntExists = !_vm.PayeeList.Contains(_vm.Bill.BillPayee) && _vm.Bill.BillPayee != "";
        }
        else
        {
            _vm.PayeeDoesntExists = !_vm.PayeeList.Contains(_vm.Transaction.Payee) && _vm.Transaction.Payee != "";
        }
        

        if(_vm.PayeeDoesntExists)
        {
            if(_vm.PageType != "ViewList")
            {
                hslAddNewPayee.IsVisible = true;
            }
            else
            {
                hslAddNewPayee.IsVisible = false;
            }            
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
        Application.Current.Resources.TryGetValue("InfoLL", out var InfoLL);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("brdPrimary", out var brdPrimary);
        Application.Current.Resources.TryGetValue("PrimaryDark", out var PrimaryDark);
        Application.Current.Resources.TryGetValue("PrimaryLightLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("White", out var White);

        List<string> FilteredList = new List<string>();
        if(!string.IsNullOrWhiteSpace(SearchQuery))
        {        
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
                            Color = (Color)Info,
                            HorizontalOptions = LayoutOptions.End,
                            Margin = new Thickness(0, 0, 0, 10)

                        };
                        LetterVSL.Children.Add(LetterLabel);
                        LetterVSL.Children.Add(LetterBoxView);
                    }

                    Border PayeeBorder = new Border
                    {
                        Style = (Style)brdPrimary,
                        Margin = new Thickness(5,0,5,4)                   
                    };

                    if (_vm.PageType != "ViewList")
                    {
                        TapGestureRecognizer PayeeTapGesture = new TapGestureRecognizer
                        {
                            CommandParameter = Payee
                        };

                        PayeeTapGesture.Tapped += (s, e) => SelectExistingPayee_Tapped(s, e);
                        PayeeBorder.GestureRecognizers.Add(PayeeTapGesture);
                    }


                    Label PayeeLabel = new Label
                    {
                        Text = Payee,
                        TextColor = (Color)Gray900,
                        FontSize = 14,
                        Padding = new Thickness(2,2,2,2),
                        Margin = new Thickness(10,0,0,0)
                    };

                    string PayeeImageGlyph = "";

                    if (_vm.SelectedPayee == Payee)
                    {
                        PayeeImageGlyph = "\ue837";
                    }
                    else
                    {
                        PayeeImageGlyph = "\ue836";
                    }

                    Image PayeeImage = new Image
                    {
                        VerticalOptions = LayoutOptions.Center,
                        BackgroundColor = (Color)White,
                        Source = new FontImageSource
                        {
                            FontFamily = "MaterialDesignIcons",
                            Glyph = PayeeImageGlyph,
                            Size = 12,
                            Color = (Color)Info
                        }
                    };

                    Border ImageBorder = new Border
                    {
                        StrokeThickness = 0,
                        HeightRequest = 12,
                        WidthRequest = 12,
                        StrokeShape = new RoundRectangle
                        {
                            CornerRadius = 6
                        },
                        BackgroundColor = (Color)White
                    };

                    ImageBorder.Content = PayeeImage;

                    HorizontalStackLayout PayeeHSL = new HorizontalStackLayout();

                    PayeeHSL.Children.Add(ImageBorder);
                    PayeeHSL.Children.Add(PayeeLabel);

                    PayeeBorder.Content = PayeeHSL;
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
        else
        {
            vslPayeeList.Children.Clear();

            brdPayeeNotSetUp.IsVisible = true;
            PayeeList.IsVisible = false;
            _vm.FilteredListEmptyText = "Start searching for a payee, enter your payee name .. if it doesn't exist you can create a new payee!";
        }

    }

    private async void SelectExistingPayee_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            string Payee = (string)e.Parameter;

            bool result = await DisplayAlert($"Select {Payee}", $"Are you sure you want to Select {Payee} as the Payee?", "Yes, continue", "No, go back!");
            if (result)
            {
                entBillPayee.IsEnabled = false;
                entBillPayee.IsEnabled = true;
                entBillPayee.Unfocus();
                entTransactionPayee.IsEnabled = false;
                entTransactionPayee.IsEnabled = true;
                entTransactionPayee.Unfocus();

                if (_vm.PageType == "Transaction")
                {
                    if (string.IsNullOrEmpty(_vm.Transaction.Category))
                    {
                        Categories LastCategory = await _ds.GetPayeeLastCategory(_vm.BudgetID, Payee);
                        _vm.Transaction.Category = LastCategory.CategoryName;
                        _vm.Transaction.CategoryID = LastCategory.CategoryID;
                    }

                    _vm.Transaction.Payee = Payee;
                    await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}&NavigatedFrom=SelectPayeePage&TransactionID={_vm.Transaction.TransactionID}",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = _vm.Transaction
                    });
                }
                else if (_vm.PageType == "Bill")
                {
                    _vm.Bill.BillPayee = Payee;
                    await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}&NavigatedFrom=SelectPayeePage&BillID={_vm.Bill.BillID}{(_vm.FamilyAccountID > 0 ? $"&FamilyAccountID={_vm.FamilyAccountID}" : "")}",
                    new Dictionary<string, object>
                    {
                        ["Bill"] = _vm.Bill
                    });
                }

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "SelectPayee", "SelectExistingPayee_Tapped");
        }


    }

    private async void BackButton_Clicked(object sender, TappedEventArgs e)
    {
        try
        {
            entTransactionPayee.IsEnabled = false;
            entTransactionPayee.IsEnabled = true;
            entTransactionPayee.Unfocus();
            entBillPayee.IsEnabled = false;
            entBillPayee.IsEnabled = true;
            entBillPayee.Unfocus();

            if (_vm.PageType == "Transaction")
            {
                _vm.Transaction.Payee = "";

                await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}&NavigatedFrom=SelectPayeePage&TransactionID={_vm.Transaction.TransactionID}",
                new Dictionary<string, object>
                {
                ["Transaction"] = _vm.Transaction
                });
            }
            else if (_vm.PageType == "Bill")
            {
                _vm.Bill.BillPayee = "";

                await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}&NavigatedFrom=SelectPayeePage&BillID={_vm.Bill.BillID}{(_vm.FamilyAccountID > 0 ? $"&FamilyAccountID={_vm.FamilyAccountID}" : "")}",
                new Dictionary<string, object>
                {
                    ["Bill"] = _vm.Bill
                });
            }
            else
            {
                await Shell.Current.GoToAsync($"..");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "SelectPayee", "BackButton_Clicked");
        }
    }

    private void ClearEntPayee_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.PageType == "Bill")
            {
                _vm.Bill.BillPayee = "";
                LoadHeader();
                LoadPayeeList(_vm.Bill.BillPayee);

                entBillPayee.IsEnabled = false;
                entBillPayee.IsEnabled = true;
                entBillPayee.Unfocus();
            }
            else if(_vm.PageType == "Transaction")
            {
                _vm.Transaction.Payee = "";
                LoadHeader();
                LoadPayeeList(_vm.Transaction.Payee);

                entTransactionPayee.IsEnabled = false;
                entTransactionPayee.IsEnabled = true;
                entTransactionPayee.Unfocus();
            }
            else
            {
                _vm.Transaction.Payee = "";
                LoadHeader();
                LoadPayeeList(_vm.Transaction.Payee);

                entTransactionPayee.IsEnabled = false;
                entTransactionPayee.IsEnabled = true;
                entTransactionPayee.Unfocus();
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectPayee", "ClearEntPayee_Clicked");
        }
    }

    private void entPayee_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            _payeeSearchTimer.Stop();
            if (LastSearchPayee != e.NewTextValue) 
            {
                _payeeSearchTimer.Start();
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectPayee", "entPayee_TextChanged");
        }
    }

    private async Task UpdateAfterPayeeChanged()
    {
        await Task.Delay(100);
        LoadHeader();
        if (_vm.PageType == "Bill")
        {
            LoadPayeeList(_vm.Bill.BillPayee);
            LastSearchPayee = _vm.Bill.BillPayee;
            _vm.PayeeName = _vm.Bill.BillPayee;
        }
        else if (_vm.PageType == "Transaction")
        {
            LoadPayeeList(_vm.Transaction.Payee);
            LastSearchPayee = _vm.Transaction.Payee;
            _vm.PayeeName = _vm.Transaction.Payee;
        }
        else
        {
            LoadPayeeList(_vm.Transaction.Payee);
            LastSearchPayee = _vm.Transaction.Payee;
            _vm.PayeeName = _vm.Transaction.Payee;
        }
    }

    private async void SavePayeeName_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_vm.PageType == "Bill")
            {
                bool result = await DisplayAlert("Add New Payee", $"Are you sure you want to add {_vm.Bill.BillPayee} as a new Payee?", "Yes, continue", "No, go back!");
                if (result)
                {
                    entBillPayee.IsEnabled = false;
                    entBillPayee.IsEnabled = true;
                    entBillPayee.Unfocus();

                    await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}&NavigatedFrom=SelectPayeePage&BillID={_vm.Bill.BillID}{(_vm.FamilyAccountID > 0 ? $"&FamilyAccountID={_vm.FamilyAccountID}" : "")}",
                    new Dictionary<string, object>
                    {
                        ["Bill"] = _vm.Bill
                    });
                }
            }
            else if (_vm.PageType == "Transaction")
            {
                bool result = await DisplayAlert("Add New Payee", $"Are you sure you want to add {_vm.Transaction.Payee} as a new Payee?", "Yes, continue", "No, go back!");
                if (result)
                {
                    entTransactionPayee.IsEnabled = false;
                    entTransactionPayee.IsEnabled = true;
                    entTransactionPayee.Unfocus();

                    await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}&NavigatedFrom=SelectPayeePage&TransactionID={_vm.Transaction.TransactionID}",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = _vm.Transaction
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "SelectPayee", "SavePayeeName_Clicked");
        }
    }
}