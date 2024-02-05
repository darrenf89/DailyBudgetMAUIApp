using DailyBudgetMAUIApp.DataServices;
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

            ProductTools pt = new ProductTools(new RestDataService());

            return pt.GetIcon(Icon).Result;

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
