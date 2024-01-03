using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Globalization;
using The49.Maui.BottomSheet;

namespace DailyBudgetMAUIApp;

public partial class MainPage : ContentPage
{
    private readonly MainPageViewModel _vm;
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;

    public MainPage(MainPageViewModel viewModel, IRestDataService ds, IProductTools pt)
	{
        _ds = ds;
        _pt = pt;

        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
 
    }

    protected async override void OnAppearing()
    {

        base.OnAppearing();

        await _pt.NavigateFromPendingIntent(Preferences.Get("NavigationType", ""));

        _vm.DefaultBudgetID = Preferences.Get(nameof(App.DefaultBudgetID), 1);
        if (_vm.DefaultBudgetID != 0)
        {
            App.DefaultBudgetID = _vm.DefaultBudgetID;
        }

        if (App.DefaultBudgetID != 0)
        {
            if (App.CurrentSettings == null || App.CurrentSettings.IsUpdatedFlag)
            {
                BudgetSettingValues Settings = _ds.GetBudgetSettingsValues(App.DefaultBudgetID).Result;
                App.CurrentSettings = Settings;
            }

            _pt.SetCultureInfo(App.CurrentSettings);
        }
        else
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-gb");
        }

        if (App.DefaultBudget == null || App.DefaultBudget.BudgetID == 0)
        {
            _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

            App.DefaultBudget = _vm.DefaultBudget;
            _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
            App.SessionLastUpdate = DateTime.UtcNow;
        }
        else
        {
            if (App.SessionLastUpdate == default(DateTime))
            {

                _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

                App.DefaultBudget = _vm.DefaultBudget;
                _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;

            }
            else
            {
                if (DateTime.UtcNow.Subtract(App.SessionLastUpdate) > new TimeSpan(0, 0, 3, 0))
                {
                    DateTime LastUpdated = _ds.GetBudgetLastUpdatedAsync(_vm.DefaultBudgetID).Result;

                    if (App.SessionLastUpdate < LastUpdated)
                    {
                        _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;
                        App.DefaultBudget = _vm.DefaultBudget;
                        _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                        App.SessionLastUpdate = DateTime.UtcNow;
                    }
                }
            }
        }

        if(App.DefaultBudget.IsCreated)
        {
            App.DefaultBudget = await _pt.BudgetDailyCycle(App.DefaultBudget);
            _vm.DefaultBudget = App.DefaultBudget;
        }

        if (!App.DefaultBudget.IsCreated && !App.HasVisitedCreatePage)
        {
            App.HasVisitedCreatePage = true;

            await Shell.Current.GoToAsync($"{nameof(CreateNewBudget)}?BudgetID={App.DefaultBudgetID}&NavigatedFrom=Budget Settings");
        }

        datetime.Text = _pt.GetBudgetLocalTime(DateTime.UtcNow).ToString("dd MM yyyy HH:mm:ss");

        if (_pt.GetBudgetLocalTime(DateTime.UtcNow).Hour > 12)
        {
            _vm.Title = $"Good afternoon {App.UserDetails.NickName}!";
        }
        else
        {
            _vm.Title = $"Good morning {App.UserDetails.NickName}!";
        }

        ProcessSnackBar();

    }

    private async void ProcessSnackBar()
    {
        string text;
        string actionButtonText;
        Action action;
        TimeSpan duration;

        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Warning", out var Warning);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Danger", out var Danger);
        Application.Current.Resources.TryGetValue("Info", out var Info);
        Application.Current.Resources.TryGetValue("White", out var White);

        var snackbarSuccessOptions = new SnackbarOptions
        {
            BackgroundColor = (Color)Success,
            TextColor = (Color)White,
            ActionButtonTextColor = (Color)White,
            CornerRadius = new CornerRadius(2),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(22),
            CharacterSpacing = 0.1
        };

        var snackbarInfoOptions = new SnackbarOptions
        {
            BackgroundColor = (Color)Info,
            TextColor = (Color)White,
            ActionButtonTextColor = (Color)White,
            CornerRadius = new CornerRadius(2),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(22),
            CharacterSpacing = 0.1
        };

        var snackbarWarningOptions = new SnackbarOptions
        {
            BackgroundColor = (Color)Warning,
            TextColor = (Color)White,
            ActionButtonTextColor = (Color)White,
            CornerRadius = new CornerRadius(2),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(22),
            CharacterSpacing = 0.1
        };

        var snackbarDangerOptions = new SnackbarOptions
        {
            BackgroundColor = (Color)Danger,
            TextColor = (Color)White,
            ActionButtonTextColor = (Color)White,
            CornerRadius = new CornerRadius(2),
            Font = Microsoft.Maui.Font.SystemFontOfSize(14),
            ActionButtonFont = Microsoft.Maui.Font.SystemFontOfSize(22),
            CharacterSpacing = 0.1
        };

        if (_vm.SnackBar == null || _vm.SnackBar == "")
        {

        }
        else
        {
            if(_vm.SnackBar == "Budget Created")
            {
                text=$"Hurrrrah, you have created a budget!";
                actionButtonText="Undo";
                int BudgetID = _vm.SnackID;
                action = async () => await UndoCreateBudget(BudgetID);
                duration = TimeSpan.FromSeconds(10);

                await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();
            }

            if(_vm.SnackBar == "Transaction Updated")
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;

                text = $"Nice one, transaction updated!";
                actionButtonText = "Ok";
                action = async() =>
                {
                    source.Cancel();
                };
                duration = TimeSpan.FromSeconds(10);

                var SnackBar = Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions);
                await SnackBar.Show(token);

                _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

                App.DefaultBudget = _vm.DefaultBudget;
                _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;
            }


            if (_vm.SnackBar == "Transaction Added")
            {
                text = $"Sweet, transaction created!";
                actionButtonText = "Undo";
                int TransactionID = _vm.SnackID;
                action = async () => await UndoAddTransaction(TransactionID);
                duration = TimeSpan.FromSeconds(10);

                await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

                App.DefaultBudget = _vm.DefaultBudget;
                _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;
            }

            _vm.SnackBar = "";
            _vm.SnackID = 0;
        }
    }
    private async Task UndoAddTransaction(int TransactionID)
    {
        Transactions Transaction = await _ds.GetTransactionFromID(TransactionID);
        Transaction.TransactionID = 0;

        await _ds.DeleteTransaction(TransactionID);

        await Shell.Current.GoToAsync($"{nameof(AddTransaction)}?BudgetID={_vm.DefaultBudgetID}",
                new Dictionary<string, object>
                {
                    ["Transaction"] = Transaction
                });
    }
    private async Task UndoCreateBudget(int BudgetID)
    {
        var popup = new PopUpPage();
        Application.Current.MainPage.ShowPopup(popup);

        List<PatchDoc> BudgetUpdate = new List<PatchDoc>();

        PatchDoc BudgetStage = new PatchDoc
        {
            op = "replace",
            path = "/IsCreated",
            value = false
        };

        BudgetUpdate.Add(BudgetStage);
        var PatchBudgetResult = await _ds.PatchBudget(BudgetID, BudgetUpdate);

        if(PatchBudgetResult == "OK")
        {
            await Shell.Current.GoToAsync($"{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Finalise Budget");
        }

    }

    private void ShareBudget_Tapped(object sender, TappedEventArgs e)
    {
        ShareBudgetRequest SBR = new ShareBudgetRequest
        {
            SharedBudgetID = _vm.DefaultBudgetID,
            IsVerified = false,
            SharedByUserEmail = App.UserDetails.Email
        };

        ShareBudget page = new ShareBudget(SBR, new RestDataService());

        page.Detents = new DetentsCollection()
        {
            new ContentDetent(),
            new FullscreenDetent()

        };

        page.HasBackdrop = true;
        page.CornerRadius = 30;
        
        page.ShowAsync();

    }


}

