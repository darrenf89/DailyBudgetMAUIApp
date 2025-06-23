using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;


namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpPageSingleInput : Popup<String>
{

    private readonly PopUpPageSingleInputViewModel _vm;

    public PopUpPageSingleInput(PopUpPageSingleInputViewModel viewModel)
    {
        InitializeComponent();

        Loaded += async (s, e) => await Load();

        BindingContext = viewModel;
        _vm = viewModel;
    }

    private async Task Load()
    {
        await Task.Delay(1); 

        lblTitle.Text = _vm.InputTitle;
        lblDescription.Text = _vm.Description;
        if (_vm.DescriptionSub == "" || _vm.DescriptionSub == null)
        {
            _vm.IsSubDesc = false;
        }
        else
        {
            _vm.IsSubDesc = true;
            lblDescriptionSub.Text = _vm.DescriptionSub;
        }

        txtReturnData.Placeholder = _vm.Placeholder;
        lblErrorMessage.Text = $"Let us know the {_vm.InputTitle} before you continue";

        if (!string.IsNullOrWhiteSpace(_vm.Input))
        {
            _vm.ReturnData = _vm.Input;
        }

        double width = _vm.PopupWidth - 11;
        Rect rt = new Rect(width, 123, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
        AbsLayout.SetLayoutBounds(btnClose, rt);
    }


    private void Close_Popup(object sender, EventArgs e)
	{
		if(_vm.ReturnData == "" || _vm.ReturnData == null)
		{
            _vm.ReturnDataRequired = false;
        }
		else
		{
            this.CloseAsync(_vm.ReturnData);
        }

    }

    private void Close_Window(object sender, EventArgs e)
    { 
        this.CloseAsync(_vm.ReturnData);
    }

    private void txtReturnData_Loaded(object sender, EventArgs e)
    {
        txtReturnData.Focus();
    }
}