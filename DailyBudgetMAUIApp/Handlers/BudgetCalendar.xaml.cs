using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Syncfusion.Maui.Scheduler;
using System.Globalization;


namespace DailyBudgetMAUIApp.Handlers;

public partial class BudgetCalendar : ContentView
{
    private readonly IProductTools _pt;

    public BudgetCalendar()
    {
        _pt = IPlatformApplication.Current.Services.GetService<IProductTools>();
        IsBusy = false;
        Budget = null;
        InitializeComponent();
    }

    public static readonly BindableProperty BudgetProperty =
    BindableProperty.Create(nameof(Budget), typeof(Budgets), typeof(BudgetCalendar), propertyChanged: OnBudgetsChanged, defaultBindingMode: BindingMode.TwoWay);

    public Budgets Budget
    {
        get => (Budgets)GetValue(BudgetProperty);
        set => SetValue(BudgetProperty, value);
    }

    private static async void OnBudgetsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not BudgetCalendar control) return;
        if (newValue is not Budgets Budget) return;

        if (Budget is not null && Budget.IsCreated && Budget.NextIncomePayday is not null)
        {
            control.IsBusy = true;
            await control.LoadBudgetCalendar(Budget);
            control.IsBusy = false;
        }
        
        control.InvalidateMeasure();
    }

    public static readonly BindableProperty IsBusyProperty =
        BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(BudgetCalendar), propertyChanged: OnIsBusyChanged, defaultBindingMode: BindingMode.TwoWay);

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    static void OnIsBusyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (BudgetCalendar)bindable;
        control.InvalidateMeasure();
    }

    private async Task LoadBudgetCalendar(Budgets Budget)
    {
        List<SchedulerAppointment> eventList = new List<SchedulerAppointment>();

        await Task.Delay(1);
        Application.Current.Resources.TryGetValue("White", out var White);
        Application.Current.Resources.TryGetValue("Success", out var Success);
        Application.Current.Resources.TryGetValue("Primary", out var Primary);
        Application.Current.Resources.TryGetValue("Tertiary", out var Tertiary);
        Application.Current.Resources.TryGetValue("Gray900", out var Gray900);

        DateTime MaxDate = DateTime.UtcNow.AddMonths(1);
        DateTime BudgetDate = Budget.NextIncomePayday.GetValueOrDefault();

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
                Subject = $"Getting paid {Budget.PaydayAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)}",
                IsReadOnly = true,
                Background = (Color)Success,
                TextColor = (Color)White,
                Notes = "PayDay",
                Id = 0
            };

            eventList.Add(PayDay);
            List<Savings> Savings = Budget.Savings.Where(s => !s.IsRegularSaving).ToList();
            if (Savings is not null && Savings.Count > 0)
            {            
                foreach (Savings saving in Savings)
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

                    eventList.Add(EnvelopeEvent);
                }
            }

            BudgetDate = _pt.CalculateNextDate(BudgetDate, Budget.PaydayType, Budget.PaydayValue.GetValueOrDefault(), Budget.PaydayDuration);
        }

        var Transactions = Budget.Transactions.Where(s => !s.IsTransacted).ToList();
        if(Transactions is not null && Transactions.Count > 0 )
        {
            foreach (Transactions transaction in Transactions)
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

                eventList.Add(TransactionEvent);
            }
        }

        var Envelopes = Budget.Savings.Where(s => s.IsRegularSaving).ToList();
        if(Envelopes is not null && Envelopes.Count > 0)
        {
            foreach (Savings saving in Envelopes)
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

                    eventList.Add(SavingEvent);
                }
            }
        }

        var Bills = Budget.Bills.ToList();
        if(Bills is not null && Bills.Count > 0)
        {
            foreach (Bills bill in Bills)
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

                    eventList.Add(BillEvent);

                    if (bill.IsRecuring.GetValueOrDefault())
                    {
                        BillDate = _pt.CalculateNextDate(BillDate, bill.BillType, bill.BillValue.GetValueOrDefault(), bill.BillDuration);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        var Incomes = Budget.IncomeEvents.ToList();
        if(Incomes is not null && Incomes.Count > 0)
        {
            foreach (IncomeEvents income in Incomes)
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

                    eventList.Add(IncomeEvent);

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

        Scheduler.AppointmentsSource = eventList;
    }

}