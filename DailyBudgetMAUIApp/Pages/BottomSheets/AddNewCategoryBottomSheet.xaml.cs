using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Handlers;
using The49.Maui.BottomSheet;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Layouts;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.ObjectModel;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Helpers;
using System.Reflection;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class AddNewCategoryBottomSheet : BottomSheet
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }
    public ObservableCollection<Categories> Categories { get; set; }
    public Dictionary<string, string> Icons = new Dictionary<string, string>();
    private readonly IProductTools _pt;

    public AddNewCategoryBottomSheet(ObservableCollection<Categories> Categories, IProductTools pt)
	{
		InitializeComponent();

        BindingContext = this;
        _pt = pt;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;
        MainScrollView.MaximumHeightRequest = ScreenHeight - 280;

        MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, AbsoluteLayout.AutoSize));
        MainAbs.SetLayoutFlags(BtnApply, AbsoluteLayoutFlags.PositionProportional);
        MainAbs.SetLayoutBounds(BtnApply, new Rect(0, 1, ScreenWidth, AbsoluteLayout.AutoSize));

        lblTitle.Text = $"Create a new category";

        this.Categories = Categories;

        MaterialDesignIconsFonts obj = new MaterialDesignIconsFonts();

        Icons = _pt.GetIcons().Result;

    }

    private async void AddCategory_Clicked(object sender, EventArgs e)
    {

        ViewCategories CurrentPage = (ViewCategories)Shell.Current.CurrentPage;
        ViewCategoriesViewModel ViewModel = (ViewCategoriesViewModel)CurrentPage.BindingContext;
        ViewModel.Categories = Categories;

        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await this.DismissAsync();
                App.CurrentBottomSheet = null;
            }
        }
        catch (Exception)
        {

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

    }
}