// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.Models;
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
        public override async void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Create the service provider
                var services = new ServiceCollection();
                this.ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();

                // Get the MainViewModel and initialize it
                var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                await mainViewModel.InitializeAsync();

                // Set the MainWindow
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel,
                };
                desktop.MainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services (ensure these are consistent with Program.cs or remove from Program.cs)
            services.AddSingleton<EasySave.Services.Translation.TranslationService>();
            services.AddSingleton<ModelConfig>();

            // Register ViewModels (ensure these are consistent with Program.cs or remove from Program.cs)
            services.AddSingleton<BackupViewModel>();
            services.AddSingleton<ConfigViewModel>();
            services.AddSingleton<MainViewModel>();
        }
    }
} 