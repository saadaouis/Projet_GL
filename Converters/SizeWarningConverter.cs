using System;
using System.Globalization;
using Avalonia.Data.Converters;
using EasySave.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.Converters
{
    public class SizeWarningConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is double size)
                {
                    var modelConfig = App.ServiceProvider!.GetRequiredService<ModelConfig>();
                    var config = modelConfig.Load();
                    var maxSize = config.MaxFolderSize / 1000000.0; // Convert to MB
                    return maxSize > 0 && size > maxSize;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SizeWarningConverter: {ex.Message}");
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 