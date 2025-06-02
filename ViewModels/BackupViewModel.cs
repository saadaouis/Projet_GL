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
using EasySave.Services.Server;

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
        private readonly ModelConfig modelConfig;

        private readonly BackupServer serverService;

        private bool isBackupInProgress;
        private Dictionary<string, double> projectProgress;
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
            this.modelConfig = App.ServiceProvider!.GetRequiredService<ModelConfig>();

            this.availableProjects = [];
            this.availableBackups = [];
            this.SelectedProjects = new ObservableCollection<ModelBackup.Project>();
            this.projectProgress = new Dictionary<string, double>();

            this.serverService = new BackupServer(8080, this.modelBackup, this.logger);
            Task.Run(async () => await this.serverService.StartAsync());

            // Initialize pause/resume/stop commands
            this.PauseProjectCommand = new RelayCommand<string>(name => modelBackup.PauseProject(name));
            this.ResumeProjectCommand = new RelayCommand<string>(name => modelBackup.ResumeProject(name));
            this.StopProjectCommand = new RelayCommand<string>(name => modelBackup.StopProject(name));


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
                Console.WriteLine($"Erreur : \"{appName}\" est lancé. Sauvegarde désactivée.");
            }

            // Initialize commands
            this.RefreshProjectsCommand = new MainViewModel.AsyncRelayCommand(async () => await this.LoadProjectsAsync());
            this.RefreshBackupCommand = new MainViewModel.AsyncRelayCommand(async () => await this.LoadProjectsAsync("destination"));
            this.RefreshAllCommand = new MainViewModel.AsyncRelayCommand(async () => await this.RefreshAll());
            this.SaveSelectedProjectCommand = new MainViewModel.AsyncRelayCommand(async () => await this.SaveSelectedProjectsAsync(false));
            this.DifferentialBackupCommand = new MainViewModel.AsyncRelayCommand(async () => await this.SaveSelectedProjectsAsync(true));
        }

        /// <summary>
        /// Gets the collection of currently selected projects.
        /// </summary>
        public ObservableCollection<ModelBackup.Project> SelectedProjects { get; }

        /// <summary>
        /// Gets a value indicating whether the backup can start.
        /// </summary>
        public bool CanStartBackup
        {
            get => !this.IsBackupInProgress && this.canStartBackup && !this.HasSelectedProjectsExceedingMaxSize();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the backup is in progress.
        /// </summary>
        public bool IsBackupInProgress
        {
            get => this.isBackupInProgress;
            set
            {
                if (this.SetProperty(ref this.isBackupInProgress, value))
                {
                    this.OnPropertyChanged(nameof(this.CanStartBackup));
                }
            }
        }

        /// <summary>
        /// Gets the progress for a specific project by name.
        /// This method is used by the View to bind to individual progress bars.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The progress value between 0 and 100.</returns>
        public double GetProjectProgress(string projectName)
        {
            return this.projectProgress.TryGetValue(projectName, out double progress) ? progress : 0;
        }

        /// <summary>
        /// Sets the progress for a specific project and notifies the UI.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="progress">The progress value between 0 and 100.</param>
        public void SetProjectProgress(string projectName, double progress)
        {
            this.projectProgress[projectName] = progress;
            // Notify that the progress for this specific project has changed
            this.OnPropertyChanged($"ProjectProgress[{projectName}]");
        }

        /// <summary>
        /// Gets the overall progress of all active backups.
        /// </summary>
        public double OverallProgress
        {
            get
            {
                if (!this.IsBackupInProgress || this.SelectedProjects.Count == 0)
                    return 0;

                var totalProgress = this.SelectedProjects.Sum(p => this.GetProjectProgress(p.Name));
                return totalProgress / this.SelectedProjects.Count;
            }
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

        public ICommand PauseProjectCommand { get; }

        public ICommand ResumeProjectCommand { get; }
        
        public ICommand StopProjectCommand { get; }


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

            this.OnPropertyChanged(nameof(this.CanStartBackup));
            Console.WriteLine($"Selected projects updated. Count: {this.SelectedProjects.Count}");
        }

        /// <summary>
        /// Checks if a project exceeds the maximum folder size.
        /// </summary>
        /// <param name="project">The project to check.</param>
        /// <returns>True if the project exceeds the maximum size, false otherwise.</returns>
        public bool IsProjectExceedingMaxSize(ModelBackup.Project project)
        {
            var config = this.modelConfig.Load();
            return config.MaxFolderSize > 0 && project.Size > config.MaxFolderSize / 1000000;
        }

        /// <summary>
        /// Checks if any selected project exceeds the maximum folder size.
        /// </summary>
        /// <returns>True if any selected project exceeds the maximum size, false otherwise.</returns>
        public bool HasSelectedProjectsExceedingMaxSize()
        {
            return this.SelectedProjects.Any(p => this.IsProjectExceedingMaxSize(p));
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
                Console.WriteLine($"Impossible de démarrer la sauvegarde, le processus \"{this.forbiddenAppName}\" est actif.");
                return;
            }

            if (this.SelectedProjects == null || this.SelectedProjects.Count == 0)
            {
                Console.WriteLine("No projects selected to save.");
                return;
            }

            if (this.HasSelectedProjectsExceedingMaxSize())
            {
                Console.WriteLine("Cannot start backup: One or more selected projects exceed the maximum folder size.");
                return;
            }

            this.IsBackupInProgress = true;
            this.projectProgress.Clear();
            
            // Initialize progress for each selected project
            foreach (var project in this.SelectedProjects)
            {
                this.SetProjectProgress(project.Name, 0);
            }
            
            // Notify that overall progress should be recalculated
            this.OnPropertyChanged(nameof(this.OverallProgress));
            
            Console.WriteLine($"Starting backup process for {this.SelectedProjects.Count} project(s).");

            try
            {
                // Vérification simple : ne pas dépasser 5 projets
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
                        this.SetProjectProgress(project.Name, currentProjectProgress);
                        // Update overall progress whenever any project progress changes
                        this.OnPropertyChanged(nameof(this.OverallProgress));
                        Console.WriteLine($"Project {project.Name} Progress: {currentProjectProgress:F2}%");
                    });

                    tasks.Add(Task.Run(async () =>
                    {
                        bool result = await this.modelBackup.SaveProjectAsync(project.Name, isDifferential, progressReporter);
                        return (project.Name, result);
                    }));
                }

                var results = await Task.WhenAll(tasks);

                // Analyse les résultats
                foreach (var (projectName, result) in results)
                {
                    if (result)
                    {
                        this.SetProjectProgress(projectName, 100);
                        Console.WriteLine($"✅ Project {projectName} saved successfully.");
                        this.logger.Log(new Dictionary<string, string> { { "message", $"Project {projectName} saved successfully." } });
                    }
                    else
                    {
                        this.SetProjectProgress(projectName, 0);
                        Console.WriteLine($"❌ Project {projectName} failed to save.");
                        this.logger.Log(new Dictionary<string, string> { { "message", $"Project {projectName} failed to save." } });
                    }
                }

                // Final overall progress update
                this.OnPropertyChanged(nameof(this.OverallProgress));
                Console.WriteLine("Backup process finished.");
                await this.LoadProjectsAsync("destination");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during batch backup: {ex.Message}");
                foreach (var project in this.SelectedProjects)
                {
                    this.SetProjectProgress(project.Name, 0);
                }
                this.OnPropertyChanged(nameof(this.OverallProgress));
            }
            finally
            {
                this.IsBackupInProgress = false;
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