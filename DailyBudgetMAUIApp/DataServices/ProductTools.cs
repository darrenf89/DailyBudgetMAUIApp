using DailyBudgetMAUIApp.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using System.Globalization;
using DailyBudgetMAUIApp.ViewModels;
using DailyBudgetMAUIApp.Converters;
using Newtonsoft.Json;
using Maui.FixesAndWorkarounds;
using DailyBudgetMAUIApp.Pages;
using DailyBudgetMAUIApp.Helpers;
using System.Reflection;


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

        public async Task<ErrorLog> HandleCatchedException(Exception ex, string Page, string Method)
        {
            try
            {
                ErrorLog NewLog = new ErrorLog(ex, Page, Method);


                ErrorLog Response = await _ds.CreateNewErrorLog(NewLog);

                return Response;
            }
            catch (Exception EndExcption)
            {
                Debug.WriteLine($"Error Trying to Log the Error --> {EndExcption.Message}");
                //TODO: Write the error to a physical file

                throw new Exception("Fatal Error Trying to Log an Error");
            }



        }

        public DateTime GetBudgetLastUpdated(int BudgetID)
        {
            DateTime LastUpdated = _ds.GetBudgetLastUpdatedAsync(BudgetID).Result;

            return LastUpdated;
        }

        public void ShowPopup(PopUpPage popup)
        {
            Page page = Application.Current.MainPage ?? throw new NullReferenceException();
            page.ShowPopup(popup);
        }

        public double FormatCurrencyNumber(string input)
        {
            input = input.Replace(App.CurrentSettings.CurrencySymbol, "").Replace(App.CurrentSettings.CurrencyGroupSeparator, "").Replace(App.CurrentSettings.CurrencyDecimalSeparator, "");
            input = input.Trim();

            //TODO: GET THE NUMBER OF DIGITS - CHECK THAT IT IS GREATER THAN 2
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
            status = status == "OK" ? UpdateBudgetCreateBillsSpend(ref Budget) : status;

            Budget.LastUpdated = DateTime.UtcNow;
            int DaysToPayDay = (int)Math.Ceiling((Budget.NextIncomePayday.GetValueOrDefault().Date - GetBudgetLocalTime(DateTime.UtcNow).Date).TotalDays);
            if(DaysToPayDay == 0)
            {
                DaysToPayDay = 1;
            }

            //If Budget is Borrow Pay add Next Pay day value to MaB and Lts
            if(Budget.IsBorrowPay)
            {
                Budget.MoneyAvailableBalance += Budget.PaydayAmount;
                Budget.LeftToSpendBalance += Budget.PaydayAmount;
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
                            DateTime NextIncomeDate = CalculateNextDate(Income.DateOfIncomeEvent, Income.RecurringIncomeType, Income.RecurringIncomeValue ?? 1, Income.RecurringIncomeDuration);
                            //Next Income Date happens in this Pay window so process
                            if (Income.DateOfIncomeEvent.Date < NextPayDay.Date)
                            {
                                Income.IncomeActiveDate = DateTime.UtcNow;
                                Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance + Income.IncomeAmount;
                                Budget.LeftToSpendBalance = Budget.LeftToSpendBalance + Income.IncomeAmount;
                                Budget.CurrentActiveIncome += Income.IncomeAmount;
                                while (NextIncomeDate.Date < NextPayDay.Date)
                                {
                                    Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance + Income.IncomeAmount;
                                    Budget.LeftToSpendBalance = Budget.LeftToSpendBalance + Income.IncomeAmount;
                                    Budget.CurrentActiveIncome += Income.IncomeAmount;
                                    //TODO: Add a Transaction into transactions
                                    NextIncomeDate = CalculateNextDate(NextIncomeDate, Income.RecurringIncomeType, Income.RecurringIncomeValue ?? 1, Income.RecurringIncomeDuration);
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
                }

                PeriodTotalSavingOutgoing += Saving.CurrentBalance ?? 0;
            }

            Budget.DailySavingOutgoing = DailySavingOutgoing;
            Budget.LeftToSpendBalance = Budget.LeftToSpendBalance - PeriodTotalSavingOutgoing;

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
                        if (DaysToBill < DaysToPayDay)
                        {
                            PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToBill;
                        }
                    }

                    PeriodTotalBillOutgoing += Bill.BillBalanceAtLastPayDay;

                }
            }

            Budget.DailyBillOutgoing = DailyBillOutgoing;
            Budget.LeftToSpendBalance = Budget.LeftToSpendBalance - PeriodTotalBillOutgoing;
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
                }
                else if (Duration == "weeks")
                {
                    IntDuration = 7;
                }
                else if (Duration == "years")
                {
                    IntDuration = 365;
                }
                else if (Duration == "months")
                {
                    IntDuration = 30;
                }
                else
                {
                    return "Duration not valid or null";
                }

                int DaysBetween = IntDuration * Value;

                NextDate = LastDate.AddDays(DaysBetween);
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

                int NextYear = new int();
                int NextMonth = new int();

                if (month != 12)
                {
                    NextYear = LastDate.Year;
                    NextMonth = month + 1;
                }
                else
                {
                    NextYear = year + 1;
                    NextMonth = 1;
                }

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
                NextDate = LastDate.AddMonths(1);
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

                int year = LastDate.Year;
                int month = LastDate.Month;

                int NextYear = new int();
                int NextMonth = new int();

                if (month != 12)
                {
                    NextYear = LastDate.Year;
                    NextMonth = month + 1;
                }
                else
                {
                    NextYear = year + 1;
                    NextMonth = 1;
                }

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

            PayDayTransaction = _ds.SaveNewTransaction(PayDayTransaction, budget.BudgetID).Result;

            budget.BankBalance += budget.PaydayAmount;
            budget.MoneyAvailableBalance += budget.PaydayAmount;
            budget.LeftToSpendBalance += budget.PaydayAmount;
        }

        private void CloseSaving(ref Savings Saving)
        {
            Saving.CurrentBalance = Saving.SavingsGoal;
            Saving.GoalDate = null;
            Saving.RegularSavingValue = null;
            Saving.PeriodSavingValue = null;
            Saving.IsSavingsClosed = true;
        }

        private void TransactBill(ref Bills Bill, int BudgetID)
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

            BillTransaction = _ds.SaveNewTransaction(BillTransaction, BudgetID).Result;

            if (BillTransaction.TransactionID != 0)
            {
                Bill.BillCurrentBalance = 0;
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

        private void TransactIncomeEvent(ref IncomeEvents Income, int BudgetID)
        {
            Transactions IncomeTransaction = new Transactions();
            IncomeTransaction.TransactionAmount = Income.IncomeAmount;
            IncomeTransaction.EventType = "IncomeEvent";
            IncomeTransaction.TransactionDate = Income.DateOfIncomeEvent;
            IncomeTransaction.Notes = $"Transaction added for Income Event, {Income.IncomeName}";
            IncomeTransaction.IsTransacted = true;
            IncomeTransaction.IsIncome = true;

            IncomeTransaction = _ds.SaveNewTransaction(IncomeTransaction, BudgetID).Result;

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

                if(!(Bill.BillCurrentBalance >= Bill.BillAmount))
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
                        var popup = new PopupDailyTransaction(Transaction, new PopupDailyTransactionViewModel(), new ProductTools(new RestDataService()));
                        var result = await Application.Current.MainPage.ShowPopupAsync(popup);
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

            if (budget.NextIncomePayday.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date)
            {
                //Confirm pay amount and date!
                var popup = new PopupDailyPayDay(budget, new PopupDailyPayDayViewModel(), new ProductTools(new RestDataService()));
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

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
                            Stats.SavingsToDate += (Saving.PeriodSavingValue.GetValueOrDefault() - Saving.CurrentBalance.GetValueOrDefault());

                            Saving.CurrentBalance = Saving.PeriodSavingValue;
                            Saving.LastUpdatedValue = Saving.PeriodSavingValue;
                            Saving.GoalDate = budget.NextIncomePayday;

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
                                Stats.SavingsToDate += (Saving.PeriodSavingValue.GetValueOrDefault() - Saving.CurrentBalance.GetValueOrDefault());

                                Saving.CurrentBalance = Saving.PeriodSavingValue;
                                Saving.LastUpdatedValue = Saving.PeriodSavingValue;
                                Saving.GoalDate = budget.NextIncomePayday;

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
                    if (Saving.GoalDate.GetValueOrDefault().Date == budget.BudgetValuesLastUpdated.Date)
                    {
                        if (Saving.IsAutoComplete)
                        {
                            CloseSaving(ref Saving);
                            budget.Savings[i] = Saving;
                        }
                        else
                        {
                            var popup = new PopupDailySaving(Saving, new PopupDailySavingViewModel(), new ProductTools(new RestDataService()));
                            var result = await Application.Current.MainPage.ShowPopupAsync(popup);

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

                if (Bill.BillDueDate.GetValueOrDefault().Date == budget.BudgetValuesLastUpdated.Date)
                {
                    var popup = new PopupDailyBill(Bill, new PopupDailyBillViewModel(), new ProductTools(new RestDataService()));
                    var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                    if ((string)result.ToString() == "OK")
                    {
                        TransactBill(ref Bill, budget.BudgetID);
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
                            TransactBill(ref Bill, budget.BudgetID);
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

                if (Income.DateOfIncomeEvent.Date == budget.BudgetValuesLastUpdated.Date)
                {
                    var popup = new PopupDailyIncome(Income, new PopupDailyIncomeViewModel(), new ProductTools(new RestDataService()));
                    var result = await Application.Current.MainPage.ShowPopupAsync(popup);
                    if ((string)result.ToString() == "OK")
                    {
                        TransactIncomeEvent(ref Income, budget.BudgetID);
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
                            TransactIncomeEvent(ref Income, budget.BudgetID);
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
                    if (Transaction.TransactionDate.GetValueOrDefault().Date <= budget.BudgetValuesLastUpdated.Date)
                    {
                        if (Transaction.TransactionDate.GetValueOrDefault().Date < GetBudgetLocalTime(DateTime.UtcNow).Date)
                        {
                            var popup = new PopupDailyTransaction(Transaction, new PopupDailyTransactionViewModel(), new ProductTools(new RestDataService()));
                            var result = await Application.Current.MainPage.ShowPopupAsync(popup);
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
                Budget.LeftToSpendBalance -= T.TransactionAmount;
                Budget.LeftToSpendDailyAmount -= T.TransactionAmount ?? 0;
                Budget.PayPeriodStats[0].SpendToDate += T.TransactionAmount ?? 0;
                Budget.LastUpdated = DateTime.UtcNow;
            }

            T.IsTransacted = true;

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
                        Budget.LastUpdated = DateTime.UtcNow;
                        S.CurrentBalance -= T.TransactionAmount;
                        S.LastUpdatedValue = T.TransactionAmount;
                        S.LastUpdatedDate = DateTime.UtcNow;
                        Budget.PayPeriodStats[0].SpendToDate += T.TransactionAmount ?? 0;
                    }
                }

                T.IsTransacted = true;

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

                    var popup = new PopUpOTP(ShareBudgetRequestID, new PopUpOTPViewModel(new RestDataService()), "ShareBudget", new ProductTools(new RestDataService()), new RestDataService());
                    var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                    if ((string)result.ToString() != "User Closed")
                    {
                        ShareBudgetRequest BudgetRequest = (ShareBudgetRequest)result;

                        bool DefaultBudgetYesNo = await Application.Current.MainPage.DisplayAlert($"Update Default Budget ", $"CONGRATS!! You have shared a budget with {BudgetRequest.SharedByUserEmail}, do you want to make this budget your default Budget?", "Yes, continue", "No Thanks!");

                        if (DefaultBudgetYesNo)
                        {
                            await ChangeDefaultBudget(App.UserDetails.UserID, BudgetRequest.SharedBudgetID, true);                      
                        }
                    }

                    break;
                case "BudgetShared":
                    Preferences.Remove("NavigationType");

                    break;
                default:
                    break;     
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

            picker.ItemDisplayBinding = new Binding(".", BindingMode.Default, new ChangeBudgetStringConvertor());

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

            List<PatchDoc> UpdateUserDetails = new List<PatchDoc>();

            PatchDoc DefaultBudgetID = new PatchDoc
            {
                op = "replace",
                path = "/DefaultBudgetID",
                value = BudgetID
            };

            UpdateUserDetails.Add(DefaultBudgetID);
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
            
            if(navigate && App.CurrentPopUp == null)
            {
                var PopUp = new PopUpPage();
                App.CurrentPopUp = PopUp;
                Application.Current.MainPage.ShowPopup(PopUp);

                await Shell.Current.GoToAsync($"///{nameof(LandingPage)}");
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
            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Dashboard",
                Route = "MainPage",
                Icon = "dashboard.svg",
                ContentTemplate = new DataTemplate(() => new MainPage(new MainPageViewModel(new RestDataService(), new ProductTools(new RestDataService())), new RestDataService(), new ProductTools(new RestDataService())))

            });

            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Outgoings",
                Route = "AddBill",
                Icon = "bill.svg",
                ContentTemplate = new DataTemplate(() => new AddBill(new AddBillViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Transaction",
                Route = "AddTransactions",
                Icon = "transaction.svg",
                ContentTemplate = new DataTemplate(() => new AddTransaction(new AddTransactionViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Income",
                Route = "AddIncome",
                Icon = "income.svg",
                ContentTemplate = new DataTemplate(() => new AddIncome(new AddIncomeViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Saving",
                Route = "AddSaving",
                Icon = "saving.svg",
                ContentTemplate = new DataTemplate(() => new AddSaving(new AddSavingViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Transaction",
                Route = "ViewTransactions",
                Icon = "transaction.svg",
                ContentTemplate = new DataTemplate(() => new ViewTransactions(new ViewTransactionsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new RestDataService(), new ProductTools(new RestDataService())))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Savings",
                Route = "ViewSavings",
                Icon = "saving.svg",
                ContentTemplate = new DataTemplate(() => new ViewSavings(new ViewSavingsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Outgoings",
                Route = "ViewBills",
                Icon = "bill.svg",
                ContentTemplate = new DataTemplate(() => new ViewBills(new ViewBillsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Envelopes",
                Route = "ViewEnvelopes",
                Icon = "envelope.svg",
                ContentTemplate = new DataTemplate(() => new ViewEnvelopes(new ViewEnvelopesViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Incomes",
                Route = "ViewIncomes",
                Icon = "income.svg",
                ContentTemplate = new DataTemplate(() => new ViewIncomes(new ViewIncomesViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });
        }

        private async Task LoadPremiumPlusTabBar()
        {
            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Dashboard",
                Route = "MainPage",
                Icon = "dashboard.svg",
                ContentTemplate = new DataTemplate(() => new MainPage(new MainPageViewModel(new RestDataService(), new ProductTools(new RestDataService())), new RestDataService(), new ProductTools(new RestDataService())))

            });

            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Outgoings",
                Route = "AddBill",
                Icon = "bill.svg",
                ContentTemplate = new DataTemplate(() => new AddBill(new AddBillViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Transaction",
                Route = "AddTransactions",
                Icon = "transaction.svg",
                ContentTemplate = new DataTemplate(() => new AddTransaction(new AddTransactionViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Income",
                Route = "AddIncome",
                Icon = "income.svg",
                ContentTemplate = new DataTemplate(() => new AddIncome(new AddIncomeViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Saving",
                Route = "AddSaving",
                Icon = "saving.svg",
                ContentTemplate = new DataTemplate(() => new AddSaving(new AddSavingViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Transaction",
                Route = "ViewTransactions",
                Icon = "transaction.svg",
                ContentTemplate = new DataTemplate(() => new ViewTransactions(new ViewTransactionsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new RestDataService(), new ProductTools(new RestDataService())))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Savings",
                Route = "ViewSavings",
                Icon = "saving.svg",
                ContentTemplate = new DataTemplate(() => new ViewSavings(new ViewSavingsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Outgoings",
                Route = "ViewBills",
                Icon = "bill.svg",
                ContentTemplate = new DataTemplate(() => new ViewBills(new ViewBillsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Envelopes",
                Route = "ViewEnvelopes",
                Icon = "envelope.svg",
                ContentTemplate = new DataTemplate(() => new ViewEnvelopes(new ViewEnvelopesViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Incomes",
                Route = "ViewIncomes",
                Icon = "income.svg",
                ContentTemplate = new DataTemplate(() => new ViewIncomes(new ViewIncomesViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });
        }

        private async Task LoadBasicTabBar()
        {
            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Dashboard",
                Route = "MainPage",
                Icon = "dashboard.svg",
                ContentTemplate = new DataTemplate(() => new MainPage(new MainPageViewModel(new RestDataService(), new ProductTools(new RestDataService())), new RestDataService(), new ProductTools(new RestDataService())))

            });

            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Outgoings",
                Route = "AddBill",
                Icon = "bill.svg",
                ContentTemplate = new DataTemplate(() => new AddBill(new AddBillViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContent()
            {
                Title = "Transaction",
                Route = "AddTransactions",
                Icon = "transaction.svg",
                ContentTemplate = new DataTemplate(() => new AddTransaction(new AddTransactionViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Income",
                Route = "AddIncome",
                Icon = "income.svg",
                ContentTemplate = new DataTemplate(() => new AddIncome(new AddIncomeViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.MainTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Saving",
                Route = "AddSaving",
                Icon = "saving.svg",
                ContentTemplate = new DataTemplate(() => new AddSaving(new AddSavingViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Transaction",
                Route = "ViewTransactions",
                Icon = "transaction.svg",
                ContentTemplate = new DataTemplate(() => new ViewTransactions(new ViewTransactionsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new RestDataService(), new ProductTools(new RestDataService())))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Savings",
                Route = "ViewSavings",
                Icon = "saving.svg",
                ContentTemplate = new DataTemplate(() => new ViewSavings(new ViewSavingsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Outgoings",
                Route = "ViewBills",
                Icon = "bill.svg",
                ContentTemplate = new DataTemplate(() => new ViewBills(new ViewBillsViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Envelopes",
                Route = "ViewEnvelopes",
                Icon = "envelope.svg",
                ContentTemplate = new DataTemplate(() => new ViewEnvelopes(new ViewEnvelopesViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

            App.ViewTabBar.Items.Add(new ShellContentDI()
            {
                Title = "Incomes",
                Route = "ViewIncomes",
                Icon = "income.svg",
                ContentTemplate = new DataTemplate(() => new ViewIncomes(new ViewIncomesViewModel(new ProductTools(new RestDataService()), new RestDataService()), new ProductTools(new RestDataService()), new RestDataService()))
            });

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
    }
}
