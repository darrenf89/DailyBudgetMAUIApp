using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Expander;
using System;
using System.Globalization;
using System.Xml.XPath;

namespace DailyBudgetMAUIApp.Pages;

public partial class SelectSavingCategoryPage : ContentPage
{
	private readonly IRestDataService _ds;
	private readonly IProductTools _pt;
	private readonly SelectSavingCategoryPageViewModel _vm;


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

        _vm.EnvelopeSavingList = _ds.GetBudgetEnvelopeSaving(_vm.BudgetID).Result;

        decimal Total = 0;
        decimal Balance = 0;

        foreach(Savings Saving in _vm.EnvelopeSavingList)
        {
            Total += Saving.PeriodSavingValue.GetValueOrDefault();
            Balance += Saving.CurrentBalance.GetValueOrDefault();
        }

        entTotal.Text = Total.ToString("c", CultureInfo.CurrentCulture);
        entBalanceRemaining.Text = Balance.ToString("c", CultureInfo.CurrentCulture);

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
    }

    async protected override void OnAppearing()
    {
       base.OnAppearing();
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
                    BackgroundColor = (Color)PrimaryLight,
                    Source = new FontImageSource                            
                    {
                        FontFamily = "MaterialDesignIcons",
                        Glyph = SavingImageGlyph,
                        Size = 20,
                        Color = (Color)PrimaryDark
                    }
                };

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

                SavingHSL.Add(SavingImage,0,0);
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

        Savings Saving = (Savings)e.Parameter;

        bool result = await DisplayAlert($"Select {Saving.SavingsName}", $"Are you sure you want to spend from your {Saving.SavingsName} envelope?", "Yes, continue", "No, go back!");
        if (result)
        {
            _vm.Transaction.SavingName = Saving.SavingsName;
            _vm.Transaction.SavingID = Saving.SavingID;
            _vm.Transaction.SavingsSpendType = "EnvelopeSaving";
            _vm.Transaction.EventType = "Envelope";

            await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}",
                new Dictionary<string, object>
                {
                    ["Transaction"] = _vm.Transaction
                });
        }
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        _vm.Transaction.SavingName = "";
        _vm.Transaction.SavingID = 0;
        _vm.Transaction.SavingsSpendType = "";
        _vm.Transaction.EventType = "Transaction";

        await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}",
        new Dictionary<string, object>
        {
            ["Transaction"] = _vm.Transaction
        });
    }

    private async void ShowHideSortFiler_Tapped(object sender, TappedEventArgs e)
    {
        if(FilterHidden.IsVisible)
        {
            FilterHidden.IsVisible = false;
            FilterShown.IsVisible = true;

            FilterOptions.IsVisible = true;
            FilterOptions.HeightRequest = 0;
            var animation = new Animation(v => FilterOptions.HeightRequest = v, 0, _vm.SortFilterHeight);
            animation.Commit(this, "FilterOptionsShow", 16, 300, Easing.CubicOut);
        }
        else
        {
            FilterHidden.IsVisible = true;
            FilterShown.IsVisible = false;

            var animation = new Animation(v => FilterOptions.HeightRequest = v, _vm.SortFilterHeight, 0);
            animation.Commit(this, "FilterOptionsHide", 16, 300, Easing.CubicOut, (v,c) =>
            {
                FilterOptions.IsVisible = false;
            });

        }
    }

    private void AscSort_Clicked(object sender, EventArgs e)
    {
        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);

        if (AscSort.Style == (Style)buttonUnclicked)
        {
            AscSort.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue876",
                Size = 15,
                Color = (Color)Info
            };
            AscSort.Style = (Style)buttonClicked;

            DesSort.Style = (Style)buttonUnclicked;
            DesSort.ImageSource = null;

            BalanceAscSort.Style = (Style)buttonUnclicked;
            BalanceAscSort.ImageSource = null;

            BalanceDesSort.Style = (Style)buttonUnclicked;
            BalanceDesSort.ImageSource = null;
        }
        else
        {
            AscSort.Style = (Style)buttonUnclicked;
            AscSort.ImageSource = null;
        }
    }

    private void DesSort_Clicked(object sender, EventArgs e)
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
                Color = (Color)Info
            };
            DesSort.Style = (Style)buttonClicked;

            AscSort.Style = (Style)buttonUnclicked;
            AscSort.ImageSource = null;

            BalanceAscSort.Style = (Style)buttonUnclicked;
            BalanceAscSort.ImageSource = null;

            BalanceDesSort.Style = (Style)buttonUnclicked;
            BalanceDesSort.ImageSource = null;
        }
        else
        {
            DesSort.Style = (Style)buttonUnclicked;
            DesSort.ImageSource = null;
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
        entEnvelopeSearch.IsEnabled = false;
        entEnvelopeSearch.IsEnabled = true;

        _vm.EnvelopeFilteredSavingList = _vm.EnvelopeSavingList;

        var LoadingPage = new LoadingPageTwo();
        await Application.Current.MainPage.Navigation.PushModalAsync(LoadingPage, true);

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

        FilterHidden.IsVisible = true;
        FilterShown.IsVisible = false;

        var animation = new Animation(v => FilterOptions.HeightRequest = v, _vm.SortFilterHeight, 0);
        animation.Commit(this, "FilterOptionsHide", 16, 300, Easing.CubicOut, (v, c) =>
        {
            FilterOptions.IsVisible = false;
        });

        await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    private async void ClearAllFilter_Tapped(object sender, TappedEventArgs e)
    {
        var LoadingPage = new LoadingPageTwo();
        await Application.Current.MainPage.Navigation.PushModalAsync(LoadingPage, true);

        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);

        DesSort.Style = (Style)buttonUnclicked;
        DesSort.ImageSource = null;
        AscSort.Style = (Style)buttonUnclicked;
        AscSort.ImageSource = null;
        AscSort.Style = (Style)buttonUnclicked;
        AscSort.ImageSource = null;

        entEnvelopeSearch.Text = "";

        _vm.EnvelopeFilteredSavingList = _vm.EnvelopeSavingList;

        LoadSavingList();        

        FilterHidden.IsVisible = true;
        FilterShown.IsVisible = false;

        var animation = new Animation(v => FilterOptions.HeightRequest = v, _vm.SortFilterHeight, 0);
        animation.Commit(this, "FilterOptionsHide", 16, 300, Easing.CubicOut, (v, c) =>
        {
            FilterOptions.IsVisible = false;
        });

        await Application.Current.MainPage.Navigation.PopModalAsync();
    }

    private void BalanceDesSort_Clicked(object sender, EventArgs e)
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
                Color = (Color)Info
            };
            BalanceDesSort.Style = (Style)buttonClicked;

            AscSort.Style = (Style)buttonUnclicked;
            AscSort.ImageSource = null;

            DesSort.Style = (Style)buttonUnclicked;
            DesSort.ImageSource = null;

            BalanceAscSort.Style = (Style)buttonUnclicked;
            BalanceAscSort.ImageSource = null;
        }
        else
        {
            BalanceDesSort.Style = (Style)buttonUnclicked;
            BalanceDesSort.ImageSource = null;
        }
    }

    private void BalanceAscSort_Clicked(object sender, EventArgs e)
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
                Color = (Color)Info
            };
            BalanceAscSort.Style = (Style)buttonClicked;

            AscSort.Style = (Style)buttonUnclicked;
            AscSort.ImageSource = null;

            DesSort.Style = (Style)buttonUnclicked;
            DesSort.ImageSource = null;

            BalanceDesSort.Style = (Style)buttonUnclicked;
            BalanceDesSort.ImageSource = null;
        }
        else
        {
            BalanceAscSort.Style = (Style)buttonUnclicked;
            BalanceAscSort.ImageSource = null;
        }
    }
}