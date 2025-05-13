using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Services.Logger;
using EasySave.Services.Translation;

namespace EasySave.ViewModels
{
    public class BackupViewModel : ViewModelBase
    {
        private readonly ModelBackup modelBackup;
        private readonly ILogger logger;
        private readonly TranslationService translationService;
        private bool isBackupInProgress;

        public bool IsBackupInProgress
        {
            get => isBackupInProgress;
            set => SetProperty(ref isBackupInProgress, value);
        }

        public BackupViewModel(
            ILogger logger,
            TranslationService translationService)
        {
            this.modelBackup = new ModelBackup();
            this.logger = logger;
            this.translationService = translationService;
        }

        public async Task SaveProjectAsync()
        {
            try
            {
                IsBackupInProgress = true;
                logger.Log(LogLevel.Info, "Starting backup process");
                
                var projects = await modelBackup.GetProjectsAsync();
                if (projects.Count >= 5)
                {
                    logger.Log(LogLevel.Warning, "Maximum number of projects reached");
                    return;
                }

                var project = await modelBackup.CreateProjectAsync();
                await modelBackup.SaveProjectAsync(project);
                
                logger.Log(LogLevel.Info, $"Project saved successfully: {project.Name}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error during backup: {ex.Message}");
                throw;
            }
            finally
            {
                IsBackupInProgress = false;
            }
        }

        public async Task DownloadBackupAsync()
        {
            try
            {
                IsBackupInProgress = true;
                logger.Log(LogLevel.Info, "Starting backup download");

                var projects = await modelBackup.GetProjectsAsync();
                if (projects.Count == 0)
                {
                    logger.Log(LogLevel.Warning, "No projects available to download");
                    return;
                }

                var project = await modelBackup.SelectProjectAsync(projects);
                await modelBackup.DownloadBackupAsync(project);
                
                logger.Log(LogLevel.Info, $"Backup downloaded successfully: {project.Name}");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, $"Error during backup download: {ex.Message}");
                throw;
            }
            finally
            {
                IsBackupInProgress = false;
            }
        }

        public async Task<List<BackupState>> GetBackupStatesAsync()
        {
            return await modelBackup.GetBackupStatesAsync();
        }
    }
} 