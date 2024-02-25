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

        Label CurrentLbl = new Label
        {
            Text = "Current"
        };

        Label ToLbl = new Label
        {
            Text = "New"
        };

        grid.Add(CurrentLbl, 0, 0);
        grid.Add(ToLbl, 2, 0);

        Label ToLabel = new Label 
        { 
            Text = "Assign To"
        };

        int i = 1;
        foreach (Categories Category in _vm.Categories)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            Border CurrentBorder = new Border
            {

            };

            Border ToBorder = new Border
            {

            };

            grid.Add(CurrentBorder, 0, i);
            grid.Add(ToLabel, 1, i);
            grid.Add(ToBorder, 2, i);

            i++;
        }

    }

    private void Close_Window(object sender, EventArgs e)
    {
        this.Close("Cancel");
    }
}