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
            if (value is double size)
            {
                var modelConfig = App.ServiceProvider!.GetRequiredService<ModelConfig>();
                var config = modelConfig.Load();
                return config.MaxFolderSize > 0 && size > config.MaxFolderSize / 1000000;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 