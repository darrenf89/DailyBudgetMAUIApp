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

            Application.Current.Resources.TryGetValue("SuccessLight", out var Success);
            Application.Current.Resources.TryGetValue("DangerLight", out var Danger);

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
}
