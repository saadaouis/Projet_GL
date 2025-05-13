using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Services.Logger;
using EasySave.Services.Translation;
using EasySave.Views;

namespace EasySave.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly BackupViewModel backupViewModel;
        private readonly ConfigViewModel configViewModel;
        private readonly LogViewModel logViewModel;
        private readonly ILogger logger;
        private readonly TranslationService translationService;
        private bool isAutoSaveEnabled;
        private bool isInitialized;
        private object currentView;

        /// <summary>
        /// Gets or sets a value indicating whether auto-save is enabled.
        /// </summary>
        public bool IsAutoSaveEnabled
        {
            get => this.isAutoSaveEnabled;
            set => this.SetProperty(ref this.isAutoSaveEnabled, value);
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
        public object CurrentView
        {
            get => this.currentView;
            set => this.SetProperty(ref this.currentView, value);
        }

        /// <summary>
        /// Gets the command for downloading a backup.
        /// </summary>
        public ICommand DownloadBackupCommand { get; }

        /// <summary>
        /// Gets the command for saving a project.
        /// </summary>
        public ICommand SaveProjectCommand { get; }

        /// <summary>
        /// Gets the command for toggling auto-save.
        /// </summary>
        public ICommand ToggleAutoSaveCommand { get; }

        /// <summary>
        /// Gets the command for modifying the configuration.
        /// </summary>
        public ICommand ModifyConfigCommand { get; }

        /// <summary>
        /// Gets the command for showing the logs.
        /// </summary>
        public ICommand ShowLogsCommand { get; }

        /// <summary>
        /// Gets the command for exiting the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        public MainViewModel()
        {
            this.logger = new Logger();
            this.translationService = new TranslationService();

            this.backupViewModel = new BackupViewModel(this.logger, this.translationService);
            this.configViewModel = new ConfigViewModel(this.logger, this.translationService);
            this.logViewModel = new LogViewModel(this.logger, this.translationService);
            IsAutoSaveEnabled = true;

            // Initialize commands
            DownloadBackupCommand = new RelayCommand(async () => await this.backupViewModel.DownloadBackupAsync());
            SaveProjectCommand = new RelayCommand(async () => await this.backupViewModel.SaveProjectAsync());
            ToggleAutoSaveCommand = new RelayCommand(() => ToggleAutoSave());
            ModifyConfigCommand = new RelayCommand(async () => 
            {
                CurrentView = new ConfigView { DataContext = this.configViewModel };
                await this.configViewModel.ModifyConfigAsync();
            });
            ShowLogsCommand = new RelayCommand(async () => 
            {
                CurrentView = new LogView { DataContext = this.logViewModel };
                await this.logViewModel.ShowLogsAsync();
            });
            ExitCommand = new RelayCommand(() => Environment.Exit(0));

            // Set initial view
            CurrentView = new BackupView { DataContext = this.backupViewModel };
        }

        public async Task InitializeAsync()
        {
            try
            {
                await configViewModel.LoadConfigAsync();
                IsInitialized = true;
                logger.Log(LogLevel.Info, "Application initialized successfully");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Failed to initialize application: {ex.Message}");
                throw;
            }
        }

        private void ToggleAutoSave()
        {
            IsAutoSaveEnabled = !IsAutoSaveEnabled;
            logger.Log(LogLevel.Info, $"Auto-save {(IsAutoSaveEnabled ? "enabled" : "disabled")}");
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool>? canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => execute();

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 