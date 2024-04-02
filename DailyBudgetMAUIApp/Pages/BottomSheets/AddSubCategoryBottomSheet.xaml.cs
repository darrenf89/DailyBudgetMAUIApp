using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui.Layouts;
using DailyBudgetMAUIApp.Helpers;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Handlers;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class AddSubCategoryBottomSheet : BottomSheet
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }
    public Categories Category { get; set; }
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public AddSubCategoryBottomSheet(Categories Category, IProductTools pt, IRestDataService ds)
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

        lblTitle.Text = $"Add new category";

        this.Category = Category;

        this.PropertyChanged += ViewTransactionFilterBottomSheet_PropertyChanged;
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

        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        await Task.Delay(500);

        Categories NewCat = new Categories
        {
            CategoryGroupID = Category.CategoryGroupID,
            IsSubCategory = true,
            CategoryName = entCategoryName.Text
        };

        NewCat = _ds.AddNewSubCategory(App.DefaultBudgetID, NewCat).Result;

        entCategoryName.IsEnabled = false;
        entCategoryName.IsEnabled = true;

        ViewCategory CurrentPage = (ViewCategory)Shell.Current.CurrentPage;
        CurrentPage.AddCategory = NewCat;

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
    
}