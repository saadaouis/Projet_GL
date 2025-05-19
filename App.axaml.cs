// <copyright file="App.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using EasySave.ViewModels;
using EasySave.Views;
using EasySave.Logging; // Pour Logger

namespace EasySave
{
    /// <summary>
    /// Classe principale de l'application.
    /// </summary>
    public partial class App : Application
    {
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

                // Récupération et initialisation du MainViewModel
                var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                await mainViewModel.InitializeAsync();

                // Exemple d'utilisation du Logger
                var logger = serviceProvider.GetRequiredService<Logger>();
                logger.Log("Application démarrée avec succès.");
                // Exemple de log JSON, tu peux adapter les valeurs selon ton contexte

                logger.LogJson("BackupProjet", @"C:\Users\MSi\OneDrive - ESPRIT\Images\Captures d’écran", @"C:\Users\MSi\OneDrive - ESPRIT\Images", 2038, 1.25666);
               
                logger.LogXml("BackupProjet", @"C:\Users\MSi\OneDrive - ESPRIT\Images\Captures d’écran", @"C:\Users\MSi\OneDrive - ESPRIT\Images", 2038, 1.25666);

                // Configuration et affichage de la fenêtre principale
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
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
        }
    }
}
