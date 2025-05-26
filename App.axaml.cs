// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CryptoSoftService;
using EasySave.Models;
using EasySave.Services.Logging; // Pour Logger
using EasySave.Services.ProcessControl; // Pour ForbiddenAppManager
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
        private string? argument = Program.LaunchArgument ?? string.Empty;

        /// <summary>
        /// Gets the project argument.
        /// </summary>
        private string? projnum = Program.ProjectArgument ?? string.Empty;

        /// <summary>
        /// Gets the x.
        /// </summary>
        private int x = 0;

        /// <summary>
        /// Gets the y.
        /// </summary>
        private int y = 0;
        
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
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        ServiceProvider = serviceProvider;

        // Récupération et initialisation du MainViewModel via DI
        this.MainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
        this.ModelBackup = serviceProvider.GetRequiredService<ModelBackup>();
        this.ModelConfig = serviceProvider.GetRequiredService<ModelConfig>();
        await this.MainViewModel.InitializeAsync();

        try
            {
                // Load config
                var config = this.ModelConfig.Load();

                // Set paths in ModelBackup
                if (!string.IsNullOrEmpty(config.Source))
                    {
                        this.ModelBackup.SourcePath = config.Source;
                    }

                if (!string.IsNullOrEmpty(config.Destination))
                    {
                        this.ModelBackup.DestinationPath = config.Destination;
                    }
                }
            catch
            {
                throw new Exception("Erreur lors du chargement de la configuration.");
            }

        string backupSource = this.ModelBackup.SourcePath;
        string backupDestination = this.ModelBackup.DestinationPath;
            
        List<EasySave.Models.ModelBackup.Project> list = await this.ModelBackup.FetchProjectsAsync();

        char? separator = null;
        string[] parts = Array.Empty<string>();

        if (!string.IsNullOrEmpty(this.projnum))
        {
            if (this.projnum.Contains('-'))
            {
                separator = '-';
            }
            else if (this.projnum.Contains(';'))
            {
                separator = ';';
            }

            if (separator != null)
            {
                parts = this.projnum.Split(separator.Value);
                if (parts.Length == 2)
                {
                    this.x = int.Parse(parts[0]);
                    this.y = int.Parse(parts[1]);
                }
            }

            if (list != null && list.Count > 0)
            {
                if (this.argument == "save")
                {
                    if (separator == '-')
                    {
                        if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                        {
                            // Convert to zero-based indices
                            this.x = start - 1;
                            this.y = end - 1;
                            for (int i = this.x; i <= this.y; i++)
                            {
                                Console.WriteLine($"Saving project (-): {list[i].Name}");
                                await this.ModelBackup.SaveProjectAsync(list[i].Name, isDifferential: false);
                            }
                        }
                    }
                    else if (separator == ';')
                    {
                        parts = this.projnum.Split(';');
                        foreach (var part in parts)
                        {
                            if (int.TryParse(part, out int idx))
                            {
                                idx -= 1; // zero-based
                                if (idx >= 0 && idx < list.Count)
                                {
                                    Console.WriteLine($"Saving project (;): {list[idx].Name}");
                                    await this.ModelBackup.SaveProjectAsync(list[idx].Name, isDifferential: false);
                                }
                            }
                        }
                    }
                }
                else if (this.argument == "diff")
                {
                    if (separator == '-')
                    {
                        for (int i = this.x; i <= this.y; i++)
                        {
                            await this.ModelBackup.SaveProjectAsync(list[i].Name, isDifferential: true);
                        }
                    }
                    else if (separator == ';')
                    {
                        parts = this.projnum.Split(';');
                        foreach (var part in parts)
                        {
                            if (int.TryParse(part, out int idx))
                            {
                                idx -= 1; // Convert to zero-based index
                                if (idx >= 0 && idx < list.Count)
                                {
                                    Console.WriteLine($"Saving project (;): {list[idx].Name} (diff: true)");
                                    await this.ModelBackup.SaveProjectAsync(list[idx].Name, isDifferential: true);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Récupération des chemins dynamiques depuis le backupViewModel
        string source = this.MainViewModel.BackupViewModel?.SourcePath ?? string.Empty;
        string destination = this.MainViewModel.BackupViewModel?.DestinationPath ?? string.Empty;

        // Configuration et affichage de la fenêtre principale
        desktop.MainWindow = new MainWindow
        {
            DataContext = this.MainViewModel,
        };
        var forbiddenAppManager = new ForbiddenAppManager();
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
            // Console visible uniquement si ton projet est en mode Console Application
            Console.WriteLine($"[ALERTE] Le processus interdit '{runningApp}' est en cours d'exécution. Fermeture de l'application.");

            // Créer une fenêtre temporaire pour afficher l'alerte
            var warningWindow = new Window
            {
                Title = "Processus interdit détecté",
                Width = 400,
                Height = 150,
                Content = new TextBlock
                {
                    Text = $"L'application interdite '{runningApp}' est en cours d'exécution.\nVeuillez la fermer avant de continuer.",
                    Margin = new Thickness(20),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                },
            };

            // Afficher la fenêtre en mode modal sans parent (car desktop.MainWindow pas encore créée)
            await warningWindow.ShowDialog(desktop.MainWindow);

            Environment.Exit(1);
            return;
        }

        desktop.MainWindow = new MainWindow
        {
            DataContext = this.MainViewModel,
        };

        desktop.MainWindow.Show();
    }

           base.OnFrameworkInitializationCompleted();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Enregistrement des services
            services.AddSingleton<EasySave.Services.Translation.TranslationService>();
            services.AddSingleton<EasySave.Models.ModelConfig>();
            services.AddSingleton<LoggingService>(sp =>
            {
                var config = sp.GetRequiredService<ModelConfig>().Load();
                return new LoggingService(config.LogType);
            }); // Enregistrement du Logger avec le type de log depuis la config

            // Enregistrement des ViewModels
            services.AddSingleton<BackupViewModel>();
            services.AddSingleton<ConfigViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<CryptosoftService>();
            
            // Enregistrement des modèles
            services.AddSingleton<ModelBackup>();
            services.AddSingleton<ModelConfig>();
        }
    }
}
