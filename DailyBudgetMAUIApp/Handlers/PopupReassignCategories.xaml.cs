using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Models;
using System.Globalization;
using DailyBudgetMAUIApp.DataServices;
using Android.Views;

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

        Grid grid = new Grid
        {
            BackgroundColor = Color.FromArgb("#00FFFFFF"),
            Padding = new Thickness(0),
            Margin = new Thickness(0),
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
            TextColor = (Color)Tertiary,
            FontSize = 12,
            CharacterSpacing = 0
        };

        Label To = new Label
        {
            Text = "New",
            TextColor = (Color)Tertiary,
            FontSize = 12,
            CharacterSpacing = 0
        };

        grid.Add(Current, 0, 0);
        grid.Add(To, 2, 0);

        Label AssignLabel = new Label 
        { 
            Text = "assign To",
            TextColor = (Color)Gray400,
            FontSize = 8,
            CharacterSpacing = 0
        };

        int i = 1;
        foreach (Categories Category in _vm.Categories)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            Border CurrentBorder = new Border
            {
                Style = (Style)StandardInputBorder
            };

            Label CurrentLabel = new Label
            {
                Text = Category.CategoryName,
                TextColor = (Color)Tertiary,
                FontSize = 12,
                CharacterSpacing = 0
            };

            CurrentBorder.Content = CurrentLabel;

            Border ToBorder = new Border
            {
                Style = (Style)StandardInputBorder
            };

            BorderlessPicker ToPicker = new BorderlessPicker
            {
                ItemsSource = _vm.DdlCategories,
                FontSize = 12,
                CharacterSpacing = 0,
                TextColor = (Color)Primary
            };

            _vm.SelectedReAssignCat.Add("Do not reassign");
            ToPicker.SetBinding(Picker.SelectedIndexProperty, _vm.SelectedReAssignCat[i-1]);

            grid.Add(CurrentBorder, 0, i);
            grid.Add(AssignLabel, 1, i);
            grid.Add(ToBorder, 2, i);

            i++;
        }

        vslSelectors.Children.Add(grid);
    }

    private void Close_Window(object sender, EventArgs e)
    {
        this.Close("Cancel");
    }
}