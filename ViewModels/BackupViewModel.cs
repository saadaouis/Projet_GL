// <copyright file="BackupViewModel.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using EasySave.Models;
using EasySave.Services.Translation;

namespace EasySave.ViewModels
{
    /// <summary>
    /// The view model for the backup view.
    /// </summary>
    public class BackupViewModel : ViewModelBase
    {
        private readonly ModelBackup modelBackup;
        private readonly TranslationService translationService;
        private bool isBackupInProgress;
        private double overallProgress;
        private List<ModelBackup.Project> availableProjects;
        private List<ModelBackup.Project> availableBackups;
        private string sourcePath;
        private string destinationPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupViewModel"/> class.
        /// </summary>
        /// <param name="translationService">The translation service.</param>
        public BackupViewModel(TranslationService translationService)
            : base(translationService)
        {
            this.sourcePath = string.Empty;
            this.destinationPath = string.Empty;

            this.modelBackup = new ModelBackup();
            this.translationService = translationService;

            this.availableProjects = new List<ModelBackup.Project>();
            this.availableBackups = new List<ModelBackup.Project>();
            this.SelectedProjects = new ObservableCollection<ModelBackup.Project>();

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
                Console.WriteLine($"Setting DestinationPath: {value}");
                if (this.SetProperty(ref this.destinationPath, value))
                {
                    this.destinationPath = value;
                    Console.WriteLine($"DestinationPath set to: {this.destinationPath}");
                    this.modelBackup.DestinationPath = value;
                    Console.WriteLine($"ModelBackup DestinationPath set to: {this.modelBackup.DestinationPath}");
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
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SaveSelectedProjectsAsync(bool isDifferential = false)
        {
            if (this.SelectedProjects == null || this.SelectedProjects.Count == 0)
            {
                Console.WriteLine("No projects selected to save.");
                return;
            }

            this.IsBackupInProgress = true;
            this.OverallProgress = 0; // Reset overall progress
            Console.WriteLine($"Starting backup process for {this.SelectedProjects.Count} project(s).");
            
            int totalProjectsToBackup = this.SelectedProjects.Count;
            int projectsBackedUpSoFar = 0;
            bool allSucceeded = true;

            try
            {
                // Basic check, might need refinement based on overall project limit logic
                // This check should ideally be done before setting IsBackupInProgress to true
                if (this.AvailableProjects.Count + this.SelectedProjects.Count > 5 && !this.SelectedProjects.All(sp => this.AvailableProjects.Any(ap => ap.Name == sp.Name)))
                {
                    Console.WriteLine("Adding these projects would exceed the maximum number of projects.");

                    // Potentially provide more specific feedback to the user
                    this.IsBackupInProgress = false; // Ensure progress bar is hidden
                    return;
                }

                foreach (var project in this.SelectedProjects)
                {
                    Console.WriteLine($"Starting save for project: {project.Name}");
                    
                    var singleProjectProgressReporter = new Progress<double>(currentProjectProgress =>
                    {
                        // currentProjectProgress is 0-100 for the individual project
                        this.OverallProgress = ((projectsBackedUpSoFar * 100.0) + currentProjectProgress) / totalProjectsToBackup;
                        Console.WriteLine($"Overall Progress: {this.OverallProgress:F2}%, Current Project ({project.Name}): {currentProjectProgress:F2}%");
                    });

                    bool success = await this.modelBackup.SaveProjectAsync(project.Name, isDifferential, singleProjectProgressReporter);
                    if (success)
                    {
                        projectsBackedUpSoFar++;
                        Console.WriteLine($"Project {project.Name} saved successfully.");
                    }
                    else
                    {
                        allSucceeded = false;
                        Console.WriteLine($"Project {project.Name} failed to save.");

                        // Optionally, decide if you want to stop on first failure or continue
                        // break; // Uncomment to stop on first failure
                    }

                    // Ensure overall progress reflects full completion of this project if successful
                    this.OverallProgress = (projectsBackedUpSoFar * 100.0) / totalProjectsToBackup;
                }
                
                Console.WriteLine($"Backup process finished. {projectsBackedUpSoFar} out of {totalProjectsToBackup} projects processed.");
                if (allSucceeded)
                {
                    this.OverallProgress = 100; // Ensure it hits 100% if all successful
                }

                await this.LoadProjectsAsync("destination"); // Refresh the project list
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during batch backup: {ex.Message}");
                allSucceeded = false; // Mark as failed

                // OverallProgress will be at its last reported state or could be set to 0 or a specific error indication
            }
            finally
            {
                this.IsBackupInProgress = false;
                if (!allSucceeded && totalProjectsToBackup > 0) 
                {
                    // If not all succeeded, ensure progress isn't stuck at 100%
                    // It will be at the progress of the last successfully completed segment + partial of failed one.
                    // Or, explicitly set to 0 or an error value if preferred.
                    // For now, it will reflect the actual progress made before failure.
                }
                else if (totalProjectsToBackup == 0) 
                {
                  this.OverallProgress = 0; // No projects, no progress.
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




        /* private async Task DownloadSelectedProjectAsync()
        // {
        //     if (this.SelectedProject == null)
        //     {
        //         this.logger.Log("No project selected", "warning");
        //         return;
        //     }

        //     try
        //     {
        //         this.IsBackupInProgress = true;
        //         this.logger.Log("Starting backup download", "info");

        //         await this.modelBackup.DownloadBackupAsync(this.SelectedProject);
        //         this.logger.Log($"Backup downloaded successfully: {this.SelectedProject.Name}", "info");
        //     }
        //     catch (Exception ex)
        //     {
        //         this.logger.Log($"Error during backup download: {ex.Message}", "error");
        //         throw;
        //     }
        //     finally
        //     {
        //         this.IsBackupInProgress = false;
        //     }
        // }*/
    }
}