using CommunityToolkit.Maui.Views;
using System.ComponentModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpPageSingleInput : Popup
{

    private readonly PopUpPageSingleInputViewModel _vm;

    public PopUpPageSingleInput(string Title, string Description, string DescriptionSub, string Placeholder, string Input, PopUpPageSingleInputViewModel viewModel)
    {
        InitializeComponent();

        lblTitle.Text = Title;
        lblDescription.Text = Description;
        if (DescriptionSub == "" || DescriptionSub == null)
        {
            viewModel.IsSubDesc = false;
        }
        else
        {
            viewModel.IsSubDesc = true;
            lblDescriptionSub.Text = DescriptionSub;
        }

        txtReturnData.Placeholder = Placeholder;
        lblErrorMessage.Text = $"Let us know the {Title} before you continue";

        if(!string.IsNullOrWhiteSpace(Input))
        {
            viewModel.ReturnData = Input;
        }

        double width = viewModel.PopupWidth -11;
        Rect rt = new Rect(width, 123, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
        AbsLayout.SetLayoutBounds(btnClose, rt);

        BindingContext = viewModel;
        _vm = viewModel;
    }


    private void Close_Popup(object sender, EventArgs e)
	{
		if(_vm.ReturnData == "" || _vm.ReturnData == null)
		{
            _vm.ReturnDataRequired = false;
        }
		else
		{
            this.Close(_vm.ReturnData);
        }

    }

    private void Close_Window(object sender, EventArgs e)
    { 
        this.Close(_vm.ReturnData);
    }

    private void txtReturnData_Loaded(object sender, EventArgs e)
    {
        txtReturnData.Focus();
    }
}