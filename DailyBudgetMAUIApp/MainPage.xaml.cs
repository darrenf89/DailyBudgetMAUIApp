using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using DailyBudgetMAUIApp.Converters;
using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Pages.BottomSheets;
using DailyBudgetMAUIApp.ViewModels;
using Microsoft.Maui.Controls.Shapes;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Syncfusion.Maui.ProgressBar;
using Syncfusion.Maui.Scheduler;
using Syncfusion.Maui.Toolkit.Carousel;
using System.Globalization;
using The49.Maui.BottomSheet;
using static DailyBudgetMAUIApp.Pages.ViewAccounts;


namespace DailyBudgetMAUIApp;

public partial class MainPage : BasePage
{
    private readonly MainPageViewModel _vm;
    private readonly IRestDataService _ds;
    private readonly IProductTools _pt;
    public Picker SwitchBudgetPicker { get; set; }

    public MainPage(MainPageViewModel viewModel, IRestDataService ds, IProductTools pt)
	{
        _ds = ds;
        _pt = pt;

        InitializeComponent();

        this.BindingContext = viewModel;
        _vm = viewModel;
    }

    protected async override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;

        DateTime PermissionRequestExpiry = new();
        if (Preferences.ContainsKey(nameof(PermissionRequestExpiry)))
        {
            PermissionRequestExpiry = Preferences.Get(nameof(PermissionRequestExpiry), new DateTime());
        }

        if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false && DateTime.UtcNow > PermissionRequestExpiry)
        {
            bool IsNotificationEnabled = await LocalNotificationCenter.Current.RequestNotificationPermission();
            if (!IsNotificationEnabled)
            {
                Preferences.Set(nameof(PermissionRequestExpiry), DateTime.UtcNow.AddDays(60));
            }
        }

        if (App.CurrentPopUp != null)
        {
            await App.CurrentPopUp.CloseAsync();
            App.CurrentPopUp = null;
        }

        _vm.ReloadPage += async () =>
        {
            await LoadMainDashboardContent();

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }
        };
    }

    private async Task UpdateDefaultBudget()
    {
        List<Budgets> UserBudgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, "EditAccountSettings");
        App.DefaultBudget = UserBudgets[0];
        App.DefaultBudgetID = App.DefaultBudget.BudgetID;
        _vm.DefaultBudgetID = App.DefaultBudget.BudgetID;

        List<PatchDoc> UserUpdate = new List<PatchDoc>();

        PatchDoc DefaultBudgetID = new PatchDoc
        {
            op = "replace",
            path = "/DefaultBudgetID",
            value = App.DefaultBudgetID
        };

        UserUpdate.Add(DefaultBudgetID);

        PatchDoc PreviousDefaultBudgetID = new PatchDoc
        {
            op = "replace",
            path = "/PreviousDefaultBudgetID",
            value = 0
        };

        UserUpdate.Add(PreviousDefaultBudgetID);

        await _ds.PatchUserAccount(App.UserDetails.UserID, UserUpdate);
    }

    protected async override void OnAppearing()
    {        
        try
        {
            base.OnAppearing();

            ProcessSnackBar();

            _vm.DefaultBudget = null;
            _vm.DefaultBudgetID = Preferences.Get(nameof(App.DefaultBudgetID), 1);
            if (_vm.DefaultBudgetID != 0)
            {
                App.DefaultBudgetID = _vm.DefaultBudgetID;
            }            

            if (App.DefaultBudget == null || App.DefaultBudget.BudgetID == 0)
            {

                App.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;
                if (App.DefaultBudget.Error != null)
                {
                    if (App.DefaultBudget.Error.ErrorMessage == "Budget Not found")
                    {
                        await UpdateDefaultBudget();
                    }
                }
            }
            else
            {
                if (App.SessionLastUpdate == default(DateTime))
                {
                    App.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

                    if(App.DefaultBudget.Error != null)
                    {
                        if (App.DefaultBudget.Error.ErrorMessage == "Budget Not found")
                        {
                            await UpdateDefaultBudget();
                        }
                    }
                }
                else
                {
                    if (DateTime.UtcNow.Subtract(App.SessionLastUpdate) > new TimeSpan(0, 0, 3, 0))
                    {
                        DateTime? LastUpdated = _ds.GetBudgetLastUpdatedAsync(_vm.DefaultBudgetID).Result;
                        if(LastUpdated is null)
                        {
                            return;
                        }
                        if (App.SessionLastUpdate < LastUpdated)
                        {

                            App.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;
                            if (App.DefaultBudget.Error != null)
                            {
                                if (App.DefaultBudget.Error.ErrorMessage == "Budget Not found")
                                {
                                    await UpdateDefaultBudget();
                                }
                            }
                        }
                    }
                }
            }            

            if (App.DefaultBudgetID != 0)
            {
                BudgetSettingValues Settings = _ds.GetBudgetSettingsValues(App.DefaultBudgetID).Result;
                App.CurrentSettings = Settings;

                _pt.SetCultureInfo(App.CurrentSettings);
            }
            else
            {
                CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-gb");
            }


            _vm.DefaultBudget = App.DefaultBudget;
            _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
            App.SessionLastUpdate = DateTime.UtcNow;

            if (App.DefaultBudget.IsCreated)
            {
                App.DefaultBudget = await _pt.BudgetDailyCycle(App.DefaultBudget);
                _vm.DefaultBudget = App.DefaultBudget;
            }

            if (!App.DefaultBudget.IsCreated && !App.HasVisitedCreatePage)
            {
                App.HasVisitedCreatePage = true;

                await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={App.DefaultBudgetID}&NavigatedFrom=Budget Settings");
                return;
            }

            await LoadMainDashboardContent();
            await RegisterForWeakMessages();

            if (App.CurrentPopUp != null)
            {
                await App.CurrentPopUp.CloseAsync();
                App.CurrentPopUp = null;
            }

        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "OnAppearing");
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        base.OnNavigatedFrom(args);
        
    }

    private async Task RegisterForWeakMessages()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);

        WeakReferenceMessenger.Default.Register<UpdateViewAccount>(this, async (r, m) =>
        {
            try
            {
                if(!m._isBackground)
                {               
                    if (App.CurrentPopUp == null)
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                    }
                }

                await Task.Delay(1);

                await _ds.ReCalculateBudget(App.DefaultBudgetID);
                App.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;
                _vm.DefaultBudget = App.DefaultBudget;

                if (!m._isBackground)
                {
                    await LoadMainDashboardContent(); 
                }

                if (!m._isBackground)
                {
                    if (App.CurrentPopUp != null)
                    {
                        await App.CurrentPopUp.CloseAsync();
                        App.CurrentPopUp = null;
                    } 
                }
            }
            catch
            {

            }
        });
    }

    private async Task LoadMainDashboardContent()
    {
        if (_pt.GetBudgetLocalTime(DateTime.UtcNow).Hour > 12)
        {
            _vm.Title = $"Good Afternoon {App.UserDetails.NickName}!";
        }
        else
        {
            _vm.Title = $"Good Morning {App.UserDetails.NickName}!";
        }

        _vm.IsPreviousBudget = App.UserDetails.PreviousDefaultBudgetID != 0;

        if (_vm.DefaultBudget.IsCreated)
        {
            _vm.EnvelopeStats = new EnvelopeStats(_vm.DefaultBudget.Savings);

            if (_vm.EnvelopeStats.NumberOfEnvelopes == 0)
            {
                brdYourEnvelopes.IsVisible = false;
            }
            else
            {
                brdYourEnvelopes.IsVisible = true;
            }

            SavingCarousel.Children.Clear();
            SavingCarouselIdent.Children.Clear();
            if (_vm.DefaultBudget.Savings.Where(s => s.IsRegularSaving).ToList().Count() != 0)
            {
                absSaving.IsVisible = true;
                _vm.SavingCarousel = await CreateSavingCarousel();
                SavingCarousel.Children.Add(_vm.SavingCarousel);
            }
            else
            {
                absSaving.IsVisible = false;
            }

            BillCarousel.Children.Clear();
            BillCarouselIdent.Children.Clear();
            if (_vm.DefaultBudget.Bills.Count() != 0)
            {
                absBill.IsVisible = true;
                _vm.BillCarousel = await CreateBillCarousel();
                BillCarousel.Children.Add(_vm.BillCarousel);
            }
            else
            {
                absBill.IsVisible = false;
            }

            IncomeCarousel.Children.Clear();
            IncomeCarouselIdent.Children.Clear();
            if (_vm.DefaultBudget.IncomeEvents.Any())
            {
                absIncome.IsVisible = true;
                _vm.IncomeCarousel = await CreateIncomeCarousel();
                IncomeCarousel.Children.Add(_vm.IncomeCarousel);
            }
            else
            {
                absIncome.IsVisible = false;
            }

            _vm.RecentTransactions.Clear();
            List<Transactions> RecentTrans = await _ds.GetRecentTransactions(_vm.DefaultBudgetID, 6, "MainPage");

            if (RecentTrans.Count() > 0)
            {
                foreach (Transactions T in RecentTrans)
                {
                    _vm.RecentTransactions.Add(T);
                }
            }


            _vm.MaxBankBalance = _vm.DefaultBudget.BankBalance.GetValueOrDefault();
            _vm.MaxBankBalance += _vm.DefaultBudget.CurrentActiveIncome;

            decimal Amount = 0;
            //entTransactionAmount.Text = Amount.ToString("c", CultureInfo.CurrentCulture);

            int Days = (int)Math.Ceiling((_vm.DefaultBudget.NextIncomePayday.GetValueOrDefault().Date - _pt.GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);
            if (Days == 1)
            {
                _vm.FutureDailySpend = -1;
            }
            else
            {
                _vm.FutureDailySpend = (decimal)(_vm.DefaultBudget.LeftToSpendBalance.GetValueOrDefault() / (Days - 1));
            }

            List<Categories> CategoryList = await _ds.GetAllHeaderCategoryDetailsFull(App.DefaultBudgetID);
            await LoadCategoryChartData(CategoryList, false);

            _vm.Payees = await _ds.GetPayeeListFull(App.DefaultBudgetID);
            _vm.Payees = _vm.Payees.OrderByDescending(p => p.PayeeSpendAllTime).ToList();
            _vm.CurrentPayeeOffset = 0;
            _vm.IsUnreadMessage = _vm.DefaultBudget.AccountInfo.NumberUnreadMessages > 0;
            await LoadPayeeChartData();
            await LoadBudgetCalendar();
        }
    }

    private async Task LoadBudgetCalendar()
    {
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        _vm.EventList.Clear();

        DateTime MaxDate = DateTime.UtcNow.AddMonths(1);
        DateTime BudgetDate = _vm.DefaultBudget.NextIncomePayday.GetValueOrDefault();

        Scheduler.HeaderView.TextStyle = new SchedulerTextStyle
        {
            TextColor = (Color)Primary,
            FontSize = 25,
            FontFamily = "OpenSansSemibold"
            
        };

        Scheduler.MinimumDateTime = DateTime.UtcNow;
        Scheduler.MaximumDateTime = MaxDate;

        while (BudgetDate < MaxDate.AddDays(30))
        {
            SchedulerAppointment PayDay = new SchedulerAppointment
            {
                StartTime = BudgetDate.Date,
                EndTime = BudgetDate.Date.AddMinutes(1439),
                Subject = $"Getting paid {_vm.DefaultBudget.PaydayAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)}",
                IsReadOnly = true,
                Background = (Color)Success,
                TextColor = (Color)White,
                Notes = "PayDay",
                Id = 0
            };

            _vm.EventList.Add(PayDay);

            foreach (Savings saving in _vm.DefaultBudget.Savings.Where(s => !s.IsRegularSaving))
            {
                SchedulerAppointment EnvelopeEvent = new SchedulerAppointment
                {
                    StartTime = BudgetDate.Date,
                    EndTime = BudgetDate.Date.AddMinutes(1439),
                    Subject = $"Putting {saving.PeriodSavingValue.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} away for {saving.SavingsName}",
                    IsReadOnly = true,
                    Background = App.ChartColor[2],
                    TextColor = (Color)White,
                    Notes = "Envelope",
                    Id = saving.SavingID
                };

                _vm.EventList.Add(EnvelopeEvent);
            }

            BudgetDate = _pt.CalculateNextDate(BudgetDate, _vm.DefaultBudget.PaydayType, _vm.DefaultBudget.PaydayValue.GetValueOrDefault(), _vm.DefaultBudget.PaydayDuration);
        }

        foreach (Transactions transaction in _vm.DefaultBudget.Transactions.Where(s => !s.IsTransacted))
        {
            SchedulerAppointment TransactionEvent = new SchedulerAppointment
            {
                StartTime = transaction.TransactionDate.GetValueOrDefault().Date,
                EndTime = transaction.TransactionDate.GetValueOrDefault().Date.AddMinutes(1439),
                Subject = $"Future Transaction",
                IsReadOnly = true,
                Background = App.ChartColor[5],
                TextColor = (Color)White,
                Notes = "Transaction",
                Id = transaction.TransactionID
            };

            _vm.EventList.Add(TransactionEvent);
        }

        foreach (Savings saving in _vm.DefaultBudget.Savings.Where(s => s.IsRegularSaving))
        {
            if (saving.SavingsType != "SavingsBuilder")
            {
                SchedulerAppointment SavingEvent = new SchedulerAppointment
                {
                    StartTime = saving.GoalDate.GetValueOrDefault().Date,
                    EndTime = saving.GoalDate.GetValueOrDefault().Date.AddMinutes(1439),
                    Subject = $"Saving {saving.SavingsGoal.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} for {saving.SavingsName}",
                    IsReadOnly = true,
                    Background = App.ChartColor[1],
                    TextColor = (Color)White,
                    Notes = "Saving",
                    Id = saving.SavingID
                };

                _vm.EventList.Add(SavingEvent);
            }
        }

        foreach (Bills bill in _vm.DefaultBudget.Bills)
        {
            DateTime BillDate = bill.BillDueDate.GetValueOrDefault();

            while (BillDate <= MaxDate)
            {
                SchedulerAppointment BillEvent = new SchedulerAppointment
                {
                    StartTime = BillDate.Date,
                    EndTime = BillDate.Date.AddMinutes(1439),
                    Subject = $"{bill.BillAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} outgoing for {bill.BillName}",
                    IsReadOnly = true,
                    Background = App.ChartColor[3],
                    TextColor = (Color)White,
                    Notes = "Bill",
                    Id = bill.BillID
                };

                _vm.EventList.Add(BillEvent);

                if (bill.IsRecuring)
                {
                    BillDate = _pt.CalculateNextDate(BillDate, bill.BillType, bill.BillValue.GetValueOrDefault(), bill.BillDuration);
                }
                else
                {
                    break;
                }
            }
        }

        foreach (IncomeEvents income in _vm.DefaultBudget.IncomeEvents)
        {
            DateTime IncomeDate = income.DateOfIncomeEvent;

            while (IncomeDate <= MaxDate)
            {
                SchedulerAppointment IncomeEvent = new SchedulerAppointment
                {
                    StartTime = IncomeDate.Date,
                    EndTime = IncomeDate.Date.AddMinutes(1439),
                    Subject = $"{income.IncomeAmount.ToString("c", CultureInfo.CurrentCulture)} income for {income.IncomeName}",
                    IsReadOnly = true,
                    Background = App.ChartColor[4],
                    TextColor = (Color)White,
                    Notes = "Income",
                    Id = income.IncomeEventID
                };

                _vm.EventList.Add(IncomeEvent);

                if (income.IsRecurringIncome)
                {
                    IncomeDate = _pt.CalculateNextDate(IncomeDate, income.RecurringIncomeType, income.RecurringIncomeValue.GetValueOrDefault(), income.RecurringIncomeDuration);
                }
                else
                {
                    break;
                }
            }
        }

    }

    private async Task LoadPayeeChartData()
    {
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);

        _vm.PayeesChart.Clear();
        PayeeLegend.Children.Clear();
        PreviousNextPayee.Children.Clear();

        int MaxIndex = _vm.CurrentPayeeOffset + 8 >= _vm.Payees.Count() ? _vm.Payees.Count() - _vm.CurrentPayeeOffset : 8;

        List<Payees> Payees = _vm.Payees.GetRange(_vm.CurrentPayeeOffset, MaxIndex);

        int i = 0;
        decimal TotalValue = 0;
        foreach (Payees payee in Payees)
        {
            ChartClass Value = new ChartClass
            {
                XAxesString = payee.Payee,
                YAxesDouble = (double)payee.PayeeSpendPayPeriod
            };

            TotalValue += payee.PayeeSpendPayPeriod;

            _vm.PayeesChart.Add(Value);

            Border border = new Border
            {
                BackgroundColor = App.ChartColor[i],
                Stroke = (Brush)PrimaryBrush,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(4)
                },
                Margin = new Thickness(0, 2, 10, 2),
                Padding = new Thickness(10, 0, 0, 0)
            };

            Label label = new Label
            {
                Text = payee.Payee,
                TextColor = (Color)White,
                FontSize = 16,
                Padding = new Thickness(0, 8, 0, 8)
            };

            border.Content = label;

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();

            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += async (s, e) =>
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
                await Task.Delay(1000);
                FilterModel Filters = new FilterModel
                {
                    PayeeFilter = new List<string>
                        {
                            payee.Payee
                        }
                };

                await Shell.Current.GoToAsync($"/{nameof(ViewFilteredTransactions)}",
                    new Dictionary<string, object>
                    {
                        ["Filters"] = Filters
                    });
                return;
            };

            border.GestureRecognizers.Add(TapGesture);
            PayeeLegend.Children.Add(border);

            i++;
        }

        if(TotalValue == 0)
        {
            _vm.PayeeChartVisible = false;
        }
        else
        {
            _vm.PayeeChartVisible = true;
        }

        if(_vm.CurrentPayeeOffset >= 8)
        {
            HorizontalStackLayout HSLPrevious = new HorizontalStackLayout
            {
                Padding = new Thickness(10,0,0,5)
            };

            Image PreviousImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0,0,10,0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5c4",
                    Size = 25,
                    Color = (Color)Tertiary,
                }
            };

            TapGestureRecognizer PreviousImageTapGesture = new TapGestureRecognizer();
            PreviousImageTapGesture.NumberOfTapsRequired = 1;
            PreviousImageTapGesture.Tapped += async (s, e) =>
            {
                _vm.CurrentPayeeOffset -= 8;
                await LoadPayeeChartData();
            };

            HSLPrevious.GestureRecognizers.Add(PreviousImageTapGesture);
            HSLPrevious.Children.Add(PreviousImage);

            Label PreviousLabel = new Label
            {
                Text = "Previous",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                TextColor = (Color)Tertiary,
                FontSize = 18,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                FontAttributes = FontAttributes.Bold
            };

            HSLPrevious.Children.Add(PreviousLabel);
            PreviousNextPayee.Add(HSLPrevious, 0, 0);
        }

        if(_vm.CurrentPayeeOffset + 8 < _vm.Payees.Count())
        {
            HorizontalStackLayout HSLNext = new HorizontalStackLayout
            {
                Padding = new Thickness(0, 0, 0, 5),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };

            Image NextImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(10, 0, 10, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue5c8",
                    Size = 25,
                    Color = (Color)Tertiary,
                }
            };

            TapGestureRecognizer NextImageTapGesture = new TapGestureRecognizer();
            NextImageTapGesture.NumberOfTapsRequired = 1;
            NextImageTapGesture.Tapped += async (s, e) =>
            {
                _vm.CurrentPayeeOffset += 8;
                await LoadPayeeChartData();
            };

            HSLNext.GestureRecognizers.Add(NextImageTapGesture);
            

            Label NextLabel = new Label
            {
                Text = "Next",
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                TextColor = (Color)Tertiary,
                FontSize = 18,
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                FontAttributes = FontAttributes.Bold
            };

            HSLNext.Children.Add(NextLabel);
            HSLNext.Children.Add(NextImage);

            PreviousNextPayee.Add(HSLNext, 1, 0);
        }
    }

    private async Task LoadCategoryChartData(List<Categories> CategoryList, bool IsBackButton)
    {
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("White", out var White);

        _vm.CategoriesChart.Clear();
        CategoryLegend.Children.Clear();

        if(IsBackButton)
        {
            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(0, 2, 0, 2),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue31b",
                    Size = 32,
                    Color = (Color)Primary,
                }
            };

            TapGestureRecognizer ImageTapGesture = new TapGestureRecognizer();
            ImageTapGesture.NumberOfTapsRequired = 1;
            ImageTapGesture.Tapped += async (s, e) =>
            {
                List<Categories> CategoryList = _ds.GetAllHeaderCategoryDetailsFull(App.DefaultBudgetID).Result;
                await LoadCategoryChartData(CategoryList, false);
            };

            image.GestureRecognizers.Add(ImageTapGesture);
            CategoryLegend.Children.Add(image);
        }

        int i = 0;
        decimal TotalValue = 0;
        foreach (Categories cat in CategoryList)
        {
            if ((IsBackButton && cat.IsSubCategory) || !IsBackButton)
            {
                ChartClass Value = new ChartClass
                {
                    XAxesString = cat.CategoryName,
                    YAxesDouble = (double)cat.CategorySpendPayPeriod
                };

                TotalValue += cat.CategorySpendPayPeriod;

                _vm.CategoriesChart.Add(Value);

                Border border = new Border
                {
                    BackgroundColor = App.ChartColor[i],
                    Stroke = (Brush)PrimaryBrush,
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(4)
                    },
                    Margin = new Thickness(0, 2, 10, 2),
                    Padding = new Thickness(10, 0, 0, 0)
                };

                Label label = new Label
                {
                    Text = cat.CategoryName,
                    TextColor = (Color)White,
                    FontSize = 16,
                    Padding = new Thickness(0, 8, 0, 8)
                };

                border.Content = label;

                TapGestureRecognizer TapGesture = new TapGestureRecognizer();

                if (cat.IsSubCategory)
                {
                    TapGesture.NumberOfTapsRequired = 1;
                    TapGesture.Tapped += async (s, e) =>
                    {
                        var PopUp = new PopUpPage();
                        App.CurrentPopUp = PopUp;
                        Application.Current.Windows[0].Page.ShowPopup(PopUp);
                        await Task.Delay(1000);
                        FilterModel Filters = new FilterModel
                        {
                            CategoryFilter = new List<int>
                            {
                                cat.CategoryID
                            }
                        };

                        await Shell.Current.GoToAsync($"/{nameof(ViewFilteredTransactions)}",
                            new Dictionary<string, object>
                            {
                                ["Filters"] = Filters
                            });
                        return;
                    };
                }
                else
                {
                    TapGesture.NumberOfTapsRequired = 1;
                    TapGesture.Tapped += async (s, e) =>
                    {
                        List<Categories> CategoryList = _ds.GetHeaderCategoryDetailsFull(cat.CategoryID, App.DefaultBudgetID).Result;
                        await LoadCategoryChartData(CategoryList, true);
                    };
                }

                border.GestureRecognizers.Add(TapGesture);

                CategoryLegend.Children.Add(border);
                i++;
            }
        }

        if (TotalValue == 0)
        {
            _vm.CategoryChartVisible = false;
        }
        else
        {
            _vm.CategoryChartVisible = true;
        }
    }

    void TransactionAmount_Changed(object sender, TextChangedEventArgs e)
    {
        //decimal Amount = (decimal)_pt.FormatCurrencyNumber(e.NewTextValue);
        //entTransactionAmount.Text = Amount.ToString("c", CultureInfo.CurrentCulture);
        //int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
        //if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entTransactionAmount.CursorPosition > position)
        //{
        //    entTransactionAmount.CursorPosition = entTransactionAmount.Text.Length;
        //}
        //_vm.TransactionAmount = Amount;
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

        if (string.IsNullOrEmpty(_vm.SnackBar))
        {

        }
        else
        {
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            switch (_vm.SnackBar)
            {       
                case "Budget Created":

                    text = $"Hurrrrah, you have created a budget!";
                    actionButtonText = "Undo";
                    int BudgetID = _vm.SnackID;
                    action = async () => await UndoCreateBudget(BudgetID);
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();
                    break;

                case "Transaction Updated":

                    text = $"Nice one, transaction updated!";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        source.Cancel();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    var SnackBar = Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions);
                    await SnackBar.Show(token);


                    break;

                case "Transaction Added":

                    text = $"Sweet, transaction created!";
                    actionButtonText = "Undo";
                    int TransactionID = _vm.SnackID;
                    action = async () => await UndoAddNew(TransactionID, "Transaction");
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                    break;

                case "Saving Added":

                    text = $"Congrats, you have a new saving goal";
                    actionButtonText = "Undo";
                    int SavingID = _vm.SnackID;
                    action = async () => await UndoAddNew(SavingID, "Saving");
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                    break;

                case "Bill Added":

                    text = $"Ah no not another Outgoing!";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        source.Cancel();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                    break;

                case "Income Added":

                    text = $"Congrats, you have added a new income";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        source.Cancel();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarSuccessOptions).Show();

                    break;

                case "BudgetSettingsUpdated":

                    text = $"Budget settings successfully updated";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        source.Cancel();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarInfoOptions).Show();

                    BudgetSettingValues Settings = _ds.GetBudgetSettingsValues(App.DefaultBudgetID).Result;
                    App.CurrentSettings = Settings;
                    _pt.SetCultureInfo(App.CurrentSettings);

                    break;
                case "BudgetDeleted":

                    text = $"Budget has been deleted";
                    actionButtonText = "Ok";
                    action = async () =>
                    {
                        source.Cancel();
                    };
                    duration = TimeSpan.FromSeconds(10);

                    await Snackbar.Make(text, action, actionButtonText, duration, snackbarDangerOptions).Show();

                    break;

                default:
                    break;
            }

            _vm.SnackBar = "";
            _vm.SnackID = 0;

            _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

            App.DefaultBudget = _vm.DefaultBudget;
            _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
            App.SessionLastUpdate = DateTime.UtcNow;

            await LoadMainDashboardContent();
        }
    }

    private async Task UndoAddNew(int ID, string Type)
    {
        try
        {
            if (Type == "Transaction")
            {
                Transactions Transaction = await _ds.GetTransactionFromID(ID);
                Transaction.TransactionID = 0;

                await _ds.DeleteTransaction(ID);

                await Shell.Current.GoToAsync($"{nameof(MainPage)}/{nameof(AddTransaction)}?BudgetID={_vm.DefaultBudgetID}&NavigatedFrom=ViewMainPage",
                        new Dictionary<string, object>
                        {
                            ["Transaction"] = Transaction
                        });
            }
            else if (Type == "Saving")
            {
                Savings saving = await _ds.GetSavingFromID(ID);
                saving.SavingID = 0;

                await _ds.DeleteSaving(ID);

                await Shell.Current.GoToAsync($"///{nameof(MainPage)}/{nameof(AddSaving)}?BudgetID={_vm.DefaultBudgetID}&SavingID={-1}",
                        new Dictionary<string, object>
                        {
                            ["Saving"] = saving
                        });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "UndoAddNew");
        }

    }
    private async Task UndoCreateBudget(int BudgetID)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

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
                await Shell.Current.GoToAsync($"/{nameof(CreateNewBudget)}?BudgetID={BudgetID}&NavigatedFrom=Finalise Budget");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "UndoCreateBudget");
        }
    }

    private void ShareBudget_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            ShareBudgetRequest SBR = new ShareBudgetRequest
            {
                SharedBudgetID = _vm.DefaultBudgetID,
                IsVerified = false,
                SharedByUserEmail = App.UserDetails.Email,
                RequestInitiated = DateTime.UtcNow
            };

            ShareBudget page = new ShareBudget(SBR, _ds, _pt);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent(),
                new FullscreenDetent()

            };

            page.HasBackdrop = true;
            page.CornerRadius = 30;

            App.CurrentBottomSheet = page;
            page.ShowAsync();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MainPage", "ShareBudget_Tapped");
        }
    }

    private void ViewShareBudget_Tapped(object sender, TappedEventArgs e)
    {


    }

    private async void VerifyBudgetShare_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var popup = new PopUpOTP(_vm.DefaultBudget.AccountInfo.BudgetShareRequestID, new PopUpOTPViewModel(_ds, _pt), "ShareBudget", _pt, _ds);
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

            if ((string)result.ToString() != "User Closed")
            {
                ShareBudgetRequest BudgetRequest = (ShareBudgetRequest)result;

                bool DefaultBudgetYesNo = await Application.Current.Windows[0].Page.DisplayAlert($"Update Default Budget ", $"CONGRATS!! You have shared a budget with {BudgetRequest.SharedByUserEmail}, do you want to make this budget your default Budget?", "Yes, continue", "No Thanks!");

                if (DefaultBudgetYesNo)
                {
                    await _pt.ChangeDefaultBudget(App.UserDetails.UserID, BudgetRequest.SharedBudgetID, true);
                }
            }
        }
        catch (Exception ex)
        {
            Page TopPage = Shell.Current.Navigation.NavigationStack[Shell.Current.Navigation.NavigationStack.Count - 1];
            if (TopPage.GetType() != typeof(NoNetworkAccess) && TopPage.GetType() != typeof(ErrorPage))
            {
                await _pt.HandleException(ex, "MainPage", "VerifyBudgetShare_Tapped");
            }
        }
    }

    private void NoMoneyLeft_Tapped(object sender, TappedEventArgs e)
    {

    }

    private void YourTransactionsOption_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            TransactionOptionsBottomSheet page = new TransactionOptionsBottomSheet(_ds, _pt);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            page.ShowAsync();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MainPage", "YourTransactionsOption_Tapped");
        }
    }


    private void BudgetMoreOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            BudgetOptionsBottomSheet page = new BudgetOptionsBottomSheet(_vm.DefaultBudget, _pt,_ds);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            page.ShowAsync();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MainPage", "BudgetMoreOptions_Tapped");
        }
    }

    private void EnvelopeSavingsMoreOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            EnvelopeOptionsBottomSheet page = new EnvelopeOptionsBottomSheet(_ds, _pt);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            page.ShowAsync();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MainPage", "EnvelopeSavingsMoreOptions_Tapped");
        }

    }

    private async Task<SfCarousel> CreateSavingCarousel()
    {
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("Gray100", out var Gray100);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Info", out var Info);

        DataTemplate dt = new DataTemplate(() =>
        {
            Border border = new Border
            {
                Stroke = (Brush)PrimaryBrush,
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(4)
                },
                BackgroundColor = (Color)White
            };

            Grid grid = new Grid
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width = new GridLength(45)},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2)-50)},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2)+50)}
                },
                RowDefinitions =
                {
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)}
                }

            };

            Image ClickImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 5, 5, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue25d",
                    Size = 40,
                    Color = (Color)Primary,
                }
            };

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();
            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += (s, e) =>
            {
                SavingsMoreOptions_Tapped(s, e);
            };

            ClickImage.GestureRecognizers.Add(TapGesture);

            //grid.AddWithSpan(ClickImage, 0, 2, 2, 1);


            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10, 15, 0, 0),
                //Source = ImageSource.FromFile("saving.svg"),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue2eb",
                    Size = 45,
                    Color = (Color)Primary,
                },
                WidthRequest = 28
            };
            grid.AddWithSpan(image, 0, 0, 6, 1);

            Label lblTitle = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 21,
                Padding = new Thickness(10, 5, 0, 0),
                TextColor = (Color)Primary,
                Margin = new Thickness(0)
            };
            lblTitle.SetBinding(Label.TextProperty, "SavingsName");
            grid.AddWithSpan(lblTitle, 0, 1, 1, 2);

            Label lblSavingType = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 0),
                TextColor = (Color)Tertiary,
                Margin = new Thickness(0),
                CharacterSpacing = 0
            };
            lblSavingType.SetBinding(Label.TextProperty, "SavingsType", BindingMode.Default, new SavingTypeConverter());
            grid.AddWithSpan(lblSavingType, 1, 1, 1, 2);

            Label lblCurrentBalance = new Label
            {
                FontSize = 16,
                Padding = new Thickness(10, 10, 0, 0),
                TextColor = (Color)Gray900,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0)
            };
            lblCurrentBalance.SetBinding(Label.TextProperty, "CurrentBalance", BindingMode.Default, new DecimalToCurrencyString());
            grid.Add(lblCurrentBalance, 1, 2);

            Label lblBalance = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 10),
                TextColor = (Color)Gray400,
                CharacterSpacing = 0,
                Text = "Current Saving Balance",
                Margin = new Thickness(0)
            };
            grid.AddWithSpan(lblBalance, 3, 1, 1, 2);

            SfLinearProgressBar ProgressBar = new SfLinearProgressBar
            {
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = _vm.ProgressBarCarWidthRequest,
                TrackFill = (Color)Gray100,
                ProgressFill = (Color)Success,
                TrackHeight = 10,
                TrackCornerRadius = 5,
                ProgressCornerRadius = 5,
                ProgressHeight = 10,
                Margin = new Thickness(10, 0, 10, 5),
                Minimum = 0
            };
            ProgressBar.SetBinding(SfLinearProgressBar.MaximumProperty, "SavingProgressBarMax");
            ProgressBar.SetBinding(SfLinearProgressBar.ProgressProperty, "CurrentBalance");
            grid.AddWithSpan(ProgressBar, 4, 1, 1, 2);

            Label lblSavingGoalText = new Label
            {
                FontSize = 12,
                Padding = new Thickness(10, 0, 10, 0),
                TextColor = (Color)Tertiary,
                CharacterSpacing = 0,
                HorizontalTextAlignment = TextAlignment.End,
                Text = "Saving Goal"
            };
            grid.AddWithSpan(lblSavingGoalText, 5, 2, 1, 1);

            Label SavingProgressBarMaxString = new Label
            {
                FontSize = 12,
                Padding = new Thickness(10, 0, 10, 5),
                TextColor = (Color)Gray900,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.End
            };
            SavingProgressBarMaxString.SetBinding(Label.TextProperty, "SavingProgressBarMaxString");
            grid.Add(SavingProgressBarMaxString, 2, 6);

            BoxView bv = new BoxView
            {
                Color = (Color)Gray100,
                HeightRequest = 2,
                Margin = new Thickness(10, 5, 10, 10)
            };
            grid.AddWithSpan(bv, 7, 1, 1, 2);

            HorizontalStackLayout hsl1 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelOne = new Label
            {
                Text = "Daily Saving Amount | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl1.Children.Add(labelOne);

            Label labelTwo = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,

            };
            labelTwo.SetBinding(Label.TextProperty, "RegularSavingValueString");

            hsl1.Children.Add(labelTwo);
            grid.AddWithSpan(hsl1, 8, 1, 1, 2);

            HorizontalStackLayout hsl2 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelThree = new Label
            {
                Text = "Saving Goal Date | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl2.Children.Add(labelThree);

            Label labelFour = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
            };
            labelFour.SetBinding(Label.TextProperty, "SavingGoalDateString");

            hsl2.Children.Add(labelFour);
            grid.AddWithSpan(hsl2, 9, 1, 1, 2);


            border.Content = grid;

            return border;
        });

        SfCarousel sc = new SfCarousel
        {
            ScaleOffset = (float)0.9,
            RotationAngle = 20,
            Duration = 1000,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = _vm.SignOutButtonWidth + 10,
            ItemWidth = (int)Math.Ceiling(_vm.SignOutButtonWidth) - 10,
            ItemHeight = 250,
            ItemSpacing = 20
        };

        sc.ItemTemplate = dt;
        sc.ItemsSource = _vm.DefaultBudget.Savings.Where(s => s.IsRegularSaving).Take(7).ToList();


        if (sc.ItemsSource.Any())
        {
            if ((sc.ItemsSource.Count() % 2) == 0)
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2) - 1;
            }
            else
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2);
            }
        }

        sc.SwipeStarted += async (s, e) =>
        {
            await CarouselSwipeStarted(s, e);
        };

        sc.SwipeEnded += async (s, e) =>
        {
            await CarouselSwipeEnded(s, e);
        };

        for (int i = 0; i < sc.ItemsSource.Count(); i++)
        {
            Border button = new Border
            {
                HeightRequest = 10,
                WidthRequest = 10,
                Margin = new Thickness(2, 0, 2, 0),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5)
                }
            };

            if (i == sc.SelectedIndex)
            {
                button.BackgroundColor = (Color)PrimaryLight;
            }
            else
            {
                button.BackgroundColor = (Color)Gray100;
            }

            SavingCarouselIdent.Children.Add(button);


        }

        return sc;
    }

    private async Task<SfCarousel> CreateBillCarousel()
    {
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("Gray100", out var Gray100);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Info", out var Info);

        DataTemplate dt = new DataTemplate(() =>
        {
            Border border = new Border
            {
                Stroke = (Brush)PrimaryBrush,
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(4)
                },
                BackgroundColor = (Color)White
            };

            Grid grid = new Grid
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width = new GridLength(45)},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2)-50)},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2)+50)}
                },
                RowDefinitions =
                {
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)}
                }
                
            };

            Image ClickImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 5, 5, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue25d",
                    Size = 40,
                    Color = (Color)Primary,                    
                }
            };

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();
            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += (s, e) =>
            {
                SavingsMoreOptions_Tapped(s,e);
            };
            
            ClickImage.GestureRecognizers.Add(TapGesture);

            //grid.AddWithSpan(ClickImage, 0, 2, 2, 1);


            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10,15,0,0),
                //Source = ImageSource.FromFile("saving.svg"),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\uef6e",
                    Size = 45,
                    Color = (Color)Primary,                    
                },
                WidthRequest = 28
            };
            grid.AddWithSpan(image,0,0,6,1);

            Label lblTitle = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 21,
                Padding = new Thickness(10,5,0,0),
                TextColor = (Color)Primary,
                Margin = new Thickness(0)
            };
            lblTitle.SetBinding(Label.TextProperty, "BillName");
            grid.AddWithSpan(lblTitle, 0, 1, 1, 2);

            Label lblSavingType = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 0),
                TextColor = (Color)Tertiary,
                Margin = new Thickness(0),
                CharacterSpacing = 0
            };
            lblSavingType.SetBinding(Label.TextProperty, "IsRecuring", BindingMode.Default,new BillTypeConverter());
            grid.AddWithSpan(lblSavingType, 1, 1, 1, 2);

            Label lblCurrentBalance = new Label
            {
                FontSize = 16,
                Padding = new Thickness(10, 10, 0, 0),
                TextColor = (Color)Gray900,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0)
            };
            lblCurrentBalance.SetBinding(Label.TextProperty, "BillCurrentBalance", BindingMode.Default, new DecimalToCurrencyString());
            grid.Add(lblCurrentBalance, 1, 2);

            Label lblBalance = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 10),
                TextColor = (Color)Gray400,
                CharacterSpacing = 0,
                Text = "Current Balance",
                Margin = new Thickness(0)
            };
            grid.AddWithSpan(lblBalance, 3, 1,1,2);

            SfLinearProgressBar ProgressBar = new SfLinearProgressBar
            {
                HorizontalOptions = LayoutOptions.Start,
                WidthRequest = _vm.ProgressBarCarWidthRequest,
                TrackFill = (Color)Gray100,
                ProgressFill = (Color)Success,
                TrackHeight = 10,
                TrackCornerRadius = 5,
                ProgressCornerRadius = 5,
                ProgressHeight = 10,
                Margin = new Thickness(10,0,10,5),
                Minimum = 0
            };
            ProgressBar.SetBinding(SfLinearProgressBar.MaximumProperty, "BillAmount");
            ProgressBar.SetBinding(SfLinearProgressBar.ProgressProperty, "BillCurrentBalance");
            grid.AddWithSpan(ProgressBar, 4, 1, 1, 2);

            Label lblSavingGoalText = new Label
            {
                FontSize = 12,
                Padding = new Thickness(10, 0, 10, 0),
                TextColor = (Color)Tertiary,
                CharacterSpacing = 0,
                HorizontalTextAlignment = TextAlignment.End,
                Text = "Outgoing Amount Due"
            };
            grid.AddWithSpan(lblSavingGoalText, 5, 2, 1, 1);

            Label SavingProgressBarMaxString = new Label
            {
                FontSize = 12,
                Padding = new Thickness(10, 0, 10, 5),
                TextColor = (Color)Gray900,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.End
            };
            SavingProgressBarMaxString.SetBinding(Label.TextProperty, "BillAmount", BindingMode.Default, new DecimalToCurrencyString());
            grid.Add(SavingProgressBarMaxString, 2, 6);

            BoxView bv = new BoxView
            {
                Color = (Color)Gray100,
                HeightRequest = 2,
                Margin = new Thickness(10, 5, 10, 10)
            };
            grid.AddWithSpan(bv, 7, 1,1,2);

            HorizontalStackLayout hsl1 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelOne = new Label
            {
                Text = "Due Date | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl1.Children.Add(labelOne);

            Label labelTwo = new Label
            {                
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,

            };
            labelTwo.SetBinding(Label.TextProperty, "BillDueDate", BindingMode.Default, new BillDueDate());

            hsl1.Children.Add(labelTwo);

            grid.AddWithSpan(hsl1, 9, 1, 1, 2);

            HorizontalStackLayout hsl2 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelThree = new Label
            {
                Text = "Daily Amount Put Away | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl2.Children.Add(labelThree);

            Label labelFour = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
            };
            labelFour.SetBinding(Label.TextProperty, "RegularBillValue", BindingMode.Default, new DecimalToCurrencyString());

            hsl2.Children.Add(labelFour);            
            grid.AddWithSpan(hsl2, 10, 1, 1, 2);

            HorizontalStackLayout hsl3 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelFive= new Label
            {
                Text = "Recurring SavingsMauiDetails | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl3.Children.Add(labelFive);

            Label labelSix = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
            };
            labelSix.SetBinding(Label.TextProperty, "RecurringBillDetails", BindingMode.Default);

            hsl3.Children.Add(labelSix);
            grid.AddWithSpan(hsl3, 8, 1, 1, 2);

            hsl3.SetBinding(HorizontalStackLayout.IsVisibleProperty, "IsRecuring");

            border.Content = grid;

            return border;
        });

        SfCarousel sc = new SfCarousel
        {
            ScaleOffset = (float)0.9,
            RotationAngle = 20,
            Duration = 1000,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = _vm.SignOutButtonWidth + 10,
            ItemWidth = (int)Math.Ceiling(_vm.SignOutButtonWidth) - 10,
            ItemHeight = 270,
            ItemSpacing = 20
        };

        sc.ItemTemplate = dt;
        sc.ItemsSource = _vm.DefaultBudget.Bills;


        if (sc.ItemsSource.Any())
        {
            if((sc.ItemsSource.Count() % 2) == 0)
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2) - 1;
            }
            else
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2);
            }            
        }

        sc.SwipeStarted += async (s, e)  =>
        {
            await BillCarouselSwipeStarted(s,e);
        };

        sc.SwipeEnded += async (s, e) =>
        {
            await BillCarouselSwipeEnded(s,e);
        };

        for (int i = 0; i < sc.ItemsSource.Count(); i++)
        {
            Border button = new Border
            {
                HeightRequest = 10,
                WidthRequest = 10,
                Margin = new Thickness(2, 0, 2, 0),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5)
                }
            };

            if(i == sc.SelectedIndex)
            {
                button.BackgroundColor = (Color)PrimaryLight;
            }
            else
            {
                button.BackgroundColor = (Color)Gray100;
            }

            BillCarouselIdent.Children.Add(button);            

        }

        return sc;
    }

    private async Task<SfCarousel> CreateIncomeCarousel()
    {
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);
        Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
        Application.Current.Resources.TryGetValue("Gray100", out var Gray100);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("PrimaryBrush", out var PrimaryBrush);
        Application.Current.Resources.TryGetValue("Info", out var Info);

        DataTemplate dt = new DataTemplate(() =>
        {
            Border border = new Border
            {
                Stroke = (Brush)PrimaryBrush,
                StrokeThickness = 2,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(4)
                },
                BackgroundColor = (Color)White
            };

            Grid grid = new Grid
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                Padding = new Thickness(0),
                Margin = new Thickness(0),
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width = new GridLength(45)},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2)-50)},
                    new ColumnDefinition{Width = new GridLength(((_vm.SignOutButtonWidth - 65)/2)+50)}
                },
                RowDefinitions =
                {
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)},
                    new RowDefinition{Height = new GridLength(1, GridUnitType.Auto)}

                }

            };

            Image ClickImage = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                Margin = new Thickness(5, 5, 5, 0),
                ZIndex = 999,
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue25d",
                    Size = 40,
                    Color = (Color)Primary,
                }
            };

            TapGestureRecognizer TapGesture = new TapGestureRecognizer();
            TapGesture.NumberOfTapsRequired = 1;
            TapGesture.Tapped += (s, e) =>
            {
                SavingsMoreOptions_Tapped(s, e);
            };

            ClickImage.GestureRecognizers.Add(TapGesture);

            //grid.AddWithSpan(ClickImage, 0, 2, 2, 1);


            Image image = new Image
            {
                BackgroundColor = Color.FromArgb("#00FFFFFF"),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Start,
                Margin = new Thickness(10, 15, 0, 0),
                //Source = ImageSource.FromFile("saving.svg"),
                Source = new FontImageSource
                {
                    FontFamily = "MaterialDesignIcons",
                    Glyph = "\ue8e5",
                    Size = 45,
                    Color = (Color)Primary,
                },
                WidthRequest = 28
            };
            grid.AddWithSpan(image, 0, 0, 6, 1);

            Label lblTitle = new Label
            {
                FontAttributes = FontAttributes.Bold,
                FontSize = 21,
                Padding = new Thickness(10, 5, 0, 0),
                TextColor = (Color)Primary,
                Margin = new Thickness(0)
            };
            lblTitle.SetBinding(Label.TextProperty, "IncomeName");
            grid.AddWithSpan(lblTitle, 0, 1, 1, 2);

            Label lblSavingType = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 0),
                TextColor = (Color)Tertiary,
                Margin = new Thickness(0),
                CharacterSpacing = 0
            };
            lblSavingType.SetBinding(Label.TextProperty, "IncomeTypeConverter", BindingMode.Default);
            grid.AddWithSpan(lblSavingType, 1, 1, 1, 2);

            Label lblCurrentBalance = new Label
            {
                FontSize = 16,
                Padding = new Thickness(10, 10, 0, 0),
                TextColor = (Color)Gray900,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0)
            };
            lblCurrentBalance.SetBinding(Label.TextProperty, "IncomeAmount", BindingMode.Default, new DecimalToCurrencyString());
            grid.Add(lblCurrentBalance, 1, 2);

            Label lblBalance = new Label
            {
                FontSize = 14,
                Padding = new Thickness(10, 0, 0, 10),
                TextColor = (Color)Gray400,
                CharacterSpacing = 0,
                Text = "Extra Income Amount",
                Margin = new Thickness(0)
            };
            grid.AddWithSpan(lblBalance, 3, 1, 1, 2);


            BoxView bv = new BoxView
            {
                Color = (Color)Gray100,
                HeightRequest = 2,
                Margin = new Thickness(10, 5, 10, 10)
            };
            grid.AddWithSpan(bv, 4, 1, 1, 2);

            HorizontalStackLayout hsl1 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelOne = new Label
            {
                Text = "Due Date | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl1.Children.Add(labelOne);

            Label labelTwo = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,

            };
            labelTwo.SetBinding(Label.TextProperty, "DateOfIncomeEvent", BindingMode.Default, new BillDueDate());

            hsl1.Children.Add(labelTwo);

            grid.AddWithSpan(hsl1, 5, 1, 1, 2);

            HorizontalStackLayout hsl2 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelThree = new Label
            {
                Text = "Number Of Days To Income | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl2.Children.Add(labelThree);

            Label labelFour = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
            };
            labelFour.SetBinding(Label.TextProperty, "DateOfIncomeEvent", BindingMode.Default, new DateToNumberOfDays());

            hsl2.Children.Add(labelFour);
            grid.AddWithSpan(hsl2, 6, 1, 1, 2);

            HorizontalStackLayout hsl3 = new HorizontalStackLayout
            {
                Margin = new Thickness(10, 0, 10, 0)
            };

            Label labelFive = new Label
            {
                Text = "Recurring SavingsMauiDetails | ",
                TextColor = (Color)Info,
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0
            };
            hsl3.Children.Add(labelFive);

            Label labelSix = new Label
            {
                TextColor = (Color)Gray900,
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                CharacterSpacing = 0,
                FontAttributes = FontAttributes.Bold,
            };
            labelSix.SetBinding(Label.TextProperty, "RecurringIncomeDetails", BindingMode.Default);

            hsl3.Children.Add(labelSix);
            grid.AddWithSpan(hsl3, 7, 1, 1, 2);

            hsl3.SetBinding(HorizontalStackLayout.IsVisibleProperty, "IsRecurringIncome");

            border.Content = grid;

            return border;
        });

        SfCarousel sc = new SfCarousel
        {
            ScaleOffset = (float)0.9,
            RotationAngle = 20,
            Duration = 1000,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            WidthRequest = _vm.SignOutButtonWidth + 10,
            ItemWidth = (int)Math.Ceiling(_vm.SignOutButtonWidth) - 10,
            ItemHeight = 215,
            ItemSpacing = 20
        };

        sc.ItemTemplate = dt;
        sc.ItemsSource = _vm.DefaultBudget.IncomeEvents;


        if (sc.ItemsSource.Any())
        {
            if ((sc.ItemsSource.Count() % 2) == 0)
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2) - 1;
            }
            else
            {
                sc.SelectedIndex = (sc.ItemsSource.Count() / 2);
            }
        }

        sc.SwipeStarted += async (s, e) =>
        {
            await IncomeCarouselSwipeStarted(s, e);
        };

        sc.SwipeEnded += async (s, e) =>
        {
            await IncomeCarouselSwipeEnded(s, e);
        };

        for (int i = 0; i < sc.ItemsSource.Count(); i++)
        {
            Border button = new Border
            {
                HeightRequest = 10,
                WidthRequest = 10,
                Margin = new Thickness(2, 0, 2, 0),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(5)
                }
            };

            if (i == sc.SelectedIndex)
            {
                button.BackgroundColor = (Color)PrimaryLight;
            }
            else
            {
                button.BackgroundColor = (Color)Gray100;
            }

            IncomeCarouselIdent.Children.Add(button);

        }

        return sc;
    }

    private async Task IncomeCarouselSwipeStarted(object Sender, Syncfusion.Maui.Toolkit.Carousel.SwipeStartedEventArgs Event)
    {
        try
        {
            Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

            var Carousel = (SfCarousel)Sender;
            var Elements = IncomeCarouselIdent.GetVisualTreeDescendants();

            int Index = (Carousel.SelectedIndex * 2) + 1;

            Border button = (Border)Elements[Index];
            await button.BackgroundColorTo((Color)Gray100, 16, 500, Easing.CubicIn);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "IncomeCarouselSwipeStarted");
        }

    }
    private async Task IncomeCarouselSwipeEnded(object Sender, EventArgs Event)
    {
        try
        {
            Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);

            var Carousel = (SfCarousel)Sender;
            var Elements = IncomeCarouselIdent.GetVisualTreeDescendants();

            int Index = (Carousel.SelectedIndex * 2) + 1;

            Border button = (Border)Elements[Index];
            await button.BackgroundColorTo((Color)PrimaryLight, 16, 500, Easing.CubicIn);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "IncomeCarouselSwipeEnded");
        }
    }

    private async Task CarouselSwipeStarted(object Sender, Syncfusion.Maui.Toolkit.Carousel.SwipeStartedEventArgs Event)
    {
        try
        {
            Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

            var Carousel = (SfCarousel)Sender;
            var Elements = SavingCarouselIdent.GetVisualTreeDescendants();

            int Index = (Carousel.SelectedIndex * 2) + 1;

            Border button = (Border)Elements[Index];
            await button.BackgroundColorTo((Color)Gray100,16,500,Easing.CubicIn);

        }
        catch (Exception ex)
        {
           await _pt.HandleException(ex, "MainPage", "CarouselSwipeStarted");
        }

    }

    private async Task CarouselSwipeEnded(object Sender, EventArgs Event)
    {
        try
        {
            Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);

            var Carousel = (SfCarousel)Sender;
            var Elements = SavingCarouselIdent.GetVisualTreeDescendants();

            int Index = (Carousel.SelectedIndex * 2) + 1;

            Border button = (Border)Elements[Index];
            await button.BackgroundColorTo((Color)PrimaryLight, 16, 500, Easing.CubicIn);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "CarouselSwipeEnded");
        }
    }
    private async Task BillCarouselSwipeStarted(object Sender, Syncfusion.Maui.Toolkit.Carousel.SwipeStartedEventArgs Event)
    {
        try
        {
            Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

            var Carousel = (SfCarousel)Sender;
            var Elements = BillCarouselIdent.GetVisualTreeDescendants();

            int Index = (Carousel.SelectedIndex * 2) + 1;

            Border button = (Border)Elements[Index];
            await button.BackgroundColorTo((Color)Gray100, 16, 500, Easing.CubicIn);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "BillCarouselSwipeStarted");
        }
    }

    private async Task BillCarouselSwipeEnded(object Sender, EventArgs Event)
    {
        try
        {
            Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);

            var Carousel = (SfCarousel)Sender;
            var Elements = BillCarouselIdent.GetVisualTreeDescendants();

            int Index = (Carousel.SelectedIndex * 2) + 1;

            Border button = (Border)Elements[Index];
            await button.BackgroundColorTo((Color)PrimaryLight, 16, 500, Easing.CubicIn);
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "BillCarouselSwipeEnded");
        }
    }
    private void RefreshView_Refreshing(object sender, EventArgs e)
    {
        LoadMainDashboardContent();
    }

    private void SavingsMoreOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            EnvelopeOptionsBottomSheet page = new EnvelopeOptionsBottomSheet(_ds, _pt);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            page.ShowAsync();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MainPage", "SavingsMoreOptions_Tapped");
        }

    }

    private async void ExtraIncoDetails_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            if (App.CurrentBottomSheet != null)
            {
                await App.CurrentBottomSheet.DismissAsync();
                App.CurrentBottomSheet = null;
            }
            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewIncomes)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "ExtraIncoDetails_Tapped");
        }
    }

    private async void ExtraBillInfo_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewBills)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "ExtraBillInfo_Tapped");
        }

    }

    private async void ExtraSavingInfo_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.Windows[0].Page.ShowPopup(PopUp);
            }

            await Shell.Current.GoToAsync($"//{nameof(DailyBudgetMAUIApp.Pages.ViewSavings)}");
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "ExtraSavingInfo_Tapped");
        }

    }

    private async void SeeMoreTransactions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (!vslRecentTransactions.IsVisible)
            {           
                vslRecentTransactions.IsVisible = true;
                fisRecentTransactionHideShow.Glyph = "\ue5cf";
                vslRecentTransactions.HeightRequest = 0;

                var animation = new Animation(v => vslRecentTransactions.HeightRequest = v, 0, _vm.RecentTransactionsHeight);
                animation.Commit(this, "ShowRecentTran", 16, 100, Easing.CubicIn);

                await MainScrollView.ScrollToAsync(vslRecentTransactions, ScrollToPosition.Start, true);
            }
            else
            {           
                var animation = new Animation(v => vslRecentTransactions.HeightRequest = v, _vm.RecentTransactionsHeight, 0);
                animation.Commit(this, "HideRecentTran", 16, 1000, Easing.CubicIn, async (v, c) =>
                {
                    fisRecentTransactionHideShow.Glyph = "\ue5ce";
                    vslRecentTransactions.IsVisible = false;
                    await MainScrollView.ScrollToAsync(brdYourTransactions,ScrollToPosition.Start, true);
                
                });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "SeeMoreTransactions_Tapped");
        }
    }

    private async void EditTransaction_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            bool EditTransaction = await Application.Current.Windows[0].Page.DisplayAlert($"Are your sure?", $"Are you sure you want to Edit this transaction?", "Yes, continue", "No Thanks!");
            if (EditTransaction)
            {
                Transactions T = (Transactions)e.Parameter;
                await Shell.Current.GoToAsync($"{nameof(MainPage)}/{nameof(AddTransaction)}?BudgetID={App.DefaultBudgetID}&TransactionID={T.TransactionID}&NavigatedFrom=ViewMainPage",
                    new Dictionary<string, object>
                    {
                        ["Transaction"] = T
                    });
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "EditTransaction_Tapped");
        }
    }

    private async void DeleteTransaction_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            bool DeleteTransaction = await Application.Current.Windows[0].Page.DisplayAlert($"Are your sure?", $"Are you sure you want to Delete this transaction?", "Yes", "No Thanks!");
            if (DeleteTransaction)
            {
                Transactions transaction = (Transactions)e.Parameter;
                await _ds.DeleteTransaction(transaction.TransactionID);

                _vm.RecentTransactions.Clear();

                List<Transactions> RecentTrans = await _ds.GetRecentTransactions(_vm.DefaultBudgetID, 6, "MainPage");
                foreach (Transactions T in RecentTrans)
                {
                    _vm.RecentTransactions.Add(T);
                }

                LVTransactions.RefreshItem();
                LVTransactions.RefreshView();


                _vm.DefaultBudget = _ds.GetBudgetDetailsAsync(_vm.DefaultBudgetID, "Full").Result;

                App.DefaultBudget = _vm.DefaultBudget;
                _vm.IsBudgetCreated = App.DefaultBudget.IsCreated;
                App.SessionLastUpdate = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "DeleteTransaction_Tapped");
        }
    }

    private async void QuickTransaction_Tapped(object sender, TappedEventArgs e)
    {
        //if (!brdTransactionAmount.IsVisible)
        //{
        //    var a1 = brdTransaction.RotateTo(90, 250, Easing.CubicOut);
        //    var a2 = brdTransactionAmount.FadeTo(1, 250, Easing.CubicOut);
        //    var a3 = btnTransactionAmount.FadeTo(1, 250, Easing.CubicOut);

        //    await Task.WhenAll(a1, a2, a3);

        //    Shadow shadow = new Shadow
        //    {
        //        Opacity = (float)0.95,
        //        Radius = 15
        //    };

        //    brdTransactionAmount.Shadow = shadow;
        //    brdTransactionAmount.IsVisible = true;
        //    brdTransactionAmount.IsVisible = false;
        //    brdTransactionAmount.IsVisible = true;
        //}
        //else
        //{
            
        //    entTransactionAmount.IsEnabled = false;
        //    entTransactionAmount.IsEnabled = true;

        //    var a1 = brdTransaction.RotateTo(0, 250, Easing.CubicOut);
        //    var a2 = brdTransactionAmount.FadeTo(0, 250, Easing.CubicOut);
        //    var a3 = btnTransactionAmount.FadeTo(0, 250, Easing.CubicOut);

        //    await Task.WhenAll(a1, a2, a3);
        //    brdTransactionAmount.IsVisible = false;
        //    brdTransactionAmount.IsVisible = true;
        //    brdTransactionAmount.IsVisible = false;
        //}      
    }

    private async void btnTransactionAmount_Clicked(object sender, EventArgs e)
    {
        try
        {
            Transactions T = new Transactions
            {
                TransactionAmount = _vm.TransactionAmount,
                IsSpendFromSavings = false,
                SavingID = null,
                SavingName = null,
                TransactionDate = DateTime.UtcNow,
                WhenAdded = DateTime.UtcNow,
                IsIncome = false,
                Category = null,
                Payee = null,
                Notes = null,
                CategoryID = null,
                IsTransacted = true,
                SavingsSpendType = null,
                EventType = "Transaction"
            };

            T = await _ds.SaveNewTransaction(T, App.DefaultBudget.BudgetID);
            QuickTransaction_Tapped(null, null);      

            _vm.SnackBar = "Transaction Added";
            _vm.SnackID = T.TransactionID;

            ProcessSnackBar();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "btnTransactionAmount_Clicked");
        }

    }

    private async void CoverOverspend_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            var popup = new PopupMoveBalance(App.DefaultBudget, "Budget", 0, true, new PopupMoveBalanceViewModel(), _pt, _ds);
            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
            await Task.Delay(100);
            if (result.ToString() == "OK")
            {
                App.DefaultBudget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "CoverOverspend_Tapped");
        }
    }

    private void CategoryOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            CategoryOptionsBottomSheet page = new CategoryOptionsBottomSheet(_ds, _pt);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            page.ShowAsync();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MainPage", "CategoryOptions_Tapped");
        }
    }

    private void PayeeOptions_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            PayeeOptionsBottomSheet page = new PayeeOptionsBottomSheet(_ds, _pt);

            page.Detents = new DetentsCollection()
            {
                new FixedContentDetent()
            };

            page.HasBackdrop = true;
            page.CornerRadius = 0;

            App.CurrentBottomSheet = page;

            page.ShowAsync();
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MainPage", "PayeeOptions_Tapped");
        }
    }

    private void YourBudgetCalendarOptions_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void SwitchBudgetMain_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            SwitchBudgetPicker = await _pt.SwitchBudget("Dashboard");
            MainVSLView.Children.Add(SwitchBudgetPicker);
            SwitchBudgetPicker.Focus();
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "SwitchBudgetMain_Tapped");
        }
    }

    private async void Upload_Clicked(object sender, EventArgs e)
    {
        try
        {
            FileResult UploadFile = await MediaPicker.PickPhotoAsync();

            if (UploadFile is null) return;

            if (UploadFile.OpenReadAsync().Result.Length < 3000000)
            {
                await _ds.UploadUserProfilePicture(App.UserDetails.UserID, UploadFile);
            }
            else
            {

            }
        }
        catch (Exception ex)
        {
            await _pt.HandleException(ex, "MainPage", "Upload_Clicked");
        }

    }

    private void QuickTransaction_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            _pt.FormatEntryNumber(sender, e, entQuickTransaction);
        }
        catch (Exception ex)
        {
            _pt.HandleException(ex, "MainPage", "QuickTransaction_TextChanged");
        }

    }
}

