using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Popups;
using DailyBudgetMAUIApp.ViewModels;
using Microsoft.Maui.Layouts;
using Plugin.LocalNotification;
using Plugin.Maui.AppRating;
using Syncfusion.Maui.Core;


namespace DailyBudgetMAUIApp.Pages;

public partial class EditAccountDetails : BasePage
{
    public double ButtonWidth { get; set; }
    public double ScreenWidth { get; set; }
    public double ScreenHeight { get; set; }

    private readonly IAppRating _ar;
    private const string androidPackageName = "com.companyname.dailybudgetmauiapp";

    private readonly EditAccountDetailsViewModel _vm;
    private readonly IProductTools _pt;

    public EditAccountDetails(EditAccountDetailsViewModel viewModel, IProductTools pt, IAppRating ar)
	{
		InitializeComponent();      

        this.BindingContext = viewModel;
        _vm = viewModel;
        _pt = pt;
        _ar = ar;

        ScreenWidth = DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;
        ScreenHeight = (DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density) - 60;
        ButtonWidth = ScreenWidth - 40;
    }

    protected async override void OnAppearing()
    {
        try
        {
            base.OnAppearing();

            MainAbs.WidthRequest = ScreenWidth;
            MainAbs.SetLayoutFlags(MainVSL, AbsoluteLayoutFlags.PositionProportional);
            MainAbs.SetLayoutBounds(MainVSL, new Rect(0, 0, ScreenWidth, ScreenHeight));

            await _vm.OnLoad();

            if(App.IsFamilyAccount)
            {
                ViewSubDetails.IsVisible = false;
            }            

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "OnAppearing");
        }
    }

    private async void ViewSubDetails_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            // Intent to open the Google Play app's subscription page directly
            var playStoreUri = "market://details?id=com.android.vending&url=https://play.google.com/store/account/subscriptions";

            if (await Launcher.CanOpenAsync(playStoreUri))
            {
                await Launcher.OpenAsync(playStoreUri);
            }
            else
            {
                var subscriptionUrl = "https://play.google.com/store/account/subscriptions";

                if (await Launcher.CanOpenAsync(subscriptionUrl))
                {
                    await Launcher.OpenAsync(subscriptionUrl);
                }
                else
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Error", "Unable to open the subscription page.", "OK");
                }
            }

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "ViewSubDetails_Tapped");
        }
    }    
    
    private async void TOS_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var subscriptionUrl = "https://www.dbudgeting.com/terms-of-service";

            if (await Launcher.CanOpenAsync(subscriptionUrl))
            {
                await Launcher.OpenAsync(subscriptionUrl);
            }
            else
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", "Sorry we weren't able to open the dBudget web site. You can visit us at www.dbudgeting.com", "OK");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "TOS_Tapped");
        }
    }    

    private async void PrivacyPolicy_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            try
            {
                var subscriptionUrl = "https://www.dbudgeting.com/privacy-policy";

                if (await Launcher.CanOpenAsync(subscriptionUrl))
                {
                    await Launcher.OpenAsync(subscriptionUrl);
                }
                else
                {
                    await Application.Current.Windows[0].Page.DisplayAlert("Error", "Sorry we weren't able to open the dBudget web site. You can visit us at www.dbudgeting.com", "OK");
                }
            }
            catch (Exception ex)
            {
                await _pt.HandleException(ex, "EditAccountDetails", "PrivacyPolicy_Tapped");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "PrivacyPolicy_Tapped");
        }
    }   

    private async void ViewUs_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Task.Run(RateApplicationStore);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "Rate_Tapped");
        }
    }
    private async void Rate_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Task.Run(RateApplicationInApp);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "Rate_Tapped");
        }
    }
    private Task RateApplicationStore()
    {
        Dispatcher.Dispatch(async () =>
        {
            await _ar.PerformRatingOnStoreAsync(packageName: "com.companyname.dailybudgetmauiapp");
        });

        return Task.CompletedTask;
    }
    private Task RateApplicationInApp()
    {
        Dispatcher.Dispatch(async () =>
        {
# if DEBUG
            await _ar.PerformInAppRateAsync(true);
#else
            await _ar.PerformInAppRateAsync();
#endif
        });

        return Task.CompletedTask;
    }

    private async void Share_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Text = "Want to improve your finances? This budgeting app helps you stay on top of your spending and build savings habits. Highly recommend!",
                Title = "Share dBudget",
                Uri = "https://play.google.com/store/apps/details?id=com.companyname.dailybudgetmauiapp"
               
            });
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "Share_Tapped");
        }
    }    
    
    private async void Version_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync($"///{nameof(PatchNotes)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "Version_Tapped");
        }
    }    
    
    private async void Logout_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            await Task.Delay(1);

            if (Preferences.ContainsKey(nameof(App.UserDetails)))
            {
                Preferences.Remove(nameof(App.UserDetails));
            }

            if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
            {
                Preferences.Remove(nameof(App.DefaultBudgetID));
            }

            if (Preferences.ContainsKey(nameof(App.IsFamilyAccount)))
            {
                Preferences.Remove(nameof(App.IsFamilyAccount));
            }

            if (Preferences.ContainsKey("IsTopStickyVisible"))
            {
                Preferences.Remove("IsTopStickyVisible");
            }

            if (await SecureStorage.Default.GetAsync("Session") != null)
            {
                SecureStorage.Default.Remove("Session");
            }

            App.DefaultBudgetID = 0;
            App.DefaultBudget = null;

            Application.Current!.MainPage = new AppShell();
            LocalNotificationCenter.Current.CancelAll();
            await Shell.Current.GoToAsync($"//{nameof(LoadUpPage)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "Logout_Tapped");
        }
    }    
    
    private async void ContactUs_Tapped(object sender, TappedEventArgs e)
    {
        try
        {

            var popup = new PopUpContactUs(new PopUpContactUsViewModel(_pt, IPlatformApplication.Current.Services.GetService<IRestDataService>()));            
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
            if (result is int)
            {

                int SupportID = (int)result;
                Action action = async () =>
                {
                    await Task.Delay(500);
                    await Shell.Current.GoToAsync($"../{nameof(ViewSupport)}?SupportID={SupportID}");
                    return;
                };
                await _pt.MakeSnackBar("We have received your inquiry", action, "View", new TimeSpan(0, 0, 10), "Success");
            }
            else if((string)result.ToString() == "Closed")
            {

            }
            else
            {
                await _pt.MakeSnackBar("Sorry something went wrong, inquiry not received.", null, null, new TimeSpan(0, 0, 10), "Danger");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "ContactUs_Tapped");
        }
    }    
    
    private async void Help_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var subscriptionUrl = "https://www.dbudgeting.com/about";

            if (await Launcher.CanOpenAsync(subscriptionUrl))
            {
                await Launcher.OpenAsync(subscriptionUrl);
            }
            else
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", "Sorry we weren't able to open the dBudget web site. You can visit us at www.dbudgeting.com", "OK");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "EditAccountDetails", "Help_Tapped");
        }
    }
}