// <copyright file="BackupViewModel.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Services.ProcessControl;
using EasySave.Services.Translation;
using EasySave.Services.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.ViewModels
{
    /// <summary>
    /// The view model for the backup view.
    /// </summary>
    public class BackupViewModel : ViewModelBase
    {
        private readonly ModelBackup modelBackup;
        private readonly TranslationService translationService;
        private readonly ForbiddenAppManager appManager;

        private bool isBackupInProgress;
        private double overallProgress;
        private List<ModelBackup.Project> availableProjects;
        private List<ModelBackup.Project> availableBackups;
        private string sourcePath;
        private string destinationPath;
        private bool canStartBackup;
        private string forbiddenAppName;
        private readonly LoggingService logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupViewModel"/> class.
        /// </summary>
        /// <param name="translationService">The translation service.</param>
        public BackupViewModel(TranslationService translationService)
            : base(translationService)
        {
            this.sourcePath = string.Empty;
            this.destinationPath = string.Empty;

            this.modelBackup = App.ServiceProvider!.GetRequiredService<ModelBackup>();
            this.logger = App.ServiceProvider!.GetRequiredService<LoggingService>();
            this.translationService = translationService;

            this.availableProjects = [];
            this.availableBackups = [];
            this.SelectedProjects = [];

            // Setup forbidden app manager and processes to block
            this.appManager = new ForbiddenAppManager();
            this.appManager.AddForbiddenProcess("calc");
            this.appManager.AddForbiddenProcess("Calculator");
            this.forbiddenAppName = "notepad"; // Exemple, tu peux changer selon besoin

            this.canStartBackup = this.CheckIfCanStartBackup();

            if (this.appManager.IsAnyForbiddenAppRunning(out string appName))
            {
                this.canStartBackup = false;
                this.forbiddenAppName = appName;
                Console.WriteLine($"Erreur : \"{appName}\" est lanc�. Sauvegarde d�sactiv�e.");
            }

            // Initialize commands
            this.RefreshProjectsCommand = new MainViewModel.AsyncRelayCommand(async () => await this.LoadProjectsAsync());
            this.RefreshBackupCommand = new MainViewModel.AsyncRelayCommand(async () => await this.LoadProjectsAsync("destination"));
            this.RefreshAllCommand = new MainViewModel.AsyncRelayCommand(async () => await this.RefreshAll());
            this.SaveSelectedProjectCommand = new MainViewModel.AsyncRelayCommand(async () => await this.SaveSelectedProjectsAsync());
            this.DifferentialBackupCommand = new MainViewModel.AsyncRelayCommand(async () => await this.SaveSelectedProjectsAsync(true));
        }

        /// <summary>
        /// Gets the collection of currently selected projects.
        /// </summary>
        public ObservableCollection<ModelBackup.Project> SelectedProjects { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the backup is in progress.
        /// </summary>
        public bool IsBackupInProgress
        {
            get => this.isBackupInProgress;
            set => this.SetProperty(ref this.isBackupInProgress, value);
        }

        /// <summary>
        /// Gets or sets the overall backup progress percentage (0-100).
        /// </summary>
        public double OverallProgress
        {
            get => this.overallProgress;
            set => this.SetProperty(ref this.overallProgress, value);
        }

        /// <summary>
        /// Gets or sets the source path and updates the underlying ModelBackup instance.
        /// </summary>
        public string SourcePath
        {
            get => this.sourcePath;
            set
            {
                if (this.SetProperty(ref this.sourcePath, value))
                {
                    this.modelBackup.SourcePath = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the destination path and updates the underlying ModelBackup instance.
        /// </summary>
        public string DestinationPath
        {
            get => this.destinationPath;
            set
            {
                if (this.SetProperty(ref this.destinationPath, value))
                {
                    this.modelBackup.DestinationPath = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the available projects.
        /// </summary>
        public List<ModelBackup.Project> AvailableProjects
        {
            get => this.availableProjects;
            set => this.SetProperty(ref this.availableProjects, value);
        }

        /// <summary>
        /// Gets or sets the available backups.
        /// </summary>
        public List<ModelBackup.Project> AvailableBackups
        {
            get => this.availableBackups;
            set => this.SetProperty(ref this.availableBackups, value);
        }

        /// <summary>
        /// Gets the refresh projects command.
        /// </summary>
        public ICommand RefreshProjectsCommand { get; }

        /// <summary>
        /// Gets the refresh backup command.
        /// </summary>
        public ICommand RefreshBackupCommand { get; }

        /// <summary>
        /// Gets the save project command.
        /// </summary>
        public ICommand SaveSelectedProjectCommand { get; }

        /// <summary>
        /// Gets the differential backup command.
        /// </summary>
        public ICommand DifferentialBackupCommand { get; }

        /// <summary>
        /// Gets the refresh all command.
        /// </summary>
        public ICommand RefreshAllCommand { get; }

        /// <summary>
        /// Get the backup states.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The backup states.</returns>
        public async Task<BackupState> GetBackupStateAsync(string projectName)
        {
            return await this.modelBackup.GetBackupStateAsync(projectName);
        }

        /// <summary>
        /// Loads the projects asynchronously.
        /// </summary>
        /// <param name="directory">The directory to load projects from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadProjectsAsync(string directory = "source")
        {
            try
            {
                if (directory == "source")
                {
                    this.AvailableProjects = await this.modelBackup.FetchProjectsAsync();
                }
                else
                {
                    this.AvailableBackups = await this.modelBackup.FetchProjectsAsync(directory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading projects: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the selected projects.
        /// </summary>
        /// <param name="selectedItems">The selected items.</param>
        public void UpdateSelectedProjects(IEnumerable<object> selectedItems)
        {
            this.SelectedProjects.Clear();
            foreach (var item in selectedItems.OfType<ModelBackup.Project>())
            {
                this.SelectedProjects.Add(item);
            }

            Console.WriteLine($"Selected projects updated. Count: {this.SelectedProjects.Count}");
        }

        /// <summary>
        /// Saves the currently selected projects.
        /// </summary>
        /// <param name="isDifferential">If true, performs a differential backup.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SaveSelectedProjectsAsync(bool isDifferential = false)
        {
            List<Task<(string ProjectName, bool Result)>> tasks = new();
            
            if (!this.canStartBackup)
            {
                Console.WriteLine($"Impossible de d�marrer la sauvegarde, le processus \"{this.forbiddenAppName}\" est actif.");
                return;
            }

            if (this.SelectedProjects == null || this.SelectedProjects.Count == 0)
            {
                Console.WriteLine("No projects selected to save.");
                return;
            }

            this.IsBackupInProgress = true;
            this.OverallProgress = 0;
            Console.WriteLine($"Starting backup process for {this.SelectedProjects.Count} project(s).");

            int totalProjectsToBackup = this.SelectedProjects.Count;
            int projectsBackedUpSoFar = 0;
            bool allSucceeded = true;

            try
            {
                // V�rification simple : ne pas d�passer 5 projets
                if (this.AvailableProjects.Count + this.SelectedProjects.Count > 5 &&
                    !this.SelectedProjects.All(sp => this.AvailableProjects.Any(ap => ap.Name == sp.Name)))
                {
                    Console.WriteLine("Adding these projects would exceed the maximum number of projects.");
                    this.IsBackupInProgress = false;
                    return;
                }

                foreach (var project in this.SelectedProjects)
                {
                    Console.WriteLine($"Starting save for project: {project.Name}");

                    var progressReporter = new Progress<double>(currentProjectProgress =>
                    {
                        this.OverallProgress = ((projectsBackedUpSoFar * 100.0) + currentProjectProgress) / totalProjectsToBackup;
                        Console.WriteLine($"Overall Progress: {this.OverallProgress:F2}%, Current Project ({project.Name}): {currentProjectProgress:F2}%");
                    });

                    tasks.Add(Task.Run(async () =>
                    {
                        bool result = await this.modelBackup.SaveProjectAsync(project.Name, isDifferential, progressReporter);
                        return (project.Name, result);
                    }));
                }

                var results = await Task.WhenAll(tasks);

                // ✅ Analyse les résultats
                foreach (var (projectName, result) in results)
                {
                    if (result)
                    {
                        projectsBackedUpSoFar++;
                        Console.WriteLine($"✅ Project {projectName} saved successfully.");
                        this.logger.Log(new Dictionary<string, string> { { "message", $"Project {projectName} saved successfully." } });
                    }
                    else
                    {
                        allSucceeded = false;
                        Console.WriteLine($"❌ Project {projectName} failed to save.");
                        this.logger.Log(new Dictionary<string, string> { { "message", $"Project {projectName} failed to save." } });
                    }

                    this.OverallProgress = (projectsBackedUpSoFar * 100.0) / totalProjectsToBackup;
                }

                Console.WriteLine($"Backup process finished. {projectsBackedUpSoFar} out of {totalProjectsToBackup} projects processed.");

                if (allSucceeded)
                {
                    this.OverallProgress = 100;
                }

                await this.LoadProjectsAsync("destination");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during batch backup: {ex.Message}");
                allSucceeded = false;
            }
            finally
            {
                this.IsBackupInProgress = false;

                if (!allSucceeded && totalProjectsToBackup > 0)
                {
                    // Progress remains at last progress
                }
                else if (totalProjectsToBackup == 0)
                {
                    this.OverallProgress = 0;
                }

                Console.WriteLine($"Final Overall Progress: {this.OverallProgress:F2}%");
            }
        }

        /// <summary>
        /// Refreshes all projects and backups.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RefreshAll()
        {
            await this.LoadProjectsAsync();
            await this.LoadProjectsAsync("destination");
        }

        /// <summary>
        /// Checks if the backup can start by verifying forbidden apps are not running.
        /// </summary>
        /// <returns>True if backup can start, false otherwise.</returns>
        private bool CheckIfCanStartBackup()
        {
            var forbiddenProcesses = new[] { "notepad", "calc", "Calculator" };
            foreach (var processName in forbiddenProcesses)
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    this.forbiddenAppName = processName;
                    return false;
                }
            }
            
            return true;
        }
    }
}
