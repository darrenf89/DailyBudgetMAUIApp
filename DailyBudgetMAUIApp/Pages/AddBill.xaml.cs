using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using CommunityToolkit.Maui.ApplicationModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Internals;

namespace DailyBudgetMAUIApp.Pages;

public partial class AddBill : ContentPage
{
    private readonly AddBillViewModel _vm;
    private readonly IProductTools _pt;
    private readonly IRestDataService _ds;
    public AddBill(AddBillViewModel viewModel, IProductTools pt, IRestDataService ds)
	{
		InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ds = ds;
    }

    async protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _pt.SetCultureInfo(App.CurrentSettings);

        if (_vm.BillID == 0)
        {
            _vm.Bill = new Bills();
            _vm.Title = "Add a New Outgoing";
            btnAddBill.IsVisible = true;

        }
        else
        {
            //TODO: GET THE BILL DETAILS
            _vm.Bill = new Bills();
            _vm.Title = $"Update Outgoing {_vm.Bill.BillName}";
            btnUpdateBill.IsVisible = true;
        }

        double AmountDue = (double?)_vm.Bill.BillAmount ?? 0;
        entAmountDue.Text = AmountDue.ToString("c", CultureInfo.CurrentCulture);

        double CurrentSaved = (double?)_vm.Bill.BillCurrentBalance ?? 0;
        entCurrentSaved.Text = CurrentSaved.ToString("c", CultureInfo.CurrentCulture);


    }

    private void btnRecurringBill_Clicked(object sender, EventArgs e)
    {
        _vm.Bill.IsRecuring = true;

        SelectBillType.IsVisible = false;
        BillTypeSelected.IsVisible = true;
        lblSelectedBillTitle.Text = "You are adding a recurring outgoing";
        lblSelectedBillParaOne.Text = "For most of your bills! Phone, car, Netflix, the list goes on ...";
        lblSelectedBillParaTwo.Text = "Tell us how much, when the next bill is due and how often it occurs";

        brdBillDetails.IsVisible = true;
        vslBillDetails.IsVisible = true;
        vslBillTypes.IsVisible = true;
    }
    private void btnOneoffBill_Clicked(object sender, EventArgs e)
    {
        _vm.Bill.IsRecuring = false;

        SelectBillType.IsVisible = false;
        BillTypeSelected.IsVisible = true;
        lblSelectedBillTitle.Text = "You are adding a one off outgoing";
        lblSelectedBillParaOne.Text = "For those one off bills, owe someone money?";
        lblSelectedBillParaTwo.Text = "Tell us how much and when the bill is due";

        brdBillDetails.IsVisible = true;
        vslBillDetails.IsVisible = true;
        vslBillTypes.IsVisible = false;
    }

    void AmountDue_Changed(object sender, TextChangedEventArgs e)
    {
        double AmountDue = _pt.FormatCurrencyNumber(e.NewTextValue);
        entAmountDue.Text = AmountDue.ToString("c", CultureInfo.CurrentCulture);
        entAmountDue.CursorPosition = _pt.FindCurrencyCursorPosition(entAmountDue.Text);
    }

    void CurrentSaved_Changed(object sender, TextChangedEventArgs e)
    {
        double CurrentSaved = _pt.FormatCurrencyNumber(e.NewTextValue);
        entCurrentSaved.Text = CurrentSaved.ToString("c", CultureInfo.CurrentCulture);
        entCurrentSaved.CursorPosition = _pt.FindCurrencyCursorPosition(entCurrentSaved.Text);
    }

    private void dtpckBillDueDate_DateSelected(object sender, DateChangedEventArgs e)
    {

    }

    private void Option1Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("Everynth");
    }

    private void Option2Select_Tapped(object sender, TappedEventArgs e)
    {
        UpdateSelectedOption("OfEveryMonth");
    }

    private void UpdateSelectedOption(string option)
    {
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Light", out var Light);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        if (option == "Everynth")
        {
            vslOption1Select.BackgroundColor = (Color)Success;
            vslOption2Select.BackgroundColor = (Color)Light;

            lblOption1.FontAttributes = FontAttributes.Bold;
            lblOption2.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)White;
            lblOption2.TextColor = (Color)Gray900;;

            vslOption1.IsVisible = true;
            vslOption2.IsVisible = false;

            pckrEverynthDuration.SelectedItem = _vm.Bill.BillDuration ?? "days";
            entEverynthValue.Text = _vm.Bill.BillValue.ToString() ?? "1";

            _vm.BillTypeText = "Everynth";

        }
        else if (option == "OfEveryMonth")
        {
            vslOption1Select.BackgroundColor = (Color)Light;
            vslOption2Select.BackgroundColor = (Color)Success;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.Bold;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)White;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = true;

            entOfEveryMonthValue.Text = _vm.Bill.BillValue.ToString() ?? "1";

            _vm.BillTypeText = "OfEveryMonth";
        }
        else
        {
            vslOption1Select.BackgroundColor = (Color)Light;
            vslOption2Select.BackgroundColor = (Color)Light;

            lblOption1.FontAttributes = FontAttributes.None;
            lblOption2.FontAttributes = FontAttributes.None;

            lblOption1.TextColor = (Color)Gray900;
            lblOption2.TextColor = (Color)Gray900;

            vslOption1.IsVisible = false;
            vslOption2.IsVisible = false;

            _vm.BillTypeText = "";
        }
    }

    void OfEveryMonthValue_Changed(object sender, TextChangedEventArgs e)
    {
        Regex regex = new Regex(@"^\d+$");

        if (e.NewTextValue != null && e.NewTextValue != "")
        {
            if (!regex.IsMatch(e.NewTextValue))
            {
                entEverynthValue.Text = e.OldTextValue;
            }
        }
    }

    void EveryNthValue_Changed(object sender, TextChangedEventArgs e)
    {
        Regex regex = new Regex(@"^\d+$");

        if (e.NewTextValue != null && e.NewTextValue != "")
        {
            if (!regex.IsMatch(e.NewTextValue))
            {
                entEverynthValue.Text = e.OldTextValue;
            }
        }
    }
}