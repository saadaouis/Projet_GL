// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CryptoSoftService;
using EasySave.Models;
using EasySave.Services.Logging; // Pour Logger
using EasySave.Services.ProcessControl; // Pour ForbiddenAppManager
using EasySave.Services.Translation;
using EasySave.ViewModels;
using EasySave.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave
{
    /// <summary>
    /// Classe principale de l'application.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the argument.
        /// </summary>
        private readonly string? argument = Program.LaunchArgument ?? string.Empty;

        /// <summary>
        /// Gets the project argument.
        /// </summary>
        private readonly string? projnum = Program.ProjectArgument ?? string.Empty;
        
        /// <summary>
        /// Gets global access to the service provider.
        /// </summary>
        public static IServiceProvider? ServiceProvider { get; private set; }

        /// <summary>
        /// Gets the main view model.
        /// </summary>
        public MainViewModel? MainViewModel { get; private set; }

        /// <summary>
        /// Gets the model backup.
        /// </summary>
        public ModelBackup? ModelBackup { get; private set; }

        /// <summary>
        /// Gets the model config.
        /// </summary>
        public ModelConfig? ModelConfig { get; private set; }

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
                try
                {
                    await this.InitializeApplicationAsync(desktop);
                }
                catch (Exception ex)
                {
                    await this.ShowErrorWindowAsync(
                        "Initialization Error", 
                        $"An error occurred during application initialization: {ex.Message}");
                    Environment.Exit(1);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Enregistrement des services
            services.AddSingleton<LoggingService>(sp =>
            {
                var config = sp.GetRequiredService<ModelConfig>().Load();
                return new LoggingService(config.LogType);
            });
            services.AddSingleton<EasySave.Services.Translation.TranslationService>(sp =>
            {
                var logger = sp.GetRequiredService<LoggingService>();
                return new TranslationService("Resources/translations.json");
            });
            services.AddSingleton<EasySave.Services.Translation.TranslationManager>();
            services.AddSingleton<EasySave.Models.ModelConfig>();
            services.AddSingleton<ForbiddenAppManager>();

            // Enregistrement des ViewModels
            services.AddSingleton<BackupViewModel>();
            services.AddSingleton<ConfigViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<CryptosoftService>();
            
            // Enregistrement des modèles
            services.AddSingleton<ModelBackup>();
        }

        private async Task InitializeApplicationAsync(IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            ServiceProvider = serviceProvider;

            // Initialize core services
            this.MainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
            this.ModelBackup = serviceProvider.GetRequiredService<ModelBackup>();
            this.ModelConfig = serviceProvider.GetRequiredService<ModelConfig>();
            
            await this.MainViewModel.InitializeAsync();
            this.LoadConfigurationAsync();
            await this.HandleCommandLineArgumentsAsync();
            
            // Check for forbidden applications
            if (await this.CheckForbiddenApplicationsAsync())
            {
                Environment.Exit(1);
                return;
            }

            // Create and show main window
            desktop.MainWindow = new MainWindow
            {
                DataContext = this.MainViewModel,
            };
            desktop.MainWindow.Show();
        }

        private void LoadConfigurationAsync()
        {
            try
            {
                var config = this.ModelConfig!.Load();
                if (!string.IsNullOrEmpty(config.Source))
                {
                    this.ModelBackup!.SourcePath = config.Source;
                }

                if (!string.IsNullOrEmpty(config.Destination))
                {
                    this.ModelBackup!.DestinationPath = config.Destination;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading configuration: {ex.Message}", ex);
            }
        }

        private async Task HandleCommandLineArgumentsAsync()
        {
            if (string.IsNullOrEmpty(this.projnum))
            {
                return;
            }   

            var list = await this.ModelBackup!.FetchProjectsAsync();
            if (list == null || list.Count == 0)
            {
                return;
            }

            var (start, end) = this.ParseProjectRange(this.projnum);
            if (start == -1 || end == -1)
            {
                return;
            }

            if (this.argument == "save")
            {
                await this.HandleSaveCommandAsync(list, start, end);
            }
            else if (this.argument == "diff")
            {
                await this.HandleDiffCommandAsync(list, start, end);
            }
        }

        private (int Start, int End) ParseProjectRange(string range)
        {
            char? separator = null;
            if (range.Contains('-'))
            {
                separator = '-';
            }
            else if (range.Contains(';'))
            {
                separator = ';';
            }

            if (separator == null)
            {
                return (-1, -1);
            }

            var parts = range.Split(separator.Value);
            if (parts.Length != 2)
            {
                return (-1, -1);
            }

            if (!int.TryParse(parts[0], out int start) || !int.TryParse(parts[1], out int end))
            {
                return (-1, -1);
            }

            return (start - 1, end - 1); // Convert to zero-based indices
        }

        private async Task HandleSaveCommandAsync(List<ModelBackup.Project> projects, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                if (i >= 0 && i < projects.Count)
                {
                    Console.WriteLine($"Saving project: {projects[i].Name}");
                    await this.ModelBackup!.SaveProjectAsync(projects[i].Name, isDifferential: false);
                }
            }
        }

        private async Task HandleDiffCommandAsync(List<ModelBackup.Project> projects, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                if (i >= 0 && i < projects.Count)
                {
                    Console.WriteLine($"Saving project (diff): {projects[i].Name}");
                    await this.ModelBackup!.SaveProjectAsync(projects[i].Name, isDifferential: true);
                }
            }
        }

        private async Task<bool> CheckForbiddenApplicationsAsync()
        {
            var forbiddenAppManager = App.ServiceProvider!.GetRequiredService<ForbiddenAppManager>();
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    forbiddenAppManager.AddForbiddenProcess("Calculator");
                }
                else
                {
                    forbiddenAppManager.AddForbiddenProcess("notepad");
                    forbiddenAppManager.AddForbiddenProcess("calc");
                }

                if (forbiddenAppManager.IsAnyForbiddenAppRunning(out var runningApp))
                {
                    await this.ShowErrorWindowAsync(
                        "Forbidden Application Detected",
                        $"The forbidden application '{runningApp}' is currently running.\nPlease close it before continuing.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                await this.ShowErrorWindowAsync(
                    "Error",
                    $"An error occurred while checking for forbidden applications: {ex.Message}");
                return true;
            }

            return false;
        }

        private async Task ShowErrorWindowAsync(string title, string message)
        {
            var warningWindow = new Window
            {
                Title = title,
                Width = 400,
                Height = 150,
                Content = new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(20),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                },
            };
            
            Console.WriteLine("Error: " + title + " " + message);

            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                await warningWindow.ShowDialog(desktop.MainWindow);
            }
        }
    }
}
