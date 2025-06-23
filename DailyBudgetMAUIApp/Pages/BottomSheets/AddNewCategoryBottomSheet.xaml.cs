using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui.Layouts;
using System.Collections.ObjectModel;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Helpers;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class AddNewCategoryBottomSheet : BottomSheet
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }
    public List<Categories> Categories { get; set; } = new List<Categories>();
    public Dictionary<string, string> Icons = new Dictionary<string, string>();
    private Dictionary<string, Button> IconsButtons = new Dictionary<string, Button>();
    public List<string> SubCategories = new List<string>();
    public string SelectedIcon { get; set; }
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public AddNewCategoryBottomSheet(ObservableCollection<Categories> Categories, IProductTools pt, IRestDataService ds)
	{
		InitializeComponent();

        BindingContext = this;
        _pt = pt;
        _ds = ds;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;

        MainScrollView.MaximumHeightRequest = ScreenHeight - App.NavBarHeight - App.StatusBarHeight - 80;
        MainAbs.HeightRequest = ScreenHeight - App.NavBarHeight - App.StatusBarHeight;

        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, AbsoluteLayout.AutoSize));
        MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));

        lblTitle.Text = $"Create a new category";

        this.Categories.Clear();
        if (Categories != null)
        {
            this.Categories.AddRange(Categories);
        }

        this.PropertyChanged += ViewTransactionFilterBottomSheet_PropertyChanged;

        MaterialDesignIconsFonts obj = new MaterialDesignIconsFonts();
        try
        {
            Icons = _pt.GetIcons().Result;
            FillIconFlexLayout("");
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddNewCategoryBottomSheet", "AddNewCategoryBottomSheet");
        }
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
            _pt.HandleException(ex, "AddNewCategoryBottomSheet", "ViewTransactionFilterBottomSheet_PropertyChanged");
        }
    }

    private async void AddCategory_Clicked(object sender, EventArgs e)
    {
        validator.IsVisible = false;
        lblValidator.Text = "";

        if (string.IsNullOrEmpty(entCategoryName.Text))
        {
            validator.IsVisible = true;
            lblValidator.Text = "You have to give the category a name";
            return;
        }

        if (string.IsNullOrEmpty(SelectedIcon))
        {
            validator.IsVisible = true;
            lblValidator.Text = "You have to give the category an icon";
            return;
        }

        if (SubCategories.Count == 0)
        {
            validator.IsVisible = true;
            lblValidator.Text = "You have to add at least one sub category";
            return;
        }

        DefaultCategories cat = new DefaultCategories
        {
            CatName = entCategoryName.Text,
            CategoryIcon = SelectedIcon,
            SubCategories = new List<SubCategories>()
        };

        foreach(string s in SubCategories)
        {
            SubCategories SubCat = new SubCategories
            {
                SubCatName = s
            };

            cat.SubCategories.Add(SubCat);
        }

        int CategoryID = await _ds.AddNewCategory(App.DefaultBudgetID, cat);

        if(Categories.Count > 0)
        {
            Categories NewCat = new Categories
            {
                CategoryIcon = SelectedIcon,
                CategoryName = entCategoryName.Text,
                CategoryGroupID = CategoryID,
                CategoryID = CategoryID,
                CategorySpendPayPeriod = 0,
                CategorySpendAllTime = 0
            };

            Categories? AddNewCat = Categories.Where(c => c.CategoryID == -1).FirstOrDefault();

            if (Categories.Contains(AddNewCat))
            {
                Categories.Remove(AddNewCat);
            }

            Categories.Add(NewCat);
            Categories.Add(AddNewCat);

            ViewCategories CurrentPage = (ViewCategories)Shell.Current.CurrentPage;
            CurrentPage.AddCategoryList.Clear();
            //CurrentPage.AddCategoryList = new List<Categories>();
            CurrentPage.AddCategoryList = Categories;
        }        

        if (App.CurrentBottomSheet != null)
        {
            await this.DismissAsync();
            App.CurrentBottomSheet = null;
        }

    }

    private void acrCategoryName_Tapped(object sender, TappedEventArgs e)
    {
        if (!CategoryName.IsVisible)
        {
            CategoryName.IsVisible = true;
            CategoryNameIcon.Glyph = "\ue5cf";
        }
        else
        {
            CategoryName.IsVisible = false;
            CategoryNameIcon.Glyph = "\ue5ce";
        }
    }

    private void acrCategoryIcon_Tapped(object sender, TappedEventArgs e)
    {
        if (!CategoryIcon.IsVisible)
        {
            CategoryIcon.IsVisible = true;
            CategoryIconIcon.Glyph = "\ue5cf";
        }
        else
        {
            CategoryIcon.IsVisible = false;
            CategoryIconIcon.Glyph = "\ue5ce";
        }
    }

    private void entIconSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            FillIconFlexLayout(e.NewTextValue);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "AddNewCategoryBottomSheet", "entIconSearch_TextChanged");
        }
    }

    private void FillIconFlexLayout(string search)
    {
        Dictionary<string, string> FilteredIcons = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(search))
        {
            FilteredIcons = Icons.Take(20).ToDictionary();
        }
        else
        {
            FilteredIcons =  Icons.Where(i => i.Key.Contains(search,StringComparison.OrdinalIgnoreCase)).Take(20).ToDictionary();            
        }

        flxIcons.Children.Clear();
        IconsButtons.Clear();

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        foreach (var icon in FilteredIcons)
        {

            Button FilterButton = new Button
            {
                HeightRequest = 30,
                WidthRequest = 30,
                Padding = new Thickness(0)
            };

            FilterButton.Style = (Style)buttonUnclicked;
            FilterButton.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = icon.Value,
                Size = 22,
                Color = (Color)Info
            };

            FilterButton.Clicked += (s, e) => ToggleIconButtons(icon.Key);

            flxIcons.Children.Add(FilterButton);
            IconsButtons.Add(icon.Key, FilterButton);

        }
    }

    private void ToggleIconButtons(string Icon)
    {
        validator.IsVisible = false;
        lblValidator.Text = "";

        Application.Current.Resources.TryGetValue("buttonClicked", out var buttonClicked);
        Application.Current.Resources.TryGetValue("buttonUnclicked", out var buttonUnclicked);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("White", out var White);

        Button SelectedIconFilter = IconsButtons[Icon];
        var Glyph = Icons[Icon];

        foreach (var Button in IconsButtons)
        {
            
            Button IconButton = Button.Value;
            var ButtonGlyph = Icons[Button.Key];

            if(Button.Key != Icon)
            {
                IconButton.Style = (Style)buttonUnclicked;
                IconButton.ImageSource = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = ButtonGlyph,
                    Size = 22,
                    Color = (Color)Info
                };
            }
        }

        if (SelectedIconFilter.Style == (Style)buttonUnclicked)
        {
            SelectedIconFilter.Style = (Style)buttonClicked;

            SelectedIconFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = Glyph,
                Size = 22,
                Color = (Color)White
            };

            SelectedIcon = Icon;

        }
        else
        {
            SelectedIconFilter.Style = (Style)buttonUnclicked;

            SelectedIconFilter.ImageSource = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = Glyph,
                Size = 22,
                Color = (Color)Info
            };

            SelectedIcon = "";
        }
    }

    private void acrSubCategoryIcon_Tapped(object sender, TappedEventArgs e)
    {
        if (!SubCategory.IsVisible)
        {
            SubCategory.IsVisible = true;
            SubCategoryIcon.Glyph = "\ue5cf";
        }
        else
        {
            SubCategory.IsVisible = false;
            SubCategoryIcon.Glyph = "\ue5ce";
        }
    }

    private async void AddSubCat_Clicked(object sender, EventArgs e)
    {
        try
        {
            validator.IsVisible = false;
            lblValidator.Text = "";

            if (string.IsNullOrEmpty(entSubCategory.Text))
            {
                validator.IsVisible = true;
                lblValidator.Text = "You have to give the sub category a name";
                return;
            }

            bool IsAddSub = await Application.Current.Windows[0].Page.DisplayAlert($"Add new sub category?", $"Are you sure you want to add {entSubCategory.Text} as a sub category?", "Yes", "No");
            if (IsAddSub)
            {
                Application.Current.Resources.TryGetValue("StandardInputBorderOptionSelect", out var StandardInputBorderOptionSelect);
                Application.Current.Resources.TryGetValue("Primary", out var Primary);
                Application.Current.Resources.TryGetValue("White", out var White);
                Application.Current.Resources.TryGetValue("Info", out var Info);

                Border border = new Border
                {
                    Style = (Style)StandardInputBorderOptionSelect,
                    Stroke = (Color)Info,
                    Margin = new Thickness(5,0,15,5)
                };

                HorizontalStackLayout hsl = new HorizontalStackLayout();

                Image image = new Image
                {
                    BackgroundColor = (Color)White,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0),
                    Source = new FontImageSource
                    {
                        FontFamily = "MaterialDesignIcons",
                        Glyph = "\ue39e",
                        Size = 20,
                        Color = (Color)Primary,
                    }
                };

                hsl.Children.Add(image);

                Label label = new Label
                {
                    Text = entSubCategory.Text,
                    TextColor = (Color)Primary,
                    FontSize = 18,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    Margin = new Thickness(20, 0, 0, 0)
                };

                hsl.Children.Add(label);

                SubCategories.Add(entSubCategory.Text);
                entSubCategory.Text = "";
                entSubCategory.IsEnabled = false;
                entSubCategory.IsEnabled = true;

                border.Content = hsl;
                vslSubCats.Children.Add(border);
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddNewCategoryBottomSheet", "AddSubCat_Clicked");
        }

    }
}