// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.ViewModels;
using EasySave.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave
{
    /// <summary>@
    /// The main application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the application.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the framework initialization is completed.
        /// </summary>
        public override void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Create the service provider
                var services = new ServiceCollection();
                this.ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();

                // Get the MainViewModel and initialize it
                var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                mainViewModel.Initialize();

                // Set the MainWindow
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel,
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services (ensure these are consistent with Program.cs or remove from Program.cs)
            services.AddSingleton<EasySave.Services.Translation.TranslationService>();

            // Register ViewModels (ensure these are consistent with Program.cs or remove from Program.cs)
            services.AddTransient<BackupViewModel>();
            services.AddTransient<ConfigViewModel>();
            services.AddSingleton<MainViewModel>();
        }
    }
} 