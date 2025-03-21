using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using DailyBudgetMAUIApp.Converters;
using DailyBudgetMAUIApp.Handlers;
using DailyBudgetMAUIApp.Helpers;
using DailyBudgetMAUIApp.Models;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.ViewModels;
using Newtonsoft.Json;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;



namespace DailyBudgetMAUIApp.DataServices
{
    internal class ProductTools : IProductTools
    {
        private readonly IRestDataService _ds;

        public ProductTools(IRestDataService ds)
        {
            _ds = ds;
        }

        public RegisterModel ResetUserPassword(RegisterModel obj)
        {

            string HashedPassword = GenerateHashedPassword(obj.Password, obj.Salt);

            obj.Password = HashedPassword;

            return obj;
        }

        public RegisterModel CreateUserSecurityDetails(RegisterModel obj)
        {
            Random rnd = new();
            int number = rnd.Next(2000);
            string Salt = GenerateSalt(number);

            string HashedPassword = GenerateHashedPassword(obj.Password, Salt);

            obj.Password = HashedPassword;
            obj.Salt = Salt;

            return obj;
        }

        public string GenerateSalt(int nSalt)
        {
            Byte[] saltBytes = new Byte[nSalt];
            RandomNumberGenerator.Create().GetNonZeroBytes(saltBytes);

            return Convert.ToBase64String(saltBytes);

        }

        public string GenerateHashedPassword(string NonHasdedPassword, string Salt)
        {
            int nHash = 70;
            int nIteraitons = 10101;

            Byte[] saltBytes = Convert.FromBase64String(Salt);

            Rfc2898DeriveBytes obj = new(NonHasdedPassword, saltBytes, nIteraitons);

            using (obj)
            {
                return Convert.ToBase64String(obj.GetBytes(nHash));
            }

        }

        public async Task HandleException(Exception ex, string Page, string Method)
        {
            if (ex.Message == "Connectivity")
            {
                await Shell.Current.GoToAsync($"{nameof(NoNetworkAccess)}");
            }
            else if (ex.Message == "Server Connectivity")
            {
                await Shell.Current.GoToAsync($"{nameof(NoServerAccess)}");
            }
            else if (ex.Message == "Invalid_Session")
            {
                await Shell.Current.GoToAsync($"{nameof(LogoutPage)}");
            }
            else
            {
                ErrorLog Error = new ErrorLog(ex, Page, Method);
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
                {
                    Error = await _ds.CreateNewErrorLog(Error);
                }
                await Shell.Current.GoToAsync(nameof(ErrorPage),
                    new Dictionary<string, object>
                    {
                        ["Error"] = Error
                    });
            }
        }

        public DateTime? GetBudgetLastUpdated(int BudgetID)
        {
            DateTime? LastUpdated = _ds.GetBudgetLastUpdatedAsync(BudgetID).Result;

            return LastUpdated;
        }

        public void ShowPopup(PopUpPage popup)
        {
            Page page = Application.Current.Windows[0].Page ?? throw new NullReferenceException();
            page.ShowPopup(popup);
        }

        public double FormatBorderlessEntryNumber(object sender, TextChangedEventArgs e, BorderlessEntry entry)
        {

#if ANDROID
            var handler = entry.Handler as Microsoft.Maui.Handlers.EntryHandler;
            var editText = handler?.PlatformView as AndroidX.AppCompat.Widget.AppCompatEditText;
            if (editText != null)
            {
                editText.EmojiCompatEnabled = false;
                editText.SetTextKeepState(entry.Text);
            }
#endif

            double Number = FormatCurrencyNumber(e.NewTextValue);
            string NumberString = Number.ToString("c", CultureInfo.CurrentCulture);
            entry.Text = NumberString;
            int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
            if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entry.CursorPosition > position)
            {
                entry.CursorPosition = entry.Text.Length;
            }
            else if(!string.IsNullOrEmpty(e.OldTextValue) && ((NumberString.Length - e.OldTextValue.Length) == 2))
            {
                entry.CursorPosition = entry.CursorPosition + 1;
            }

            return Number;
        }
        
        public double FormatEntryNumber(object sender, TextChangedEventArgs e, Entry entry)
        {

#if ANDROID
            var handler = entry.Handler as Microsoft.Maui.Handlers.EntryHandler;
            var editText = handler?.PlatformView as AndroidX.AppCompat.Widget.AppCompatEditText;
            if (editText != null)
            {
                editText.EmojiCompatEnabled = false;
                editText.SetTextKeepState(entry.Text);
            }
#endif

            double Number = FormatCurrencyNumber(e.NewTextValue);
            string NumberString = Number.ToString("c", CultureInfo.CurrentCulture);
            entry.Text = NumberString;
            int position = e.NewTextValue.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
            if (!string.IsNullOrEmpty(e.OldTextValue) && (e.OldTextValue.Length - position) == 2 && entry.CursorPosition > position)
            {
                entry.CursorPosition = entry.Text.Length;
            }
            else if(!string.IsNullOrEmpty(e.OldTextValue) && ((NumberString.Length - e.OldTextValue.Length) == 2))
            {
                entry.CursorPosition = entry.CursorPosition + 1;
            }

            return Number;
        }
        
        public double FormatCurrencyNumber(string input)
        {
            input = input.Replace(App.CurrentSettings.CurrencySymbol, "").Replace(App.CurrentSettings.CurrencyGroupSeparator, "").Replace(App.CurrentSettings.CurrencyDecimalSeparator, "");
            input = input.Trim();

            try
            {
                double Number = Convert.ToDouble(input);
                Number = Number / 100;

                return Number;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int FindCurrencyCursorPosition(string input)
        {
            try
            {
                int position = input.IndexOf(App.CurrentSettings.CurrencyDecimalSeparator);
                position = position + 3;

                return position;
            }
            catch (Exception)
            {
                return input.Length;
            }
        }

        public int GetNumberOfDaysLastWorkingDay(int? NumberOfDaysBefore)
        {
            if (NumberOfDaysBefore == null)
            {
                NumberOfDaysBefore = 1;
            }

            int year = GetBudgetLocalTime(DateTime.UtcNow).Year;
            int month = GetBudgetLocalTime(DateTime.UtcNow).Month;

            int NextYear = new int();
            int NextMonth = new int();

            if (month != 12)
            {
                NextYear = GetBudgetLocalTime(DateTime.UtcNow).Year;
                NextMonth = month + 1;
            }
            else
            {
                NextYear = year + 1;
                NextMonth = 1;
            }

            DateTime CurrentDate = new DateTime();
            var i = DateTime.DaysInMonth(year, month);
            int j = 1;
            while (i > 0)
            {
                var dtCurrent = new DateTime(year, month, i);
                if (dtCurrent.DayOfWeek < DayOfWeek.Saturday && dtCurrent.DayOfWeek > DayOfWeek.Sunday)
                {
                    CurrentDate = dtCurrent;
                    if (j == NumberOfDaysBefore)
                    {
                        i = 0;
                    }
                    else
                    {
                        i = i - 1;
                        j = j + 1;
                    }
                }
                else
                {
                    i = i - 1;
                }
            }

            DateTime NextDate = new DateTime();
            i = DateTime.DaysInMonth(NextYear, NextMonth);
            j = 1;
            while (i > 0)
            {
                var dtCurrent = new DateTime(NextYear, NextMonth, i);
                if (dtCurrent.DayOfWeek < DayOfWeek.Saturday && dtCurrent.DayOfWeek > DayOfWeek.Sunday)
                {
                    NextDate = dtCurrent;
                    if (j == NumberOfDaysBefore)
                    {
                        i = 0;
                    }
                    else
                    {
                        i = i - 1;
                        j = j + 1;
                    }
                }
                else
                {
                    i = i - 1;
                }
            }

            int DaysBetweenPay = (int)Math.Ceiling((NextDate.Date - CurrentDate.Date).TotalDays);

            return DaysBetweenPay;
        }

        public int GetNumberOfDaysLastDayOfWeek(int dayNumber)
        {

            int year = GetBudgetLocalTime(DateTime.UtcNow).Year;
            int month = GetBudgetLocalTime(DateTime.UtcNow).Month;

            int NextYear = new int();
            int NextMonth = new int();

            if (month != 12)
            {
                NextYear = GetBudgetLocalTime(DateTime.UtcNow).Year;
                NextMonth = month + 1;
            }
            else
            {
                NextYear = year + 1;
                NextMonth = 1;
            }

            DateTime CurrentDate = new DateTime();

            var i = DateTime.DaysInMonth(year, month);
            while (i > 0)
            {
                var dtCurrent = new DateTime(year, month, i);
                if ((int)dtCurrent.DayOfWeek == dayNumber)
                {
                    CurrentDate = dtCurrent;
                    i = 0;
                }
                else
                {
                    i = i - 1;
                }
            }


            DateTime NextDate = new DateTime();

            i = DateTime.DaysInMonth(NextYear, NextMonth);
            while (i > 0)
            {
                var dtCurrent = new DateTime(NextYear, NextMonth, i);
                if ((int)dtCurrent.DayOfWeek == dayNumber)
                {
                    NextDate = dtCurrent;
                    i = 0;
                }
                else
                {
                    i = i - 1;
                }
            }

            int DaysBetweenPay = (int)Math.Ceiling((NextDate.Date - CurrentDate.Date).TotalDays);

            return DaysBetweenPay;
        }

        public string UpdatePayPeriodStats(int? BudgetID)
        {
            //TODO: IMPLEMENT CALLING UpdatePayPeriodStats in API
            return "OK";
        }

        public string BudgetDailyCycleBudgetValuesUpdate(ref Budgets Budget)
        {
            string status = "OK";

            status = status == "OK" ? UpdateApproxDaysBetweenPay(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateSavings(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateBills(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateIncome(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateSavingsSpend(ref Budget) : status;
            //IF BORROW PAY THEN DONT PAY BILLS UNTIL NEXT PAYDAY
            if(Budget.IsBorrowPay)
            {
                status = status == "OK" ? UpdateBudgetCreateBillsSpendBorrowPay(ref Budget) : status;
            }
            else
            {
                status = status == "OK" ? UpdateBudgetCreateBillsSpend(ref Budget) : status;
            }


            Budget.LastUpdated = DateTime.UtcNow;
            int DaysToPayDay = (int)Math.Ceiling((Budget.NextIncomePayday.GetValueOrDefault().Date - GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);
            if(DaysToPayDay == 0)
            {
                DaysToPayDay = 1;
            }

            Budget.LeftToSpendDailyAmount = (Budget.LeftToSpendBalance ?? 0) / DaysToPayDay;
            Budget.StartDayDailyAmount = Budget.LeftToSpendDailyAmount;

            return status;
        }

        public string UpdateApproxDaysBetweenPay(ref Budgets Budget)
        {

            if (Budget != null)
            {
                int DaysBetweenPayDay = CalculateBudgetDaysBetweenPay(Budget);

                Budget.AproxDaysBetweenPay = DaysBetweenPayDay;

                return "OK";
            }
            else
            {
                return "Budget not found";
            }

        }

        public int CalculateBudgetDaysBetweenPay(Budgets Budget)
        {
            int NumberOfDays = 30;

            if (Budget.PaydayType == "Everynth")
            {
                int Duration = new int();
                if (Budget.PaydayDuration == "days")
                {
                    Duration = 1;
                }
                else if (Budget.PaydayDuration == "weeks")
                {
                    Duration = 7;
                }
                else if (Budget.PaydayDuration == "years")
                {
                    Duration = 365;
                }

                NumberOfDays = Duration * Budget.PaydayValue ?? 30;
            }
            else if (Budget.PaydayType == "WorkingDays")
            {
                int? NumberOfDaysBefore = Budget.PaydayValue;
                NumberOfDays = GetNumberOfDaysLastWorkingDay(NumberOfDaysBefore);
            }
            else if (Budget.PaydayType == "OfEveryMonth")
            {
                int year = GetBudgetLocalTime(DateTime.UtcNow).Year;
                int month = GetBudgetLocalTime(DateTime.UtcNow).Month;
                int days = DateTime.DaysInMonth(year, month);
                NumberOfDays = days;
            }
            else if (Budget.PaydayType == "LastOfTheMonth")
            {
                int dayNumber = ((int)Enum.Parse(typeof(DayOfWeek), Budget.PaydayDuration));
                NumberOfDays = GetNumberOfDaysLastDayOfWeek(dayNumber);
            }

            return NumberOfDays;
        }
        public string UpdateBudgetCreateBills(ref Budgets Budget)
        {
            if (Budget != null)
            {
                foreach (Bills Bill in Budget.Bills)
                {
                    if (!Bill.IsClosed)
                    {
                        decimal? BalanceLeft = Bill.BillAmount - Bill.BillCurrentBalance;

                        TimeSpan TimeToGoal = (Bill.BillDueDate.GetValueOrDefault().Date - Budget.BudgetValuesLastUpdated.Date);
                        int? DaysToGoal = (int)TimeToGoal.TotalDays;

                        Bill.RegularBillValue = BalanceLeft / DaysToGoal;
                    }
                }

                return "OK";
            }
            else
            {
                return "No Budget Detected";
            }
        }

        public string UpdateBudgetCreateSavings(ref Budgets Budget)
        {
            if (Budget != null)
            {
                foreach (Savings Saving in Budget.Savings)
                {
                    if (!Saving.IsSavingsClosed)
                    {                    
                        if (Saving.IsRegularSaving)
                        {
                            if (Saving.SavingsType == "TargetAmount")
                            {
                                if (!Saving.IsDailySaving)
                                {
                                    Saving.RegularSavingValue = Saving.PeriodSavingValue / Budget.AproxDaysBetweenPay;
                                }

                                decimal? BalanceLeft = Saving.SavingsGoal - (Saving.CurrentBalance ?? 0);
                                int NumberOfDays = (int)Math.Ceiling(BalanceLeft / Saving.RegularSavingValue ?? 0);

                                DateTime Today = GetBudgetLocalTime(DateTime.UtcNow);
                                Saving.GoalDate = Today.AddDays(NumberOfDays);

                            }
                            else if (Saving.SavingsType == "TargetDate")
                            {
                                decimal? BalanceLeft = Saving.SavingsGoal - (Saving.CurrentBalance ?? 0);

                                TimeSpan TimeToGoal = (Saving.GoalDate.GetValueOrDefault().Date - Budget.BudgetValuesLastUpdated.Date);
                                int? DaysToGoal = (int)TimeToGoal.TotalDays;

                                Saving.RegularSavingValue = BalanceLeft / DaysToGoal;
                            }
                            else if (Saving.SavingsType == "SavingsBuilder")
                            {
                                if (!Saving.IsDailySaving)
                                {
                                    Saving.RegularSavingValue = Saving.PeriodSavingValue / Budget.AproxDaysBetweenPay;
                                }
                            }
                        }
                    }
                }

                return "OK";
            }
            else
            {
                return "No Budget Detected";
            }
        }

        public string UpdateBudgetCreateIncome(ref Budgets Budget)
        {
            if (Budget != null)
            {
                DateTime Today = GetBudgetLocalTime(DateTime.UtcNow).Date;
                Budget.CurrentActiveIncome = 0;
                foreach (IncomeEvents Income in Budget.IncomeEvents)
                {
                    if (!Income.IsClosed)
                    {
                        DateTime NextPayDay = Budget.NextIncomePayday ?? default;
                        if (Income.IsInstantActive ?? false)
                        {
                            DateTime PayDayAfterNext = CalculateNextDate(NextPayDay, Budget.PaydayType, Budget.PaydayValue ?? 1, Budget.PaydayDuration);
                            //Next Income Date happens in this Pay window so process
                            if (Income.DateOfIncomeEvent.Date < NextPayDay.Date)
                            {
                                Income.IncomeActiveDate = DateTime.UtcNow;
                                Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance + Income.IncomeAmount;
                                Budget.LeftToSpendBalance = Budget.LeftToSpendBalance + Income.IncomeAmount;
                                Budget.PlusStashSpendBalance = Budget.PlusStashSpendBalance + Income.IncomeAmount;
                                Budget.CurrentActiveIncome += Income.IncomeAmount;

                                if (Income.IsRecurringIncome)
                                {
                                    DateTime NextIncomeDate = CalculateNextDate(Income.DateOfIncomeEvent, Income.RecurringIncomeType, Income.RecurringIncomeValue ?? 1, Income.RecurringIncomeDuration);

                                    while (NextIncomeDate.Date < NextPayDay.Date)
                                    {
                                        Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance + Income.IncomeAmount;
                                        Budget.PlusStashSpendBalance = Budget.LeftToSpendBalance + Income.IncomeAmount;
                                        Budget.LeftToSpendBalance = Budget.PlusStashSpendBalance + Income.IncomeAmount;
                                        Budget.CurrentActiveIncome += Income.IncomeAmount;
                                        //TODO: Add a Transaction into transactions
                                        NextIncomeDate = CalculateNextDate(NextIncomeDate, Income.RecurringIncomeType, Income.RecurringIncomeValue ?? 1, Income.RecurringIncomeDuration);
                                    } 
                                }
                            }
                            else
                            {
                                DateTime CalPayDate = NextPayDay.Date;
                                while (Income.DateOfIncomeEvent.Date >= NextPayDay.Date)
                                {
                                    CalPayDate = NextPayDay;
                                    NextPayDay = CalculateNextDate(NextPayDay, Budget.PaydayType, Budget.PaydayValue ?? 1, Budget.PaydayDuration);
                                }
                                Income.IncomeActiveDate = CalPayDate.Date;
                            }
                        }
                    }
                }
            }
            else
            {
                return "No Budget Detected";
            }

            return "OK";
        }


        public string UpdateBudgetCreateSavingsSpend(ref Budgets Budget)
        {
            decimal DailySavingOutgoing = new();
            decimal PeriodTotalSavingOutgoing = new();
            decimal PeriodEnvelopeBalance = new();

            int DaysToPayDay = (int)Math.Ceiling((Budget.NextIncomePayday.GetValueOrDefault().Date - Budget.BudgetValuesLastUpdated.Date).TotalDays);

            foreach (Savings Saving in Budget.Savings)
            {
                if (!Saving.IsSavingsClosed)
                {
                    if (Saving.IsRegularSaving & Saving.SavingsType == "SavingsBuilder")
                    {
                        DailySavingOutgoing += Saving.RegularSavingValue ?? 0;
                        PeriodTotalSavingOutgoing += ((Saving.RegularSavingValue ?? 0) * DaysToPayDay);
                    }
                    else if (Saving.IsRegularSaving)
                    {
                        DailySavingOutgoing += Saving.RegularSavingValue ?? 0;
                        //check if goal date is before pay day
                        int DaysToSaving = (int)Math.Ceiling((Saving.GoalDate.GetValueOrDefault().Date - Budget.BudgetValuesLastUpdated.Date).TotalDays);
                        if (DaysToSaving < DaysToPayDay)
                        {
                            PeriodTotalSavingOutgoing += ((Saving.RegularSavingValue ?? 0) * DaysToSaving);
                        }
                        else
                        {
                            PeriodTotalSavingOutgoing += ((Saving.RegularSavingValue ?? 0) * DaysToPayDay);
                        }

                    }
                    else
                    {
                        PeriodEnvelopeBalance += Saving.CurrentBalance ?? 0;
                    }
                }

                if (Saving.CurrentBalance >= 0)
                {
                    PeriodTotalSavingOutgoing += Saving.CurrentBalance ?? 0;
                }                
            }

            Budget.DailySavingOutgoing = DailySavingOutgoing;
            Budget.LeftToSpendBalance = Budget.LeftToSpendBalance - PeriodTotalSavingOutgoing;
            Budget.PlusStashSpendBalance = Budget.PlusStashSpendBalance - PeriodTotalSavingOutgoing + PeriodEnvelopeBalance;


            return "OK";
        }

        public string UpdateBudgetCreateBillsSpend(ref Budgets Budget)
        {
            decimal DailyBillOutgoing = new();
            decimal PeriodTotalBillOutgoing = new();
            int DaysToPayDay = (int)Math.Ceiling((Budget.NextIncomePayday.GetValueOrDefault().Date - Budget.BudgetValuesLastUpdated.Date).TotalDays);
            foreach (Bills Bill in Budget.Bills)
            {
                if (!Bill.IsClosed)
                {
                    DailyBillOutgoing += Bill.RegularBillValue ?? 0;
                    //Check if Due Date is before Pay Dat
                    int DaysToBill = (int)Math.Ceiling((Bill.BillDueDate.GetValueOrDefault().Date - Budget.BudgetValuesLastUpdated.Date).TotalDays);
                    if (Bill.IsRecuring)
                    {
                        if (DaysToBill < DaysToPayDay)
                        {
                            PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToBill;

                            DateTime BillDueAfterNext = CalculateNextDate(Bill.BillDueDate.GetValueOrDefault(), Bill.BillType, Bill.BillValue.GetValueOrDefault(), Bill.BillDuration);
                            int NumberOfDaysBill = (int)Math.Ceiling((BillDueAfterNext - Bill.BillDueDate.GetValueOrDefault()).TotalDays);
                            decimal? BillRegularValue = Bill.BillAmount / NumberOfDaysBill;

                            PeriodTotalBillOutgoing += BillRegularValue.GetValueOrDefault() * (DaysToPayDay - DaysToBill);

                        }
                        else
                        {
                            PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToPayDay;
                        }
                    }
                    else
                    {
                        if (DaysToBill < DaysToPayDay)
                        {
                            PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToBill;
                        }
                        else
                        {
                            PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToPayDay;
                        }
                    }

                    PeriodTotalBillOutgoing += Bill.BillCurrentBalance;

                }
            }
            Budget.DailyBillOutgoing = DailyBillOutgoing;
            Budget.LeftToSpendBalance = Budget.LeftToSpendBalance - PeriodTotalBillOutgoing;
            Budget.PlusStashSpendBalance = Budget.PlusStashSpendBalance - PeriodTotalBillOutgoing;
            Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance - PeriodTotalBillOutgoing;
            return "OK";
        }


        public string UpdateBudgetCreateBillsSpendBorrowPay(ref Budgets Budget)
        {
            decimal DailyBillOutgoing = new();
            decimal PeriodTotalBillOutgoing = new();

            int DaysToPayDay = (int)Math.Ceiling((Budget.NextIncomePayday.GetValueOrDefault().Date - Budget.BudgetValuesLastUpdated.Date).TotalDays);

            foreach (Bills Bill in Budget.Bills)
            {
                if (!Bill.IsClosed)
                {
                    DailyBillOutgoing += Bill.RegularBillValue ?? 0;
                    //Check if Due Date is before Pay Dat
                    int DaysToBill = (int)Math.Ceiling((Bill.BillDueDate.GetValueOrDefault().Date - Budget.BudgetValuesLastUpdated.Date).TotalDays);
                    if (DaysToBill < DaysToPayDay)
                    {
                        if (Bill.IsRecuring)
                        {
                            DateTime BillDueAfterNext = Bill.BillDueDate.GetValueOrDefault().Date;
                            while(BillDueAfterNext < Budget.NextIncomePayday.GetValueOrDefault().Date)
                            {
                                BillDueAfterNext = CalculateNextDate(BillDueAfterNext.Date, Bill.BillType, Bill.BillValue.GetValueOrDefault(), Bill.BillDuration);
                                if(BillDueAfterNext < Budget.NextIncomePayday.GetValueOrDefault().Date)
                                {
                                    DaysToBill = (int)Math.Ceiling((BillDueAfterNext.Date - Budget.BudgetValuesLastUpdated.Date).TotalDays);
                                }
                            }

                            PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToBill;
                        }
                        else
                        {

                            PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToBill;
                        }

                        PeriodTotalBillOutgoing += Bill.BillCurrentBalance;
                    }
                    else
                    {
                        PeriodTotalBillOutgoing += Bill.BillBalanceAtLastPayDay;
                    }

                    

                }
            }

            Budget.DailyBillOutgoing = DailyBillOutgoing;
            Budget.LeftToSpendBalance = Budget.LeftToSpendBalance - PeriodTotalBillOutgoing;
            Budget.PlusStashSpendBalance = Budget.PlusStashSpendBalance - PeriodTotalBillOutgoing;
            Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance - PeriodTotalBillOutgoing;

            return "OK";

        }

        public DateTime CalculateNextDate(DateTime LastDate, string Type, int Value, string? Duration)
        {
            DateTime NextDate = new DateTime();
            string status = "";

            if (Type == "Everynth")
            {
                status = CalculateNextDateEverynth(ref NextDate, LastDate, Value, Duration);
            }
            else if (Type == "WorkingDays")
            {
                status = CalculateNextDateWorkingDays(ref NextDate, LastDate, Value);
            }
            else if (Type == "OfEveryMonth")
            {
                status = CalculateNextDateOfEveryMonth(ref NextDate, LastDate, Value);
            }
            else if (Type == "LastOfTheMonth")
            {
                status = CalculateNextDateLastOfTheMonth(ref NextDate, LastDate, Duration);
            }

            if (status == "OK")
            {
                return NextDate;
            }
            else
            {
                throw new Exception(status);
            }

        }

        public string CalculateNextDateEverynth(ref DateTime NextDate, DateTime LastDate, int Value, string? Duration)
        {
            try
            {
                int IntDuration;
                if (Duration == "days")
                {
                    IntDuration = 1;
                    int DaysBetween = IntDuration * Value;
                    NextDate = LastDate.AddDays(DaysBetween);

                    return "OK";
                }
                else if (Duration == "weeks")
                {
                    IntDuration = 7;
                    int DaysBetween = IntDuration * Value;
                    NextDate = LastDate.AddDays(DaysBetween);

                    return "OK";
                }
                else if (Duration == "years")
                {
                    NextDate = LastDate.AddYears(Value);

                    return "OK";
                }
                else if (Duration == "months")
                {
                    NextDate = LastDate.AddMonths(Value);

                    return "OK";
                }
                else
                {
                    return "Duration not valid or null";
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        public string CalculateNextDateWorkingDays(ref DateTime NextDate, DateTime LastDate, int Value)
        {
            try
            {
                int year = LastDate.Year;
                int month = LastDate.Month;

                int NextMonth = LastDate.Month == 12 ? 1 : LastDate.Month + 1;
                int NextYear = LastDate.Month == 12 ? LastDate.Year + 1 : LastDate.Year;

                DateTime NextCurrentDate = new DateTime();
                var i = DateTime.DaysInMonth(NextYear, NextMonth);
                var j = 1;
                while (i > 0)
                {
                    var dtCurrent = new DateTime(NextYear, NextMonth, i);
                    if (dtCurrent.DayOfWeek < DayOfWeek.Saturday && dtCurrent.DayOfWeek > DayOfWeek.Sunday)
                    {
                        NextCurrentDate = dtCurrent;
                        if (j == Value)
                        {
                            i = 0;
                        }
                        else
                        {
                            i = i - 1;
                            j = j + 1;
                        }
                    }
                    else
                    {
                        i = i - 1;
                    }
                }

                NextDate = NextCurrentDate.Date;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        public string CalculateNextDateOfEveryMonth(ref DateTime NextDate, DateTime LastDate, int Value)
        {
            try
            {
                int year = LastDate.Year;
                int month = LastDate.Month;

                int NextMonth = LastDate.Month == 12 ? 1 : LastDate.Month + 1;
                int NextYear = LastDate.Month == 12 ? LastDate.Year + 1 : LastDate.Year;

                int days = DateTime.DaysInMonth(NextYear, NextMonth);

                if(Value > days)
                {
                    Value = days;
                }

                NextDate = new DateTime(NextYear, NextMonth, Value).Date;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        public string CalculateNextDateLastOfTheMonth(ref DateTime NextDate, DateTime LastDate, string? Duration)
        {

            try
            {
                int dayNumber = ((int)Enum.Parse(typeof(DayOfWeek), Duration));

                int NextMonth = LastDate.Month == 12 ? 1 : LastDate.Month + 1;
                int NextYear = LastDate.Month == 12 ? LastDate.Year + 1 : LastDate.Year;

                DateTime NewDate = new DateTime();

                var i = DateTime.DaysInMonth(NextYear, NextMonth);
                while (i > 0)
                {
                    var dtCurrent = new DateTime(NextYear, NextMonth, i);
                    if ((int)dtCurrent.DayOfWeek == dayNumber)
                    {
                        NewDate = dtCurrent;
                        i = 0;
                    }
                    else
                    {
                        i = i - 1;
                    }
                }

                NextDate = NewDate.Date;

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "OK";
        }

        public void SetCultureInfo(BudgetSettingValues Settings)
        {
            CultureInfo CultureSetting = new CultureInfo("en-gb");

            CultureSetting.NumberFormat.CurrencySymbol = Settings.CurrencySymbol;
            CultureSetting.NumberFormat.CurrencyDecimalSeparator = Settings.CurrencyDecimalSeparator;
            CultureSetting.NumberFormat.CurrencyGroupSeparator = Settings.CurrencyGroupSeparator;
            CultureSetting.NumberFormat.CurrencyDecimalDigits = Settings.CurrencyDecimalDigits;
            CultureSetting.NumberFormat.CurrencyPositivePattern = Settings.CurrencyPositivePattern;
            CultureSetting.DateTimeFormat.ShortDatePattern = Settings.ShortDatePattern;
            CultureSetting.DateTimeFormat.LongDatePattern = Settings.ShortDatePattern + " HH:mm:ss";
            CultureSetting.DateTimeFormat.DateSeparator = Settings.DateSeparator;

            Thread.CurrentThread.CurrentCulture = CultureSetting;
            Thread.CurrentThread.CurrentUICulture = CultureSetting;
            CultureInfo.DefaultThreadCurrentCulture = CultureSetting;
            CultureInfo.DefaultThreadCurrentUICulture = CultureSetting;
        }

        private void TransactPayDay(ref Budgets budget)
        {
            Transactions PayDayTransaction = new Transactions();
            PayDayTransaction.TransactionAmount = budget.PaydayAmount;
            PayDayTransaction.EventType = "PayDay";
            PayDayTransaction.TransactionDate = budget.NextIncomePayday;
            PayDayTransaction.Notes = $"Transaction added for Budget Payday";
            PayDayTransaction.IsTransacted = true;
            PayDayTransaction.IsIncome = true;

            if (budget.IsMultipleAccounts) 
            {
                BankAccounts Account = budget.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                if(Account != null)
                {
                    PayDayTransaction.AccountID = Account.ID;
                }
                
            }

            PayDayTransaction = _ds.SaveNewTransaction(PayDayTransaction, budget.BudgetID).Result;

            budget.BankBalance += budget.PaydayAmount;
            budget.MoneyAvailableBalance += budget.PaydayAmount;
            budget.LeftToSpendBalance += budget.PaydayAmount;
            budget.PlusStashSpendBalance += budget.PaydayAmount;
        }

        private void CloseSaving(ref Savings Saving)
        {
            Saving.CurrentBalance = Saving.SavingsGoal;
            Saving.GoalDate = null;
            Saving.RegularSavingValue = null;
            Saving.PeriodSavingValue = null;
            Saving.IsSavingsClosed = true;
        }

        private void TransactBill(ref Bills Bill, Budgets budget)
        {
            Transactions BillTransaction = new Transactions();
            BillTransaction.TransactionAmount = Bill.BillAmount;
            BillTransaction.EventType = "Bill";
            BillTransaction.TransactionDate = Bill.BillDueDate;
            BillTransaction.Notes = $"Transaction added for bill, {Bill.BillName}";
            BillTransaction.IsTransacted = true;
            BillTransaction.Payee = Bill.BillPayee;
            BillTransaction.Category = Bill.Category;
            BillTransaction.CategoryID = Bill.CategoryID;

            if (budget.IsMultipleAccounts)
            {
                if(Bill.AccountID == null || Bill.AccountID == 0)
                {
                    BankAccounts Account = budget.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                    if (Account != null)
                    {
                        BillTransaction.AccountID = Account.ID;
                    }
                }
                else
                {
                    BillTransaction.AccountID = Bill.AccountID;
                }     
            }

            BillTransaction = _ds.SaveNewTransaction(BillTransaction, budget.BudgetID).Result;

            if (BillTransaction.TransactionID != 0)
            {
                Bill.BillCurrentBalance = 0;
                Bill.BillBalanceAtLastPayDay = 0;
            }

            if (Bill.IsRecuring)
            {

                Bill.BillDueDate = CalculateNextDate(Bill.BillDueDate.GetValueOrDefault(), Bill.BillType, Bill.BillValue.GetValueOrDefault(), Bill.BillDuration);
                TimeSpan Difference = (TimeSpan)(Bill.BillDueDate - GetBudgetLocalTime(DateTime.UtcNow).Date);
                int NumberOfDays = (int)Difference.TotalDays;
                decimal RemainingBillAmount = Bill.BillAmount - Bill.BillCurrentBalance ?? 0;

                if (NumberOfDays != 0)
                {
                    Bill.RegularBillValue = RemainingBillAmount / NumberOfDays;
                    Bill.RegularBillValue = Math.Round(Bill.RegularBillValue.GetValueOrDefault(), 2);
                }
                else
                {
                    Bill.RegularBillValue = RemainingBillAmount;
                }

                Bill.LastUpdatedDate = DateTime.UtcNow;
            }
            else
            {
                Bill.RegularBillValue = 0;
                Bill.IsClosed = true;
                Bill.LastUpdatedDate = DateTime.UtcNow;
            }
        }

        private void TransactIncomeEvent(ref IncomeEvents Income, Budgets budget)
        {
            Transactions IncomeTransaction = new Transactions();
            IncomeTransaction.TransactionAmount = Income.IncomeAmount;
            IncomeTransaction.EventType = "IncomeEvent";
            IncomeTransaction.TransactionDate = Income.DateOfIncomeEvent;
            IncomeTransaction.Notes = $"Transaction added for Income Event, {Income.IncomeName}";
            IncomeTransaction.IsTransacted = true;
            IncomeTransaction.IsIncome = true;

            if (budget.IsMultipleAccounts)
            {
                if (Income.AccountID == null || Income.AccountID == 0)
                {
                    BankAccounts Account = budget.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                    if (Account != null)
                    {
                        IncomeTransaction.AccountID = Account.ID;
                    }
                }
                else
                {
                    IncomeTransaction.AccountID = Income.AccountID;
                }
            }

            IncomeTransaction = _ds.SaveNewTransaction(IncomeTransaction, budget.BudgetID).Result;

            if (IncomeTransaction.TransactionID != 0)
            {
                if (Income.IsRecurringIncome)
                {
                    Income.DateOfIncomeEvent = CalculateNextDate(Income.DateOfIncomeEvent, Income.RecurringIncomeType, Income.RecurringIncomeValue.GetValueOrDefault(), Income.RecurringIncomeDuration);

                    if (Income.IsInstantActive.GetValueOrDefault())
                    {
                        Income.IncomeActiveDate = DateTime.UtcNow;
                    }

                }
                else
                {
                    Income.IsClosed = true;
                    Income.IsIncomeAddedToBalance = true;
                }
            }
        }

        public string BudgetDailyEventsValuesUpdate(ref Budgets Budget)
        {
            int Index = Budget.PayPeriodStats.FindIndex(p => p.IsCurrentPeriod);
            PayPeriodStats Stats = Budget.PayPeriodStats[Index];
            //ADD REGULAR VALUES TO ALL SAVINGS AND BILLS
            for (int i = Budget.Savings.Count - 1; i >= 0; i--)
            {
                Savings Saving = Budget.Savings[i];

                if (!Saving.IsSavingsClosed)
                {
                    if (Saving.IsRegularSaving)
                    {
                        Saving.LastUpdatedDate = DateTime.UtcNow;
                        Saving.LastUpdatedValue = Saving.RegularSavingValue;

                        if (Saving.SavingsType == "SavingsBuilder")
                        {
                            Saving.CurrentBalance += Saving.RegularSavingValue;
                            Stats.SavingsToDate += Saving.RegularSavingValue.GetValueOrDefault();
                        }
                        else if (!(Saving.CurrentBalance >= Saving.SavingsGoal))
                        {
                            Saving.CurrentBalance += Saving.RegularSavingValue;
                            Stats.SavingsToDate += Saving.RegularSavingValue.GetValueOrDefault();
                        }
                        else if (!Saving.CanExceedGoal)
                        {
                            Saving.CurrentBalance = Saving.SavingsGoal;
                        }
                    }
                }
                Budget.Savings[i] = Saving;
            }

            for (int i = Budget.Bills.Count - 1; i >= 0; i--)
            {
                Bills Bill = Budget.Bills[i];

                if (!(Bill.BillCurrentBalance >= Bill.BillAmount))
                {
                    Bill.BillCurrentBalance += Bill.RegularBillValue.GetValueOrDefault();
                    Stats.BillsToDate += Bill.RegularBillValue.GetValueOrDefault();
                }
                
                Bill.LastUpdatedDate = DateTime.UtcNow;                                

                Budget.Bills[i] = Bill;
            }

            Budget.PayPeriodStats[Index] = Stats;

            return "OK";
        }

        public async Task<Budgets> BudgetDailyCycle(Budgets budget)
        {
            if (budget.IsCreated)
            {
                while (budget.BudgetValuesLastUpdated < GetBudgetLocalTime(DateTime.UtcNow).Date)
                {
                    budget.BudgetValuesLastUpdated = budget.BudgetValuesLastUpdated.AddDays(1);

                    string status = "OK";
                    status = status == "OK" ? BudgetDailyEventsValuesUpdate(ref budget) : status;

                    if(status == "OK")
                    {
                        budget = await BudgetDailyEventsCheck(budget);
                    }

                    budget.LeftToSpendBalance = budget.BankBalance;
                    budget.PlusStashSpendBalance = budget.BankBalance;
                    budget.MoneyAvailableBalance = budget.BankBalance;

                    status = status == "OK" ? BudgetDailyCycleBudgetValuesUpdate(ref budget) : status;


                    budget = await TransactTodaysTransactions(budget);

                    if (status == "OK")
                    {
                        //IF PAY DAY THEN UPDATE THE START PAY PERIOD STATS
                        int Index = budget.PayPeriodStats.FindIndex(p => p.IsCurrentPeriod);
                        PayPeriodStats Stats = budget.PayPeriodStats[Index];

                        if (Stats.StartBBPeiordAmount == 0)
                        {
                            Stats.StartLtSDailyAmount = budget.LeftToSpendDailyAmount;
                            Stats.StartLtSPeiordAmount = budget.LeftToSpendBalance;
                            Stats.StartBBPeiordAmount = budget.BankBalance;
                            Stats.StartMaBPeiordAmount = budget.MoneyAvailableBalance;
                        }
                        Stats.EndDate = budget.NextIncomePayday.GetValueOrDefault();

                        budget.PayPeriodStats[Index] = Stats;
                        

                        for (int i = budget.PayPeriodStats.Count - 1; i >= 0; i--)
                        {
                            _ds.UpdatePayPeriodStats(budget.PayPeriodStats[i]);
                        }                        

                        //SAVE BUDGET UPDATES - BEFORE CHECK IT HASNT ALREADY BEEN UPDATED
                        DateTime BudgetUpdatedCheck = await _ds.GetBudgetValuesLastUpdatedAsync(budget.BudgetID, "DailyChecks");

                        if(BudgetUpdatedCheck.Date >= GetBudgetLocalTime(DateTime.UtcNow).Date)
                        {
                            await Shell.Current.DisplayAlert("Budget Already Updated!", "Your budget has already been updated for today, any changes you have just made will not be saved.", "OK");
                            budget = await _ds.GetBudgetDetailsAsync(budget.BudgetID, "Full");

                            return budget;
                        }
                        else
                        {
                            budget = await _ds.SaveBudgetDailyCycle(budget);
                        }
                        
                    }
                }
            }
            return budget;
        }

        private async Task<Budgets> TransactTodaysTransactions(Budgets budget)
        {
            for (int i = budget.Transactions.Count - 1; i >= 0; i--)
            {
                Transactions Transaction = budget.Transactions[i];
                if (!Transaction.IsTransacted)
                {
                    if (Transaction.TransactionDate.GetValueOrDefault().Date == GetBudgetLocalTime(DateTime.UtcNow).Date)
                    {
                        var popup = new PopupDailyTransaction(Transaction, new PopupDailyTransactionViewModel(), this);
                        var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
                        if ((string)result.ToString() == "OK")
                        {
                            Transaction.IsTransacted = true;
                            Transact(ref Transaction, ref budget);
                            budget.Transactions[i] = Transaction;
                        }
                        else if ((string)result.ToString() == "Delete")
                        {
                            _ds.DeleteTransaction(Transaction.TransactionID);
                            budget.Transactions.RemoveAt(i);
                        }
                        else
                        {
                            Transaction = (Transactions)result;

                            if (Transaction.TransactionDate.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date && Transaction.TransactionDate.GetValueOrDefault().Date != GetBudgetLocalTime(DateTime.UtcNow).Date)
                            {
                                Transaction.IsTransacted = true;
                                Transact(ref Transaction, ref budget);
                            }
                            budget.Transactions[i] = Transaction;
                        }
                    }
                }
            }

            return budget;
        }

        private async Task<Budgets> BudgetDailyEventsCheck(Budgets budget)
        {
            int Index = budget.PayPeriodStats.FindIndex(p => p.IsCurrentPeriod);
            PayPeriodStats Stats = budget.PayPeriodStats[Index];

            int notificationID = int.Parse($"1{budget.BudgetID}");
            LocalNotificationCenter.Current.Cancel(notificationID);

            if (budget.NextIncomePayday > DateTime.UtcNow && budget.NextIncomePayday <= DateTime.Today.AddDays(7))
            {
                var notification = new NotificationRequest
                {
                    NotificationId = notificationID,
                    Title = $"It's payday for budget {budget.BudgetName}!",
                    Description = $"You just got paid {budget.PaydayAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} log into the app to confirm and get budgeting the money!",
                    Schedule =
                    {
                        NotifyTime = budget.NextIncomePayday.GetValueOrDefault().Date.AddHours(8)
                    },
                    Android =
                    {
                        ChannelId = "EventSchedule",
                        IconSmallName =
                        {
                              ResourceName = "appicon",
                        },
                        PendingIntentFlags = AndroidPendingIntentFlags.OneShot,
                        Priority = AndroidPriority.High,
                    }
                };

                await LocalNotificationCenter.Current.Show(notification);

                if (DateTime.Today < budget.NextIncomePayday.GetValueOrDefault().Date.AddDays(-2))
                {
                    notificationID = int.Parse($"10{budget.BudgetID}");
                    LocalNotificationCenter.Current.Cancel(notificationID);

                    notification = new NotificationRequest
                    {
                        NotificationId = notificationID,
                        Title = $"Get prepared, {budget.BudgetName}'s payday is tomorrow!",
                        Description = $"You are going to get paid {budget.PaydayAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} tomorrow, log into the app now if you need to make any changes!",
                        Schedule =
                        {
                            NotifyTime = budget.NextIncomePayday.GetValueOrDefault().Date.AddDays(-1).AddHours(8)
                        },
                        Android =
                        {
                            ChannelId = "EventSchedule",
                            IconSmallName =
                            {
                                  ResourceName = "appicon",
                            },
                            PendingIntentFlags = AndroidPendingIntentFlags.OneShot,
                            Priority = AndroidPriority.High,
                        }
                    };

                    await LocalNotificationCenter.Current.Show(notification);
                }
            }

            if (budget.NextIncomePayday.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date)
            {
                //Confirm pay amount and date!
                var popup = new PopupDailyPayDay(budget, new PopupDailyPayDayViewModel(), this);
                var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

                if ((string)result.ToString() == "OK")
                {
                    Stats.IsCurrentPeriod = false;
                    budget.PayPeriodStats[Index] = Stats;
                    budget.PayPeriodStats.Add(await _ds.CreateNewPayPeriodStats(budget.BudgetID));

                    Index = budget.PayPeriodStats.FindIndex(p => p.IsCurrentPeriod);
                    Stats = budget.PayPeriodStats[Index];

                    //Add next pay amount
                    TransactPayDay(ref budget);
                    Stats.IncomeToDate += budget.PaydayAmount.GetValueOrDefault();
                    //Update the next pay date
                    budget.NextIncomePayday = CalculateNextDate(budget.NextIncomePayday.GetValueOrDefault(), budget.PaydayType, budget.PaydayValue.GetValueOrDefault(), budget.PaydayDuration);
                    TimeSpan NoOfDays = budget.NextIncomePayday.GetValueOrDefault().Date - budget.BudgetValuesLastUpdated.Date;
                    budget.AproxDaysBetweenPay = (int)NoOfDays.TotalDays;
                    //Reset any envelope savings
                    for (int i = budget.Savings.Count - 1; i >= 0; i--)
                    {
                        Savings Saving = budget.Savings[i];

                        if (!Saving.IsRegularSaving)
                        {
                            if(Saving.IsTopUp)
                            {
                                Stats.SavingsToDate += Saving.PeriodSavingValue.GetValueOrDefault();
                                Saving.CurrentBalance += Saving.PeriodSavingValue;
                                Saving.LastUpdatedValue = Saving.PeriodSavingValue;
                                Saving.GoalDate = budget.NextIncomePayday;
                            }
                            else
                            {
                                Stats.SavingsToDate += (Saving.PeriodSavingValue.GetValueOrDefault() - Saving.CurrentBalance.GetValueOrDefault());

                                Saving.CurrentBalance = Saving.PeriodSavingValue;
                                Saving.LastUpdatedValue = Saving.PeriodSavingValue;
                                Saving.GoalDate = budget.NextIncomePayday;
                            }


                        }

                        budget.Savings[i] = Saving;
                    }

                    for (int i = budget.Bills.Count - 1; i >= 0; i--)
                    {
                        Bills Bill = budget.Bills[i];
                        if(!Bill.IsClosed)
                        {
                            Bill.BillBalanceAtLastPayDay = Bill.BillCurrentBalance;
                        }

                        budget.Bills[i] = Bill;
                    }
                }
                else
                {
                    budget = (Budgets)result;

                    if (budget.NextIncomePayday.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date)
                    {
                        Stats.IsCurrentPeriod = false;
                        budget.PayPeriodStats[Index] = Stats;
                        budget.PayPeriodStats.Add(await _ds.CreateNewPayPeriodStats(budget.BudgetID));

                        Index = budget.PayPeriodStats.FindIndex(p => p.IsCurrentPeriod);
                        Stats = budget.PayPeriodStats[Index];

                        //Add next pay amount
                        TransactPayDay(ref budget);
                        Stats.IncomeToDate += budget.PaydayAmount.GetValueOrDefault();
                        //Update the next pay date
                        budget.NextIncomePayday = CalculateNextDate(budget.NextIncomePayday.GetValueOrDefault(), budget.PaydayType, budget.PaydayValue.GetValueOrDefault(), budget.PaydayDuration);
                        TimeSpan NoOfDays = budget.NextIncomePayday.GetValueOrDefault().Date - budget.BudgetValuesLastUpdated.Date;
                        budget.AproxDaysBetweenPay = (int)NoOfDays.TotalDays;
                        //Reset any envelope savings
                        for (int i = budget.Savings.Count - 1; i >= 0; i--)
                        {
                            Savings Saving = budget.Savings[i];

                            if (!Saving.IsRegularSaving)
                            {
                                if (Saving.IsTopUp)
                                {
                                    Stats.SavingsToDate += Saving.PeriodSavingValue.GetValueOrDefault();
                                    Saving.CurrentBalance += Saving.PeriodSavingValue;
                                    Saving.LastUpdatedValue = Saving.PeriodSavingValue;
                                    Saving.GoalDate = budget.NextIncomePayday;
                                }
                                else
                                {
                                    Stats.SavingsToDate += (Saving.PeriodSavingValue.GetValueOrDefault() - Saving.CurrentBalance.GetValueOrDefault());

                                    Saving.CurrentBalance = Saving.PeriodSavingValue;
                                    Saving.LastUpdatedValue = Saving.PeriodSavingValue;
                                    Saving.GoalDate = budget.NextIncomePayday;
                                }

                            }

                            budget.Savings[i] = Saving;
                        }

                        for (int i = budget.Bills.Count - 1; i >= 0; i--)
                        {
                            Bills Bill = budget.Bills[i];
                            if(!Bill.IsClosed)
                            {
                                Bill.BillBalanceAtLastPayDay = Bill.BillCurrentBalance;
                            }

                            budget.Bills[i] = Bill;
                        }
                    }
                }
            }

            for (int i = budget.Savings.Count - 1; i >= 0; i--)
            {
                Savings Saving = budget.Savings[i];

                if (Saving.SavingsType == "TargetAmount" || Saving.SavingsType == "TargetDate")
                {
                    notificationID = int.Parse($"2{Saving.SavingID}");
                    LocalNotificationCenter.Current.Cancel(notificationID);

                    if (Saving.GoalDate.GetValueOrDefault() > DateTime.UtcNow && Saving.GoalDate.GetValueOrDefault() <= DateTime.Today.AddDays(7))
                    {

                        var notification = new NotificationRequest
                        {
                            NotificationId = notificationID,
                            Title = $"Congrats you have reached your {Saving.SavingsName} saving goal!",
                            Description = $"You have reached a saving goal of {Saving.SavingsGoal.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} for budget, {budget.BudgetName}. Enjoying spending the money GUILT FREE!",
                            Schedule =
                            {
                                NotifyTime = Saving.GoalDate.GetValueOrDefault().Date.AddHours(8)
                            },
                            Android =
                            {
                                ChannelId = "EventSchedule",
                                IconSmallName =
                                {
                                      ResourceName = "appicon",
                                },
                                PendingIntentFlags = AndroidPendingIntentFlags.OneShot,
                                Priority = AndroidPriority.High,
                            }
                        };

                        await LocalNotificationCenter.Current.Show(notification);
                    }

                    if (Saving.GoalDate.GetValueOrDefault().Date == budget.BudgetValuesLastUpdated.Date)
                    {
                        if (Saving.IsAutoComplete)
                        {
                            CloseSaving(ref Saving);
                            budget.Savings[i] = Saving;
                        }
                        else
                        {
                            var popup = new PopupDailySaving(Saving, new PopupDailySavingViewModel(), this);
                            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

                            if ((string)result.ToString() == "OK")
                            {
                                CloseSaving(ref Saving);
                                budget.Savings[i] = Saving;
                            }
                            else if ((string)result.ToString() == "Delete")
                            {
                                await _ds.DeleteSaving(Saving.SavingID);
                                budget.Savings.RemoveAt(i);
                            }
                            else
                            {
                                Saving = (Savings)result;

                                if (Saving.GoalDate.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date)
                                {
                                    CloseSaving(ref Saving);
                                }

                                budget.Savings[i] = Saving;
                            }
                        }
                    }
                }
            }

            for (int i = budget.Bills.Count - 1; i >= 0; i--)
            {
                Bills Bill = budget.Bills[i];

                notificationID = int.Parse($"3{Bill.BillID}");
                LocalNotificationCenter.Current.Cancel(notificationID);

                if (Bill.BillDueDate.GetValueOrDefault() > DateTime.UtcNow && Bill.BillDueDate.GetValueOrDefault() <= DateTime.Today.AddDays(7))
                {

                    var notification = new NotificationRequest
                    {
                        NotificationId = notificationID,
                        Title = $"Your bill {Bill.BillName} is due today!",
                        Description = $"You are due to pay a bill today of {Bill.BillAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} for budget, {budget.BudgetName}. Log into the app to finalise the bill.",
                        Schedule =
                        {
                            NotifyTime = Bill.BillDueDate.GetValueOrDefault().Date.AddHours(8)
                        },
                        Android =
                        {
                            ChannelId = "EventSchedule",
                            IconSmallName =
                            {
                                  ResourceName = "appicon",
                            },
                            PendingIntentFlags = AndroidPendingIntentFlags.OneShot,
                            Priority = AndroidPriority.High,
                        }
                    };

                    await LocalNotificationCenter.Current.Show(notification);
                }

                if (Bill.BillDueDate.GetValueOrDefault().Date == budget.BudgetValuesLastUpdated.Date)
                {
                    var popup = new PopupDailyBill(Bill, new PopupDailyBillViewModel(), this);
                    var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

                    if ((string)result.ToString() == "OK")
                    {
                        TransactBill(ref Bill, budget);
                        Stats.SpendToDate += Bill.BillAmount.GetValueOrDefault();
                        budget.BankBalance = budget.BankBalance - Bill.BillAmount;
                        budget.Bills[i] = Bill;
                    }
                    else if ((string)result.ToString() == "Delete")
                    {
                        await _ds.DeleteBill(Bill.BillID);
                        budget.Bills.RemoveAt(i);
                    }
                    else
                    {
                        Bill = (Bills)result;

                        if (Bill.BillDueDate.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date)
                        {
                            TransactBill(ref Bill, budget);
                            Stats.SpendToDate += Bill.BillAmount.GetValueOrDefault();
                            budget.BankBalance = budget.BankBalance - Bill.BillAmount;
                        }
                        budget.Bills[i] = Bill;
                    }
                }
            }

            for (int i = budget.IncomeEvents.Count - 1; i >= 0; i--)
            {
                IncomeEvents Income = budget.IncomeEvents[i];

                notificationID = int.Parse($"4{Income.IncomeEventID}");
                LocalNotificationCenter.Current.Cancel(notificationID);

                if (Income.DateOfIncomeEvent > DateTime.UtcNow && Income.DateOfIncomeEvent <= DateTime.Today.AddDays(7))
                {
                    var notification = new NotificationRequest
                    {
                        NotificationId = notificationID,
                        Title = $"Your are about to get paid for {Income.IncomeName}!",
                        Description = $"You are due an income of {Income.IncomeAmount.ToString("c", CultureInfo.CurrentCulture)} for budget, {budget.BudgetName}. Log into the app to confirm you have recieved the money.",
                        Schedule =
                        {
                            NotifyTime = Income.DateOfIncomeEvent.Date.AddHours(8)
                        },
                        Android =
                        {
                            ChannelId = "EventSchedule",
                            IconSmallName =
                            {
                                  ResourceName = "appicon",
                            },
                            PendingIntentFlags = AndroidPendingIntentFlags.OneShot,
                            Priority = AndroidPriority.High,
                        }
                    };

                    await LocalNotificationCenter.Current.Show(notification);
                }

                if (Income.DateOfIncomeEvent.Date == budget.BudgetValuesLastUpdated.Date)
                {
                    var popup = new PopupDailyIncome(Income, new PopupDailyIncomeViewModel(), this);
                    var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
                    if ((string)result.ToString() == "OK")
                    {
                        TransactIncomeEvent(ref Income, budget);
                        Stats.IncomeToDate += Income.IncomeAmount;
                        budget.BankBalance = budget.BankBalance + Income.IncomeAmount;
                        budget.IncomeEvents[i] = Income;
                    }
                    else if ((string)result.ToString() == "Delete")
                    {
                        await _ds.DeleteIncome(Income.IncomeEventID);
                        budget.IncomeEvents.RemoveAt(i);
                    }
                    else
                    {
                        Income = (IncomeEvents)result;

                        if (Income.DateOfIncomeEvent.Date <= budget.BudgetValuesLastUpdated.Date)
                        {
                            TransactIncomeEvent(ref Income, budget);
                            Stats.IncomeToDate += Income.IncomeAmount;
                            budget.BankBalance = budget.BankBalance + Income.IncomeAmount;
                        }
                        budget.IncomeEvents[i] = Income;
                    }
                }

            }

            for (int i = budget.Transactions.Count - 1; i >= 0; i--)
            {
                Transactions Transaction = budget.Transactions[i];
                if (!Transaction.IsTransacted)
                {
                    notificationID = int.Parse($"5{Transaction.TransactionID}");
                    LocalNotificationCenter.Current.Cancel(notificationID);

                    if (Transaction.TransactionDate > DateTime.UtcNow && Transaction.TransactionDate <= DateTime.Today.AddDays(7))
                    {
                        var notification = new NotificationRequest
                        {
                            NotificationId = notificationID,
                            Title = $"You a due to make a payment of {Transaction.TransactionAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} today!",
                            Description = $"Log into the app and confirm the payment of {Transaction.TransactionAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture)} for budget, {budget.BudgetName}.",
                            Schedule =
                            {
                                NotifyTime = Transaction.TransactionDate.GetValueOrDefault().Date.AddHours(8)
                            },
                            Android =
                            {
                                ChannelId = "EventSchedule",
                                IconSmallName =
                                {
                                      ResourceName = "appicon",
                                },
                                PendingIntentFlags = AndroidPendingIntentFlags.OneShot,
                                Priority = AndroidPriority.High,
                            }
                        };

                        await LocalNotificationCenter.Current.Show(notification);
                    }

                    if (Transaction.TransactionDate.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date)
                    {
                        if (Transaction.TransactionDate.GetValueOrDefault().Date < GetBudgetLocalTime(DateTime.UtcNow).Date)
                        {
                            var popup = new PopupDailyTransaction(Transaction, new PopupDailyTransactionViewModel(), this);
                            var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
                            if ((string)result.ToString() == "OK")
                            {
                                Transaction.IsTransacted = true;
                                Transact(ref Transaction, ref budget);
                                budget.Transactions[i] = Transaction;
                            }
                            else if ((string)result.ToString() == "Delete")
                            {
                                _ds.DeleteTransaction(Transaction.TransactionID);
                                budget.Transactions.RemoveAt(i);
                            }
                            else
                            {
                                Transaction = (Transactions)result;

                                if (Transaction.TransactionDate.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date && Transaction.TransactionDate.GetValueOrDefault().Date != GetBudgetLocalTime(DateTime.UtcNow).Date)
                                {
                                    Transaction.IsTransacted = true;
                                    Transact(ref Transaction, ref budget);
                                }
                                budget.Transactions[i] = Transaction;
                            }
                        }
                    }
                }
            }

            budget.PayPeriodStats[Index] = Stats;

            return budget;

        }

        private void Transact(ref Transactions T, ref Budgets Budget)
        {
            if (T.IsSpendFromSavings)
            {
                TransactSavingsTransaction(ref T, ref Budget);
            }
            else
            {
                TransactTransaction(ref T, ref Budget);
            }
        }

        public string TransactTransaction(ref Transactions T, ref Budgets Budget)
        {
            if (T.IsIncome)
            {
                Budget.BankBalance += T.TransactionAmount;
                Budget.MoneyAvailableBalance += T.TransactionAmount;
                Budget.PlusStashSpendBalance += T.TransactionAmount;
                Budget.LeftToSpendBalance += T.TransactionAmount;
                int DaysToPayDay = (int)Math.Ceiling((Budget.NextIncomePayday.GetValueOrDefault().Date - DateTime.Today.Date).TotalDays);
                Budget.LeftToSpendDailyAmount += (T.TransactionAmount ?? 0) / DaysToPayDay;
                Budget.PayPeriodStats[0].IncomeToDate += T.TransactionAmount ?? 0;
                Budget.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                Budget.BankBalance -= T.TransactionAmount;
                Budget.MoneyAvailableBalance -= T.TransactionAmount;
                Budget.PlusStashSpendBalance -= T.TransactionAmount;
                Budget.LeftToSpendBalance -= T.TransactionAmount;
                Budget.LeftToSpendDailyAmount -= T.TransactionAmount ?? 0;
                Budget.PayPeriodStats[0].SpendToDate += T.TransactionAmount ?? 0;
                Budget.LastUpdated = DateTime.UtcNow;
            }

            T.IsTransacted = true;

            if (Budget.IsMultipleAccounts)
            {
                BankAccounts? Account;
                if (T.AccountID.GetValueOrDefault() == 0)
                {
                    Account = Budget.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                }
                else
                {
                    int AccountID = T.AccountID.GetValueOrDefault();
                    Account = Budget.BankAccounts.Where(b => b.ID == AccountID).FirstOrDefault();
                }

                if (Account != null)
                {
                    if (T.IsIncome)
                    {
                        Account.AccountBankBalance += T.TransactionAmount ?? 0;
                    }
                    else
                    {
                        Account.AccountBankBalance -= T.TransactionAmount ?? 0;
                    }
                }

            }
            return "OK";

        }

        public string TransactSavingsTransaction(ref Transactions T, ref Budgets Budget)
        {

            int TransactionsSavingsID = T.SavingID ?? 0;

            Savings S = Budget.Savings.Where(s => s.SavingID == TransactionsSavingsID).First();

            if (S == null)
            {
                return "Couldnt find saving";
            }
            else
            {
                if (T.SavingsSpendType == "UpdateValues")
                {
                    if (T.IsIncome)
                    {
                        Budget.BankBalance += T.TransactionAmount;
                        Budget.MoneyAvailableBalance += T.TransactionAmount;
                        Budget.PayPeriodStats[0].IncomeToDate += T.TransactionAmount ?? 0;
                        Budget.PayPeriodStats[0].SavingsToDate += T.TransactionAmount ?? 0;
                        Budget.LastUpdated = DateTime.UtcNow;
                        S.CurrentBalance += T.TransactionAmount;
                        S.LastUpdatedValue = T.TransactionAmount;
                        S.LastUpdatedDate = DateTime.UtcNow;
                    }
                    else
                    {
                        Budget.BankBalance -= T.TransactionAmount;
                        Budget.MoneyAvailableBalance -= T.TransactionAmount;
                        Budget.LastUpdated = DateTime.UtcNow;
                        S.CurrentBalance -= T.TransactionAmount;
                        S.LastUpdatedValue = T.TransactionAmount;
                        S.LastUpdatedDate = DateTime.UtcNow;
                        Budget.PayPeriodStats[0].SpendToDate += T.TransactionAmount ?? 0;
                    }

                    RecalculateRegularSavingFromTransaction(ref S);

                }
                else if (T.SavingsSpendType == "MaintainValues")
                {
                    if (T.IsIncome)
                    {
                        Budget.BankBalance += T.TransactionAmount;
                        Budget.MoneyAvailableBalance += T.TransactionAmount;
                        Budget.LeftToSpendBalance += T.TransactionAmount;
                        Budget.LastUpdated = DateTime.UtcNow;
                        S.CurrentBalance += T.TransactionAmount;
                        S.SavingsGoal += T.TransactionAmount;
                        S.LastUpdatedValue = T.TransactionAmount;
                        S.LastUpdatedDate = DateTime.UtcNow;
                        Budget.PayPeriodStats[0].IncomeToDate += T.TransactionAmount ?? 0;
                        Budget.PayPeriodStats[0].SavingsToDate += T.TransactionAmount ?? 0;
                    }
                    else
                    {
                        Budget.BankBalance -= T.TransactionAmount;
                        Budget.MoneyAvailableBalance -= T.TransactionAmount;
                        Budget.LeftToSpendBalance += T.TransactionAmount;
                        Budget.LastUpdated = DateTime.UtcNow;
                        S.CurrentBalance -= T.TransactionAmount;
                        S.SavingsGoal -= T.TransactionAmount;
                        S.LastUpdatedValue = T.TransactionAmount;
                        S.LastUpdatedDate = DateTime.UtcNow;
                        Budget.PayPeriodStats[0].SpendToDate += T.TransactionAmount ?? 0;
                    }
                }
                else if (T.SavingsSpendType == "BuildingSaving" | T.SavingsSpendType == "EnvelopeSaving")
                {
                    if (T.IsIncome)
                    {
                        Budget.BankBalance += T.TransactionAmount;
                        Budget.MoneyAvailableBalance += T.TransactionAmount;
                        if(T.SavingsSpendType == "EnvelopeSaving")
                        {
                            Budget.PlusStashSpendBalance += T.TransactionAmount;
                        }
                        Budget.LastUpdated = DateTime.UtcNow;
                        S.CurrentBalance += T.TransactionAmount;
                        S.LastUpdatedValue = T.TransactionAmount;
                        S.LastUpdatedDate = DateTime.UtcNow;
                        Budget.PayPeriodStats[0].IncomeToDate += T.TransactionAmount ?? 0;
                        Budget.PayPeriodStats[0].SavingsToDate += T.TransactionAmount ?? 0;
                    }
                    else
                    {
                        Budget.BankBalance -= T.TransactionAmount;
                        Budget.MoneyAvailableBalance -= T.TransactionAmount;
                        if (T.SavingsSpendType == "EnvelopeSaving")
                        {
                            Budget.PlusStashSpendBalance -= T.TransactionAmount;
                        }
                        Budget.LastUpdated = DateTime.UtcNow;
                        S.CurrentBalance -= T.TransactionAmount;
                        S.LastUpdatedValue = T.TransactionAmount;
                        S.LastUpdatedDate = DateTime.UtcNow;
                        Budget.PayPeriodStats[0].SpendToDate += T.TransactionAmount ?? 0;
                    }
                }

                T.IsTransacted = true;

                if (Budget.IsMultipleAccounts)
                {
                    BankAccounts? Account;
                    if (T.AccountID.GetValueOrDefault() == 0)
                    {
                        Account = Budget.BankAccounts.Where(b => b.IsDefaultAccount).FirstOrDefault();
                    }
                    else
                    {
                        int AccountID = T.AccountID.GetValueOrDefault();
                        Account = Budget.BankAccounts.Where(b => b.ID == AccountID).FirstOrDefault();
                    }

                    if (Account != null)
                    {
                        if (T.IsIncome)
                        {
                            Account.AccountBankBalance += T.TransactionAmount ?? 0;
                        }
                        else
                        {
                            Account.AccountBankBalance -= T.TransactionAmount ?? 0;
                        }
                    }

                }

                return "OK";
            }

        }

        private string RecalculateRegularSavingFromTransaction(ref Savings S)
        {
            if (S.IsRegularSaving)
            {
                if (S.SavingsType == "TargetAmount")
                {
                    CalculateSavingsTargetAmount(ref S);
                }
                else if (S.SavingsType == "TargetDate")
                {
                    CalculateSavingsTargetDate(ref S);
                }
            }

            return "OK";
        }

        private string CalculateSavingsTargetAmount(ref Savings S)
        {
            decimal? BalanceLeft = S.SavingsGoal - (S.CurrentBalance ?? 0);
            int NumberOfDays = (int)Math.Ceiling(BalanceLeft / S.RegularSavingValue ?? 0);

            DateTime Today = GetBudgetLocalTime(DateTime.UtcNow).Date;
            S.GoalDate = Today.AddDays(NumberOfDays);

            return "OK";
        }

        private string CalculateSavingsTargetDate(ref Savings S)
        {
            int DaysToSavingDate = (int)Math.Ceiling((S.GoalDate.GetValueOrDefault().Date - DateTime.Today.Date).TotalDays);
            decimal? AmountOutstanding = S.SavingsGoal - S.CurrentBalance;

            S.RegularSavingValue = AmountOutstanding / DaysToSavingDate;

            return "OK";
        }
        public DateTime GetBudgetLocalTime(DateTime UtcDate)
        {
            DateTime LocalDate = UtcDate;

            if(App.CurrentSettings != null)
            {
                if(!string.IsNullOrEmpty(App.CurrentSettings.TimeZoneName))
                {
                    try
                    {
                        TimeZoneInfo BudgetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(App.CurrentSettings.TimeZoneName);
                        LocalDate = TimeZoneInfo.ConvertTime(UtcDate, BudgetTimeZone);
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        LocalDate = UtcDate.AddHours(App.CurrentSettings.TimeZoneUTCOffset);
                    }
                }
            }

            return LocalDate;
        }

        public async Task NavigateFromPendingIntent(string NavigationType)
        {
            switch(NavigationType)
            {
                case "ShareBudget":

                    int ShareBudgetRequestID = Convert.ToInt32(Preferences.Get("NavigationID", "0"));

                    Preferences.Remove("NavigationType");
                    Preferences.Remove("NavigationID");

                    var popup = new PopUpOTP(ShareBudgetRequestID, new PopUpOTPViewModel(_ds, this), "ShareBudget", this, _ds);
                    var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);

                    if ((string)result.ToString() != "User Closed")
                    {
                        ShareBudgetRequest BudgetRequest = (ShareBudgetRequest)result;

                        bool DefaultBudgetYesNo = await Application.Current.Windows[0].Page.DisplayAlert($"Update Default Budget ", $"CONGRATS!! You have shared a budget with {BudgetRequest.SharedByUserEmail}, do you want to make this budget your default Budget?", "Yes, continue", "No Thanks!");

                        if (DefaultBudgetYesNo)
                        {
                            await ChangeDefaultBudget(App.UserDetails.UserID, BudgetRequest.SharedBudgetID, true);                      
                        }
                    }

                    break;
                case "BudgetShared":
                    Preferences.Remove("NavigationType");

                    break;
                case "SupportReplay":
                    Preferences.Remove("NavigationType");
                    int SupportID = Convert.ToInt32(Preferences.Get("NavigationID", "0"));
                    await Shell.Current.GoToAsync($"{nameof(ViewSupport)}?SupportID={SupportID}");
                    break;
                default:
                    break;     
            }
        }

        public async Task UpdateNotificationPermission()
        {
            var modalStack = Application.Current.Windows[0].Navigation.ModalStack;

            if (modalStack is not null && modalStack.Count > 0)
            {
                var currentModalPage = modalStack[modalStack.Count - 1];
                if (currentModalPage != null)
                {
                    string modalPageName = currentModalPage.GetType().Name;
                    if (modalPageName == "EditAccountSettings")
                    {
                        EditAccountSettingsViewModel bindingContext = (EditAccountSettingsViewModel)currentModalPage.BindingContext;

                        if (bindingContext != null)
                        {
                            bindingContext.IsChanging = true;
                            bindingContext.IsPushNotificationsEnabled = await LocalNotificationCenter.Current.AreNotificationsEnabled();
                            bindingContext.IsChanging = false;
                        }

                    }
                }
            }
        }

        public async Task<Picker> SwitchBudget(string page)
        {
            Application.Current.Resources.TryGetValue("White", out var White);
            Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
            Application.Current.Resources.TryGetValue("Primary", out var Primary);

            List<Budgets> Budgets = await _ds.GetUserAccountBudgets(App.UserDetails.UserID, page);

            Picker picker = new Picker
            {
                Title = "Select a budget",
                ItemsSource = Budgets,
                TitleColor = (Color)Primary,
                BackgroundColor = (Color)White,
                TextColor = (Color)Gray400
            };

            picker.ItemDisplayBinding = new Binding("ChangeBudgetStringConvertor", BindingMode.Default);

            picker.SelectedIndexChanged += async (s, e) =>
            {
                var picker = (Picker)s;
                var SelectedBudget = (Budgets)picker.SelectedItem;

                await ChangeDefaultBudget(App.UserDetails.UserID, SelectedBudget.BudgetID, true);
            };

            if(page == "Dashboard")
            {
                picker.HeightRequest = 2;
            }

            return picker;
        }

        public async Task ChangeDefaultBudget(int UserID, int BudgetID, bool navigate)
        {
            if(BudgetID != App.DefaultBudgetID)
            {
                int PreviousDefaultBudgetIDint = App.DefaultBudgetID;

                List<PatchDoc> UpdateUserDetails = new List<PatchDoc>();

                PatchDoc DefaultBudgetID = new PatchDoc
                {
                    op = "replace",
                    path = "/DefaultBudgetID",
                    value = BudgetID
                };

                UpdateUserDetails.Add(DefaultBudgetID);

                PatchDoc PreviousDefaultBudgetID = new PatchDoc
                {
                    op = "replace",
                    path = "/PreviousDefaultBudgetID",
                    value = PreviousDefaultBudgetIDint
                };

                UpdateUserDetails.Add(PreviousDefaultBudgetID);
                await _ds.PatchUserAccount(App.UserDetails.UserID, UpdateUserDetails);

                App.DefaultBudget = null;
                App.CurrentSettings = null;

                string userDetailsStr = Preferences.Get(nameof(App.UserDetails), "");
                UserDetailsModel userDetails = JsonConvert.DeserializeObject<UserDetailsModel>(userDetailsStr);
                userDetails.SessionExpiry = DateTime.UtcNow.AddDays(App.SessionPeriod);
                userDetails.DefaultBudgetID = BudgetID;

                userDetailsStr = JsonConvert.SerializeObject(userDetails);

                if (Preferences.ContainsKey(nameof(App.DefaultBudgetID)))
                {
                    Preferences.Remove(nameof(App.DefaultBudgetID));
                }

                if (Preferences.ContainsKey(nameof(App.UserDetails)))
                {
                    Preferences.Remove(nameof(App.UserDetails));
                }

                Preferences.Set(nameof(App.UserDetails), userDetailsStr);
                Preferences.Set(nameof(App.DefaultBudgetID), userDetails.DefaultBudgetID);

                App.UserDetails = userDetails;
                App.DefaultBudgetID = BudgetID;

                if (App.CurrentBottomSheet != null)
                {
                    await App.CurrentBottomSheet.DismissAsync();
                    App.CurrentBottomSheet = null;
                }

                if (navigate && App.CurrentPopUp == null)
                {
                    var PopUp = new PopUpPage();
                    App.CurrentPopUp = PopUp;
                    Application.Current.Windows[0].Page.ShowPopup(PopUp);

                    await Shell.Current.GoToAsync($"///{nameof(LandingPage)}");
                }
            }   
            else
            {
                if (App.CurrentBottomSheet != null)
                {
                    await App.CurrentBottomSheet.DismissAsync();
                    App.CurrentBottomSheet = null;
                }
            }
            
        }

        public async Task LoadTabBars(string UserSub, DateTime SubExpiryDate ,string BudgetType)
        {
            int SubLevel = 0;
            int BudgetLevel = 0;

            if (BudgetType == "Premium")
            {
                BudgetLevel = 1;
            }
            else if (BudgetType == "PremiumPlus")
            {
                BudgetLevel = 2;
            }

            if (UserSub == "Premium")
            {
                SubLevel = 1;
            }
            else if (UserSub == "PremiumPlus")
            {
                SubLevel = 2;
            }

            App.MainTabBar.Items.Clear();

            if(BudgetType == "Basic")
            {
                await LoadBasicTabBar();
                return;
            }

            if(SubExpiryDate.AddDays(7) < DateTime.UtcNow)
            {
                //TODO: SUB EXPIRED AND BUDGET WILL REVERT WARNING OR CREATE NEW BUDGET 

                BudgetType = SubLevel == 1 ? "Premium" : "Basic";
                await LoadTabBarBudgetType(BudgetType);
                return;
            }
            else if(SubExpiryDate > DateTime.UtcNow)
            {
                //SUB HAS NOT EXPIRED YET AND ALL IS GOOD
                if (BudgetLevel > SubLevel)
                {
                    //TODO: GIVE WARNING THAT BUDGET ISN'T THE RIGHT LEVEL AND BUDGET WILL REVERT WARNING OR CREATE NEW BUDGET
                    BudgetType = SubLevel == 1 ? "Premium" : "Basic";
                    await LoadTabBarBudgetType(BudgetType);
                    return;
                }
                else
                {
                    await LoadTabBarBudgetType(BudgetType);
                    return;
                }
            }
            else
            {
                //TODO: SUB EXPIRING AND BUDGET WILL REVERT AFTER X DAYS WARNING

                await LoadTabBarBudgetType(BudgetType);
                return;

            }
            
        }

        private async Task LoadTabBarBudgetType(string BudgetType)
        {
            if (BudgetType == "Premium")
            {
                await LoadPremiumTabBar();
            }
            else if (BudgetType == "PremiumPlus")
            {
                await LoadPremiumPlusTabBar();
            }
            else
            {
                await LoadBasicTabBar();
            }
        }

        private async Task LoadPremiumTabBar()
        {
           
        }

        private async Task LoadPremiumPlusTabBar()
        {
            
        }

        private async Task LoadBasicTabBar()
        {            

        }

        public async Task<Dictionary<string, string>> GetIcons()
        {
            MaterialDesignIconsFonts obj = new MaterialDesignIconsFonts();

            return obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
                      .Where(f => f.FieldType == typeof(string))
                      .ToDictionary(f => f.Name.Replace("_", " "),
                                    f => (string)f.GetValue(null));
        }

        public async Task<string> GetIcon(string Name)
        {
            MaterialDesignIconsFonts obj = new MaterialDesignIconsFonts();

            string Icon =  obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Static)
                          .Where(f => f.FieldType == typeof(string) && f.Name.Replace("_", " ") == Name)
                          .Select(f => (string)f.GetValue(null))
                          .FirstOrDefault();

            return Icon;
        }

        public async Task SetSubDetails()
        {
            if (App.UserDetails != null)
            {
                if (App.UserDetails.SubscriptionType == "PremiumPlus")
                {
                    App.IsPremiumAccount = true;
                    if (App.UserDetails.SubscriptionExpiry.AddDays(5) < DateTime.UtcNow)
                    {
                        await _ds.DowngradeUserAccount(App.UserDetails.UserID);
                        App.IsPremiumAccount = false;
                    }
                    else if(App.UserDetails.SubscriptionExpiry < DateTime.UtcNow)
                    {
                        List<string> SubTitle = new List<string>{
                            "Opps, looks like you have let your subscription expire...",
                            "",
                            ""
                        };

                        List<string> Info = new List<string>{
                            $"Your subscription to our premium service expired on {App.UserDetails.SubscriptionExpiry.ToString("dd MMM yyyy")}. If you do not upgrade your account before {App.UserDetails.SubscriptionExpiry.AddDays(5).ToString("dd MMM yyyy")} you will lose your premium benefits. To keep budgeting with an ad free expiernce and access to all our amazing features please subscribe.",
                            "If not don't worry you can continue to use dBudget with the same budgets as before.",
                            "If you have already subscribed don't worry it may take our records a moment to update. If you are having difficulties please contact us, we are here to and love to help."
                        };

                        var popup = new PopupInfo("Subscription expired!", SubTitle, Info);
                        var result = await Application.Current.Windows[0].Page.ShowPopupAsync(popup);
                    }
                }
                else
                {
                    App.IsPremiumAccount = false;
                }
            }
        }

        public async Task MakeSnackBar(string text, Action? action, string? actionButtonText, TimeSpan duration, string snackBarType)
        {
            Application.Current.Resources.TryGetValue("Success", out var Success);
            Application.Current.Resources.TryGetValue("Warning", out var Warning);
            Application.Current.Resources.TryGetValue("Primary", out var Primary);
            Application.Current.Resources.TryGetValue("Danger", out var Danger);
            Application.Current.Resources.TryGetValue("Info", out var Info);
            Application.Current.Resources.TryGetValue("White", out var White);

            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

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

            if(action == null)
            {
                if(actionButtonText == null)
                {
                    actionButtonText = "Ok";
                }
                
                action = () =>
                {
                    source.Cancel();
                };
            }

            var Options = new SnackbarOptions();

            switch (snackBarType)
            {
                case "Success":
                    Options = snackbarSuccessOptions;
                    break;
                case "Info":
                    Options = snackbarInfoOptions;
                    break;
                case "Warning":
                    Options = snackbarWarningOptions;
                    break;
                case "Danger":
                    Options = snackbarDangerOptions;
                    break;
                default:
                    Options = snackbarSuccessOptions;
                    break;
            }

            var SB = Snackbar.Make(text, action, actionButtonText, duration, Options);
            await SB.Show(token);

            return;
        }
    }
}
