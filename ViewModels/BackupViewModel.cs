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
        private List<ModelBackup.Project> availableProjects;
        private string sourcePath;
        private string destinationPath;

        /// <summary>
        /// Gets the collection of currently selected projects.
        /// </summary>
        public ObservableCollection<ModelBackup.Project> SelectedProjects { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupViewModel"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="translationService">The translation service.</param>
        public BackupViewModel(
            TranslationService translationService)
        {
            this.sourcePath = string.Empty;
            this.destinationPath = string.Empty;

            this.modelBackup = new ModelBackup();
            this.translationService = translationService;

            this.availableProjects = new List<ModelBackup.Project>();
            this.SelectedProjects = new ObservableCollection<ModelBackup.Project>();

            // Initialize commands
            this.RefreshProjectsCommand = new MainViewModel.AsyncRelayCommand(async () => await this.LoadProjectsAsync());
            this.SaveSelectedProjectCommand = new MainViewModel.AsyncRelayCommand(async () => await this.SaveSelectedProjectsAsync());
/*          this.DownloadBackupCommand = new AsyncRelayCommand(async () => await this.DownloadSelectedProjectAsync()); */
        }

        /// <summary>
        /// Gets or sets a value indicating whether the backup is in progress.
        /// </summary>
        public bool IsBackupInProgress
        {
            get => this.isBackupInProgress;
            set => this.SetProperty(ref this.isBackupInProgress, value);
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
        /// Gets the refresh projects command.
        /// </summary>
        public ICommand RefreshProjectsCommand { get; }

        /// <summary>
        /// Gets the save project command.
        /// </summary>
        public ICommand SaveSelectedProjectCommand { get; }

        /// <summary>
        /// Get the backup states.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The backup states.</returns>
        public async Task<BackupState> GetBackupStateAsync(string projectName)
        {
            return await this.modelBackup.GetBackupStateAsync(projectName);
        }

/*         /// <summary>
        /// Gets the download backup command.
        /// </summary>
        public ICommand DownloadBackupCommand { get; } */

        /// <summary>
        /// Loads the projects asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadProjectsAsync()
        {
            try
            {
                this.AvailableProjects = await this.modelBackup.FetchProjectsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading projects: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves the currently selected projects.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SaveSelectedProjectsAsync()
        {
            if (this.SelectedProjects == null || this.SelectedProjects.Count == 0)
            {
                Console.WriteLine("No projects selected to save.");
                return;
            }

            this.IsBackupInProgress = true;
            Console.WriteLine($"Starting backup process for {this.SelectedProjects.Count} project(s).");
            try
            {
                // Basic check, might need refinement based on overall project limit logic
                if (this.AvailableProjects.Count + this.SelectedProjects.Count > 5 && !this.SelectedProjects.All(sp => this.AvailableProjects.Any(ap => ap.Name == sp.Name)))
                {
                    Console.WriteLine("Adding these projects would exceed the maximum number of projects.");
                    // Potentially provide more specific feedback to the user
                    return;
                }

                List<Task<bool>> saveTasks = new List<Task<bool>>();
                foreach (var project in this.SelectedProjects)
                {
                    Console.WriteLine($"Queueing save for project: {project.Name}");
                    // Assuming SaveProjectAsync takes the project name and handles differential flag internally or a default.
                    // If isDifferential needs to be set, it should be a parameter or another property.
                    saveTasks.Add(this.modelBackup.SaveProjectAsync(project.Name, isDifferential: false)); 
                }
                
                bool[] results = await Task.WhenAll(saveTasks);
                
                int successfulSaves = results.Count(r => r);
                Console.WriteLine($"Backup process completed. {successfulSaves} out of {this.SelectedProjects.Count} projects saved successfully.");

                await this.LoadProjectsAsync(); // Refresh the project list
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during batch backup: {ex.Message}");
                // Consider how to report errors for individual project saves if Task.WhenAll throws
            }
            finally
            {
                this.IsBackupInProgress = false;
            }
        }

        // Method to be called from View code-behind to update SelectedProjects
        public void UpdateSelectedProjects(IEnumerable<object> selectedItems)
        {
            this.SelectedProjects.Clear();
            foreach (var item in selectedItems.OfType<ModelBackup.Project>())
            {
                this.SelectedProjects.Add(item);
            }
            Console.WriteLine($"Selected projects updated. Count: {this.SelectedProjects.Count}");
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