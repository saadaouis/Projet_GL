// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using EasySave.ViewModels;
using EasySave.Views;
using CryptoSoftService;
using System;
using EasySave.Services.Logging;             // For logging service
using EasySave.Models;                        // For accessing configuration model
using EasySave.Services.ProcessControl;       // For managing forbidden apps

namespace EasySave
{
    /// <summary>
    /// Extension method to simplify service resolution throughout the app.
    /// </summary>
    public static class ServiceExtensions
    {
        public static T GetService<T>() where T : class
        {
            return App.ServiceProvider.GetRequiredService<T>();
        }
    }

    /// <summary>
    /// Main application class for EasySave (Avalonia UI).
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Provides global access to the dependency injection container (ServiceProvider).
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// The main view model bound to the MainWindow.
        /// </summary>
        public MainViewModel MainViewModel { get; private set; }

        /// <summary>
        /// Load Avalonia XAML structure when application is initialized.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the application is fully initialized (after framework setup).
        /// Registers services, initializes the main ViewModel, checks forbidden processes,
        /// and starts the main window if everything is safe.
        /// </summary>
        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Setup dependency injection services
                var services = new ServiceCollection();
                ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();
                ServiceProvider = serviceProvider;

                // Resolve and initialize the main ViewModel
                MainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                await MainViewModel.InitializeAsync();

                // Process control: forbid specific applications during EasySave execution
                var forbiddenAppManager = new ForbiddenAppManager();
                forbiddenAppManager.AddForbiddenProcess("notepad");
                // forbiddenAppManager.AddForbiddenProcess("calc"); // Example of additional forbidden app

                // If any forbidden app is running, display warning and exit
                if (forbiddenAppManager.IsAnyForbiddenAppRunning(out var runningApp))
                {
                    Console.WriteLine($"[ALERT] Forbidden process '{runningApp}' is currently running. Closing the application.");

                    // Show a modal warning window (before MainWindow is loaded)
                    var warningWindow = new Window
                    {
                        Title = "Forbidden Process Detected",
                        Width = 400,
                        Height = 150,
                        Content = new TextBlock
                        {
                            Text = $"The forbidden application '{runningApp}' is currently running.\nPlease close it before using EasySave.",
                            Margin = new Thickness(20),
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        }
                    };

                    await warningWindow.ShowDialog(null); // Modal dialog
                    Environment.Exit(1); // Forcefully close the application
                    return;
                }

                // If no forbidden apps are running, launch the main window
                desktop.MainWindow = new MainWindow
                {
                    DataContext = MainViewModel
                };

                desktop.MainWindow.Show(); // Show the main UI
            }

            base.OnFrameworkInitializationCompleted(); // Continue Avalonia lifecycle
        }

        /// <summary>
        /// Register all application services and ViewModels in the DI container.
        /// </summary>
        /// <param name="services">ServiceCollection used for DI setup</param>
        private void ConfigureServices(IServiceCollection services)
        {
            // Register services (singleton = one instance shared across app lifetime)
            services.AddSingleton<EasySave.Services.Translation.TranslationService>();
            services.AddSingleton<EasySave.Models.ModelConfig>();

            // Register the logging service and configure it based on config file
            services.AddSingleton<loggingService>(sp =>
            {
                var config = sp.GetRequiredService<ModelConfig>().Load(); // Load config to get LogType
                return new loggingService(config.LogType);                // Create logger with that log type
            });

            // Register all ViewModels for UI binding
            services.AddSingleton<BackupViewModel>();
            services.AddSingleton<ConfigViewModel>();
            services.AddSingleton<MainViewModel>();

            // Register external cryptographic service
            services.AddSingleton<CryptosoftService>();
        }
    }
}
