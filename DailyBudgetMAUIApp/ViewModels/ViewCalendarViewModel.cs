﻿using DailyBudgetMAUIApp.DataServices;
using DailyBudgetMAUIApp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Syncfusion.Maui.Scheduler;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DailyBudgetMAUIApp.ViewModels
{
    public partial class ViewCalendarViewModel : BaseViewModel
    {
        private readonly IProductTools _pt;
        private readonly IRestDataService _ds;

        [ObservableProperty]
        private bool _showBusyIndicator;
        [ObservableProperty]
        private Budgets _budget;
        [ObservableProperty]
        private Budgets _budgetCard = new Budgets();
        [ObservableProperty]
        private bool _isBudgetVisible;
        [ObservableProperty]
        private ObservableCollection<SchedulerAppointment> _events = new ObservableCollection<SchedulerAppointment>();
        [ObservableProperty]
        private DateTime _today = DateTime.Today;
        [ObservableProperty]
        private double _schedulerHeight;
        [ObservableProperty]
        private Savings _saving = new Savings();
        [ObservableProperty]
        private bool _isSavingVisible;
        [ObservableProperty]
        private Transactions _transaction = new Transactions();
        [ObservableProperty]
        private bool _isTransactionVisible;
        [ObservableProperty]
        private IncomeEvents _income = new IncomeEvents();
        [ObservableProperty]
        private bool _isIncomeVisible;
        [ObservableProperty]
        private Bills _bill = new Bills();
        [ObservableProperty]
        private bool _isBillVisible;
        [ObservableProperty]
        private Savings _envelope = new Savings();
        [ObservableProperty]
        private bool _isEnvelopeVisible;


        public ViewCalendarViewModel(IProductTools pt, IRestDataService ds)
        {
            _ds = ds;
            _pt = pt;
            SchedulerHeight = ((DeviceDisplay.Current.MainDisplayInfo.Height / DeviceDisplay.Current.MainDisplayInfo.Density)) * 0.52;
        }

        public async Task LoadData()
        {
            Title = "Budget Events' Calendar";
            Budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Full");
            await LoadPayDayEvents(Today.AddMonths(1));
            await LoadOutgoingEvents(Today, Today.AddMonths(1));
            await LoadIncomeEvents(Today, Today.AddMonths(1));
            await LoadSavingsEvents();
            await LoadTransactionsEvents();
            await LoadEventCard("PayDay", 0);
        }

        [ICommand]
        private async void LoadMoreEvents(object obj)
        {
            ShowBusyIndicator = true;
            await Task.Delay(500);
            await GenerateSchedulerAppointments(((SchedulerQueryAppointmentsEventArgs)obj).VisibleDates.ToList());
            ShowBusyIndicator = false;
        }

        private async Task GenerateSchedulerAppointments(List<DateTime> visibleDates)
        {
            DateTime MaxDate = visibleDates.Max();
            DateTime MinDate = visibleDates.Min();
            await LoadPayDayEvents(MaxDate);
            await LoadOutgoingEvents(MinDate, MaxDate);
            await LoadIncomeEvents(MinDate, MaxDate);
        }

        private async Task LoadPayDayEvents(DateTime MaxDate)
        {
            Application.Current.Resources.TryGetValue("White", out var White);
            Application.Current.Resources.TryGetValue("Success", out var Success);

            while (Budget.NextIncomePayday.GetValueOrDefault() < MaxDate.AddDays(30))
            {
                SchedulerAppointment PayDay = new SchedulerAppointment
                {
                    StartTime = Budget.NextIncomePayday.GetValueOrDefault().Date,
                    EndTime = Budget.NextIncomePayday.GetValueOrDefault().Date.AddMinutes(1439),
                    Subject = $"Getting paid {Budget.PaydayAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)}",
                    IsReadOnly = true,
                    Background = (Color)Success,
                    TextColor = (Color)White,
                    Notes = "PayDay",
                    Id = 0
                };

                Events.Add(PayDay);

                foreach (Savings saving in Budget.Savings.Where(s => !s.IsRegularSaving))
                {
                    SchedulerAppointment EnvelopeEvent = new SchedulerAppointment
                    {
                        StartTime = Budget.NextIncomePayday.GetValueOrDefault().Date,
                        EndTime = Budget.NextIncomePayday.GetValueOrDefault().Date.AddMinutes(1439),
                        Subject = $"Putting {saving.PeriodSavingValue.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} away for {saving.SavingsName}",
                        IsReadOnly = true,
                        Background = App.ChartColor[2],
                        TextColor = (Color)White,
                        Notes = "Envelope",
                        Id = saving.SavingID
                    };

                    Events.Add(EnvelopeEvent);
                }

                Budget.NextIncomePayday = _pt.CalculateNextDate(Budget.NextIncomePayday.GetValueOrDefault(), Budget.PaydayType, Budget.PaydayValue.GetValueOrDefault(), Budget.PaydayDuration);
            }
        }

        private async Task LoadTransactionsEvents()
        {
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach (Transactions transaction in Budget.Transactions.Where(s => !s.IsTransacted))
            {

                SchedulerAppointment SavingEvent = new SchedulerAppointment
                {
                    StartTime = transaction.TransactionDate.GetValueOrDefault().Date,
                    EndTime = transaction.TransactionDate.GetValueOrDefault().Date.AddMinutes(1439),
                    Subject = $"Upcoming transaction for {transaction.TransactionDate.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)}",
                    IsReadOnly = true,
                    Background = App.ChartColor[5],
                    TextColor = (Color)White,
                    Notes = "Transaction",
                    Id = transaction.TransactionID
                };

                Events.Add(SavingEvent);

            }
        }

        private async Task LoadSavingsEvents()
        {
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach(Savings saving in Budget.Savings.Where(s => s.IsRegularSaving))
            {
                if(saving.SavingsType != "SavingsBuilder")
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

                    Events.Add(SavingEvent);
                } 
            }            
        }

        private async Task LoadOutgoingEvents(DateTime MinDate, DateTime MaxDate)
        {
            Application.Current.Resources.TryGetValue("White", out var White);

            foreach (Bills bill in Budget.Bills)
            {
                while(bill.BillDueDate.GetValueOrDefault() >= MinDate && bill.BillDueDate.GetValueOrDefault() <= MaxDate.AddDays(30))
                {
                    SchedulerAppointment BillEvent = new SchedulerAppointment
                    {
                        StartTime = bill.BillDueDate.GetValueOrDefault().Date,
                        EndTime = bill.BillDueDate.GetValueOrDefault().Date.AddMinutes(1439),
                        Subject = $"{bill.BillAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} outgoing for {bill.BillName}",
                        IsReadOnly = true,
                        Background = App.ChartColor[3],
                        TextColor = (Color)White,
                        Notes = "Bill",
                        Id = bill.BillID
                    };

                    Events.Add(BillEvent);

                    if (bill.IsRecuring)
                    {
                        bill.BillDueDate = _pt.CalculateNextDate(bill.BillDueDate.GetValueOrDefault(), bill.BillType, bill.BillValue.GetValueOrDefault(), bill.BillDuration);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private async Task LoadIncomeEvents(DateTime MinDate, DateTime MaxDate)
        {
            Application.Current.Resources.TryGetValue("White", out var White);


            foreach (IncomeEvents income in Budget.IncomeEvents)
            {
                while (income.DateOfIncomeEvent >= MinDate && income.DateOfIncomeEvent <= MaxDate.AddDays(30))
                {
                    SchedulerAppointment IncomeEvent = new SchedulerAppointment
                    {
                        StartTime = income.DateOfIncomeEvent.Date,
                        EndTime = income.DateOfIncomeEvent.Date.AddMinutes(1439),
                        Subject = $"{income.IncomeAmount.ToString("c", CultureInfo.CurrentCulture)} income for {income.IncomeName}",
                        IsReadOnly = true,
                        Background = App.ChartColor[4],
                        TextColor = (Color)White,
                        Notes = "Income",
                        Id = income.IncomeEventID
                    };

                    Events.Add(IncomeEvent);

                    if (income.IsRecurringIncome)
                    {
                        income.DateOfIncomeEvent = _pt.CalculateNextDate(income.DateOfIncomeEvent, income.RecurringIncomeType, income.RecurringIncomeValue.GetValueOrDefault(), income.RecurringIncomeDuration);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public async Task LoadEventCard(string Type, int ID)
        {
            switch (Type)
            {
                case "Saving":
                    if (Saving.SavingID != ID)
                    {
                        Savings saving = await _ds.GetSavingFromID(ID);
                        Saving = saving;
                    }
                    IsSavingVisible = true;
                    IsBillVisible = false;
                    IsEnvelopeVisible = false;
                    IsBudgetVisible = false;
                    IsIncomeVisible = false;
                    IsTransactionVisible = false;
                    break;
                case "Income":
                    if (Income.IncomeEventID != ID)
                    {
                        IncomeEvents income = await _ds.GetIncomeFromID(ID);
                        Income = income;
                    }
                    IsSavingVisible = false;
                    IsBillVisible = false;
                    IsEnvelopeVisible = false;
                    IsBudgetVisible = false;
                    IsIncomeVisible = true;
                    IsTransactionVisible = false;
                    break;
                case "Bill":
                    if (Bill.BillID != ID)
                    {
                        Bills bill = await _ds.GetBillFromID(ID);
                        Bill = bill;
                    }
                    IsSavingVisible = false;
                    IsBillVisible = true;
                    IsEnvelopeVisible = false;
                    IsBudgetVisible = false;
                    IsIncomeVisible = false;
                    IsTransactionVisible = false;
                    break;
                case "Transaction":
                    if (Transaction.TransactionID != ID)
                    {
                        Transactions transaction = await _ds.GetTransactionFromID(ID);
                        Transaction = transaction;
                    }
                    IsSavingVisible = false;
                    IsBillVisible = false;
                    IsEnvelopeVisible = false;
                    IsBudgetVisible = false;
                    IsIncomeVisible = false;
                    IsTransactionVisible = true;
                    break;
                case "Envelope":
                    if (Envelope.SavingID != ID)
                    {
                        Savings envelope = await _ds.GetSavingFromID(ID);
                        Envelope = envelope;
                    }
                    IsSavingVisible = false;
                    IsBillVisible = false;
                    IsEnvelopeVisible = true;
                    IsBudgetVisible = false;
                    IsIncomeVisible = false;
                    IsTransactionVisible = false;
                    break;
                case "PayDay":
                    if (BudgetCard.BudgetID == 0)
                    {
                        Budgets budget = await _ds.GetBudgetDetailsAsync(App.DefaultBudgetID, "Limited");
                        BudgetCard = budget;
                    }
                    IsSavingVisible = false;
                    IsBillVisible = false;
                    IsEnvelopeVisible = false;
                    IsBudgetVisible = true;
                    IsIncomeVisible = false;
                    IsTransactionVisible = false;
                    break;
                default:
                    break;

            }
        }
    }
}