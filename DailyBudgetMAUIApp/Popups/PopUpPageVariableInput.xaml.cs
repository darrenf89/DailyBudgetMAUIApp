using AndroidX.Lifecycle;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.ViewModels;
using System.Globalization;

namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpPageVariableInput : Popup<Object>
{
    private readonly PopUpPageVariableInputViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;

    public PopUpPageVariableInput(PopUpPageVariableInputViewModel viewModel)
    {
        _pt = IPlatformApplication.Current.Services.GetService<IProductTools>();
        _ds = IPlatformApplication.Current.Services.GetService<IRestDataService>();

        InitializeComponent();

        BindingContext = viewModel;
        _vm = viewModel;

        Loaded += async (s, e) => await Load();
    }

    private async Task Load()
    {
        await Task.Delay(1);

        if (_vm.Type == "DateTime")
        {
            _vm.DateTimeInput = (DateTime)_vm.Input;
            pckDateTimeInput.MinimumDate = _pt.GetBudgetLocalTime(DateTime.UtcNow);
            pckDateTimeInput.IsVisible = true;
        }
        else if (_vm.Type == "Currency")
        {
            _vm.DecimalInput = (decimal)_vm.Input;
            entCurrencyInput.IsVisible = true;
            entCurrencyInput.Focus();
            entCurrencyInput.Text = _vm.DecimalInput.ToString("c", CultureInfo.CurrentCulture);
        }

        lblTitle.Text = _vm.TitleText;
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

        double width = _vm.PopupWidth - 11;
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
                this.CloseAsync(_vm.DateTimeInput);

            }
            else if (_vm.Type == "Currency")
            {
                this.CloseAsync(_vm.DecimalInput);
            }
        }

    }

    private void Close_Window(object sender, EventArgs e)
    {
        this.CloseAsync("");
    }

    private void txtCurrencyInput_TextChanged(object sender, TextChangedEventArgs e)
    {       
        decimal Amount = (decimal)_pt.FormatEntryNumber(sender, e, entCurrencyInput);

        _vm.DecimalInput = Amount;
    }
}