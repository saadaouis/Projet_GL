// <copyright file="MainViewModel.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using EasySave.Models;
using EasySave.Services.Translation;
using EasySave.Views;

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
        private bool isAutoSaveEnabled;
        private bool isInitialized;
        private ViewModelBase currentView;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            this.translationService = new TranslationService();

            this.backupViewModel = new BackupViewModel(this.translationService);
            this.configViewModel = new ConfigViewModel(this.translationService);
            this.IsAutoSaveEnabled = true;

            // Initialize commands
            this.SaveProjectCommand = new RelayCommand(() =>
            {
                this.backupViewModel.SaveSelectedProjectCommand.Execute(null);
            });
            this.ModifyConfigCommand = new RelayCommand(() =>
            {
                this.configViewModel.SaveOrOverrideConfig();
            });
            this.ExitCommand = new RelayCommand(() => Environment.Exit(0));

            // Set initial view
            try
            {
                this.currentView = this.configViewModel;
            }
            catch (System.Exception)
            {
                this.currentView = this.backupViewModel;
                Console.WriteLine("Failed to set initial view");
                Environment.Exit(0);
                throw;
            }
        }

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
        public required ViewModelBase CurrentView
        {
            get => this.currentView;
            set => this.SetProperty(ref this.currentView, value);
        }

        /// <summary>
        /// Gets the command for toggling auto-save.
        /// </summary>
   /*      public ICommand ToggleAutoSaveCommand { get; } */

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
        /// Initializes the application.
        /// </summary>
        public void Initialize()
        {
            try
            {
                this.configViewModel.LoadConfig();
                this.IsInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize application: {ex.Message}");
                throw;
            }
        }

/*         private void ToggleAutoSave()
        {
            this.IsAutoSaveEnabled = !this.IsAutoSaveEnabled;
            Console.WriteLine($"Auto-save {(this.IsAutoSaveEnabled ? "enabled" : "disabled")}");
        }
    } */

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