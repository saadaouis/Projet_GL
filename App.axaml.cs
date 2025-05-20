// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using EasySave.ViewModels;
using EasySave.Views;
using Microsoft.Extensions.DependencyInjection;
using CryptoSoftService;
using System;
using EasySave.Logging; // Pour Logger

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
  
    /// Classe principale de l'application.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Global access to the service provider
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }
        
        public MainViewModel MainViewModel { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Configuration du container d'injection de dépendances
                var services = new ServiceCollection();
                ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();
                ServiceProvider = serviceProvider;  // Store the service provider

                // Récupération et initialisation du MainViewModel via DI
                MainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                await MainViewModel.InitializeAsync();

                // Récupération du logger
                var logger = serviceProvider.GetRequiredService<Logger>();
                logger.Log("Application démarrée avec succès.");

                // Récupération des chemins dynamiques depuis le backupViewModel
                string source = MainViewModel.BackupViewModel?.SourcePath ?? string.Empty;
                string destination = MainViewModel.BackupViewModel?.DestinationPath ?? string.Empty;
            

                // Logs avec chemins dynamiques
                logger.LogJson("BackupProjet", source, destination,2028, 1.25666);
                logger.LogXml("BackupProjet", source, destination, 2038, 1.25666);

                // Configuration et affichage de la fenêtre principale
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
            services.AddSingleton<Logger>(); // Enregistrement du Logger

            // Enregistrement des ViewModels
            services.AddSingleton<BackupViewModel>();
            services.AddSingleton<ConfigViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<CryptosoftService>();
        }
    }
}
