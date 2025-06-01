// <copyright file="MainViewModel.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CryptoSoftService;
using EasySave.Models;
using EasySave.Services.Translation;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using EasySave.Services.Logging;

namespace EasySave.ViewModels
{


    /// <summary>
    /// Main view model class that manages the main window and its views.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly BackupViewModel backupViewModel;
        private readonly ConfigViewModel configViewModel;
        private readonly TranslationService translationService;
        private readonly ModelConfig modelConfig;
        private bool isInitialized;
        private ViewModelBase currentView;

        public BackupViewModel BackupViewModel => this.backupViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="modelConfig">The model configuration.</param>
        /// <param name="translationService">The translation service.</param>
        /// <param name="backupViewModel">The backup view model.</param>
        /// <param name="configViewModel">The config view model.</param>
        public MainViewModel(ModelConfig modelConfig, TranslationService translationService, BackupViewModel backupViewModel, ConfigViewModel configViewModel)
            : base(translationService)
        {
            this.modelConfig = modelConfig;
            this.translationService = translationService;
            this.backupViewModel = backupViewModel;
            this.configViewModel = configViewModel;
            this.configViewModel.SetMainViewModel(this);
            this.currentView = this.configViewModel;

            // Initialize commands
            this.SaveProjectCommand = new RelayCommand(() =>
            {
                if (this.backupViewModel.SaveSelectedProjectCommand.CanExecute(null))
                {
                    this.backupViewModel.SaveSelectedProjectCommand.Execute(null);
                }
            });
            this.ModifyConfigCommand = new RelayCommand(() =>
            {
                Console.WriteLine("ModifyConfigCommand executed: Navigating to ConfigView.");
                this.CurrentView = this.configViewModel;
            });
            this.ExitCommand = new RelayCommand(() => Environment.Exit(0));

            // Initial view is set in InitializeAsync after config is loaded
        }

        /// <summary>
        /// Gets or sets a value indicating whether the application has been initialized.
        /// </summary>
        public bool IsInitialized
        {
            get => this.isInitialized;
            set => this.SetProperty(ref this.isInitialized, value);
        }

        /// <summary>
        /// Gets or sets the current view.
        /// </summary>
        public ViewModelBase CurrentView
        {
            get => this.currentView;
            set => this.SetProperty(ref this.currentView, value);
        }

        /// <summary>
        /// Gets the command for saving a project.
        /// </summary>
        public ICommand SaveProjectCommand { get; }

        /// <summary>
        /// Gets the command for modifying the configuration.
        /// </summary>
        public ICommand ModifyConfigCommand { get; }

        /// <summary>
        /// Gets the command for exiting the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// Navigates the CurrentView to the BackupView.
        /// </summary>
        public void NavigateToBackupView()
        {
            Console.WriteLine("NavigateToBackupView called: Setting CurrentView to BackupViewModel.");
            this.CurrentView = this.backupViewModel;
        }

        /// <summary>
        /// Initializes the application.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InitializeAsync()
        {
            try
            {
                var loadedConfig = this.modelConfig.Load();
                string languageToSet = loadedConfig.Language ?? "en";
                Console.WriteLine($"Configured language: {languageToSet}");
                await this.translationService.SetLanguageAsync(languageToSet);
                Console.WriteLine("Testing translation: " + this.translationService.GetTranslation("menu.settings.autosave"));

                var logger = App.ServiceProvider.GetRequiredService<loggingService>();
                logger.Log(new Dictionary<string, string> { { "message", "MainViewModel: Initialized" } });

                this.backupViewModel.SourcePath = loadedConfig.Source ?? string.Empty;
                this.backupViewModel.DestinationPath = loadedConfig.Destination ?? string.Empty;

                this.configViewModel.CurrentConfig = loadedConfig ?? new ModelConfig.Config();
                Console.WriteLine($"MainViewModel: Initialized ConfigViewModel.CurrentConfig.Language: {this.configViewModel.CurrentConfig.Language}");
                var cryptosoftService = new CryptosoftService();
                await cryptosoftService.Encrypt("test.txt");

                if (this.modelConfig.IsNewConfig)
                {
                    this.CurrentView = this.configViewModel;
                }
                else
                {
                    this.CurrentView = this.backupViewModel;
                }

                Console.WriteLine("MainViewModel initialization complete. Initial view set to BackupView.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!! Critical error during MainViewModel InitializeAsync: {ex.ToString()}");
                this.CurrentView = this.backupViewModel;
                this.IsInitialized = true;
            }
        }

        /// <summary>
        /// Refreshes the projects and backups.
        /// </summary>
        public void RefreshProjects()
        {
            this.backupViewModel.RefreshProjectsCommand.Execute(null);
            this.backupViewModel.RefreshBackupCommand.Execute(null);
        }

        /// <summary>
        /// Changes the paths of the projects and backups.
        /// </summary>
        /// <param name="source">The source path.</param>
        /// <param name="destination">The destination path.</param>
        public void ChangePaths(string? source, string? destination)
        {
            if (source != null)
            {
                this.backupViewModel.SourcePath = source;
            }

            if (destination != null)
            {
                this.backupViewModel.DestinationPath = destination;
            }

            this.RefreshProjects();
        }

        /// <summary>
        /// Relay command class that implements the ICommand interface.
        /// </summary>
        public class RelayCommand : ICommand
        {
            private readonly Action execute;
            private readonly Func<bool>? canExecute;

            /// <summary>
            /// Initializes a new instance of the <see cref="RelayCommand"/> class.
            /// </summary>
            /// <param name="execute">The action to execute.</param>
            /// <param name="canExecute">The function to check if the command can execute.</param>
            /// <exception cref="ArgumentNullException">Thrown when the execute parameter is null.</exception>
            public RelayCommand(Action execute, Func<bool>? canExecute = null)
            {
                this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
                this.canExecute = canExecute;
            }

            /// <summary>
            /// Event handler for the CanExecuteChanged event.
            /// </summary>
            public event EventHandler? CanExecuteChanged;

            /// <summary>
            /// Determines if the command can execute.
            /// </summary>
            /// <param name="parameter">The parameter to check.</param>
            /// <returns>True if the command can execute, false otherwise.</returns>
            public bool CanExecute(object? parameter) => this.canExecute?.Invoke() ?? true;

            /// <summary>
            /// Executes the command.
            /// </summary>
            /// <param name="parameter">The parameter to execute the command with.</param>
            public void Execute(object? parameter) => this.execute();

            /// <summary>
            /// Raises the CanExecuteChanged event.
            /// </summary>
            public void RaiseCanExecuteChanged()
            {
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Generic relay command class that implements the ICommand interface and supports parameters.
        /// </summary>
        public class RelayCommand<T> : ICommand
        {
            private readonly Action<T> execute;
            private readonly Func<T, bool>? canExecute;

            /// <summary>
            /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
            /// </summary>
            /// <param name="execute">The action to execute.</param>
            /// <param name="canExecute">The function to check if the command can execute.</param>
            /// <exception cref="ArgumentNullException">Thrown when the execute parameter is null.</exception>
            public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
            {
                this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
                this.canExecute = canExecute;
            }

            /// <summary>
            /// Event handler for the CanExecuteChanged event.
            /// </summary>
            public event EventHandler? CanExecuteChanged;

            /// <summary>
            /// Determines if the command can execute.
            /// </summary>
            /// <param name="parameter">The parameter to check.</param>
            /// <returns>True if the command can execute, false otherwise.</returns>
            public bool CanExecute(object? parameter) =>
                this.canExecute?.Invoke((T)parameter!) ?? true;

            /// <summary>
            /// Executes the command.
            /// </summary>
            /// <param name="parameter">The parameter to execute the command with.</param>
            public void Execute(object? parameter) =>
                this.execute((T)parameter!);

            /// <summary>
            /// Raises the CanExecuteChanged event.
            /// </summary>
            public void RaiseCanExecuteChanged()
            {
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Async relay command class that implements the ICommand interface.
        /// </summary>
        public class AsyncRelayCommand : ICommand
        {
            private readonly Func<Task> execute;
            private readonly Func<bool>? canExecute;
            private bool isExecuting;

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncRelayCommand"/> class.
            /// </summary>
            /// <param name="execute">The action to execute.</param>
            /// <param name="canExecute">The function to check if the command can execute.</param>
            /// <exception cref="ArgumentNullException">Thrown when the execute parameter is null.</exception>
            public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
            {
                this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
                this.canExecute = canExecute;
            }

            /// <summary>
            /// Event handler for the CanExecuteChanged event.
            /// </summary>
            public event EventHandler? CanExecuteChanged;

            /// <summary>
            /// Determines if the command can execute.
            /// </summary>
            /// <param name="parameter">The parameter to check.</param>
            /// <returns>True if the command can execute, false otherwise.</returns>
            public bool CanExecute(object? parameter) =>
                !this.isExecuting && (this.canExecute?.Invoke() ?? true);

            /// <summary>
            /// Executes the command.
            /// </summary>
            /// <param name="parameter">The parameter to execute the command with.</param>
            public async void Execute(object? parameter)
            {
                if (!this.CanExecute(parameter))
                {
                    return;
                }

                try
                {
                    this.isExecuting = true;
                    this.RaiseCanExecuteChanged();
                    await this.execute();
                }
                finally
                {
                    this.isExecuting = false;
                    this.RaiseCanExecuteChanged();
                }
            }

            /// <summary>
            /// Raises the CanExecuteChanged event.
            /// </summary>
            public void RaiseCanExecuteChanged()
            {
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}