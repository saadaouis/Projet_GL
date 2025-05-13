// <copyright file="Program.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EasySave.Services.Logger;
using EasySave.Services.Translation;
using EasySave.ViewModels;
using EasySave.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EasySave
{
    /// <summary>
    /// Main class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, desktop =>
                {
                    desktop.MainWindow = new MainWindow(serviceProvider.GetRequiredService<MainViewModel>());
                });
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<ILogger, DualTerminalLogger>();
            services.AddSingleton<TranslationService>();

            // Register ViewModels
            services.AddTransient<BackupViewModel>();
            services.AddTransient<ConfigViewModel>();
            services.AddTransient<LogViewModel>();
            services.AddSingleton<MainViewModel>();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
