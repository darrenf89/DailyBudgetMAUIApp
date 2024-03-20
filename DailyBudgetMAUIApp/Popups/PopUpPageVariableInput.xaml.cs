using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.DataServices;
using System.Globalization;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpPageVariableInput : Popup
{
    private readonly PopUpPageVariableInputViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public PopUpPageVariableInput(string Title, string Description, string DescriptionSub, string Placeholder, object Input, string Type, PopUpPageVariableInputViewModel viewModel)
    {
        _pt = new ProductTools(new RestDataService());
        _ds = new RestDataService();

        InitializeComponent();

        BindingContext = viewModel;
        _vm = viewModel;

        _vm.Type = Type;

        if(Type == "DateTime")
        {
            _vm.DateTimeInput = (DateTime)Input;
            pckDateTimeInput.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow);
            pckDateTimeInput.IsVisible = true;
        }
        else if (Type == "Currency")
        {
            _vm.DecimalInput = (decimal)Input;
            entCurrencyInput.IsVisible = true;
            entCurrencyInput.Focus();
            entCurrencyInput.Text = _vm.DecimalInput.ToString("c", CultureInfo.CurrentCulture);
            entCurrencyInput.CursorPosition = _pt.FindCurrencyCursorPosition(entCurrencyInput.Text);
        }

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

        double width = viewModel.PopupWidth -11;
        Rect rt = new Rect(width, 123, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
        AbsLayout.SetLayoutBounds(btnClose, rt);


    }

    private bool Validate()
    {
        bool IsValid = true;

        if (_vm.Type == "DateTime")
        {
            if (_vm.DateTimeInput < _pt.GetBudgetLocalTime(DateTime.UtcNow).Date)
            {
                lblErrorMessage.Text = "You have to select a date later than today dummy!";
                IsValid = false;
            }  
        }
        else if (_vm.Type == "Currency")
        {
            if(_vm.DecimalInput == 0)
            {
                lblErrorMessage.Text = "Can't have an outgoing of zero, seems obvious!";
                IsValid = false;
            }
        }

        _vm.ReturnDataError = IsValid;

        return IsValid;
    }

    private void Close_Popup(object sender, EventArgs e)
	{
        if (Validate())
        {
            if (_vm.Type == "DateTime")
            {
                this.Close(_vm.DateTimeInput);

            }
            else if (_vm.Type == "Currency")
            {
                this.Close(_vm.DecimalInput);
            }
        }

    }

    private void Close_Window(object sender, EventArgs e)
    {
        this.Close("");
    }

    private void txtCurrencyInput_TextChanged(object sender, TextChangedEventArgs e)
    {       
        decimal Amount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        entCurrencyInput.Text = Amount.ToString("c", CultureInfo.CurrentCulture);
        entCurrencyInput.CursorPosition = _pt.FindCurrencyCursorPosition(entCurrencyInput.Text);
        _vm.DecimalInput = Amount;
    }
}