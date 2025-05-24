// <copyright file="modelBackup.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Services.Logging;
using EasySave.Services.State;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.Models
{
    /// <summary>
    /// Model class for handling backup operations.
    /// </summary>
    public class ModelBackup
    {
        private const int MaxProjects = 5;
        private readonly string sourcePath = string.Empty;
        private readonly string destinationPath = string.Empty;

        private readonly LoggingService loggingService;
        private readonly StateService stateService;
        private readonly Dictionary<string, CancellationTokenSource> autoSaveTasks = new();
        private readonly Dictionary<string, BackupState> backupStates = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBackup"/> class.
        /// </summary>
        /// <param name="sourcePath">The source directory path.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        /// <param name="logger">The logger instance.</param>
        public ModelBackup(string sourcePath, string destinationPath)
        {
            this.sourcePath = sourcePath;
            this.destinationPath = destinationPath;
            this.loggingService = Program.ServiceExtensions.GetService<LoggingService>();
            this.stateService = Program.ServiceExtensions.GetService<StateService>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBackup"/> class.
        /// </summary>
        public ModelBackup()
        {
            this.sourcePath = string.Empty;
            this.destinationPath = string.Empty;
            this.loggingService = Program.ServiceExtensions.GetService<LoggingService>();
            this.stateService = Program.ServiceExtensions.GetService<StateService>();
        }

        /// <summary>
        /// Fetches the most recent projects from the filesystem.
        /// </summary>
        /// <param name="directory">The directory to fetch projects from.</param>
        /// <returns>A list of projects with their details.</returns>
        public List<Project> FetchProjects(string directory = "destination")
        {
            string path = string.Empty;

            if (directory == "destination")
            {
                path = this.destinationPath;
            }
            else
            {
                path = this.sourcePath;
            }

            var projects = new List<Project>();

            try
            {
                // Get all directories and order by last write time
                var directories = Directory.GetDirectories(path)
                    .Select(dir => new DirectoryInfo(dir))
                    .OrderByDescending(dir => dir.LastWriteTime)
                    .Take(MaxProjects);
                foreach (var dir in directories)
                {
                    try
                    {
                        // Calculate directory size
                        double sizeInMB = CalculateDirectorySize(dir) / (1024.0 * 1024.0);

                        // Create project object
                        var project = new Project
                        {
                            Name = dir.Name,
                            LastBackup = dir.LastWriteTime,
                            Size = Math.Round(sizeInMB, 2),
                        };

                        projects.Add(project);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing directory {dir.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching projects: {ex.Message}");
            }

            return projects;
        }

        /// <summary>
        /// Gets the current backup state for a project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The backup state for the project.</returns>
        public BackupState GetBackupState(string projectName)
        {
            if (!this.backupStates.ContainsKey(projectName))
            {
                this.backupStates[projectName] = new BackupState { ProjectName = projectName };
            }

            return this.backupStates[projectName];
        }

        /// <summary>
        /// Saves a project with the specified version number.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="isDifferential">Whether this is an differential backup operation.</param>
        /// <returns>True if the save was successful, false otherwise.</returns>
        public bool SaveProject(string projectName, bool isDifferential = false)
        {
            try
            {
                var state = this.GetBackupState(projectName);
                state.CurrentOperation = isDifferential ? "Differential backup" : "Backup";
                state.IsComplete = false;
                state.ErrorMessage = null;
                state.UpdateState();

                string projectDir = Path.Combine(this.destinationPath, projectName);
                string saveTypeDir = isDifferential ? "updates" : "backups";
                string saveDir = Path.Combine(projectDir, saveTypeDir);
                string sourceDirPath = Path.Combine(this.sourcePath, projectName);

                // Create directories if they don't exist
                Directory.CreateDirectory(saveDir);

                if (isDifferential)
                {
                    // For auto-save, find the latest backup directory
                    string backupsDir = Path.Combine(projectDir, "backups");
                    string lastBackupDir = string.Empty;

                    if (Directory.Exists(backupsDir))
                    {
                        var backupDirs = Directory.GetDirectories(backupsDir)
                            .Select(dir => new DirectoryInfo(dir))
                            .OrderByDescending(dir => dir.LastWriteTime)
                            .ToList();

                        if (backupDirs.Any())
                        {
                            lastBackupDir = backupDirs.First().FullName;
                        }
                    }

                    // Check if there are any changes before creating a new update
                    if (!string.IsNullOrEmpty(lastBackupDir) && !HasModifiedFiles(sourceDirPath, lastBackupDir))
                    {
                        state.IsComplete = true;
                        state.CurrentOperation = "No changes detected";
                        state.UpdateState();
                        return true;
                    }
                }

                // Get all files from source directory
                var sourceFiles = Directory.GetFiles(sourceDirPath, "*", SearchOption.AllDirectories)
                    .Select(f => new FileInfo(f))
                    .ToList();

                state.TotalFiles = sourceFiles.Count;
                state.TotalSize = sourceFiles.Sum(f => f.Length);
                state.ProcessedFiles = 0;
                state.ProcessedSize = 0;
                state.UpdateState();

                // Initialize state tracking
                this.stateService.UpdateState(
                    projectName,
                    sourceDirPath,
                    saveDir,
                    sourceFiles.Count,
                    state.TotalSize,
                    sourceFiles.Count,
                    0
                );

                var startTime = DateTime.Now;
                long totalSize = 0;
                int lastProgressUpdate = 0;

                foreach (var sourceFile in sourceFiles)
                {
                    string relativePath = sourceFile.FullName.Substring(sourceDirPath.Length).TrimStart(Path.DirectorySeparatorChar);
                    string destFile = Path.Combine(saveDir, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                    File.Copy(sourceFile.FullName, destFile, true);

                    totalSize += sourceFile.Length;
                    state.ProcessedFiles++;
                    state.ProcessedSize += sourceFile.Length;
                    state.UpdateState();

                    // Update state every 20% progress
                    int currentProgress = (int)((double)state.ProcessedFiles / state.TotalFiles * 100);
                    if (currentProgress >= lastProgressUpdate + 20)
                    {
                        lastProgressUpdate = currentProgress;
                        this.stateService.UpdateState(
                            projectName,
                            sourceDirPath,
                            saveDir,
                            state.TotalFiles,
                            state.TotalSize,
                            state.TotalFiles - state.ProcessedFiles,
                            currentProgress
                        );
                    }
                }

                var endTime = DateTime.Now;
                var transferTime = (endTime - startTime).TotalSeconds;

                // Log the entire backup operation
                var logData = new Dictionary<string, string>
                {
                    { "Name", projectName },
                    { "FileSource", sourceDirPath },
                    { "FileTarget", saveDir },
                    { "FileSize", totalSize.ToString() },
                    { "SaveType", isDifferential ? "Differential" : "Full" },
                    { "FileTransferTime", transferTime.ToString("F3") },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
                };

                this.loggingService.Log(logData);

                // Mark the backup as complete in state tracking
                this.stateService.CompleteState(projectName);

                state.IsComplete = true;
                state.UpdateState();
                return true;
            }
            catch (Exception ex)
            {
                var state = this.GetBackupState(projectName);
                state.IsComplete = true;
                state.ErrorMessage = ex.Message;
                state.UpdateState();
                return false;
            }
        }

        /// <summary>
        /// Calculates the total size of a directory in bytes.
        /// </summary>
        private static long CalculateDirectorySize(DirectoryInfo directory)
        {
            long size = 0;

            try
            {
                // Add size of all files in the directory
                foreach (var file in directory.GetFiles())
                {
                    try
                    {
                        size += file.Length;
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                // Recursively add size of all subdirectories
                foreach (var subDir in directory.GetDirectories())
                {
                    try
                    {
                        size += CalculateDirectorySize(subDir);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                return 0;
            }

            return size;
        }

        /// <summary>
        /// Checks if there are any modified files compared to the last backup.
        /// </summary>
        private static bool HasModifiedFiles(string sourceDir, string lastBackupDir)
        {
            var sourceFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .ToList();

            foreach (var sourceFile in sourceFiles)
            {
                string relativePath = sourceFile.FullName.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                string lastBackupFile = Path.Combine(lastBackupDir, relativePath);

                if (!File.Exists(lastBackupFile))
                {
                    return true; // New file found
                }

                var lastBackupInfo = new FileInfo(lastBackupFile);
                if (sourceFile.LastWriteTime > lastBackupInfo.LastWriteTime ||
                    sourceFile.Length != lastBackupInfo.Length)
                {
                    return true; // Modified file found
                }
            }

            return false; // No changes found
        }

        /// <summary>
        /// Gets the next version number for a project.
        /// </summary>
        private static (int Major, int Minor) GetNextVersionNumber(string projectDir, bool isAutoSave)
        {
            string backupsDir = Path.Combine(projectDir, "backups");
            string updatesDir = Path.Combine(projectDir, "updates");

            // Get latest major version from backups
            int latestMajor = 0;
            if (Directory.Exists(backupsDir))
            {
                var backupVersions = Directory.GetDirectories(backupsDir)
                    .Select(dir => Path.GetFileName(dir))
                    .Where(name => name.StartsWith("V"))
                    .Select(name => int.TryParse(name[1..], out int num) ? num : 0)
                    .ToList();

                if (backupVersions.Any())
                {
                    latestMajor = backupVersions.Max();
                }
            }

            // Get latest minor version from updates
            int latestMinor = 0;
            if (Directory.Exists(updatesDir))
            {
                var updateVersions = Directory.GetDirectories(updatesDir)
                    .Select(dir => Path.GetFileName(dir))
                    .Where(name => name.StartsWith($"V{latestMajor}."))
                    .Select(name => int.TryParse(name.Split('.')[1], out int num) ? num : 0)
                    .ToList();

                if (updateVersions.Any())
                {
                    latestMinor = updateVersions.Max();
                }
            }

            if (isAutoSave)
            {
                return (latestMajor, latestMinor + 1);
            }
            else
            {
                return (latestMajor + 1, 0);
            }
        }

        /// <summary>
        /// Copies a directory recursively with progress tracking.
        /// </summary>
        private bool CopyDirectoryRecursive(string sourceDir, string targetDir, BackupState state)
        {
            try
            {
                Directory.CreateDirectory(targetDir);

                var allFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)
                    .Select(f => new FileInfo(f))
                    .ToList();

                state.TotalFiles = allFiles.Count;
                state.TotalSize = allFiles.Sum(f => f.Length);
                state.ProcessedFiles = 0;
                state.ProcessedSize = 0;
                state.UpdateState();

                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    string fileName = Path.GetFileName(file);
                    string destFile = Path.Combine(targetDir, fileName);
                    File.Copy(file, destFile, true);

                    var fileInfo = new FileInfo(file);
                    state.ProcessedFiles++;
                    state.ProcessedSize += fileInfo.Length;
                    state.UpdateState();
                }

                foreach (string subDir in Directory.GetDirectories(sourceDir))
                {
                    string dirName = Path.GetFileName(subDir);
                    string destSubDir = Path.Combine(targetDir, dirName);
                    if (!this.CopyDirectoryRecursive(subDir, destSubDir, state))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                state.ErrorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Copies only modified files with progress tracking.
        /// </summary>
        private void CopyModifiedFilesWithProgress(string sourceDir, string destDir, string lastBackupDir, BackupState state)
        {
            Directory.CreateDirectory(destDir);

            // Get all files from source directory
            var sourceFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .ToList();

            // Filter only modified files
            var modifiedFiles = sourceFiles.Where(sourceFile =>
            {
                string relativePath = sourceFile.FullName.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                string lastBackupFile = Path.Combine(lastBackupDir, relativePath);

                if (!File.Exists(lastBackupFile))
                {
                    return true;
                }

                var lastBackupInfo = new FileInfo(lastBackupFile);
                return sourceFile.LastWriteTime > lastBackupInfo.LastWriteTime ||
                    sourceFile.Length != lastBackupInfo.Length;
            }).ToList();

            state.TotalFiles = modifiedFiles.Count;
            state.TotalSize = modifiedFiles.Sum(f => f.Length);
            state.ProcessedFiles = 0;
            state.ProcessedSize = 0;
            state.UpdateState();

            foreach (var sourceFile in modifiedFiles)
            {
                string relativePath = sourceFile.FullName.Substring(sourceDir.Length).TrimStart(Path.DirectorySeparatorChar);
                string destFile = Path.Combine(destDir, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(sourceFile.FullName, destFile, true);

                state.ProcessedFiles++;
                state.ProcessedSize += sourceFile.Length;
                state.UpdateState();
            }
        }

        /// <summary>
        /// Represents a backup project with its properties.
        /// </summary>
        public class Project
        {
            /// <summary>
            /// Gets or sets the name of the project.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the last backup date of the project.
            /// </summary>
            public DateTime LastBackup { get; set; } = DateTime.Now;

            /// <summary>
            /// Gets or sets the size of the project.
            /// </summary>
            public double Size { get; set; } = 0;

            /// <summary>
            /// Gets or sets a value indicating whether gets or sets whether auto-save is enabled for this project.
            /// </summary>
            public bool AutoSaveEnabled { get; set; } = false;

            /// <summary>
            /// Gets or sets the auto-save interval in seconds.
            /// </summary>
            public int AutoSaveInterval { get; set; } = 300; // Default 5 minutes
        }
    }
}
