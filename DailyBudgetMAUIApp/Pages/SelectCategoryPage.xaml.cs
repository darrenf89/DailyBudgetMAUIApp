using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using Syncfusion.Maui.Expander;
using System;
using System.Xml.XPath;

namespace DailyBudgetMAUIApp.Pages;

public partial class SelectCategoryPage : ContentPage
{
	private readonly IRestDataService _ds;
	private readonly IProductTools _pt;
	private readonly SelectCategoryPageViewModel _vm;
    private Dictionary<string, Grid> AddNewCat = new Dictionary<string, Grid>();
    private Dictionary<string, VerticalStackLayout> SubCatList = new Dictionary<string, VerticalStackLayout>();
    private Dictionary<string, double> SubCatHeight = new Dictionary<string, double>();

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

        _vm.CategoryList = _ds.GetCategories(_vm.BudgetID).Result;

        if (_vm.CategoryList.Count == 0)
        {
            brdNoCategories.IsVisible = true;
            CategoryList.IsVisible = false;
        }
        else
        {
            brdNoCategories.IsVisible = false;
            CategoryList.IsVisible = true;
        }

        foreach (Categories Category in _vm.CategoryList)
        {
            if (!Category.isSubCategory)
            {
                _vm.GroupCategoryList.Add(Category);
            }
        }

        FillSubCategoryLists(_vm.GroupCategoryList);
        LoadCategoryList();
    }

    async protected override void OnAppearing()
    {
       base.OnAppearing();

    }

    private void LoadCategoryList()
    {
        Application.Current.Resources.TryGetValue("brdPrimaryWhite", out var brdPrimaryWhite);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("TitleButtons", out var TitleButtons);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("brdTertiary", out var brdTertiary);
        Application.Current.Resources.TryGetValue("TertiaryLight", out var TertiaryLight);
        Application.Current.Resources.TryGetValue("brdSuccess", out var brdSuccess);
        Application.Current.Resources.TryGetValue("brdDanger", out var brdDanger);
        Application.Current.Resources.TryGetValue("Danger", out var Danger);
        Application.Current.Resources.TryGetValue("brdPrimary", out var brdPrimary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);

        int i = 0;

        if (_vm.GroupCategoryList.Count > 0 && _vm.SubCategoryList.Count > 0)
        {
            vslCategories.Children.Clear();
            AddNewCat.Clear();

            foreach (Categories GroupCat in _vm.GroupCategoryList)
            {
                i += 1;
                string CategoryString = $"Category{i}";

                Border CategoryBorder = new Border
                {
                    Style = (Style)brdPrimaryWhite,
                    Margin = new Thickness(0, 0, 0, 30)
                };

                VerticalStackLayout CategoryVSL = new VerticalStackLayout 
                {
                    
                };

                Grid CategoryGroupHeaderGrid = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 5),
                    VerticalOptions = LayoutOptions.Center,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)},
                        new ColumnDefinition{Width = new GridLength(40)}
                    }
                };

                Label CategoryHeaderLabel = new Label
                {
                    Text = GroupCat.CategoryName,
                    TextColor = (Color)Primary,
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Start,
                    FontAttributes = FontAttributes.Bold,
                    TextDecorations = TextDecorations.Underline,
                    VerticalOptions = LayoutOptions.Center
                };

                CategoryGroupHeaderGrid.Add(CategoryHeaderLabel, 0, 0);

                Button ExpandCategoryButton = new Button
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    Padding = new Thickness(0),
                    TextColor = (Color)White,
                    BackgroundColor = (Color)White,
                    HorizontalOptions = LayoutOptions.End,
                    Style = (Style)TitleButtons,
                    VerticalOptions = LayoutOptions.Center,
                    ImageSource = new FontImageSource
                    {
                        FontFamily = "MaterialDesignIcons",
                        Glyph = "\ue94f",
                        Size = 30,
                        Color = (Color)Primary
                    }
                };

                ExpandCategoryButton.Clicked += (s, e) => ShowHideCategoryGroup(CategoryString);

                CategoryGroupHeaderGrid.Add(ExpandCategoryButton, 1, 0);

                CategoryVSL.Children.Add(CategoryGroupHeaderGrid);

                VerticalStackLayout SubCategoryVSL = new VerticalStackLayout();

                Grid AddNewCatVSL = new Grid
                {
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    HeightRequest = 38,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition{Width = new GridLength(35)},
                        new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)}                        
                    }
                };

                Button AddNewCategoryButton = new Button
                {
                    Margin = new Thickness(0,0,5,0),
                    Padding = new Thickness(0),
                    TextColor = (Color)White,
                    BackgroundColor = (Color)White,
                    HorizontalOptions = LayoutOptions.End,
                    Style = (Style)TitleButtons,
                    VerticalOptions = LayoutOptions.Center,
                    ImageSource = new FontImageSource
                    {
                        FontFamily = "MaterialDesignIcons",
                        Glyph = "\ue146",
                        Size = 30,
                        Color = (Color)Success
                    }
                };

                AddNewCategoryButton.Clicked += (s, e) => ShowAddNewSubCategory(CategoryString, GroupCat.CategoryGroupID.GetValueOrDefault());

                AddNewCatVSL.Add(AddNewCategoryButton, 0, 0);

                Label AddNewHeaderLabel = new Label
                {
                    Text = "Add New Category",
                    Margin = new Thickness(13, 0, 0, 0),
                    TextColor = (Color)Gray400,
                    FontSize = 14,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Center
                };

                AddNewCatVSL.Add(AddNewHeaderLabel, 1, 0);

                AddNewCat.Add(CategoryString, AddNewCatVSL);

                SubCategoryVSL.Children.Add(AddNewCatVSL);                

                foreach (Categories SubCat in _vm.SubCategoryList)
                {
                    if(SubCat.CategoryGroupID == GroupCat.CategoryGroupID)
                    {
                        Border SubCategoryBorder = new Border
                        {
                            Style = (Style)brdPrimary,
                            Margin = new Thickness(35, 0, 0, 5)
                        };

                        TapGestureRecognizer SubCategoryTapGesture = new TapGestureRecognizer
                        {
                            CommandParameter = SubCat
                        };

                        SubCategoryTapGesture.Tapped += (s, e) => SelectCategory_Tapped(s, e);
                        SubCategoryBorder.GestureRecognizers.Add(SubCategoryTapGesture);

                        Label SubCategoryLabel = new Label
                        {
                            Text = SubCat.CategoryName,
                            TextColor = (Color)Gray900,
                            FontSize = 14,
                            Padding = new Thickness(2, 2, 2, 2)
                        };

                        SubCategoryBorder.Content = SubCategoryLabel;
                        SubCategoryVSL.Children.Add(SubCategoryBorder);
                    }
                }

                CategoryVSL.Children.Add(SubCategoryVSL);
                SubCatList.Add(CategoryString, SubCategoryVSL);

                CategoryBorder.Content = CategoryVSL;
                vslCategories.Children.Add(CategoryBorder);    
            }
        }

        if (_vm.GroupCategoryList.Count == 0 && _vm.SubCategoryList.Count == 0 && _vm.CategoryList.Count != 0)
        {
            _vm.NoCategoriesText = "No Categories or Sub Categories match that name!";
            brdNoCategories.IsVisible = true;
            CategoryList.IsVisible = false;
        }
        else
        {
            brdNoCategories.IsVisible = false;
            CategoryList.IsVisible = true;
        }
    }

    private void ShowAddNewSubCategory(string CategoryGroup, int CategoryID)
    {
        Application.Current.Resources.TryGetValue("brdPrimaryWhite", out var brdPrimaryWhite);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("TitleButtons", out var TitleButtons);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("brdTertiary", out var brdTertiary);
        Application.Current.Resources.TryGetValue("TertiaryLight", out var TertiaryLight);
        Application.Current.Resources.TryGetValue("brdSuccess", out var brdSuccess);
        Application.Current.Resources.TryGetValue("brdDanger", out var brdDanger);
        Application.Current.Resources.TryGetValue("Danger", out var Danger);

        Border AddNewSubCat = new Border
        {
            Style = (Style)brdTertiary,
            Margin = new Thickness(0, 0, 0, 5),
            HeightRequest = 38
        };

        Grid AddNewSubCatGrid = new Grid
        {
            Margin = new Thickness(0),
            Padding = new Thickness(0),
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)},
                new ColumnDefinition{Width = new GridLength(35)},
                new ColumnDefinition{Width = new GridLength(30)}
            }
        };

        BorderlessEntry AddNewSubCatEntry = new BorderlessEntry
        {
            WidthRequest = 180,
            BackgroundColor = (Color)TertiaryLight,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            HeightRequest = 34
        };

        AddNewSubCatGrid.Add(AddNewSubCatEntry, 0, 0);

        Border AcceptNewSubCategory = new Border
        {
            Style = (Style)brdSuccess,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 0, 5, 0),
            HeightRequest = 26
        };

        Image AcceptNewSubCategoryImage = new Image
        {
            Margin = new Thickness(2),
            VerticalOptions = LayoutOptions.Center,
            Source = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue5ca",
                Size = 22,
                Color = (Color)Success
            }
        };

        TapGestureRecognizer AcceptTapGesture = new TapGestureRecognizer
        {
            CommandParameter = CategoryGroup
        };
        AcceptTapGesture.Tapped += (s, e) => AddNewSubCategory(CategoryGroup, CategoryID);
        AcceptNewSubCategory.GestureRecognizers.Add(AcceptTapGesture);

        AcceptNewSubCategory.Content = AcceptNewSubCategoryImage;
        AddNewSubCatGrid.Add(AcceptNewSubCategory, 1, 0);

        Border DeclineNewSubCategory = new Border
        {
            Style = (Style)brdDanger,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 0, 0, 0),
            HeightRequest = 26
        };

        Image DeclineNewSubCategoryImage = new Image
        {
            Margin = new Thickness(2),
            VerticalOptions = LayoutOptions.Center,
            Source = new FontImageSource
            {
                FontFamily = "MaterialDesignIcons",
                Glyph = "\ue5cd",
                Size = 22,
                Color = (Color)Danger
            }
        };

        TapGestureRecognizer DeclineTapGesture = new TapGestureRecognizer
        {
            CommandParameter = CategoryGroup
        };
        DeclineTapGesture.Tapped += (s, e) => ClearAddNewCategory(CategoryGroup);
        DeclineNewSubCategory.GestureRecognizers.Add(DeclineTapGesture);

        DeclineNewSubCategory.Content = DeclineNewSubCategoryImage;
        AddNewSubCatGrid.Add(DeclineNewSubCategory, 2, 0);
        AddNewSubCat.Content = AddNewSubCatGrid;

        AddNewCat[CategoryGroup].RemoveAt(1);
        AddNewCat[CategoryGroup].Add(AddNewSubCat, 1, 0);

        AddNewSubCatEntry.Focus();
    }

    private async void AddNewSubCategory(string CategoryGroup, int CategoryID)
    {
        Grid VSL = AddNewCat[CategoryGroup];
        Border border = (Border)VSL.Children[1];
        Grid grid = (Grid)border.Content;
        BorderlessEntry Entry = (BorderlessEntry)grid.Children[0];
        string name = Entry.Text;

        Categories NewCat = new Categories
        {
            CategoryGroupID = CategoryID,
            isSubCategory = true,
            CategoryName = name
        };

        Entry.IsEnabled = false;
        Entry.IsEnabled = true;

        bool result = await DisplayAlert($"Add {name}", $"Are you sure you want to add {name} as a new Category?", "Yes, continue", "No, go back!");
        if (result)
        {

            AddNewCat[CategoryGroup].RemoveAt(1);

            Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
            Label AddNewHeaderLabel = new Label
            {
                Text = "Add New Category",
                Margin = new Thickness(13, 0, 0, 0),
                TextColor = (Color)Gray400,
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            AddNewCat[CategoryGroup].Add(AddNewHeaderLabel, 1, 0);

            NewCat = _ds.AddNewSubCategory(_vm.BudgetID, NewCat).Result;

            _vm.SubCategoryList.Add(NewCat);
            LoadCategoryList();
        }

    }

    private async void ShowHideCategoryGroup(string CategoryGroup)
    {
        VerticalStackLayout vsl = SubCatList[CategoryGroup];
        double vslHeight = SubCatHeight[CategoryGroup];


        if (!vsl.IsVisible)
        {
            vsl.IsVisible = true;
            vsl.HeightRequest = 0;
            var animation = new Animation(v => vsl.HeightRequest = v, 0, vslHeight);
            animation.Commit(this, "vslOptionsShow", 16, 1000, Easing.CubicOut);
        }
        else
        {
            var animation = new Animation(v => vsl.HeightRequest = v, vslHeight, 0);
            animation.Commit(this, "FilterOptionsHide", 16, 1000, Easing.CubicOut, (v, c) =>
            {
                vsl.IsVisible = false;
            });

        }
    }

    private void ClearAddNewCategory(string CategoryGroup)
    {
        Grid VSL = AddNewCat[CategoryGroup];
        Border border = (Border)VSL.Children[1];
        Grid grid = (Grid)border.Content;
        BorderlessEntry Entry = (BorderlessEntry)grid.Children[0];

        Entry.IsEnabled = false;
        Entry.IsEnabled = true;

        AddNewCat[CategoryGroup].RemoveAt(1);

        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Label AddNewHeaderLabel = new Label
        {
            Text = "Add New Category",
            Margin = new Thickness(13,0,0,0),
            TextColor = (Color)Gray400,
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center
        };

        AddNewCat[CategoryGroup].Add(AddNewHeaderLabel, 1, 0);
    }

    private async void SelectCategory_Tapped(object sender, TappedEventArgs e)
    {

        Categories Category = (Categories)e.Parameter;

        bool result = await DisplayAlert($"Select {Category.CategoryName}", $"Are you sure you want to select {Category.CategoryName} as the Category?", "Yes, continue", "No, go back!");
        if (result)
        {
            _vm.Transaction.Category = Category.CategoryName;
            _vm.Transaction.CategoryID = Category.CategoryID;

            await Shell.Current.GoToAsync($"..?BudgetID={_vm.BudgetID}",
                new Dictionary<string, object>
                {
                    ["Transaction"] = _vm.Transaction
                });
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

    protected override void LayoutChildren(double x, double y, double width, double height)
    {
        base.LayoutChildren(x, y, width, height);
        _vm.SortFilterHeight = FilterOptions.Height;

        foreach(var item in SubCatList)
        {
            VerticalStackLayout vsl = item.Value;
            SubCatHeight.Add(item.Key, vsl.Height);
        }

        FilterOptions.IsVisible = false;
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
}