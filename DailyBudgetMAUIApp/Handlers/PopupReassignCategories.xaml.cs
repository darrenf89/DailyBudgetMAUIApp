using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopupReassignCategories : Popup
{
    private readonly PopupReassignCategoriesViewModel _vm;
    

    public PopupReassignCategories(PopupReassignCategoriesViewModel viewModel)
	{
        InitializeComponent();

        BindingContext = viewModel;
        _vm = viewModel;

        CreateReAssignData();

    }

    private void CreateReAssignData()
    {
        Application.Current.Resources.TryGetValue("StandardInputBorder", out var StandardInputBorder);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        Grid grid = new Grid
        {
            BackgroundColor = Color.FromArgb("#00FFFFFF"),
            Padding = new Thickness(0),
            Margin = new Thickness(20,0,20,0),
            ColumnDefinitions =
            {
                new ColumnDefinition{Width = new GridLength((_vm.PopupWidth / 2) - 40)},
                new ColumnDefinition{Width = new GridLength(40)},
                new ColumnDefinition{Width = new GridLength((_vm.PopupWidth / 2) - 40)}
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }
            }
        };

        Label Current = new Label
        {
            Text = "Current",
            TextColor = (Color)Gray900,
            FontSize = 16,
            CharacterSpacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(0,0,0,10),
            FontAttributes = FontAttributes.Bold
        };

        Label To = new Label
        {
            Text = "New",
            TextColor = (Color)Gray900,
            FontSize = 16,
            CharacterSpacing = 0,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(0, 0, 0, 10),
            FontAttributes = FontAttributes.Bold
        };

        grid.Add(Current, 0, 0);
        grid.Add(To, 2, 0);

        int i = 1;

        foreach (Categories Category in _vm.Categories)
        {
            grid.AddRowDefinition(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            Border CurrentBorder = new Border
            {
                Style = (Style)StandardInputBorder,
                MinimumWidthRequest = (_vm.PopupWidth / 2) - 50,
                HeightRequest = 38,
                Padding = new Thickness(0, 0, 0, 0)
            };

            Label CurrentLabel = new Label
            {
                Text = Category.CategoryName,
                TextColor = (Color)Gray900,
                FontSize = 12,
                CharacterSpacing = 0,
                Padding = new Thickness(10,0,0,0),
                Margin = new Thickness(0),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            CurrentBorder.Content = CurrentLabel;

            Border ToBorder = new Border
            {
                Style = (Style)StandardInputBorder,
                MinimumWidthRequest = (_vm.PopupWidth / 2) - 50,
                HeightRequest = 38,
                Padding = new Thickness(0),
            };

            BorderlessPicker ToPicker = new BorderlessPicker
            {
                ItemsSource = _vm.DdlCategories,
                FontSize = 12,
                CharacterSpacing = 0,
                Margin = new Thickness(10,0,0,0),
                TextColor = (Color)Primary,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };

            _vm.SelectedReAssignCat.Add("Do not reassign");
            ToPicker.SetBinding(BorderlessPicker.SelectedItemProperty, $"SelectedReAssignCat[{i - 1}]");

            ToBorder.Content = ToPicker;

            Image ClickImage = new Image
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0,0,0,12),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\uea50",
                    Size = 20,
                    Color = (Color)Gray900                    
                }
            };

            grid.Add(CurrentBorder, 0, i);
            grid.Add(ClickImage, 1, i);
            grid.Add(ToBorder, 2, i);

            i++;
        }

        vslSelectors.Content = grid;
    }

    private async void ApplyCategoryReAssign_Clicked(object sender, EventArgs e)
    {
        int i = 0;
        foreach(Categories Cat in _vm.Categories)
        {
            string SelectedCat = _vm.SelectedReAssignCat[i];
            int SelectedCatID = _vm.ReAssignCategories[SelectedCat];
            bool IsReAssign = SelectedCatID != 0;

            await _vm.DeleteCategory(Cat, SelectedCatID, IsReAssign);
            i++;
        }

        this.Close("Ok");
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        this.Close("Cancel");
    }
    private void Close_Window(object sender, EventArgs e)
    {
        this.Close("Cancel");
    }
}