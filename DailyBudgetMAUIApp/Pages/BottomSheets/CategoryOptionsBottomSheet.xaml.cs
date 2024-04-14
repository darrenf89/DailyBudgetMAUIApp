using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class CategoryOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }

    public CategoryOptionsBottomSheet(IRestDataService ds, IProductTools pt)
    {
        InitializeComponent();

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        var ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;
        btnDismiss.WidthRequest = ButtonWidth;  

        _ds = ds;
        _pt = pt;
    }

    private void btnDismiss_Clicked(object sender, EventArgs e)
    {
        this.DismissAsync();
    }

    private async void AddNewCategory_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentBottomSheet != null)
        {
            await this.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        AddNewCategoryBottomSheet page = new AddNewCategoryBottomSheet(null, new ProductTools(new RestDataService()), new RestDataService());

        page.Detents = new DetentsCollection()
            {
                new FullscreenDetent()
            };

        page.HasBackdrop = true;
        page.CornerRadius = 0;

        App.CurrentBottomSheet = page;

        await page.ShowAsync();

    }

    private async void ViewCategoryList_Tapped(object sender, TappedEventArgs e)
    {

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.SelectCategoryPage)}?BudgetID={App.DefaultBudgetID}&PageType=ViewList");

    }    
    
    private async void DeleteSubCategory_Tapped(object sender, TappedEventArgs e)
    {
        Dictionary<string, int> Categories = await _ds.GetAllCategoryNames(App.DefaultBudgetID);
        string[] CategoryList = Categories.Keys.ToArray();

        var DeleteSubCategory = await Application.Current.MainPage.DisplayActionSheet($"What Category do you want to delete?", "Cancel", null, CategoryList);
        if(DeleteSubCategory == "Cancel")
        {

        }
        else
        {
            int DeleteSubCat = Categories[DeleteSubCategory];
            Categories.Remove(DeleteSubCategory);
            CategoryList = Categories.Keys.ToArray();
            var reassign = await Application.Current.MainPage.DisplayActionSheet($"Do you want to reassign this categories transactions?", "Cancel", "No", CategoryList);
            if (reassign == "Cancel")
            {

            }
            else if (reassign == "No")
            {
                await _ds.DeleteCategory(DeleteSubCat, false, 0);

                if (App.CurrentBottomSheet != null)
                {
                    await App.CurrentBottomSheet.DismissAsync();
                    App.CurrentBottomSheet = null;
                }

                await Application.Current.MainPage.DisplayAlert($"Category Deleted",$"Congrats you have deleted {DeleteSubCategory}, hopefully you meant to!","Ok");

            }
            else
            {
                int ReplacementID = Categories[reassign];
                await _ds.DeleteCategory(DeleteSubCat, true, ReplacementID);

                if (App.CurrentBottomSheet != null)
                {
                    await App.CurrentBottomSheet.DismissAsync();
                    App.CurrentBottomSheet = null;
                }

                await Application.Current.MainPage.DisplayAlert($"Category Deleted", $"Congrats you have deleted {DeleteSubCategory} and reassigned its transactions to {reassign}, hopefully you meant to!", "Ok");
            }
        }



    }    
    
    private async void AddSubCategory_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentBottomSheet != null)
        {
            await this.DismissAsync();
            App.CurrentBottomSheet = null;
        }

        List<Categories>? AllCategories = await _ds.GetCategories(App.DefaultBudgetID);
        Dictionary<string, int> CatDict = new Dictionary<string, int>();
        foreach (var cat in AllCategories)
        {
            if (!cat.IsSubCategory)
            {
                CatDict.Add(cat.CategoryName, cat.CategoryID);
            }
        }

        string[] CategoryList = CatDict.Keys.ToArray();
        var SelectCategory = await Application.Current.MainPage.DisplayActionSheet($"Select a category group you'd like to add a category to!", "Cancel", null, CategoryList);
        if (SelectCategory == "Cancel")
        {

        }
        else
        {
            int CategoryID = CatDict[SelectCategory];
            AddSubCategoryBottomSheet page = new AddSubCategoryBottomSheet(await _ds.GetCategoryFromID(CategoryID), new ProductTools(new RestDataService()), new RestDataService());

            page.Detents = new DetentsCollection()
            {
                new ContentDetent(),
                new FullscreenDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            await page.ShowAsync();
        }
    }

    private async void ViewSubCategory_Tapped(object sender, TappedEventArgs e)
    {
        List<Categories>? Categories = await _ds.GetCategories(App.DefaultBudgetID);
        Dictionary<string, int> CatDict = new Dictionary<string, int>();
        foreach (var cat in Categories) 
        {
            if(!cat.IsSubCategory)
            {
                CatDict.Add(cat.CategoryName, cat.CategoryID);
            }
        }

        string[] CategoryList = CatDict.Keys.ToArray();
        var SelectCategory = await Application.Current.MainPage.DisplayActionSheet($"What Category would you like to view", "Cancel", null, CategoryList);
        if (SelectCategory == "Cancel")
        {

        }
        else
        {
            int CategoryID = CatDict[SelectCategory];

            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.MainPage.ShowPopup(PopUp);
            }

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"{nameof(ViewCategory)}?HeaderCatId={CategoryID}");
        }
    }

    private async void ViewAllCategories_Tapped(object sender, TappedEventArgs e)
    {
        if (App.CurrentPopUp == null)
        {
            var PopUp = new PopUpPage();
            App.CurrentPopUp = PopUp;
            Application.Current.MainPage.ShowPopup(PopUp);
        }

        if (App.CurrentBottomSheet != null)
        {
            await App.CurrentBottomSheet.DismissAsync();
            App.CurrentBottomSheet = null;
        }
        await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewCategories)}");
    }    

    private async void DeleteCategory_Tapped(object sender, TappedEventArgs e)
    {
        List<Categories>? AllCategories = await _ds.GetCategories(App.DefaultBudgetID);
        Dictionary<string, int> CatDict = new Dictionary<string, int>();
        foreach (var cat in AllCategories)
        {
            if (!cat.IsSubCategory)
            {
                CatDict.Add(cat.CategoryName, cat.CategoryID);
            }
        }

        string[] CategoryList = CatDict.Keys.ToArray();
        var SelectCategory = await Application.Current.MainPage.DisplayActionSheet($"What Category would you like to delete", "Cancel", null, CategoryList);
        if (SelectCategory == "Cancel")
        {

        }
        else
        {
            int CategoryID = CatDict[SelectCategory];

            Dictionary<string, int> Categories = await _ds.GetAllCategoryNames(App.DefaultBudgetID);
            List<Categories> CategoryDetails = _ds.GetHeaderCategoryDetailsFull(CategoryID, App.DefaultBudgetID).Result;

            var Popup = new PopupReassignCategories(new PopupReassignCategoriesViewModel(Categories, CategoryID, CategoryDetails, new RestDataService()));
            var result = await Shell.Current.ShowPopupAsync(Popup);
            if (result.ToString() == "Cancel")
            {

            }
            else if (result.ToString() == "Ok")
            {
                await _ds.DeleteCategory(CategoryID, false, 0);

                if (App.CurrentBottomSheet != null)
                {
                    await App.CurrentBottomSheet.DismissAsync();
                    App.CurrentBottomSheet = null;
                }

                await Application.Current.MainPage.DisplayAlert($"Category Deleted", $"Congrats you have deleted {SelectCategory}, hopefully you meant to!", "Ok");

            }
        }
    }
}