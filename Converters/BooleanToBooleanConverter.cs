using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EasySave.Converters
{
    public class BooleanToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string condition)
            {
                // Parse the condition string (e.g., "!HasSelectedProjectsExceedingMaxSize")
                bool shouldNegate = condition.StartsWith("!");
                return shouldNegate ? !boolValue : boolValue;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 