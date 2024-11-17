using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.ViewModels;
using Microsoft.Maui.Controls;


namespace DailyBudgetMAUIApp.Handlers;

public partial class PopUpContactUs : Popup
{
    private bool IsFirstLoad = true;

    private readonly PopUpContactUsViewModel _vm;
    public PopUpContactUs(PopUpContactUsViewModel viewModel)
	{
		InitializeComponent();

        BindingContext = viewModel;
        _vm = viewModel;

        WeakReferenceMessenger.Default.UnregisterAll(this);
        WeakReferenceMessenger.Default.Register<ClosePopupMessage>(this, (r, m) =>
        {
            try
            {
                if(m._isSuccess)
                {
                    this.CloseAsync(m._supportID);
                }
                else
                {
                    this.CloseAsync("Error");
                }
                
            }
            catch
            {

            }            
        });
    }

    ~PopUpContactUs()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }

    private void ClosePopup_Clicked(object sender, EventArgs e)
    {
        try
        {
            this.CloseAsync("Closed");
        }
        catch
        {

        }
    }

    private async void Editor_Focused(object sender, FocusEventArgs e)
    {
        if(IsFirstLoad)
        {
            IsFirstLoad = false;
        }
        else
        {        
            await Task.Delay(200);

            await ScrollView.ScrollToAsync(DetailsEditor, ScrollToPosition.Center, animated: true);
        }
    }

}

public class ClosePopupMessage
{
    public bool _isSuccess;
    public int _supportID;
    public ClosePopupMessage(bool IsSuccess, int SupportID)
    {
        _isSuccess = IsSuccess;
        _supportID = SupportID;
    }
}