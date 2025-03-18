using DailyBudgetMAUIApp.Models;
using System.Globalization;
using OnScreenSizeMarkup.Maui.Helpers;


namespace DailyBudgetMAUIApp.Converters
{
    public class BoolToColorDisabledReverse : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Application.Current.Resources.TryGetValue("Gray100", out var Gray100);

            if (value == null) return null;

            bool IsTransacted = (bool)value;

            if (!IsTransacted)
            {
                return Color.FromArgb("#FFFFFFFF");
            }
            else
            {
                return Color.FromArgb("#F2E1E1E1"); ;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
    public class MessageMarginConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Check if the message is from the customer
            bool isCustomerMessage = (bool)value;

            // Return margin based on sender type
            // Adjust the numbers as needed to position the tail correctly
            if (isCustomerMessage)
            {
                // Position tail on the left side for customer messages
                return new Thickness(40, 0, 5, 0);  // Adjust as needed
            }
            else
            {
                // Position tail on the right side for staff messages
                return new Thickness(5, 0, 40, 0);  // Adjust as needed
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TailMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Check if the message is from the customer
            bool isCustomerMessage = (bool)value;

            // Return margin based on sender type
            // Adjust the numbers as needed to position the tail correctly
            if (isCustomerMessage)
            {
                // Position tail on the left side for customer messages
                return new Thickness(10, 0, 0, -10);  // Adjust as needed
            }
            else
            {
                // Position tail on the right side for staff messages
                return new Thickness(0, 0, 10, -10);  // Adjust as needed
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CustomerMessageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Application.Current.Resources.TryGetValue("TertiaryLight", out var TertiaryLight);
            Application.Current.Resources.TryGetValue("PrimaryLightLight", out var PrimaryLightLight);

            bool isCustomerMessage = (bool)value;
            return isCustomerMessage ? TertiaryLight : PrimaryLightLight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class MessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCustomerMessage = (bool)value;
            return isCustomerMessage ? LayoutOptions.End : LayoutOptions.Start;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
    public class ReverseMessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isCustomerMessage = (bool)value;
            return isCustomerMessage ? LayoutOptions.Start : LayoutOptions.End;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class CreateNewPayeeConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Create '' as a payee";

            return $"Create '{(string)value}' as a payee";

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }

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

    public class IsFocusedConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Application.Current.Resources.TryGetValue("StandardInputBorderEntry", out var StandardInputBorderEntry);
            Application.Current.Resources.TryGetValue("FocusInputBorderEntry", out var FocusInputBorderEntry);

            if (value == null) return (Style)StandardInputBorderEntry;

            if ((bool)value)
            {
                return (Style)FocusInputBorderEntry;
            }
            else
            {
                return (Style)StandardInputBorderEntry;
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
            if (value == null) return "";

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

    public class PayeeText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "No Payee Selected";

            if (string.IsNullOrEmpty((string)value))
            {
                return "No Payee Selected";
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

    public class CategoryText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "No Category Selected";

            if (string.IsNullOrEmpty((string)value))
            {
                return "No Category Selected";
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

    public class IsPayeeText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

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
            return "";
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
            return "";
        }
    }

    public class IsNoteText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";

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

    public class IsBudgetShareRequestToBool : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            int BudgetShareRequestID = (int)value;

            if (BudgetShareRequestID != 0)
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
            decimal Amount;
            if (value == null)             
            {
                Amount = 0;
            }
            else
            {
                Amount = (decimal)value;
            }
            
            if(Amount == -1)
            {
                return "Pay day tomorrow";
            }

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

    public class AmountToColor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            Application.Current.Resources.TryGetValue("Success", out var Success);
            Application.Current.Resources.TryGetValue("Danger", out var Danger);

            if (value == null) return (Color)Success;

            decimal Amount = (decimal)value;

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
            else if (EventType == "Budget")
            {
                ReturnGlyph = "\ue84f";
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

            string TString = "";

            if (value is Transactions T)
            {
                if (T.IsIncome)
                {
                    TString = "+ ";
                }
                else
                {
                    TString = "- ";
                }

                TString += T.TransactionAmount.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
            }

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

            if (value is Transactions T)
            {
                if (!T.IsTransacted)
                {
                    return "Pending";
                }
                else
                {
                    return T.RunningTotal.GetValueOrDefault().ToString("c", CultureInfo.CurrentCulture);
                }
            }

            return "";            
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

            if (value is Transactions T)
            {
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

            return "";

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

    public class SavingsTypeToText : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null) return "Put Money in an envelope";

            string SavingsType = (string)value;

            if (SavingsType == "TargetAmount")
            {
                return "Hit goal with same amount every day";
            }
            else if (SavingsType == "TargetDate")
            {
                return "Hit goal by a date";
            }
            else if (SavingsType == "SavingsBuilder")
            {
                return "Keep building every day";
            }
            else
            {
                return "Put money in envelopes";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IsBorrowPayTextConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null) return null;

            bool IsBorrowPay = (bool)value;

            if (IsBorrowPay)
            {
                return "Cover bills when paid";
            }
            else
            {
                return "Cover bills from balance every day";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }    
    
    public class IsDPATextConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null) return null;

            bool IsDPA = (bool)value;

            if (IsDPA)
            {
                return "I am happy for dBudget to contact me";
            }
            else
            {
                return "I never want to hear from dBudget";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CategoryIDToBool : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null) return null;

            int CatID = (int)value;

            if (CatID == -1)
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
    public class CategoryIDToBoolReverse : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null) return null;

            int CatID = (int)value;

            if (CatID == -1)
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

    public class CategoryIDToColor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Application.Current.Resources.TryGetValue("PrimaryLight", out var PrimaryLight);
            Application.Current.Resources.TryGetValue("Success", out var Success);

            if (value == null) return null;

            int CatID = (int)value;

            if (CatID == -1)
            {
                return (Color)Success;
            }
            else
            {
                return (Color)PrimaryLight;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class SubCategoryIDToColor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Application.Current.Resources.TryGetValue("InfoLight", out var Info);
            Application.Current.Resources.TryGetValue("Success", out var Success);

            if (value == null) return null;

            int CatID = (int)value;

            if (CatID == -1)
            {
                return (Color)Success;
            }
            else
            {
                return (Color)Info;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class SubCatIconConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {


            if (value == null) return null;

            int CatID = (int)value;

            if (CatID == -1)
            {
                return "\ue145";
            }
            else
            {
                return "\ue3c9";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
    public class PlayPauseGlyph : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null) return null;

            bool IsPlaying = (bool)value;

            if (IsPlaying)
            {   
                return "\ue034";
            }
            else
            {
                return "\ue037";
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class IndexToChartColor : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Application.Current.Resources.TryGetValue("Gray400", out var Gray400);
            if (value == null) return null;

            int Index = (int)value;

            Color ReturnColor = new();
            if (Index < 8)
            {
                ReturnColor = App.ChartColor[Index];
            }
            else
            {
                ReturnColor = (Color)Gray400;
            }            

            return ReturnColor;

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StringToSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null) return 12;

            double output;

            switch(value.ToString())
            {                  
                case "Caption":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(12,8,10, 10, 12,14);
                    break;                
                case "Subtitle":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(16,12,14, 14, 16,20);
                    break;
                case "Title":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(24, 20, 22, 22, 24, 30);
                    break;
                case "MainTitle":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(28, 24, 24, 26, 28, 34);
                    break;
                case "Header":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(13, 9, 11, 11, 13, 15);
                    break;
                case "Body":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(15, 11, 13, 13, 15, 17);
                    break;
                case "Large":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(22, 18, 20, 20, 22, 28);
                    break;
                case "Medium":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(18, 14, 16, 16, 18, 22);
                    break;
                case "Small":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(14, 10, 12, 12, 14, 16);
                    break;
                case "Micro":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(10, 6, 8, 8, 10, 12);
                    break;                
                case "XL":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(50, 42, 46, 46, 50, 60);
                    break;                
                case "XS":
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(8, 4, 6, 6, 8, 10);
                    break;
                default:
                    output = OnScreenSizeHelpers.Instance.GetScreenSizeValue(14, 10, 12, 12, 14, 16);
                    break;
            }

            return output; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

