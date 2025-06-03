using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui.Layouts;


namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class ViewTransactionFilterBottomSheet : BottomSheet
{
    public int _totalFilters;
    public int TotalFilters
    {
        get => _totalFilters;
        set
        {
            if (_totalFilters != value)
            {
                _totalFilters = value;
                OnPropertyChanged();
            }
        }
    }

    public int _selectedFilters;
    public int SelectedFilters
    {
        get => _selectedFilters;
        set
        {
            if (_selectedFilters != value)
            {
                _selectedFilters = value;
                OnPropertyChanged();
            }
        }
    }

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }
    public FilterModel Filter { get; set; }
    private List<string> Payees { get; set; } = new List<string>();
    private List<Savings> Savings { get; set; } = new List<Savings>();
    private List<Categories> Categories { get; set; } = new List<Categories>();
    private List<string> EventTypes { get; set; } = new List<string>();
    private List<string> SelectedPayees { get; set; } = new List<string>();
    private List<int> SelectedSavings { get; set; } = new List<int>();
    private List<int> SelectedCategories { get; set; } = new List<int>();
    private List<string> SelectedEventTypes { get; set; } = new List<string>();

    private Dictionary<string, Button> PayeeFilterButtons = new Dictionary<string, Button>();
    private Dictionary<int, Button> CatFilterButtons = new Dictionary<int, Button>();
    private Dictionary<int, Button> SavingFilterButtons = new Dictionary<int, Button>();
    private Dictionary<string, Button> EventTypeFilterButtons = new Dictionary<string, Button>();

    private readonly IProductTools _pt;

    public ViewTransactionFilterBottomSheet(FilterModel filter, IProductTools pt)
    {
        InitializeComponent();
        _pt = pt;

        BindingContext = this;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;

        MainScrollView.MaximumHeightRequest = ScreenHeight - App.StatusBarHeight - App.NavBarHeight - 201;
        MainAbs.HeightRequest = ScreenHeight - App.StatusBarHeight;

        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, AbsoluteLayout.AutoSize));
        MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));

        lblTitle.Text = $"Filters";
        Filter = filter;
        this.PropertyChanged += ViewTransactionFilterBottomSheet_PropertyChanged;
        Loaded += ViewTransactionFilterBottomSheet_Loaded;

    }

    private async void ViewTransactionFilterBottomSheet_Loaded(object sender, EventArgs e)
    {
        try
        {
            await FillSelectedFilterLists();

            await LoadFilters();

            pckFromDate.Date = DateTime.UtcNow.AddYears(-1);
            pckToDate.Date = DateTime.Now;
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "ViewTransactionFilterBottomSheet_Loaded");
        }

    }

    private void ViewTransactionFilterBottomSheet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        try
        {
            string PropertyChange = (string)e.PropertyName;
            if(PropertyChange == "SelectedDetent")
            {
                double Height = this.Height;

                BottomSheet Sender = (BottomSheet)sender;

                if (Sender.SelectedDetent is FullscreenDetent)
                {
                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.None);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, Height - 60, ScreenWidth, AbsoluteLayout.AutoSize));
                }
                else if(Sender.SelectedDetent is MediumDetent)
                {
                    MediumDetent detent = (MediumDetent)Sender.SelectedDetent;

                    double NewHeight = (Height * detent.Ratio) - 60;

                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.None);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, NewHeight, ScreenWidth, AbsoluteLayout.AutoSize));
                }
                else if(Sender.SelectedDetent is FixedContentDetent)
                {
                    MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
                    MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));
                }

            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "ViewTransactionFilterBottomSheet_PropertyChanged");
        }

    }

    private async Task LoadFilters()
    {
        IRestDataService ds = IPlatformApplication.Current.Services.GetService<IRestDataService>();

        TotalFilters = 2;
        SelectedFilters = 2;

        Payees = await ds.GetPayeeList(App.DefaultBudgetID);
        CreatePayeeButtons();
        TotalFilters += Payees.Count();

        Categories = await ds.GetCategories(App.DefaultBudgetID);
        CreateCategoryButtons();
        TotalFilters += Categories.Where(c => c.IsSubCategory).Count();

        Savings = await ds.GetAllBudgetSavings(App.DefaultBudgetID);
        CreateSavingsButtons();
        TotalFilters += Savings.Count();

        EventTypes = await ds.GetBudgetEventTypes(App.DefaultBudgetID);
        CreateEventTypeButtons();
        TotalFilters += EventTypes.Count();
    }

    private async Task FillSelectedFilterLists()
    {

        await Task.Delay(1);
        if(Filter != null)
        {
            if(Filter.DateFilter == null)
            {
                chbDateRange.IsChecked = false;
            }

            if(Filter.PayeeFilter != null)
            {
                SelectedPayees.AddRange(Filter.PayeeFilter);
                if(SelectedPayees.Count() == 0)
                {
                    chbPayee.IsChecked = false;
                }
                SelectedFilters += SelectedPayees.Count();
            }
            else
            {
                chbPayee.IsChecked = false;
            }

            if(Filter.CategoryFilter != null)
            {
                SelectedCategories.AddRange(Filter.CategoryFilter);
                if (SelectedCategories.Count() == 0)
                {
                    chbCategories.IsChecked = false;
                }
                SelectedFilters += SelectedCategories.Count();
            }
            else
            {
                chbCategories.IsChecked = false;
            }

            if (Filter.SavingFilter != null)
            {
                SelectedSavings.AddRange(Filter.SavingFilter);
                if (SelectedSavings.Count() == 0)
                {
                    chbSavings.IsChecked = false;
                }
                SelectedFilters += SelectedSavings.Count();
            }
            else
            {
                chbSavings.IsChecked = false;
            }

            if (Filter.TransactionEventTypeFilter != null)
            {
                SelectedEventTypes.AddRange(Filter.TransactionEventTypeFilter);
                if (SelectedEventTypes.Count() == 0)
                {
                    chbEventTypes.IsChecked = false;
                }
                SelectedFilters += SelectedEventTypes.Count();
            }
            else
            {
                chbEventTypes.IsChecked = false;
            }
        }
    }

    private void CreateEventTypeButtons()
    {
        EventTypesFlex.Children.Clear();

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        foreach (string EventType in EventTypes)
        {
            Button FilterButton = new Button
            {
                Text = EventType
            };

            if (SelectedEventTypes.Contains(EventType))
            {
                FilterButton.Style = (Style)buttonClicked;
                FilterButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
            }
            else
            {
                FilterButton.Style = (Style)buttonUnclicked;
                FilterButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }

            FilterButton.Clicked += (s, e) => {
                try
                {
                    ToggleEventTypesFilterButtons(EventType);
                }
                catch (Exception ex)
                {
                    _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "FilterButton.Clicked");
                }
            }; 

            EventTypesFlex.Children.Add(FilterButton);

            EventTypeFilterButtons.Add(EventType, FilterButton);
        }
    }

    private void ToggleEventTypesFilterButtons(string EventType)
    {
        Button SelectedEventTypesFilter = EventTypeFilterButtons[EventType];

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        if (SelectedEventTypesFilter.Style == (Style)buttonUnclicked)
        {
            SelectedEventTypesFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue876",
                Size = 15,
                Color = (Color)White
            };
            SelectedEventTypesFilter.Style = (Style)buttonClicked;

            SelectedEventTypes.Add(EventType);
            SelectedFilters += 1;
        }
        else
        {
            SelectedEventTypesFilter.Style = (Style)buttonUnclicked;
            SelectedEventTypesFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue5cd",
                Size = 15,
                Color = (Color)Info
            };

            if (SelectedEventTypes.Contains(EventType))
            {
                SelectedEventTypes.Remove(EventType);
            }

            SelectedFilters -= 1;
        }
    }

    private void CreateSavingsButtons()
    {
        SavingFlex.Children.Clear();

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        foreach (Savings Saving in Savings)
        {
            Button FilterButton = new Button
            {
                Text = Saving.SavingsName
            };

            if (SelectedSavings.Contains(Saving.SavingID))
            {
                FilterButton.Style = (Style)buttonClicked;
                FilterButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
            }
            else
            {
                FilterButton.Style = (Style)buttonUnclicked;
                FilterButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }

            FilterButton.Clicked += (s, e) => {
                try
                {
                    ToggleSavingsFilterButtons(Saving.SavingID);
                }
                catch (Exception ex)
                {
                    _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "ToggleSavingsFilterButtons");
                }
            }; 

            SavingFlex.Children.Add(FilterButton);

            SavingFilterButtons.Add(Saving.SavingID, FilterButton);
        }
    }

    private void ToggleSavingsFilterButtons(int SavingID)
    {
        Button SelectedSavingFilter = SavingFilterButtons[SavingID];

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        if (SelectedSavingFilter.Style == (Style)buttonUnclicked)
        {
            SelectedSavingFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue876",
                Size = 15,
                Color = (Color)White
            };
            SelectedSavingFilter.Style = (Style)buttonClicked;

            SelectedSavings.Add(SavingID);
            SelectedFilters += 1;
        }
        else
        {
            SelectedSavingFilter.Style = (Style)buttonUnclicked;
            SelectedSavingFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue5cd",
                Size = 15,
                Color = (Color)Info
            };

            if (SelectedSavings.Contains(SavingID))
            {
                SelectedSavings.Remove(SavingID);
            }

            SelectedFilters -= 1;
        }
    }

    private void CreateCategoryButtons()
    {
        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);

        CategoryFlex.Children.Clear();

        foreach(Categories GroupCat in Categories.Where(c => !c.IsSubCategory).ToList())
        {
            VerticalStackLayout VSL = new VerticalStackLayout
            {
                Margin = new Thickness(0,10,0,0),
                HorizontalOptions = LayoutOptions.Fill
            };

            HorizontalStackLayout HSL = new HorizontalStackLayout();

            Image Image = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(10, 0, 5, 0),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue837",
                    Size = 15,
                    Color = (Color)Primary,
                }
            };

            HSL.Children.Add(Image);

            Label HeaderLabel = new Label
            {
                Text = GroupCat.CategoryName,
                TextColor = (Color)Primary,
                FontSize = 18,
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(0),
                Margin = new Thickness(5, 0, 0, 2)
            };

            HSL.Children.Add(HeaderLabel);


            VSL.Children.Add(HSL);

            BoxView HeaderBox = new BoxView
            {
                HeightRequest = 2,
                Color = (Color)Primary,
                Margin = new Thickness(10,0,180,10)
            };

            VSL.Children.Add(HeaderBox);

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();
            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += (s, e) =>
            {
                CategoryGroupHeader_Tapped(GroupCat.CategoryID);
            };

            VSL.GestureRecognizers.Add(TapGesture);

            CategoryFlex.Children.Add(VSL);

            FlexLayout Flex = new FlexLayout
            {
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustify.Start,
                Margin = new Thickness(0, 0, 0, 0),
                Padding = new Thickness(0)
            };

            foreach (Categories SubCat in Categories.Where(c => c.IsSubCategory && c.CategoryGroupID == GroupCat.CategoryID).ToList())
            {
                Button FilterButton = new Button
                {
                    Text = SubCat.CategoryName
                };

                if (SelectedCategories.Contains(SubCat.CategoryID))
                {
                    FilterButton.Style = (Style)buttonClicked;
                    FilterButton.ImageSource = new FontImageSource
                    {
                        FontFamily = "MaterialDesignIcons",
                        Glyph = "\ue876",
                        Size = 15,
                        Color = (Color)White
                    };
                }
                else
                {
                    FilterButton.Style = (Style)buttonUnclicked;
                    FilterButton.ImageSource = new FontImageSource
                    {
                        FontFamily = "MaterialDesignIcons",
                        Glyph = "\ue5cd",
                        Size = 15,
                        Color = (Color)Info
                    };
                }

                FilterButton.Clicked += (s, e) =>
                {
                    try
                    {
                        TogglCategoryFilterButtons(SubCat.CategoryID);
                    }
                    catch (Exception ex)
                    {
                        _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "TogglCategoryFilterButtons");
                    }

                };

                Flex.Children.Add(FilterButton);

                CatFilterButtons.Add(SubCat.CategoryID, FilterButton);
            }

            CategoryFlex.Children.Add(Flex);
        }
    }

    private void CategoryGroupHeader_Tapped(int GroupCatId)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach (KeyValuePair<int, Button> x in CatFilterButtons)
            {
                Button CatButton = x.Value;
                int CategoryID = x.Key;

                Categories? Cat = Categories.Where(c => c.CategoryID == CategoryID).FirstOrDefault();

                if(Cat.CategoryGroupID == GroupCatId)
                {
                    CatButton.ImageSource = new FontImageSource
                    {
                        FontFamily = "MaterialDesignIcons",
                        Glyph = "\ue876",
                        Size = 15,
                        Color = (Color)White
                    };
                    CatButton.Style = (Style)buttonClicked;

                    if (!SelectedCategories.Contains(CategoryID))
                    {
                        SelectedCategories.Add(CategoryID);
                        SelectedFilters += 1;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "CategoryGroupHeader_Tapped");
        }

    }

    private void TogglCategoryFilterButtons(int CatID)
    {
        Button SelectedCategoryFilter = CatFilterButtons[CatID];

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        if (SelectedCategoryFilter.Style == (Style)buttonUnclicked)
        {
            SelectedCategoryFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue876",
                Size = 15,
                Color = (Color)White
            };
            SelectedCategoryFilter.Style = (Style)buttonClicked;

            SelectedCategories.Add(CatID);
            SelectedFilters += 1;
        }
        else
        {
            SelectedCategoryFilter.Style = (Style)buttonUnclicked;
            SelectedCategoryFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue5cd",
                Size = 15,
                Color = (Color)Info
            };

            if (SelectedCategories.Contains(CatID))
            {
                SelectedCategories.Remove(CatID);
            }

            SelectedFilters -= 1;
        }
    }

    private void CreatePayeeButtons()
    {
        PayeeFlex.Children.Clear();

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        foreach (string Payee in Payees)
        {
            Button FilterButton = new Button
            {
                Text = Payee
            };

            if (SelectedPayees.Contains(Payee))
            {
                FilterButton.Style = (Style)buttonClicked;
                FilterButton.ImageSource  = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                }; 
            }
            else 
            {
                FilterButton.Style = (Style)buttonUnclicked;
                FilterButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };
            }

            FilterButton.Clicked += (s, e) =>
            {
                try
                {
                    TogglePayeeFilterButtons(Payee);
                }
                catch (Exception ex)
                {
                    _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "FilterButton.Clicked");
                }
            }; 

            PayeeFlex.Children.Add(FilterButton);

            PayeeFilterButtons.Add(Payee, FilterButton);
        }
    }

    private void TogglePayeeFilterButtons(string Payee)
    {
        Button SelectedPayeeFilter = PayeeFilterButtons[Payee];

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        if (SelectedPayeeFilter.Style == (Style)buttonUnclicked)
        {
            SelectedPayeeFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue876",
                Size = 15,
                Color = (Color)White
            };
            SelectedPayeeFilter.Style = (Style)buttonClicked;

            SelectedPayees.Add(Payee);
            SelectedFilters += 1;
        }
        else
        {
            SelectedPayeeFilter.Style = (Style)buttonUnclicked;
            SelectedPayeeFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue5cd",
                Size = 15,
                Color = (Color)Info
            };

            if (SelectedPayees.Contains(Payee))
            {
                SelectedPayees.Remove(Payee);
            }

            SelectedFilters -= 1;
        }
    }

    private async void ApplyFilter_Clicked(object sender, EventArgs e)
    {
        try
        {
            ViewTransactions CurrentPage = (ViewTransactions)Shell.Current.CurrentPage;

            FilterModel NewFilter = new FilterModel();

            if(chbDateRange.IsChecked.GetValueOrDefault())
            {
                NewFilter.DateFilter = new DateFilter
                {
                    DateFrom = pckFromDate.Date,
                    DateTo = pckToDate.Date
                };
            }
            else
            {
                NewFilter.DateFilter = null;
            }

            if (chbPayee.IsChecked.GetValueOrDefault() && SelectedPayees.Count() != 0)
            {
                NewFilter.PayeeFilter = SelectedPayees;
            }
            else
            {
                NewFilter.PayeeFilter = null;
            }

            if (chbCategories.IsChecked.GetValueOrDefault() && SelectedCategories.Count() != 0)
            {
                NewFilter.CategoryFilter = SelectedCategories;
            }
            else
            {
                NewFilter.CategoryFilter = null;
            }

            if (chbSavings.IsChecked.GetValueOrDefault() && SelectedSavings.Count() != 0)
            {
                NewFilter.SavingFilter = SelectedSavings;
            }
            else
            {
                NewFilter.SavingFilter = null;
            }

            if (chbEventTypes.IsChecked.GetValueOrDefault() && SelectedEventTypes.Count() != 0)
            {
                NewFilter.TransactionEventTypeFilter = SelectedEventTypes;
            }
            else
            {
                NewFilter.TransactionEventTypeFilter = null;
            }

            CurrentPage.Filters = NewFilter;

            if (App.CurrentBottomSheet != null)
            {
                await this.DismissAsync();
                App.CurrentBottomSheet = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "ApplyFilter_Clicked");
        }
    }

    private void ClearAllFilters_Tapped(object sender, TappedEventArgs e)
    {
        chbDateRange.IsChecked = false;
        chbPayee.IsChecked = false;
        chbSavings.IsChecked = false;
        chbEventTypes.IsChecked = false;
        chbCategories.IsChecked = false;
    }

    private void acrDateRange_Tapped(object sender, TappedEventArgs e)
    {
        if (DateRangeDetails.IsVisible)
        {
            chbDateRange.IsChecked = false;
        }
        else
        {
            chbDateRange.IsChecked = true;
        }
    }

    private void chbDateRange_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
    {
        try
        {

            if (chbDateRange.IsChecked.GetValueOrDefault())
            {
                DateRangeDetails.IsVisible = true;
                DateRangeFilter.Glyph = "\ue5cf";
                SelectedFilters += 2;
            }
            else
            {
                DateRangeDetails.IsVisible = false;
                DateRangeFilter.Glyph = "\ue5ce";
                SelectedFilters -= 2;
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "chbDateRange_StateChanged");
        }

    }

    private void acrPayee_Tapped(object sender, TappedEventArgs e)
    {
        if (PayeeDetails.IsVisible)
        {
            chbPayee.IsChecked = false;
        }
        else
        {
            chbPayee.IsChecked = true;
        }
    }

    private void chbPayee_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
    {
        try
        {
            if (chbPayee.IsChecked.GetValueOrDefault())
            {
                PayeeDetails.IsVisible = true;
                PayeeFilter.Glyph = "\ue5cf";
            }
            else
            {
                PayeeDetails.IsVisible = false;
                PayeeFilter.Glyph = "\ue5ce";

                PayeeDeselectAll_Tapped(null, null);
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "chbPayee_StateChanged");
        }

    }

    private void PayeeDeselectAll_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("Info", out var Info);


            foreach (KeyValuePair<string, Button> x in PayeeFilterButtons)
            {
                Button PayeeButton = x.Value;
                string Payee = x.Key;

                PayeeButton.Style = (Style)buttonUnclicked;
                PayeeButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                if (SelectedPayees.Contains(Payee))
                {
                    SelectedPayees.Remove(Payee);
                    SelectedFilters -= 1;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "PayeeDeselectAll_Tapped");
        }

    }

    private void PayeeSelectAll_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach (KeyValuePair<string, Button> x in PayeeFilterButtons)
            {
                Button PayeeButton = x.Value;
                string Payee = x.Key;

                PayeeButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
                PayeeButton.Style = (Style)buttonClicked;

                if (!SelectedPayees.Contains(Payee))
                {
                    SelectedPayees.Add(Payee);
                    SelectedFilters += 1;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "PayeeSelectAll_Tapped");
        }

    }

    private void chbCategories_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
    {
        if (chbCategories.IsChecked.GetValueOrDefault())
        {
            CategoryDetails.IsVisible = true;
            CategoryFilter.Glyph = "\ue5cf";
        }
        else
        {
            CategoryDetails.IsVisible = false;
            CategoryFilter.Glyph = "\ue5ce";

            CategoryDeselectAll_Tapped(null, null);
        }
    }

    private void acrCategory_Tapped(object sender, TappedEventArgs e)
    {
        if (CategoryDetails.IsVisible)
        {
            chbCategories.IsChecked = false;
        }
        else
        {
            chbCategories.IsChecked = true;
        }
    }

    private void CategoryDeselectAll_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("Info", out var Info);


            foreach (KeyValuePair<int, Button> x in CatFilterButtons)
            {
                Button CatButton = x.Value;
                int Category = x.Key;

                CatButton.Style = (Style)buttonUnclicked;
                CatButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                if (SelectedCategories.Contains(Category))
                {
                    SelectedCategories.Remove(Category);
                    SelectedFilters -= 1;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "CategoryDeselectAll_Tapped");
        }

    }

    private void CategorySelectAll_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach (KeyValuePair<int, Button> x in CatFilterButtons)
            {
                Button CatButton = x.Value;
                int Category = x.Key;

                CatButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
                CatButton.Style = (Style)buttonClicked;

                if (!SelectedCategories.Contains(Category))
                {
                    SelectedCategories.Add(Category);
                    SelectedFilters += 1;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "CategorySelectAll_Tapped");
        }

    }

    private void chbSavings_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
    {
        try
        {
            if (chbSavings.IsChecked.GetValueOrDefault())
            {
                SavingDetails.IsVisible = true;
                SavingFilter.Glyph = "\ue5cf";
            }
            else
            {
                SavingDetails.IsVisible = false;
                SavingFilter.Glyph = "\ue5ce";

                SavingDeselectAll_Tapped(null, null);
                }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "chbSavings_StateChanged");
        }
    }

    private void acrSaving_Tapped(object sender, TappedEventArgs e)
    {
        if (SavingDetails.IsVisible)
        {
            chbSavings.IsChecked = false;
        }
        else
        {
            chbSavings.IsChecked = true;
        }
    }

    private void SavingDeselectAll_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("Info", out var Info);


            foreach (KeyValuePair<int, Button> x in SavingFilterButtons)
            {
                Button SavingButton = x.Value;
                int SavingID = x.Key;

                SavingButton.Style = (Style)buttonUnclicked;
                SavingButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                if (SelectedSavings.Contains(SavingID))
                {
                    SelectedSavings.Remove(SavingID);
                    SelectedFilters -= 1;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "SavingDeselectAll_Tapped");
        }
    }

    private void SavingSelectAll_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach (KeyValuePair<int, Button> x in SavingFilterButtons)
            {
                Button SavingButton = x.Value;
                int SavingID = x.Key;

                SavingButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
                SavingButton.Style = (Style)buttonClicked;

                if (!SelectedSavings.Contains(SavingID))
                {
                    SelectedSavings.Add(SavingID);
                    SelectedFilters += 1;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "SavingSelectAll_Tapped");
        }

    }

    private void chbEventTypes_StateChanged(object sender, Syncfusion.Maui.Buttons.StateChangedEventArgs e)
    {
        try
        {
            if (chbEventTypes.IsChecked.GetValueOrDefault())
            {
                EventTypesDetails.IsVisible = true;
                EventTypesFilter.Glyph = "\ue5cf";
            }
            else
            {
                EventTypesDetails.IsVisible = false;
                EventTypesFilter.Glyph = "\ue5ce";

                EventTypesDeselectAll_Tapped(null, null);
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "chbEventTypes_StateChanged");
        }
    }

    private void acrEventTypes_Tapped(object sender, TappedEventArgs e)
    {
        if (EventTypesDetails.IsVisible)
        {
            chbEventTypes.IsChecked = false;
        }
        else
        {
            chbEventTypes.IsChecked = true;
        }
    }

    private void EventTypesDeselectAll_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
            Application.Current.Resources.TryGetValue("Info", out var Info);


            foreach (KeyValuePair<string, Button> x in EventTypeFilterButtons)
            {
                Button EventTypesButton = x.Value;
                string EventType = x.Key;

                EventTypesButton.Style = (Style)buttonUnclicked;
                EventTypesButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5cd",
                    Size = 15,
                    Color = (Color)Info
                };

                if (SelectedEventTypes.Contains(EventType))
                {
                    SelectedEventTypes.Remove(EventType);
                    SelectedFilters -= 1;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "EventTypesDeselectAll_Tapped");
        }

    }

    private void EventTypesSelectAll_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach (KeyValuePair<string, Button> x in EventTypeFilterButtons)
            {
                Button EventTypesButton = x.Value;
                string EventType = x.Key;

                EventTypesButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue876",
                    Size = 15,
                    Color = (Color)White
                };
                EventTypesButton.Style = (Style)buttonClicked;

                if (!SelectedEventTypes.Contains(EventType))
                {
                    SelectedEventTypes.Add(EventType);
                    SelectedFilters += 1;
                }
            }
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "ViewTransactionFilterBottomSheet", "EventTypesSelectAll_Tapped");
        }
    }
}