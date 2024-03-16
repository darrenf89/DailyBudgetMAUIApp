using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui.Layouts;
using DailyBudgetMAUIApp.Helpers;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class EditBudgetSettingsBottomSheet : BottomSheet
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }
    public Categories Category { get; set; }
    public Dictionary<string, string> Icons = new Dictionary<string, string>();
    private Dictionary<string, Button> IconsButtons = new Dictionary<string, Button>();
    public string SelectedIcon { get; set; }
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public EditBudgetSettingsBottomSheet(Categories Category, IProductTools pt, IRestDataService ds)
	{
		InitializeComponent();

        BindingContext = this;
        _pt = pt;
        _ds = ds;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;
        MainScrollView.MaximumHeightRequest = ScreenHeight - 280;

        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, AbsoluteLayout.AutoSize));
        MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));

        lblTitle.Text = $"Edit category";

        this.Category = Category;
        entCategoryName.Text = this.Category.CategoryName;
        entIconSearch.Text = this.Category.CategoryIcon;
        this.SelectedIcon = this.Category.CategoryIcon;

        this.PropertyChanged += ViewTransactionFilterBottomSheet_PropertyChanged;

        MaterialDesignIconsFonts obj = new MaterialDesignIconsFonts();

        Icons = _pt.GetIcons().Result;
        FillIconFlexLayout(SelectedIcon);
        ToggleIconButtons(SelectedIcon);
    }

    private void ViewTransactionFilterBottomSheet_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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


        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        await Task.Delay(500);

        List<PatchDoc> patchDoc = new List<PatchDoc>();

        PatchDoc CategoryName = new PatchDoc
        {
            op = "replace",
            path = "/CategoryName",
            value = entCategoryName.Text
        };
        patchDoc.Add(CategoryName);

        PatchDoc CategoryIcon = new PatchDoc
        {
            op = "replace",
            path = "/CategoryIcon",
            value = SelectedIcon
        };

        patchDoc.Add(CategoryIcon);

        string result = await _ds.PatchCategory(Category.CategoryID, patchDoc);

        entCategoryName.IsEnabled = false;
        entCategoryName.IsEnabled = true;
        entIconSearch.IsEnabled = false;
        entIconSearch.IsEnabled = true;
       
        await Shell.Current.GoToAsync($"{nameof(ViewCategories)}");

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
        FillIconFlexLayout(e.NewTextValue);
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
                WidthRequest = 30
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
}