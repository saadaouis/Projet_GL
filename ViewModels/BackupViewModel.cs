// <copyright file="BackupViewModel.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
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
        private ModelBackup.Project? selectedProject;
        private string sourcePath;
        private string destinationPath;

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

            this.modelBackup = new ModelBackup(this.sourcePath, this.destinationPath);
            this.translationService = translationService;

            this.availableProjects = new List<ModelBackup.Project>();

            // Initialize commands
            this.RefreshProjectsCommand = new MainViewModel.AsyncRelayCommand(async () => await this.LoadProjectsAsync());
            this.SaveSelectedProjectCommand = new MainViewModel.AsyncRelayCommand(async () => await this.SaveSelectedProjectAsync());
/*          this.DownloadBackupCommand = new AsyncRelayCommand(async () => await this.DownloadSelectedProjectAsync()); */

            // Load initial projects
            _ = this.LoadProjectsAsync();
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
        /// Gets or sets the source path.
        /// </summary>
        public string SourcePath
        {
            get => this.sourcePath;
            set => this.SetProperty(ref this.sourcePath, value);
        }

        /// <summary>
        /// Gets or sets the destination path.
        /// </summary>
        public string DestinationPath
        {
            get => this.destinationPath;
            set => this.SetProperty(ref this.destinationPath, value);
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
        /// Gets or sets the selected project.
        /// </summary>
        public ModelBackup.Project? SelectedProject
        {
            get => this.selectedProject;
            set => this.SetProperty(ref this.selectedProject, value);
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

        private async Task LoadProjectsAsync()
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
        /// Saves the currently selected project.
        /// </summary>
        /// <param name="isDifferential">Whether to perform a differential backup.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task<bool> SaveSelectedProjectAsync(bool isDifferential = false)
        {
            if (this.SelectedProject == null)
            {
                Console.WriteLine("No project selected");
                return false;
            }

            try
            {
                this.IsBackupInProgress = true;
                Console.WriteLine("Starting backup process");

                if (this.AvailableProjects.Count >= 5)
                {
                    Console.WriteLine("Maximum number of projects reached");
                    return false;
                }

                await this.modelBackup.SaveProjectAsync(this.SelectedProject.Name, isDifferential);
                Console.WriteLine($"Project saved successfully: {this.SelectedProject.Name}");

                // Refresh the project list after saving
                await this.LoadProjectsAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during backup: {ex.Message}");
                return false;
            }
            finally
            {
                this.IsBackupInProgress = false;
            }
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