using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EasySave.Models;
using EasySave.ViewModels;
using EasySave.Views;
using Microsoft.Extensions.DependencyInjection;
using EasySave.Logging;

namespace EasySave
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override async void OnFrameworkInitializationCompleted()
        {
            if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Configuration des services
                var services = new ServiceCollection();
                ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();

                // Récupérer le ViewModel principal
                var mainViewModel = serviceProvider.GetRequiredService<MainViewModel>();
                await mainViewModel.InitializeAsync();

                // Utilisation du Logger
                var logger = serviceProvider.GetRequiredService<Logger>();
                logger.Log("Application démarrée avec succès.");

                // Définir la fenêtre principale
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
            // Services
            services.AddSingleton<EasySave.Services.Translation.TranslationService>();
            services.AddSingleton<ModelConfig>();
            services.AddSingleton<Logger>();

            // ViewModels
            services.AddSingleton<BackupViewModel>();
            services.AddSingleton<ConfigViewModel>();
            services.AddSingleton<MainViewModel>();
        }
    }
}
