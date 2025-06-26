using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp.Pages.BottomSheets;

public partial class PayeeOptionsBottomSheet : BottomSheet
{
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }

    public PayeeOptionsBottomSheet(IRestDataService ds, IProductTools pt)
    {
        InitializeComponent();

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        var ScreenHeight = DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density;
        ButtonWidth = ScreenWidth - 40;
        btnDismiss.WidthRequest = ButtonWidth;

        _ds = ds;
        _pt = pt;
    }

    private void btnDismiss_Clicked(object sender, EventArgs e)
    {
        this.DismissAsync();
    }

    private async void DeletePayee_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            List<string> Payees = await _ds.GetPayeeList(App.DefaultBudgetID);
            string[] PayeeList = Payees.ToArray();

            var DeletePayee = await Application.Current.Windows[0].Page.DisplayActionSheet($"What Payee do you want to delete?", "Cancel", null, PayeeList);
            if (DeletePayee == "Cancel")
            {

            }
            else
            {
                Payees.Remove(DeletePayee);
                PayeeList = Payees.ToArray();
                var reassign = await Application.Current.Windows[0].Page.DisplayActionSheet($"Do you want to reassign this payees transactions?", "Cancel", "No", PayeeList);
                if (reassign == "Cancel")
                {

                }
                else if (reassign == "No")
                {
                    await _ds.DeletePayee(App.DefaultBudgetID, DeletePayee, "");

                    if (App.CurrentBottomSheet != null)
                    {
                        await App.CurrentBottomSheet.DismissAsync();
                        App.CurrentBottomSheet = null;
                    }

                    await Application.Current.Windows[0].Page.DisplayAlert($"Payee Deleted", $"Congrats you have deleted {DeletePayee}, hopefully you meant to!", "Ok");
                }
                else
                {
                    await _ds.DeletePayee(App.DefaultBudgetID, DeletePayee, reassign);

                    if (App.CurrentBottomSheet != null)
                    {
                        await App.CurrentBottomSheet.DismissAsync();
                        App.CurrentBottomSheet = null;
                    }

                    await Application.Current.Windows[0].Page.DisplayAlert($"Category Deleted", $"Congrats you have deleted {DeletePayee} and reassigned its transactions to {reassign}, hopefully you meant to!", "Ok");
                }
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EnvelopeOptionsBottomSheet", "DeletePayee_Tapped");
        }
    }

    private async void ViewPayeeList_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
                if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }

            await Shell.Current.GoToAsync($"{nameof(DailyBudgetMAUIApp.Pages.SelectPayeePage)}?BudgetID={App.DefaultBudgetID}&PageType=ViewList");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EnvelopeOptionsBottomSheet", "ViewPayeeList_Tapped");
        }

    }

    private async void ViewPayees_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewPayees)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EnvelopeOptionsBottomSheet", "ViewPayees_Tapped");
        }

    }
}