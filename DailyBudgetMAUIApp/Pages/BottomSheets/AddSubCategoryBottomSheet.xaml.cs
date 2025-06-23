using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using The49.Maui.BottomSheet;
using Microsoft.Maui.Layouts;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class AddSubCategoryBottomSheet : BottomSheet
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }
    public Categories Category { get; set; }
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    private readonly IPopupService _ps;

    public AddSubCategoryBottomSheet(Categories Category, IProductTools pt, IRestDataService ds, IPopupService ps)
	{
		InitializeComponent();

        BindingContext = this;
        _pt = pt;
        _ds = ds;
        _ps = ps;

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
            _pt.HandleException(ex, "AddSubCategoryBottomSheet", "ViewTransactionFilterBottomSheet_PropertyChanged");
        }
    }

    private async void AddCategory_Clicked(object sender, EventArgs e)
    {
        try
        {
            validator.IsVisible = false;
            lblValidator.Text = "";

            if (string.IsNullOrEmpty(entCategoryName.Text))
            {
                validator.IsVisible = true;
                lblValidator.Text = "You have to give the category a name";
                return;
            }

            if(!App.IsPopupShowing){App.IsPopupShowing = true;_ps.ShowPopup<PopUpPage>(Application.Current.Windows[0].Page, options: new PopupOptions{CanBeDismissedByTappingOutsideOfPopup = false,PageOverlayColor = Color.FromArgb("#80000000")});}

            await Task.Delay(500);

            Categories NewCat = new Categories
            {
                CategoryGroupID = Category.CategoryGroupID,
                IsSubCategory = true,
                CategoryName = entCategoryName.Text
            };

            NewCat = await _ds.AddNewSubCategory(App.DefaultBudgetID, NewCat);

            entCategoryName.IsEnabled = false;
            entCategoryName.IsEnabled = true;

            try
            {
                ViewCategory CurrentPage = (ViewCategory)Shell.Current.CurrentPage;
                CurrentPage.AddCategory = NewCat;
            } 
            catch (Exception) 
            {
                if (App.IsPopupShowing) { App.IsPopupShowing = false; await _ps.ClosePopupAsync(Shell.Current); }
            }

            if (App.CurrentBottomSheet != null)
            {
                await this.DismissAsync();
                App.CurrentBottomSheet = null;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "AddSubCategoryBottomSheet", "AddCategory_Clicked");
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