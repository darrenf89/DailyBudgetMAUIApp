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
}
