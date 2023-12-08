using DailyBudgetMAUIApp.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using DailyBudgetMAUIApp.Handlers;
using CommunityToolkit.Maui.Views;
using System.Globalization;
using DailyBudgetMAUIApp.ViewModels;
using CommunityToolkit.Maui.ApplicationModel;

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

        public string UpdatePayPeriodStats(int? BudgetID)
        {
            //TODO: IMPLEMENT CALLING UpdatePayPeriodStats in API
            return "OK";
        }

        public string UpdateBudget(ref Budgets Budget)
        {
            string status = "OK";

            status = status == "OK" ? UpdateApproxDaysBetweenPay(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateSavings(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateBills(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateIncome(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateSavingsSpend(ref Budget) : status;
            status = status == "OK" ? UpdateBudgetCreateBillsSpend(ref Budget) : status;

            Budget.BudgetValuesLastUpdated = DateTime.UtcNow;
            int DaysToPayDay = (Budget.NextIncomePayday.GetValueOrDefault().Date - DateTime.Today.Date).Days;
            Budget.LeftToSpendDailyAmount = (Budget.LeftToSpendBalance ?? 0) / DaysToPayDay;
            Budget.StartDayDailyAmount = Budget.LeftToSpendDailyAmount;

            status = status == "OK" ? UpdatePayPeriodStats(Budget.BudgetID) : status;

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
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
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
                    decimal? BalanceLeft = Bill.BillAmount - Bill.BillCurrentBalance;

                    TimeSpan TimeToGoal = (Bill.BillDueDate - DateTime.Now).GetValueOrDefault();
                    int? DaysToGoal = TimeToGoal.Days;

                    Bill.RegularBillValue = BalanceLeft / DaysToGoal;    
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

                            DateTime Today = DateTime.UtcNow;
                            Saving.GoalDate = Today.AddDays(NumberOfDays);

                        }
                        else if (Saving.SavingsType == "TargetDate")
                        {
                            decimal? BalanceLeft = Saving.SavingsGoal - (Saving.CurrentBalance ?? 0);

                            TimeSpan TimeToGoal = (Saving.GoalDate - DateTime.Now).GetValueOrDefault();
                            int? DaysToGoal = TimeToGoal.Days;

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
                DateTime Today = DateTime.Now;

                foreach (IncomeEvents Income in Budget.IncomeEvents)
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

            int DaysToPayDay = (Budget.NextIncomePayday.GetValueOrDefault().Date - DateTime.Today.Date).Days;

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
                        int DaysToSaving = (Saving.GoalDate.GetValueOrDefault().Date - DateTime.Today.Date).Days;
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

            int DaysToPayDay = (Budget.NextIncomePayday.GetValueOrDefault().Date - DateTime.Today.Date).Days;

            foreach (Bills Bill in Budget.Bills)
            {
                DailyBillOutgoing += Bill.RegularBillValue ?? 0;
                //Check if Due Date is before Pay Dat
                int DaysToBill = (Bill.BillDueDate.GetValueOrDefault().Date - DateTime.Today.Date).Days;
                if (Bill.IsRecuring)
                {
                    if (DaysToBill < DaysToPayDay)
                    {
                        PeriodTotalBillOutgoing += (Bill.RegularBillValue ?? 0) * DaysToBill;

                        DateTime BillDueAfterNext = CalculateNextDate(Bill.BillDueDate.GetValueOrDefault(), Bill.BillType, Bill.BillValue.GetValueOrDefault(), Bill.BillDuration);
                        int NumberOfDaysBill = (BillDueAfterNext - Bill.BillDueDate.GetValueOrDefault()).Days;
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
            CultureSetting.DateTimeFormat.DateSeparator = Settings.DateSeparator;

            Thread.CurrentThread.CurrentCulture = CultureSetting;
            Thread.CurrentThread.CurrentUICulture = CultureSetting;
            CultureInfo.DefaultThreadCurrentCulture = CultureSetting;
            CultureInfo.DefaultThreadCurrentUICulture = CultureSetting;
        }

        private void CloseSaving(ref Savings Saving)
        {
            Saving.CurrentBalance = Saving.SavingsGoal;
            Saving.SavingsGoal = null;
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

            BillTransaction = _ds.SaveNewTransaction(BillTransaction, BudgetID).Result;

            if (BillTransaction.TransactionID != 0)
            {
                Bill.BillCurrentBalance = 0;
            }            

            if (Bill.IsRecuring)
            {

                Bill.BillDueDate = CalculateNextDate(Bill.BillDueDate.GetValueOrDefault(), Bill.BillType, Bill.BillValue.GetValueOrDefault(), Bill.BillDuration);
                TimeSpan Difference = (TimeSpan)(Bill.BillDueDate - DateTime.Now.AddDays(-1));
                int NumberOfDays = Difference.Days;
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

                Bill.LastUpdatedDate = DateTime.Now;
            }
            else
            {
                Bill.RegularBillValue = 0;
                Bill.IsClosed = true;
                Bill.LastUpdatedDate = DateTime.Now;
            }
        }

        private void TransactIncomeEvent(ref IncomeEvents Income)
        {
            
        }

        public async Task<Budgets> BudgetDailyCycle(Budgets budget)
        {
            while(budget.LastUpdated.Date < DateTime.UtcNow.Date)
            {
                budget.LastUpdated = budget.LastUpdated.AddDays(1);

                budget = await BudgetDailyEventsCheck(budget);

                //TODO: ADD REGULAR VALUES TO ALL SAVINGS AND BILLS

                string Status = UpdateBudget(ref budget);
                if(Status == "OK")
                {

                }

                //TODO:SAVE BUDGET
                

            }

            return budget;
        }

        private async Task<Budgets> BudgetDailyEventsCheck(Budgets budget)
        {            

            if (budget.NextIncomePayday.GetValueOrDefault().Date == budget.LastUpdated.Date)
            {
                //TODO: Confirm pay amount and date!
                var popup = new PopUpPage();
                var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                if((string)result == "OK")
                {
                    //TODO: Add next pay amount
                    //TODO: Update the next pay date
                    //TODO: Reset any envelope savings 
                }
            }

            for(int i = budget.Savings.Count - 1; i >= 0 ; i--) 
            {
                Savings Saving = budget.Savings[i];

                if (Saving.SavingsType == "TargetAmount" || Saving.SavingsType == "TargetDate")
                {
                    if (Saving.GoalDate == budget.LastUpdated.Date)
                    {
                        if (Saving.IsAutoComplete)
                        {
                            CloseSaving(ref Saving);
                            budget.Savings[i] = Saving;
                            await _ds.UpdateSaving(budget.Savings[i]);
                        }
                        else
                        {
                            var popup = new PopupDailySaving(Saving, new PopupDailySavingViewModel(), new ProductTools(new RestDataService()));
                            var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                            if ((string)result.ToString() == "OK")
                            {
                                CloseSaving(ref Saving);
                                budget.Savings[i] = Saving;
                                await _ds.UpdateSaving(budget.Savings[i]);
                            }
                            else if ((string)result.ToString() == "Delete")
                            {
                                await _ds.DeleteSaving(Saving.SavingID);
                                budget.Savings.RemoveAt(i);
                            }
                            else
                            {
                                Saving = (Savings)result;

                                if (Saving.GoalDate.GetValueOrDefault().Date <= budget.LastUpdated.Date)
                                {
                                    CloseSaving(ref Saving);                                    
                                }

                                budget.Savings[i] = Saving;
                                await _ds.UpdateSaving(budget.Savings[i]);
                            }
                        }
                    }
                }               
            }

            for (int i = budget.Bills.Count - 1; i >= 0 ; i--)
            {
                Bills Bill = budget.Bills[i];

                if (Bill.BillDueDate.GetValueOrDefault().Date == budget.LastUpdated.Date)
                {
                    var popup = new PopupDailyBill(Bill, new PopupDailyBillViewModel(), new ProductTools(new RestDataService()));
                    var result = await Application.Current.MainPage.ShowPopupAsync(popup);

                    if ((string)result.ToString() == "OK")
                    {
                        TransactBill(ref Bill, budget.BudgetID);
                        budget.BankBalance = budget.BankBalance - Bill.BillAmount;
                        budget.Bills[i] = Bill;
                        await _ds.UpdateBill(budget.Bills[i]);
                    }
                    else if ((string)result.ToString() == "Delete")
                    {
                        await _ds.DeleteBill(Bill.BillID);                        
                        budget.Bills.RemoveAt(i);
                    }
                    else
                    {
                        Bill = (Bills)result;

                        if (Bill.BillDueDate.GetValueOrDefault().Date <= budget.LastUpdated.Date)
                        {
                            TransactBill(ref Bill, budget.BudgetID);
                            budget.BankBalance = budget.BankBalance - Bill.BillAmount;
                        }
                        budget.Bills[i] = Bill;
                        await _ds.UpdateBill(budget.Bills[i]);
                    }
                }

            }

            for (int i = budget.IncomeEvents.Count - 1; i >=0 ; i--)
            {
                IncomeEvents Income = budget.IncomeEvents[i];
                if (Income.DateOfIncomeEvent.Date == budget.LastUpdated.Date)
                {

                }
                budget.IncomeEvents[i] = Income;
            }

            //TODO: PROCESS FUTURE TRANSACTIONS
            for (int i = budget.Transactions.Count - 1; i >= 0; i--)
            {
                Transactions Transaction = budget.Transactions[i];
                if (Transaction.TransactionDate.GetValueOrDefault().Date == budget.LastUpdated.Date)
                {

                }
                budget.Transactions[i] = Transaction;
            }

            return budget;
            
        }
    } 
}
