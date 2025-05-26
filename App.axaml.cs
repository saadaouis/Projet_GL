// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
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
    /// Extension methods for accessing services
    /// </summary>
    public static class ServiceExtensions
    {
        public static T GetService<T>() where T : class
        {
            return App.ServiceProvider.GetRequiredService<T>();
        }
    }

    /// <summary>
    /// Classe principale de l'application.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Global access to the service provider
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        public MainViewModel MainViewModel { get; private set; }
        public ModelBackup ModelBackup { get; private set; }
        public ModelConfig ModelConfig { get; private set; }

        private string argument = Program.LaunchArgument ?? string.Empty;
        private string projnum = Program.ProjectArgument ?? string.Empty;
        private int x = 0;
        private int y = 0;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
           if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();
        ServiceProvider = serviceProvider;

                // Récupération et initialisation du MainViewModel via DI
                MainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                ModelBackup = serviceProvider.GetRequiredService<ModelBackup>();
                ModelConfig = serviceProvider.GetRequiredService<ModelConfig>();
                await MainViewModel.InitializeAsync();

                try
                {
                    // Load config
                    var config = ModelConfig.Load();

                    // Set paths in ModelBackup
                    if (!string.IsNullOrEmpty(config.Source))
                    ModelBackup.SourcePath = config.Source;

                    if (!string.IsNullOrEmpty(config.Destination))
                    ModelBackup.DestinationPath = config.Destination;
                }
                catch
                {
                    throw new Exception("Erreur lors du chargement de la configuration.");
                }

                string BackupSource = ModelBackup.SourcePath;
                string BackupDestination = ModelBackup.DestinationPath;
                
                List<EasySave.Models.ModelBackup.Project> list = await ModelBackup.FetchProjectsAsync();

                char? separator = null;
                string[] parts = Array.Empty<string>();

                if (!string.IsNullOrEmpty(projnum))
                {
                    if (projnum.Contains('-'))
                    {
                        separator = '-';
                    }
                    else if (projnum.Contains(';'))
                    {
                        separator = ';';
                    }

                    if (separator != null)
                    {
                        parts = projnum.Split(separator.Value);
                        if (parts.Length == 2)
                        {
                            x = int.Parse(parts[0]);
                            y = int.Parse(parts[1]);
                        }
                    }

                    if (list != null && list.Count > 0)
                    {
                        if (argument == "save")
                        {
                            if (separator == '-')
                            {
                                if (parts.Length == 2 && int.TryParse(parts[0], out int start) && int.TryParse(parts[1], out int end))
                                {
                                    // Convert to zero-based indices
                                    x = start - 1;
                                    y = end - 1;
                                    for (int i = x; i <= y; i++)
                                    {
                                        Console.WriteLine($"Saving project (-): {list[i].Name}");
                                        await ModelBackup.SaveProjectAsync(list[i].Name, isDifferential: false);
                                    }
                                }
                            }
                            else if (separator == ';')
                            {
                                parts = projnum.Split(';');
                                foreach (var part in parts)
                                {
                                    if (int.TryParse(part, out int idx))
                                    {
                                        idx -= 1; // zero-based
                                        if (idx >= 0 && idx < list.Count)
                                        {
                                            Console.WriteLine($"Saving project (;): {list[idx].Name}");
                                            await ModelBackup.SaveProjectAsync(list[idx].Name, isDifferential: false);
                                        }
                                    }
                                }
                            }
                        }
                        else if (argument == "diff")
                        {
                            if (separator == '-')
                            {
                                for (int i = x; i <= y; i++)
                                {
                                    await ModelBackup.SaveProjectAsync(list[i].Name, isDifferential: true);
                                }
                            }
                            else if (separator == ';')
                            {
                                parts = projnum.Split(';');
                                foreach (var part in parts)
                                {
                                    if (int.TryParse(part, out int idx))
                                    {
                                        idx -= 1; // Convert to zero-based index
                                        if (idx >= 0 && idx < list.Count)
                                        {
                                            Console.WriteLine($"Saving project (;): {list[idx].Name} (diff: true)");
                                            await ModelBackup.SaveProjectAsync(list[idx].Name, isDifferential: true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // Récupération des chemins dynamiques depuis le backupViewModel
                string source = MainViewModel.BackupViewModel?.SourcePath ?? string.Empty;
                string destination = MainViewModel.BackupViewModel?.DestinationPath ?? string.Empty;

                // Configuration et affichage de la fenêtre principale
                desktop.MainWindow = new MainWindow
                {
                    DataContext = MainViewModel
                };
        var forbiddenAppManager = new ForbiddenAppManager();
      //  forbiddenAppManager.AddForbiddenProcess("notepad");
       // forbiddenAppManager.AddForbiddenProcess("calc");

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
                }
            };

            // Afficher la fenêtre en mode modal sans parent (car desktop.MainWindow pas encore créée)
            await warningWindow.ShowDialog(null);

            Environment.Exit(1);
            return;
        }

        desktop.MainWindow = new MainWindow
        {
            DataContext = MainViewModel
        };

        desktop.MainWindow.Show();
    }

    base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Enregistrement des services
            services.AddSingleton<EasySave.Services.Translation.TranslationService>();
            services.AddSingleton<EasySave.Models.ModelConfig>();
            services.AddSingleton<loggingService>(sp =>
            {
                var config = sp.GetRequiredService<ModelConfig>().Load();
                return new loggingService(config.LogType);
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
