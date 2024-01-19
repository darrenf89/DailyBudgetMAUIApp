using DailyBudgetMAUIApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyBudgetMAUIApp.Converters
{
    public class IsSpendFromSavingText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if ((bool)value)
            {
                return "";
            }
            else
            {
                return "No";
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        
    }

    public class IsFutureDatedTransactionText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if ((bool)value)
            {
                return "";
            }
            else
            {
                return "Transact Now";
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class IsPayeeText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if ((bool)value)
            {
                return "";
            }
            else
            {
                return "No Payee";
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class IsSpendCategoryText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if ((bool)value)
            {
                return "";
            }
            else
            {
                return "No Category";
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsNoteText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if ((bool)value)
            {
                return "";
            }
            else
            {
                return "No Note";
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class IsBudgetNotSharedBudgetToBool : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Budgets budget = (Budgets)value;

            if (budget.IsSharedValidated && budget.SharedUserID != 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class IsBudgetSharedBudgetToBool : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Budgets budget = (Budgets)value;

            if (budget.IsSharedValidated && budget.SharedUserID != 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class IsBudgetShareRequestToBool : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Budgets budget = (Budgets)value;

            if (budget.AccountInfo.BudgetShareRequestID != 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class DecimalToCurrencyString : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            decimal Amount = (decimal)value;

            string AmountString = Amount.ToString("c", CultureInfo.CurrentCulture);

            return AmountString;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class BoolToColor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            Application.Current.Resources.TryGetValue("Success", out var Success);
            Application.Current.Resources.TryGetValue("Danger", out var Danger);

            if (value == null) return (Color)Success;

            bool IsIncome = (bool)value;

            Color OutputColor;
            if (IsIncome)
            {
                OutputColor = (Color)Success;
            }
            else
            {
                OutputColor = (Color)Danger;
            }

            return OutputColor;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class GreaterLessZeroColor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            decimal Amount = (decimal)value;

            Application.Current.Resources.TryGetValue("Success", out var Success);
            Application.Current.Resources.TryGetValue("Danger", out var Danger);

            Color OutputColor;
            if(Amount > 0)
            {
                OutputColor = (Color)Success;
            }
            else
            {
                OutputColor = (Color)Danger;
            }

            return OutputColor;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
    public class GreaterLessZeroColorLight : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            decimal Amount = (decimal)value;

            Application.Current.Resources.TryGetValue("Success", out var Success);
            Application.Current.Resources.TryGetValue("Danger", out var Danger);

            Color OutputColor;
            if (Amount > 0)
            {
                OutputColor = (Color)Success;

            }
            else
            {
                OutputColor = (Color)Danger;
            }

            return OutputColor;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class LeftToSpendDailyAmountHideShow : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Budgets budget = (Budgets)value;

            if (budget.LeftToSpendDailyAmount <= 0 && budget.LeftToSpendBalance > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class LeftToSpendBalanceHideShow : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Budgets budget = (Budgets)value;

            if (budget.LeftToSpendBalance <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class ChangeBudgetStringConvertor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Budgets budget = (Budgets)value;

            string result = "";

            if (App.UserDetails.UserID == budget.SharedUserID)
            {
                result = $"[Shared]{budget.BudgetName} ({budget.LastUpdated.ToString("dd MMM yy")})";                
            }
            else
            {
                result = $"{budget.BudgetName} ({budget.LastUpdated.ToString("dd MMM yy")})";
            }

            return result;

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class SavingProgressBarMax : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Savings saving = (Savings)value;

            if(saving.SavingsType == "SavingsBuilder")
            {
                return saving.CurrentBalance;
            }
            else
            {

                return saving.SavingsGoal;                
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class SavingProgressBarMaxString : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Savings saving = (Savings)value;

            if (saving.SavingsType == "SavingsBuilder")
            {
                return saving.CurrentBalance.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
            }
            else
            {
                return saving.SavingsGoal.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);                
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class SavingTypeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string SavingType = (string)value;

            if (SavingType == "SavingsBuilder")
            {
                return "Building savings every day";
            }
            else if(SavingType == "TargetAmount")
            {
                return "Saving set amount every day";
            }
            else if(SavingType == "TargetDate")
            {
                return "Saving for a target date";
            }
            else
            {
                return "Unknown";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class RegularSavingValueString : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Savings saving = (Savings)value;

            if (saving.IsSavingsClosed)
            {
                return "Saving Closed";
            }
            else
            {
                return saving.RegularSavingValue.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class SavingGoalDateString : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Savings saving = (Savings)value;

            if (saving.IsSavingsClosed)
            {
                return "Saving Closed";
            }
            else
            {
                if(saving.SavingsType == "SavingsBuilder")
                {
                    return "Continuous Saving";
                }
                else
                {
                    return saving.GoalDate.GetValueOrDefault().ToString("dd MMM yy");
                }
                
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class BillTypeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            bool IsRecurring = (bool)value;

            if (IsRecurring)
            {
                return "Recurring Outgoing";
            }
            else
            {
                return "One-off Outgoing";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class BillDueDate : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            DateTime Date = (DateTime)value;

            return Date.ToString("dd MMM yy");

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class RecurringBillDetails : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Bills Bill = (Bills)value;

            if (Bill.IsRecuring)
            {
                if(Bill.BillType == "Everynth")
                {                    
                    return Bill.BillValue == 1 ? $"Every {Bill.BillDuration.Replace("s","")}" : $"Every {Bill.BillValue} {Bill.BillDuration}";
                }
                else if(Bill.BillType == "OfEveryMonth")
                {
                    string DayString;
                    if (Bill.BillValue == 1)
                    {
                        DayString = $"{Bill.BillValue}st";
                    }
                    else if (Bill.BillValue == 2)
                    {
                        DayString = $"{Bill.BillValue}nd";
                    }
                    else if (Bill.BillValue == 3)
                    {
                        DayString = $"{Bill.BillValue}rd";
                    }
                    else
                    {
                        DayString = $"{Bill.BillValue}th";
                    }

                    return $"{DayString} of the month";
                }
                else
                {
                    return "";
                }

            }
            else
            {
                return "";
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class IncomeTypeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            IncomeEvents Income = (IncomeEvents)value;

            string ReturnValue;

            if (Income.IsRecurringIncome)
            {
                ReturnValue = "Recurring ";
            }
            else
            {
                ReturnValue = "One-off ";
            }

            if (Income.IsInstantActive ?? false)
            {
                ReturnValue = ReturnValue + "Instant Active Extra Income";
            }
            else
            {
                ReturnValue = ReturnValue + "On Received Extra Income";
            }

            return ReturnValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class DateToNumberOfDays : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            DateTime Date = (DateTime)value;

            string ReturnValue = $"{(int)Math.Ceiling((Date.Date - DateTime.Today.Date).TotalDays)} Days";

            return ReturnValue;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class RecurringIncomeDetails : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            IncomeEvents Income = (IncomeEvents)value;

            if (Income.IsRecurringIncome)
            {
                if (Income.RecurringIncomeType == "Everynth")
                {
                    return Income.RecurringIncomeValue == 1 ? $"Every {Income.RecurringIncomeDuration.Replace("s", "")}" : $"Every {Income.RecurringIncomeValue} {Income.RecurringIncomeDuration}";
                }
                else if (Income.RecurringIncomeType == "OfEveryMonth")
                {
                    string DayString;
                    if (Income.RecurringIncomeValue == 1)
                    {
                        DayString = $"{Income.RecurringIncomeValue}st";
                    }
                    else if (Income.RecurringIncomeValue == 2)
                    {
                        DayString = $"{Income.RecurringIncomeValue}nd";
                    }
                    else if (Income.RecurringIncomeValue == 3)
                    {
                        DayString = $"{Income.RecurringIncomeValue}rd";
                    }
                    else
                    {
                        DayString = $"{Income.RecurringIncomeValue}th";
                    }

                    return $"{DayString} of the month";
                }
                else
                {
                    return "";
                }

            }
            else
            {
                return "";
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class EventTypeToGlyph : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "\uf17e";

            string EventType = (string)value;

            string ReturnGlyph = "\uf17e";

            if (EventType == "IncomeEvent")
            {
                ReturnGlyph = "\ue8e5"; 
            }
            else if (EventType == "Bill")
            {
                ReturnGlyph = "\uef6e";
            }
            else if (EventType == "PayDay")
            {
                ReturnGlyph = "\uef63";
            }
            else if (EventType == "Envelope")
            {
                ReturnGlyph = "\ue158";
            }
            else if (EventType == "Saving")
            {
                ReturnGlyph = "\ue2eb";
            }

            return ReturnGlyph;

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class EventTypeToString : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "\uf17e";

            string EventType = (string)value;

            if (EventType == "IncomeEvent")
            {
                return "Got an income";
            }
            else if (EventType == "Bill")
            {
                return "Paid for a bill";
            }
            else if (EventType == "PayDay")
            {
                return "Pay Day!";
            }
            else if (EventType == "Envelope")
            {
                return "Took from an envelope";
            }
            else if (EventType == "Saving")
            {
                return "Spent some saving";
            }
            else
            {
                return "Made a transaction";
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class TransactionAmountToCurrencyString : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Transactions T = (Transactions)value;
            string TString = "";

            if(T._isIncome)
            {
                TString = "+ ";
            }
            else
            {
                TString = "- ";
            }

            TString += T.TransactionAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);

            return TString;
            
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class RunningTotalToCurrencyString : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Transactions T = (Transactions)value;


            if(!T.IsTransacted)
            {
                return "Pending";
            }
            else
            {
                return T.RunningTotal.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
            }
            
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class TransactionTypePngConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string EventType = (string)value;

            string ReturnGlyph = "transaction.svg";

            if (EventType == "IncomeEvent")
            {
                ReturnGlyph = "income.svg";
            }
            else if (EventType == "Bill")
            {
                ReturnGlyph = "bill.svg";
            }
            else if (EventType == "PayDay")
            {
                ReturnGlyph = "pay.svg";
            }
            else if (EventType == "Envelope")
            {
                ReturnGlyph = "envelope.svg";
            }
            else if (EventType == "Saving")
            {
                ReturnGlyph = "saving.svg";
            }

            return ReturnGlyph;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class TransactionDisplayName : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            Transactions T = (Transactions)value;

            if (!string.IsNullOrEmpty(T.Payee))
            {
                return T.Payee;
            }
            else
            {
                if (T.EventType == "IncomeEvent")
                {
                    return "Got an income";
                }
                else if (T.EventType == "Bill")
                {
                    return "Paid for a bill";
                }
                else if (T.EventType == "PayDay")
                {
                    return "Pay Day!";
                }
                else if (T.EventType == "Envelope")
                {
                    return "Took from an envelope";
                }
                else if (T.EventType == "Saving")
                {
                    return "Spent some saving";
                }
                else
                {
                    return "Made a transaction";
                }
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class BoolToColorDisabled : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

            if (value == null) return null;

            bool IsTransacted = (bool)value;

            if (IsTransacted)
            {
                return Color.FromArgb("#FFFFFFFF");
            }
            else
            {
                return Color.FromArgb("#F2E1E1E1");;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
    public class IsSpendCategoryStringText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)value))
            {
                return "Taken from bank";
            }
            else
            {
                return (string)value;
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsCategoryStringText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (string.IsNullOrEmpty((string)value))
            {
                return "No category selected";
            }
            else
            {
                return (string)value;
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

    public class IsNoteStringText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (string.IsNullOrEmpty((string)value))
            {
                return "No note entered";
            }
            else
            {
                return (string)value;
            }

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
