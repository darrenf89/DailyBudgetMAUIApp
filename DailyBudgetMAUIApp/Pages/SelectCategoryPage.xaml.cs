using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Expander;

namespace DailyBudgetMAUIApp.Pages;

public partial class SelectCategoryPage : ContentPage
{
	private readonly IRestDataService _ds;
	private readonly IProductTools _pt;
	private readonly SelectCategoryPageViewModel _vm;

	public SelectCategoryPage(int BudgetID, Transactions Transaction, IRestDataService ds, IProductTools pt, SelectCategoryPageViewModel viewModel)
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
        base.OnAppearing();

        _vm.CategoryList = _ds.GetCategories(_vm.BudgetID).Result;

        if(_vm.CategoryList.Count == 0)
        {
            brdNoCategories.IsVisible = true;
            CategoryList.IsVisible = true;
        }    

        foreach (Categories Category in _vm.CategoryList)
        {
            if(!Category.isSubCategory)
            {
                _vm.GroupCategoryList.Add(Category);
            }
        }

        FillSubCategoryLists(_vm.GroupCategoryList);
        LoadCategoryList();

    }

    private void LoadCategoryList()
    {
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("brdPrimary", out var brdPrimary);
        Application.Current.Resources.TryGetValue("PrimaryDark", out var PrimaryDark);

        if (_vm.GroupCategoryList.Count > 0 && _vm.SubCategoryList.Count > 0)
        {
            //vslCategories.Children.Clear();

            foreach (Categories GroupCat in _vm.GroupCategoryList)
            {



                foreach (Categories SubCat in _vm.SubCategoryList)
                {
                    if(SubCat.CategoryGroupID == GroupCat.CategoryGroupID)
                    {

                    }
                }
            }
        }

        if (_vm.GroupCategoryList.Count == 0 && _vm.SubCategoryList.Count == 0 && _vm.CategoryList.Count != 0)
        {
            _vm.NoCategoriesText = "No Categories or Sub Categories match that name!";
            brdNoCategories.IsVisible = true;
            CategoryList.IsVisible = true;
        }
        else
        {
            brdNoCategories.IsVisible = true;
            CategoryList.IsVisible = true;
        }
    }

    private void FillSubCategoryLists(List<Categories> GroupCatList)
    {
        List<int> GroupCategories = new List<int>();

        foreach (Categories Category in GroupCatList)
        {
            GroupCategories.Add(Category.CategoryGroupID.GetValueOrDefault());
        }

        foreach (Categories Category in _vm.CategoryList)
        {
            if (Category.isSubCategory && GroupCategories.Contains(Category.CategoryGroupID.GetValueOrDefault()))
            {
                _vm.SubCategoryList.Add(Category);
            }
        }
    }

    private void FillGroupCategoryLists(List<Categories> SubCatList)
    {

    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        _vm.Transaction.Category = "";
        _vm.Transaction.CategoryID = 0;

        await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}",
        new Dictionary<string, object>
        {
            ["Transaction"] = _vm.Transaction
        });
    }

    private void ShowHideSortFiler_Tapped(object sender, TappedEventArgs e)
    {

    }
}