using DailyBudgetMAUIApp.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using System.Globalization;

namespace DailyBudgetMAUIApp.DataServices
{
    internal class ProductTools : IProductTools
    {
        private readonly IRestDataService _ds;

        public ProductTools(IRestDataService ds)
        {
            _ds = ds;
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
            input = input.Replace(App.CurrentSettings.CurrencySymbol,"").Replace(App.CurrentSettings.CurrencyGroupSeparator,"").Replace(App.CurrentSettings.CurrencyDecimalSeparator, "");
            input = input.Trim();

            //TODO: GET THE NUMBER OF DIGITS - CHECK THAT IT IS GREATER THAN 2
            try
            {
                double Number = Convert.ToDouble(input);
                Number = Number / 100;
                
                return Number;
            }
            catch (Exception ex)
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
            catch (Exception ex)
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

            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            int NextYear = new int();
            int NextMonth = new int();

            if (month != 12)
            {
                NextYear = DateTime.Now.Year;
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

            int DaysBetweenPay = (NextDate.Date - CurrentDate.Date).Days;

            return DaysBetweenPay;
        }

        public int GetNumberOfDaysLastDayOfWeek(int dayNumber)
        {

            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            int NextYear = new int();
            int NextMonth = new int();

            if (month != 12)
            {
                NextYear = DateTime.Now.Year;
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

            int DaysBetweenPay = (NextDate.Date - CurrentDate.Date).Days;

            return DaysBetweenPay;
        }

        public void UpdateBudget(ref Budgets Budget)
        {
            UpdateBudgetRecalculateSavings(ref Budget);
            UpdateBudgetIncomes(ref Budget);
            UpdateBudgetSavings(ref Budget);
            UpdateBudgetBills(ref Budget);
        }

        public void UpdateBudgetRecalculateSavings(ref Budgets Budget)
        {
            int? DaysBetweenPay = Budget.AproxDaysBetweenPay;

            foreach (Savings Saving in Budget.Savings)
            {
                if (Saving.isRegularSaving)
                {
                    if (!Saving.isDailySaving)
                    {
                        if (Saving.SavingsType == "TargetAmount")
                        {
                            //Recalculate Date and daily amount
                            Saving.RegularSavingValue = Saving.PeriodSavingValue / DaysBetweenPay;

                            decimal? BalanceLeft = Saving.SavingsGoal - (Saving.CurrentBalance ?? 0);
                            int NumberOfDays = (int)Math.Ceiling(BalanceLeft / Saving.RegularSavingValue ?? 0);

                            DateTime Today = DateTime.UtcNow;
                            Saving.GoalDate = Today.AddDays(NumberOfDays);

                        }
                        else if (Saving.SavingsType == "SavingsBuilder")
                        {
                            //Recalculate daily amount.
                            Saving.RegularSavingValue = Saving.PeriodSavingValue / DaysBetweenPay;
                        }
                    }
                }
            }
        }

        public void UpdateBudgetIncomes(ref Budgets Budget)
        {
            DateTime Today = DateTime.Now;

            foreach (IncomeEvents Income in Budget.IncomeEvents)
            {
                DateTime NextPayDay = Budget.NextIncomePayday ?? default;
                if (Income.isInstantActive ?? false)
                {
                    DateTime PayDayAfterNext = CalculateNextDate(NextPayDay, Budget.PaydayType, Budget.PaydayValue ?? 1, Budget.PaydayDuration);
                    DateTime NextIncomeDate = CalculateNextDate(Income.DateOfIncomeEvent, Income.RecurringIncomeType, Income.RecurringIncomeValue ?? 1, Income.RecurringIncomeDuration);
                    //Next Income Date happens in this Pay window so process
                    if(Income.DateOfIncomeEvent.Date < NextPayDay.Date)
                    {
                        Income.IncomeActiveDate = DateTime.UtcNow;
                        Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance + Income.IncomeAmount;
                        Budget.LeftToSpendBalance = Budget.LeftToSpendBalance + Income.IncomeAmount;
                        while (NextIncomeDate.Date < NextPayDay.Date)
                        {
                            Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance + Income.IncomeAmount;
                            Budget.LeftToSpendBalance = Budget.LeftToSpendBalance + Income.IncomeAmount;
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

                    if(Income.DateOfIncomeEvent <= Today.Date)
                    {
                        Budget.BankBalance = Budget.BankBalance + Income.IncomeAmount;
                        //TODO: Update Instant Active Income Transaction in transactions
                        Income.DateOfIncomeEvent = NextIncomeDate.Date;
                        if (Income.isRecurringIncome)
                        {
                            DateTime CalPayDate = new DateTime();
                            while (NextIncomeDate.Date > NextPayDay.Date)
                            {
                                CalPayDate = NextPayDay;
                                NextPayDay = CalculateNextDate(NextPayDay, Budget.PaydayType, Budget.PaydayValue ?? 1, Budget.PaydayDuration);
                            }
                            Income.IncomeActiveDate = CalPayDate.Date;
                        }
                        else
                        {
                            Income.isClosed = true;
                            Income.isIncomeAddedToBalance = true;
                        }
                    }
                }                    
                else
                {
                    if(Income.DateOfIncomeEvent.Date <= Today.Date)
                    {
                        Budget.BankBalance = Budget.BankBalance + Income.IncomeAmount;
                        Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance + Income.IncomeAmount;
                        Budget.LeftToSpendBalance = Budget.LeftToSpendBalance + Income.IncomeAmount;
                        //TODO: Add a Transaction into transactions
                        if (Income.isRecurringIncome)
                        {
                            //Calculate the next DateOfIncomeEvent and set IncomeActiveDate To this as well!
                            DateTime NextDate = CalculateNextDate(Income.DateOfIncomeEvent, Income.RecurringIncomeType, Income.RecurringIncomeValue ?? 1, Income.RecurringIncomeDuration);
                            Income.DateOfIncomeEvent = NextDate;
                            Income.IncomeActiveDate = NextDate;
                        }
                        else
                        {
                            Income.isClosed = true;
                            Income.isIncomeAddedToBalance = true;
                        }
                    }
                }
            }
        }

        public void UpdateBudgetSavings(ref Budgets Budget)
        {
            decimal DailySavingOutgoing = new();
            decimal PeriodTotalSavingOutgoing = new();

            int DaysToPayDay = (Budget.NextIncomePayday.GetValueOrDefault().Date - DateTime.Today.Date).Days;

            foreach (Savings Saving in Budget.Savings)
            {
                if (Saving.isRegularSaving & Saving.SavingsType == "SavingsBuilder")
                {
                    DailySavingOutgoing += Saving.RegularSavingValue ?? 0;
                    PeriodTotalSavingOutgoing += ((Saving.RegularSavingValue ?? 0) * DaysToPayDay);
                }
                else if (Saving.isRegularSaving)
                {
                    DailySavingOutgoing += Saving.RegularSavingValue ?? 0;
                    //check if goal date is before pay day
                    int DaysToSaving = (Saving.GoalDate.GetValueOrDefault().Date - DateTime.Today.Date).Days;
                    if (DaysToSaving < DaysToPayDay & !Saving.canExceedGoal)
                    {
                        PeriodTotalSavingOutgoing += ((Saving.RegularSavingValue ?? 0) * DaysToSaving);
                    }
                    else
                    {
                        PeriodTotalSavingOutgoing += ((Saving.RegularSavingValue ?? 0) * DaysToPayDay);
                    }

                }

                PeriodTotalSavingOutgoing += Saving.CurrentBalance ?? 0;
            }

            Budget.DailySavingOutgoing = DailySavingOutgoing;
            Budget.LeftToSpendBalance = Budget.LeftToSpendBalance - PeriodTotalSavingOutgoing;
        }

        public void UpdateBudgetBills(ref Budgets Budget)
        {
            decimal DailyBillOutgoing = new();
            decimal PeriodTotalBillOutgoing = new();

            int DaysToPayDay = (Budget.NextIncomePayday.GetValueOrDefault().Date - DateTime.Today.Date).Days;

            foreach (Bills Bill in Budget.Bills)
            {
                DailyBillOutgoing += Bill.RegularBillValue ?? 0;
                //Check if Due Date is before Pay Dat
                int DaysToBill = (Bill.BillDueDate.GetValueOrDefault().Date - DateTime.Today.Date).Days;
                if (DaysToBill < DaysToPayDay)
                {
                    PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToBill;
                }
                else
                {
                    PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToPayDay;
                }

                PeriodTotalBillOutgoing += Bill.BillCurrentBalance;

            }

            Budget.DailyBillOutgoing = DailyBillOutgoing;
            Budget.LeftToSpendBalance = Budget.LeftToSpendBalance - PeriodTotalBillOutgoing;
            Budget.MoneyAvailableBalance = Budget.MoneyAvailableBalance - PeriodTotalBillOutgoing;
           
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
            CultureSetting.DateTimeFormat.DateSeparator = Settings.DateSeparator;

            Thread.CurrentThread.CurrentCulture = CultureSetting;
            Thread.CurrentThread.CurrentUICulture = CultureSetting;
            CultureInfo.DefaultThreadCurrentCulture = CultureSetting;
            CultureInfo.DefaultThreadCurrentUICulture = CultureSetting;
        }

    }
 
}
