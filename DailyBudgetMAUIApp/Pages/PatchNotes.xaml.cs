using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.ViewModels;

namespace DailyBudgetMAUIApp.Pages;

public partial class PatchNotes : BasePage
{
    private readonly PatchNotesViewModel _vm;


    public PatchNotes(PatchNotesViewModel viewModel)
	{

        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
    }

    protected override void OnAppearing()
    {
        _vm.UpdatePatchNotes();
        base.OnAppearing();
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}