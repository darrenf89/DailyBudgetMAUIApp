using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui.Layouts;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Maui.ApplicationModel;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.Messaging;
using static DailyBudgetMAUIApp.Pages.ViewAccounts;


namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class MultipleAccountsBottomSheet : BottomSheet, INotifyPropertyChanged
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }
    public bool IsUpdatingCheckBox { get; set; }

    private List<BankAccounts> BankAccounts = new List<BankAccounts>();
    private Dictionary<int, CheckBox> AccountCheckBoxes = new Dictionary<int, CheckBox>();
    private Dictionary<int, BorderlessEntry> AccountNameEntries = new Dictionary<int, BorderlessEntry>();
    private Dictionary<int, BorderlessEntry> AccountBalanceEntries = new Dictionary<int, BorderlessEntry>();
    private Dictionary<int, Border> AccountNameBorders = new Dictionary<int, Border>();
    private Dictionary<int, Grid> AccountGrid = new Dictionary<int, Grid>();
    private Dictionary<int, decimal> AccountBalances = new Dictionary<int, decimal>();


    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public MultipleAccountsBottomSheet(IProductTools pt, IRestDataService ds)
	{
		InitializeComponent();

        this.BindingContext = this;
        _pt = pt;
        _ds = ds;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;

        MainScrollView.MaximumHeightRequest = ScreenHeight - App.NavBarHeight - App.StatusBarHeight - 80;

        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, AbsoluteLayout.AutoSize));
        MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));

        lblTitle.Text = $"Account balances";

        this.PropertyChanged += ViewTransactionFilterBottomSheet_PropertyChanged;
        try
        {
            LoadPageData();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "EditProfilePictureBottomSheet", "EditProfilePictureBottomSheet");
        }

    }

    private async Task LoadPageData()
    {
        BankAccounts = await _ds.GetBankAccounts(App.DefaultBudgetID);

        vslAccounts.Children.Clear();
        AccountCheckBoxes.Clear();
        AccountNameEntries.Clear();
        AccountBalanceEntries.Clear();
        AccountNameBorders.Clear();
        AccountBalances.Clear();
        AccountGrid.Clear();
        decimal TotalBalance = 0;

        foreach (BankAccounts B in BankAccounts) 
        {
            TotalBalance += B.AccountBankBalance.GetValueOrDefault();            
            Grid bb = await CreateBankAccountElements(B);
            vslAccounts.Children.Add(bb);
            AccountGrid.Add(B.ID, bb);
        }

        lblBankBalance.Text = TotalBalance.ToString("c", CultureInfo.CurrentCulture);
    }

    private async Task<Grid> CreateBankAccountElements(BankAccounts B)
    {
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("StandardInputBorder", out var StandardInputBorder);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Danger", out var Danger);


        Grid grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = 60 },
                new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }, 
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
            },
            Margin = Margin = new Thickness(0, 10, 0, 10)
        };

        CheckBox checkBox = new()
        {
            IsChecked = B.IsDefaultAccount,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center            
        };
        checkBox.CheckedChanged += async (s, e) =>
        {
            try
            {
                if(!IsUpdatingCheckBox)
                {
                    if (e.Value)
                    {
                        await OnCheckBoxCheckedChanged(B.ID);
                    }
                    else
                    {
                        (s as CheckBox).IsChecked = true;
                        await Shell.Current.DisplayAlert("Must have a Default Account", "To deselect, please choose a different default account. A default account must always be set!", "OK");
                        return;
                    }

                    IsUpdatingCheckBox = false;
                }

            }
            catch (Exception ex) 
            {
                await _pt.HandleException(ex, "MultipleAccountsBottomSheet", "checkBox.CheckedChanged");
            }

        };
        
        grid.AddWithSpan(checkBox, 0, 0,1,1);
        AccountCheckBoxes.Add(B.ID, checkBox);

        BorderlessEntry nameEntry = new()
        {
            Margin = new Thickness(10,0,0, 0),
            ReturnType = ReturnType.Done,
            Keyboard = Keyboard.Text,
            TextColor = (Color)Gray900,
            MaxLength = 25,
            Text = B.BankAccountName
        };        

        Border nameBorder = new()
        {
            Style = (Style)StandardInputBorder,
            Padding = new Thickness(0,0,0,0),
            HeightRequest = 44,
            Margin = new Thickness(20,0,20,0),
            VerticalOptions = LayoutOptions.Center
        };

        Image Img = new Image
        {
            BackgroundColor = Color.FromArgb("#00FFFFFF"),
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.End,
            Margin = new Thickness(5, 2, 5, 0),
            Source = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue92e",
                Size = 20,
                Color = (Color)Danger,
            }
        };

        TapGestureRecognizer TapGesture = new TapGestureRecognizer();
        TapGesture.NumberOfTapsRequired = 1;
        TapGesture.Tapped += async (s, e) =>
        {
            try
            {
                await DeleteAccount(B.ID);
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "MultipleAccountsBottomSheet", "DeleteAccount.Tapped");
            }
        };

        Img.GestureRecognizers.Add(TapGesture);

        if (B.IsDefaultAccount)
        {
            nameBorder.Shadow = new Shadow()
            {
                Brush = (Brush)PrimaryBrush,
                Opacity = (float)0.95,
                Offset = new Point(0, 0),
                Radius = 10
            };
        }

        Grid deleteGrid = new()
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            Margin = Margin = new Thickness(0, 0, 0, 0)
        };

        deleteGrid.AddWithSpan(Img, 0, 1, 1, 1);
        deleteGrid.AddWithSpan(nameEntry, 0, 0, 1, 1);

        nameBorder.Content = deleteGrid;        
        grid.AddWithSpan(nameBorder, 0, 1, 1, 1);
        AccountNameEntries.Add(B.ID, nameEntry);
        AccountNameBorders.Add(B.ID, nameBorder);

        string BankBalanceText = B.AccountBankBalance.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
        BorderlessEntry balanceEntry = new()
        {
            Margin = new Thickness(0,0,0,0),
            ReturnType = ReturnType.Done,
            Keyboard = Keyboard.Numeric,
            TextColor = (Color)Success,
            MaxLength = 25,
            Text = BankBalanceText,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        balanceEntry.TextChanged += async (s, e) =>
        {
            try
            {
                await BankBalanceChanged(B.ID, e.NewTextValue, e.OldTextValue);
            }
            catch (Exception ex) 
            {
                await _pt.HandleException(ex, "MultipleAccountsBottomSheet", "balanceEntry.TextChanged");
            }
            
        };

        grid.AddWithSpan(balanceEntry, 0, 2, 1, 1);
        AccountBalanceEntries.Add(B.ID, balanceEntry);
        AccountBalances.Add(B.ID, B.AccountBankBalance.GetValueOrDefault());

        return grid;
    }

    private void ViewTransactionFilterBottomSheet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            string PropertyChange = (string)e.PropertyName;
            if (PropertyChange == "SelectedDetent")
            {
                double Height = this.Height;

                BottomSheet Sender = (BottomSheet)sender;

                if (Sender.SelectedDetent is FullscreenDetent)
                {
                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.None);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, Height - 60, ScreenWidth, AbsoluteLayout.AutoSize));
                }
                else if (Sender.SelectedDetent is MediumDetent)
                {
                    MediumDetent detent = (MediumDetent)Sender.SelectedDetent;

                    double NewHeight = (Height * detent.Ratio) - 60;

                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.None);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, NewHeight, ScreenWidth, AbsoluteLayout.AutoSize));
                }
                else if (Sender.SelectedDetent is FixedContentDetent)
                {
                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));
                }

            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MultipleAccountsBottomSheet", "ViewTransactionFilterBottomSheet_PropertyChanged");
        }
    }

    private async Task OnCheckBoxCheckedChanged(int CheckedID) 
    {       

        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);

        foreach (var C in AccountCheckBoxes) 
        {
            IsUpdatingCheckBox = true;

            CheckBox checkBox = C.Value;
            if (C.Key == CheckedID) 
            {
                AccountNameBorders[C.Key].Shadow = new Shadow()
                {
                    Brush = (Brush)PrimaryBrush,
                    Opacity = (float)0.95,
                    Offset = new Point(0, 0),
                    Radius = 10
                };
                checkBox.IsChecked = true;
            }
            else
            {
                AccountNameBorders[C.Key].Shadow = null;
                checkBox.IsChecked = false;
            }
        }

    }

    private async Task BankBalanceChanged(int BalanceID, string NewAmount, string OldAmount)
    {
        try
        {
            decimal TransactionAmount = (decimal)_pt.FormatCurrencyNumber(NewAmount);
            var Entry = AccountBalanceEntries[BalanceID];
            Entry.Text = TransactionAmount.ToString("c", CultureInfo.CurrentCulture);

            int position = NewAmount.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
            if (!string.IsNullOrEmpty(OldAmount) && (OldAmount.Length - position) == 2 && Entry.CursorPosition > position)
            {
                Entry.CursorPosition = Entry.Text.Length;
            }

            AccountBalances[BalanceID] = TransactionAmount;
            decimal TotalBalance = 0;
            foreach (var accountBalance in AccountBalances.Values)
            {
                TotalBalance += accountBalance;
            }
            lblBankBalance.Text = TotalBalance.ToString("c", CultureInfo.CurrentCulture);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddTransaction", "TransactionAmount_Changed");
        }
    }

    private void acrAccounts_Tapped(object sender, TappedEventArgs e)
    {
        if (!Accounts.IsVisible)
        {
            Accounts.IsVisible = true;
            AccountsIcon.Glyph = "\ue5cf";
        }
        else
        {
            Accounts.IsVisible = false;
            AccountsIcon.Glyph = "\ue5ce";
        }
    }

    private async void UpdateBalances_Clicked(object sender, EventArgs e)
    {
        try
        {
            decimal TotalBalance = 0;
            foreach (var accountBalance in AccountBalances.Values)
            {
                TotalBalance += accountBalance;
            }

            bool result = false;
            if (TotalBalance == App.DefaultBudget.BankBalance)
            {
                result = true;
            }
            else
            {
                string message = "";
                if (TotalBalance > App.DefaultBudget.BankBalance)
                {
                    message = $"By accepting these changes, your bank balance will increase from {App.DefaultBudget.BankBalance.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} to {TotalBalance.ToString("c", CultureInfo.CurrentCulture)}, giving you more money than you originally had!";
                }
                else
                {
                    message = $"If you accept these changes, your bank balance will decrease from {App.DefaultBudget.BankBalance.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} to {TotalBalance.ToString("c", CultureInfo.CurrentCulture)}, leaving you with less money than you started with.";
                }

                result = await Shell.Current.DisplayAlert("Careful, before you proceed!", message, "Yes", "No");
            }
            
            if (result)
            {
                if (App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);
                }

                await Task.Delay(1);

                List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

                PatchDoc IsMultipleAccountsPatch = new PatchDoc
                {
                    op = "replace",
                    path = "/IsMultipleAccounts",
                    value = true
                };

                BudgetUpdate.Add(IsMultipleAccountsPatch);

                PatchDoc BankBalancePatch = new PatchDoc
                {
                    op = "replace",
                    path = "/BankBalance",
                    value = TotalBalance
                };

                BudgetUpdate.Add(BankBalancePatch);
                await _ds.PatchBudget(App.DefaultBudgetID, BudgetUpdate);


                foreach(BankAccounts B in BankAccounts)
                {
                    B.BankAccountName = AccountNameEntries[B.ID].Text;
                    B.AccountBankBalance = AccountBalances[B.ID];
                    B.IsDefaultAccount = AccountCheckBoxes[B.ID].IsChecked;
                    await _ds.UpdateBankAccounts(App.DefaultBudgetID, B);
                }

                await _ds.ReCalculateBudget(App.DefaultBudgetID);
                App.DefaultBudget = _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full").Result;

                try
                {
                    WeakReferenceMessenger.Default.Send(new UpdateViewAccount(true, false));
                }
                catch
                {

                }                

                if (App.CurrentBottomSheet != null)
                {
                    await App.CurrentBottomSheet.DismissAsync();
                    App.CurrentBottomSheet = null;
                }

                if (App.CurrentPopUp != null)
                {
                    await App.CurrentPopUp.CloseAsync();
                    App.CurrentPopUp = null;
                }

                await _pt.MakeSnackBar("Congrats you have set up multiple accounts", null, null, new TimeSpan(0, 0, 10), "Success");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MultipleAccountsBottomSheet", "UpdateBalances_Clicked");
        }

    }

    private async void AddNewAccountClicked(object sender, EventArgs e)
    {
        BankAccounts Account = new BankAccounts
        {
            BankAccountName = $"Account {BankAccounts.Count+1}",
            AccountBankBalance = 0,
            IsDefaultAccount = false
        };
        
        Account = await _ds.AddBankAccounts(App.DefaultBudgetID, Account);
        this.BankAccounts.Add(Account);

        Grid bb = await CreateBankAccountElements(Account);
        vslAccounts.Children.Add(bb);
        AccountGrid.Add(Account.ID, bb);

    }

    private async Task DeleteAccount(int AccountID)
    {
        if(AccountCheckBoxes[AccountID].IsChecked)
        {
            await Shell.Current.DisplayAlert("Must have a Default Account", "Can not delete this account as it is set to the default account. A default account must always be set!", "OK");
            return;
        }

        bool result = await Shell.Current.DisplayAlert("Are you sure?", "Are you sure you want to delete this account?", "Yes", "No");
        if(result)
        {
            await _ds.DeleteBankAccount(AccountID);

            vslAccounts.Children.Remove(AccountGrid[AccountID]);
            AccountGrid.Remove(AccountID);
            AccountCheckBoxes.Remove(AccountID);
            AccountNameEntries.Remove(AccountID);
            AccountBalanceEntries.Remove(AccountID);
            AccountNameBorders.Remove(AccountID);
            AccountBalances.Remove(AccountID);

            for (int i = BankAccounts.Count - 1; i >= 0; i--)
            {
                BankAccounts b = BankAccounts[i];
                if (b.ID == AccountID)
                {
                    BankAccounts.RemoveAt(i);
                }
            }

            decimal TotalBalance = 0;

            foreach (BankAccounts B in BankAccounts)
            {
                TotalBalance += B.AccountBankBalance.GetValueOrDefault();
            }

            lblBankBalance.Text = TotalBalance.ToString("c", CultureInfo.CurrentCulture);
        }       
    }
}