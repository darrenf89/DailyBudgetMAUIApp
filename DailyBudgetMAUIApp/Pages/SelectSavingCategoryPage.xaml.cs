using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using System.Globalization;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui;

namespace DailyBudgetMAUIApp.Pages;

public partial class SelectSavingCategoryPage : BasePage
{
	private readonly IRestDataService _ds;
	private readonly IProductTools _pt;
	private readonly IPopupService _ps;
	private readonly SelectSavingCategoryPageViewModel _vm;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }

    public SelectSavingCategoryPage(IRestDataService ds, IProductTools pt, SelectSavingCategoryPageViewModel viewModel, IPopupService ps)
    {
        _ds = ds;
        _pt = pt;
        _ps = ps;

        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 60;
        ButtonWidth = ScreenWidth - 40;
    }
    public SelectSavingCategoryPage(int BudgetID, Transactions Transaction, IRestDataService ds, IProductTools pt, SelectSavingCategoryPageViewModel viewModel)
	{
        if(Transaction.Category == null)
        {
            Transaction.Category = "";
            Transaction.CategoryID = 0;
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
        try
        {
            await Task.Delay(10);

            TopBV.WidthRequest = ScreenWidth;
            MainAbs.WidthRequest = ScreenWidth;
            MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
            MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));

            base.OnAppearing();

            _vm.EnvelopeSavingList = await _ds.GetBudgetEnvelopeSaving(_vm.BudgetID);

            decimal Total = 0;
            decimal Balance = 0;

            foreach (Savings Saving in _vm.EnvelopeSavingList)
            {
                Total += Saving.PeriodSavingValue.GetValueOrDefault();
                Balance += Saving.CurrentBalance.GetValueOrDefault();
            }

            if (_vm.EnvelopeSavingList.Count == 0)
            {
                brdNoSavings.IsVisible = true;
                SavingList.IsVisible = false;
            }
            else
            {
                brdNoSavings.IsVisible = false;
                SavingList.IsVisible = true;
            }

            _vm.EnvelopeFilteredSavingList = _vm.EnvelopeSavingList;
            LoadSavingList();

            if (App.IsPopupShowing){App.IsPopupShowing = false;await _ps.ClosePopupAsync(Shell.Current);}
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "SelectSavingCategory", "OnAppearing");
        }
    }

    private void LoadSavingList()
    {
        Application.Current.Resources.TryGetValue("brdPrimaryWhite", out var brdPrimaryWhite);
        Application.Current.Resources.TryGetValue("brdInfo", out var brdInfo);
        Application.Current.Resources.TryGetValue("brdPrimary", out var brdPrimary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("PrimaryLightLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("PrimaryDark", out var PrimaryDark);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("brdSuccess", out var brdSuccess);
        Application.Current.Resources.TryGetValue("pillSuccess", out var pillSuccess);
        Application.Current.Resources.TryGetValue("White", out var White);

        vslSavings.Children.Clear();

        if (_vm.EnvelopeFilteredSavingList.Count > 0)
        {
            foreach (Savings Saving in _vm.EnvelopeFilteredSavingList)
            {
                string SavingString = $"Saving{Saving.SavingID}";

                Grid SavingGrid = new Grid
                {
                    Margin = new Thickness(0),
                    VerticalOptions = LayoutOptions.Center,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)}
                    }
                };

                Border SavingNameBorder = new Border
                {
                    Style = (Style)brdPrimary,
                    Margin = new Thickness(0, 0, 0, 5)
                };

                TapGestureRecognizer SavingTapGesture = new TapGestureRecognizer
                {
                    CommandParameter = Saving
                };

                SavingTapGesture.Tapped += (s, e) => SelectSaving_Tapped(s, e);
                SavingNameBorder.GestureRecognizers.Add(SavingTapGesture);

                string SavingText = "";

                if (string.IsNullOrEmpty(Saving.SavingsName))
                {
                    SavingText = $"Saving ID {Saving.SavingID}";
                }
                else
                {
                    SavingText = Saving.SavingsName;
                }

                Label SavingLabel = new Label
                {
                    Text = SavingText,
                    TextColor = (Color)Gray900,
                    FontSize = 14,
                    Padding = new Thickness(12, 2, 2, 2)
                };

                string SavingImageGlyph = "";

                if (_vm.Transaction.SavingID == Saving.SavingID)
                {
                    SavingImageGlyph = "\ue837";
                }
                else
                {
                    SavingImageGlyph = "\ue836";
                }

                Image SavingImage = new Image
                {                           
                    VerticalOptions = LayoutOptions.Center,
                    BackgroundColor = (Color)White,
                    Source = new FontImageSource                            
                    {
                        FontFamily = "MaterialDesignIcons",
                        Glyph = SavingImageGlyph,
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

                ImageBorder.Content = SavingImage;

                Grid SavingHSL = new Grid
                {
                    VerticalOptions = LayoutOptions.Center,
                    ColumnDefinitions =
                    {
                         new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)},
                        new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)},
                        new ColumnDefinition{Width = new GridLength(1, GridUnitType.Auto)}
                    }
                };

                Border EnvelopeBalanceBorder = new Border
                {
                    Style = (Style)pillSuccess,
                    Margin = new Thickness(5,0,5,0),
                    Padding = new Thickness(10, 2, 10, 2),
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Center
                };

                Label EnvelopeBalanceLabel = new Label
                {
                    Text = Saving.CurrentBalance.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture),
                    TextColor = (Color)Success,
                    FontSize = 10,
                    Padding = new Thickness(1, 1, 1, 1),
                    Margin = new Thickness(0)
                };

                EnvelopeBalanceBorder.Content = EnvelopeBalanceLabel;

                SavingHSL.Add(ImageBorder, 0,0);
                SavingHSL.Add(SavingLabel,1,0);
                SavingHSL.Add(EnvelopeBalanceBorder,2,0);

                SavingNameBorder.Content = SavingHSL;

                SavingGrid.Add(SavingNameBorder, 0, 0);

                vslSavings.Children.Add(SavingGrid);
            }
        }

        if (_vm.EnvelopeSavingList.Count == 0)
        {
            brdNoSavings.IsVisible = true;
            SavingList.IsVisible = false;
        }
        else if (_vm.EnvelopeFilteredSavingList.Count == 0)
        {
            _vm.NoEnvelopeSavingText = "No Categories match that search criteria!";
            brdNoSavings.IsVisible = true;
            SavingList.IsVisible = false;
        }
        else
        {
            brdNoSavings.IsVisible = false;
            SavingList.IsVisible = true;
        }
    }   

    private async void SelectSaving_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Savings Saving = (Savings)e.Parameter;

            bool result = await DisplayAlert($"Select {Saving.SavingsName}", $"Are you sure you want to spend from your {Saving.SavingsName} envelope?", "Yes, continue", "No, go back!");
            if (result)
            {
                _vm.Transaction.SavingName = Saving.SavingsName;
                _vm.Transaction.SavingID = Saving.SavingID;
                _vm.Transaction.SavingsSpendType = "EnvelopeSaving";
                _vm.Transaction.EventType = "Envelope";

                await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}&NavigatedFrom=SelectSavingCategoryPage&TransactionID={_vm.Transaction.TransactionID}",
                new Dictionary<string, object>
                {
                    ["Transaction"] = _vm.Transaction
                });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "SelectSavingCategory", "SelectSaving_Tapped");
        }
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            _vm.Transaction.SavingName = "";
            _vm.Transaction.SavingID = 0;
            _vm.Transaction.SavingsSpendType = "";
            _vm.Transaction.EventType = "Transaction";

            await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}&NavigatedFrom=SelectSavingCategoryPage&TransactionID={_vm.Transaction.TransactionID}",
            new Dictionary<string, object>
            {
                ["Transaction"] = _vm.Transaction
            });
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "SelectSavingCategory", "BackButton_Clicked");
        }
    }

    private void AscSort_Clicked(object sender, EventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("Info", out var Info);
            Application.Current.Resources.TryGetValue("White", out var White);

            if (AscSort.Style == (Style)buttonUnclicked)
            {
                AscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
                AscSort.Style = (Style)buttonClicked;

                DesSort.Style = (Style)buttonUnclicked;
                DesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                BalanceAscSort.Style = (Style)buttonUnclicked;
                BalanceAscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                BalanceDesSort.Style = (Style)buttonUnclicked;
                BalanceDesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }
            else
            {
                AscSort.Style = (Style)buttonUnclicked;
                AscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectSavingCategory", "AscSort_Clicked");
        }
    }

    private void DesSort_Clicked(object sender, EventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("Info", out var Info);
            Application.Current.Resources.TryGetValue("White", out var White);

            if (DesSort.Style == (Style)buttonUnclicked)
            {
                DesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
                DesSort.Style = (Style)buttonClicked;

                AscSort.Style = (Style)buttonUnclicked;
                AscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                BalanceAscSort.Style = (Style)buttonUnclicked;
                BalanceAscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                BalanceDesSort.Style = (Style)buttonUnclicked;
                BalanceDesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }
            else
            {
                DesSort.Style = (Style)buttonUnclicked;
                DesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectSavingCategory", "DesSort_Clicked");
        }
    }

    private void SortCategories(string SortDirection)
    {
        if(SortDirection == "NameAsc")
        {
            _vm.EnvelopeFilteredSavingList = _vm.EnvelopeFilteredSavingList.OrderBy(c => c.SavingsName).ToList();
        }
        else if(SortDirection == "NameDes")
        {
            _vm.EnvelopeFilteredSavingList = _vm.EnvelopeFilteredSavingList.OrderByDescending(c => c.SavingsName).ToList();
        }
        else if (SortDirection == "BalanceAsc")
        {
            _vm.EnvelopeFilteredSavingList = _vm.EnvelopeFilteredSavingList.OrderBy(c => c.CurrentBalance).ToList();
        }
        else if (SortDirection == "BalanceDes")
        {
            _vm.EnvelopeFilteredSavingList = _vm.EnvelopeFilteredSavingList.OrderByDescending(c => c.CurrentBalance).ToList();
        }

    }

    private async void SortFilterApply_Clicked(object sender, EventArgs e)
    {
        try
        {

            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

            await Task.Delay(10);

            entEnvelopeSearch.IsEnabled = false;
            entEnvelopeSearch.IsEnabled = true;

            _vm.EnvelopeFilteredSavingList = _vm.EnvelopeSavingList;

            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);

            string SearchText = "";
            if(!string.IsNullOrEmpty(entEnvelopeSearch.Text))
            {
                SearchText = entEnvelopeSearch.Text;
            }

            _vm.EnvelopeFilteredSavingList = _vm.EnvelopeFilteredSavingList.Where(x => x.SavingsName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)).ToList();

            if (AscSort.Style == (Style)buttonClicked)
            {
                SortCategories("NameAsc");
            }
            else if (DesSort.Style == (Style)buttonClicked)
            {
                SortCategories("NameDes");
            }
            else if (BalanceAscSort.Style == (Style)buttonClicked)
            {
                SortCategories("BalanceAsc");
            }
            else if (BalanceDesSort.Style == (Style)buttonClicked)
            {
                SortCategories("BalanceDes");
            }

            LoadSavingList();

            acrFilterOption_Tapped(null, null);

            if (App.IsPopupShowing){App.IsPopupShowing = false;await _ps.ClosePopupAsync(Shell.Current);}
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "SelectSavingCategory", "SortFilterApply_Clicked");
        }
    }

    private void BalanceDesSort_Clicked(object sender, EventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("Info", out var Info);
            Application.Current.Resources.TryGetValue("White", out var White);

            if (BalanceDesSort.Style == (Style)buttonUnclicked)
            {
                BalanceDesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
                BalanceDesSort.Style = (Style)buttonClicked;

                AscSort.Style = (Style)buttonUnclicked;
                AscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                DesSort.Style = (Style)buttonUnclicked;
                DesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                BalanceAscSort.Style = (Style)buttonUnclicked;
                BalanceAscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }
            else
            {
                BalanceDesSort.Style = (Style)buttonUnclicked;
                BalanceDesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectSavingCategory", "BalanceDesSort_Clicked");
        }
    }

    private void BalanceAscSort_Clicked(object sender, EventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("Info", out var Info);
            Application.Current.Resources.TryGetValue("White", out var White);

            if (BalanceAscSort.Style == (Style)buttonUnclicked)
            {
                BalanceAscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
                BalanceAscSort.Style = (Style)buttonClicked;

                AscSort.Style = (Style)buttonUnclicked;
                AscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                DesSort.Style = (Style)buttonUnclicked;
                DesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                BalanceDesSort.Style = (Style)buttonUnclicked;
                BalanceDesSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }
            else
            {
                BalanceAscSort.Style = (Style)buttonUnclicked;
                BalanceAscSort.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectSavingCategory", "BalanceAscSort_Clicked");
        }
    }

    private void acrSavingCategories_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!SavingCategories.IsVisible)
            {
                SavingCategories.IsVisible = true;
                SavingCategoriesIcon.Glyph = "\ue5cf";
            }
            else
            {
                SavingCategories.IsVisible = false;
                SavingCategoriesIcon.Glyph = "\ue5ce";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectSavingCategory", "ChangeSelectedProfilePic");
        }
    }

    private void acrFilterOption_Tapped(object sender, TappedEventArgs e)
    {

        try
        {
            if (!FilterOption.IsVisible)
            {
                FilterOption.IsVisible = true;
                FilterOptionIcon.Glyph = "\ue5cf";
            }
            else
            {
                FilterOption.IsVisible = false;
                FilterOptionIcon.Glyph = "\ue5ce";
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "SelectSavingCategory", "ChangeSelectedProfilePic");
        }
    }
}