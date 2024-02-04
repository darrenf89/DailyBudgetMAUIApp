using DailyBudgetMAUIApp.Models;
using System.Globalization;

namespace DailyBudgetMAUIApp.Converters
{
    public class IconNameConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string Icon = (string)value;

            switch(Icon)
            {
                case "Today":
                    return "\ue8df";
                case "Spa":
                    return "\ueb4c";
                case "Track_changes":
                    return "\ue8e1";
                case "Receipt_long":
                    return "\uef6e";
                case "Add":
                    return "\ue145";
                default:
                    return "";

            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
