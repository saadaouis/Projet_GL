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
using EasySave.Services.Logging; // Pour Logger
using EasySave.Models; // Pour ModelConfig
using EasySave.Services.ProcessControl; // Pour ForbiddenAppManager

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

        MainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
        await MainViewModel.InitializeAsync();

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
        }
    }
}
