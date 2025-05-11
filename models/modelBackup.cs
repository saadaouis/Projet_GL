// <copyright file="modelBackup.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly Dictionary<string, CancellationTokenSource> autoSaveTasks = new();
        private readonly Dictionary<string, BackupState> backupStates = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBackup"/> class.
        /// </summary>
        /// <param name="sourcePath">The source directory path.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        public ModelBackup(string sourcePath, string destinationPath)
        {
            this.sourcePath = sourcePath;
            this.destinationPath = destinationPath;
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
            Console.WriteLine("Starting FetchProjects()...");

            try
            {
                Console.WriteLine($"Fetching directories from: {destinationPath}");

                // Get all directories and order by last write time
                var directories = Directory.GetDirectories(path)
                    .Select(dir => new DirectoryInfo(dir))
                    .OrderByDescending(dir => dir.LastWriteTime)
                    .Take(MaxProjects);

                Console.WriteLine($"Found {directories.Count()} directories.");

                foreach (var dir in directories)
                {
                    Console.WriteLine($"Processing directory: {dir.FullName}");

                try
                {
                    // Calculate directory size
                    double sizeInMB = CalculateDirectorySize(dir) / (1024.0 * 1024.0);
                    Console.WriteLine($"Size of {dir.Name}: {sizeInMB:F2} MB");

                    // Create project object
                    var project = new Project
                    {
                        Name = dir.Name,
                        LastBackup = dir.LastWriteTime,
                        Size = Math.Round(sizeInMB, 2),
                    };

                    projects.Add(project);
                    Console.WriteLine($"Project added: {project.Name}, {project.LastBackup}, {project.Size} MB");
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

            Console.WriteLine($"FetchProjects() completed. Total projects fetched: {projects.Count}");
            return projects;
        }


        /// <summary>Download a backup version.</summary>
        /// <param name="projectNumber">The project number.</param>
        /// <param name="versionNumber">The version number.</param>
        public void DownloadVersion(int projectNumber, int versionNumber)
        {
            string backupPath = Path.Combine(this.sourcePath, $"Project{projectNumber}", $"Version{versionNumber}");
            string activePath = Path.Combine(this.destinationPath, $"Project{projectNumber}");

            if (Directory.Exists(backupPath)) // Ensure the source directory exists
            {
                Directory.CreateDirectory(activePath); // Create the target directory
                foreach (var file in Directory.GetFiles(backupPath))
                {
                    var fileName = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(activePath, fileName), overwrite: true);
                }
            }
            else
            {
                throw new Exception("Backup version not found");
            }
        }

        /// <summary>
        /// Fetches the versions of a project.
        /// </summary>
        /// <param name="projectNumber">The project number.</param>
        /// <returns>A list of versions.</returns>
        public List<string> FetchVersions(int projectNumber)
        {
            var versions = new List<string>();
            var projectPath = Path.Combine(sourcePath, $"Project{projectNumber}");
            if (Directory.Exists(projectPath))
            {
                versions = Directory.GetDirectories(projectPath)
                    .Select(dir => new DirectoryInfo(dir))
                    .OrderByDescending(dir => dir.LastWriteTime)
                    .Select(dir => dir.Name)
                    .ToList();
            }
            
            return versions;
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
        /// Toggles auto-save for a project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="intervalSeconds">The auto-save interval in seconds.</param>
        /// <returns>True if auto-save was enabled, false if it was disabled.</returns>
        public bool ToggleAutoSave(string projectName, int intervalSeconds)
        {
            if (this.autoSaveTasks.ContainsKey(projectName))
            {
                this.StopAutoSave(projectName);
                return false;
            }
            else
            {
                var project = new Project { Name = projectName };
                this.StartAutoSave(new List<Project> { project }, intervalSeconds);
                return true;
            }
        }

        /// <summary>
        /// Saves a project with the specified version number.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="isAutoSave">Whether this is an auto-save operation.</param>
        /// <returns>True if the save was successful, false otherwise.</returns>
        public bool SaveProject(string projectName, bool isAutoSave = false)
        {
            try
            {
                var state = this.GetBackupState(projectName);
                state.CurrentOperation = isAutoSave ? "Auto-saving" : "Backing up";
                state.IsComplete = false;
                state.ErrorMessage = null;
                state.UpdateState();

                string projectDir = Path.Combine(this.destinationPath, projectName);
                string saveTypeDir = isAutoSave ? "updates" : "backups";
                string saveDir = Path.Combine(projectDir, saveTypeDir);
                string sourceDirPath = Path.Combine(this.sourcePath, projectName);

                // Create directories if they don't exist
                Directory.CreateDirectory(saveDir);

                if (isAutoSave)
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

                    // Get the next version number
                    var (major, minor) = GetNextVersionNumber(projectDir, isAutoSave);
                    string versionDir = Path.Combine(saveDir, $"V{major}.{minor}");

                    // Copy only modified files
                    if (!string.IsNullOrEmpty(lastBackupDir))
                    {
                        CopyModifiedFilesWithProgress(sourceDirPath, versionDir, lastBackupDir, state);
                    }
                    else
                    {
                        // If no backup exists, copy everything
                        CopyDirectoryRecursiveWithProgress(sourceDirPath, versionDir, state);
                    }
                }
                else
                {
                    // For manual backups, copy everything
                    var (major, minor) = GetNextVersionNumber(projectDir, isAutoSave);
                    string versionDir = Path.Combine(saveDir, $"V{major}");
                    CopyDirectoryRecursiveWithProgress(sourceDirPath, versionDir, state);
                }

                state.IsComplete = true;
                state.CurrentOperation = "Complete";
                state.UpdateState();
                return true;
            }
            catch (Exception ex)
            {
                var state = this.GetBackupState(projectName);
                state.IsComplete = true;
                state.ErrorMessage = ex.Message;
                state.CurrentOperation = "Error";
                state.UpdateState();
                Console.WriteLine($"Error saving project: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Starts auto-save for a project.
        /// </summary>
        /// <param name="projects">The list of projects to auto-save.</param>
        /// <param name="intervalSeconds">The auto-save interval in seconds.</param>
        public void StartAutoSave(List<ModelBackup.Project> projects, int intervalSeconds)
        {
            foreach (var project in projects)
            {
                if (this.autoSaveTasks.ContainsKey(project.Name))
                {
                    this.StopAutoSave(project.Name);
                }

                var cts = new CancellationTokenSource();
                this.autoSaveTasks[project.Name] = cts;

                Task.Run(
                async () =>
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(intervalSeconds * 1000, cts.Token);
                        if (!cts.Token.IsCancellationRequested)
                        {
                            this.SaveProject(project.Name, true);
                        }
                    }
            },
                cts.Token);
            }
        }

        /// <summary>
        /// Stops auto-save for a project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        public void StopAutoSave(string projectName)
        {
            if (this.autoSaveTasks.TryGetValue(projectName, out var cts))
            {
                cts.Cancel();
                this.autoSaveTasks.Remove(projectName);
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
        /// Copies only modified files with progress tracking.
        /// </summary>
        private static void CopyModifiedFilesWithProgress(string sourceDir, string destDir, string lastBackupDir, BackupState state)
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
        /// Copies a directory recursively with progress tracking.
        /// </summary>
        private static void CopyDirectoryRecursiveWithProgress(string sourceDir, string destDir, BackupState state)
        {
            Directory.CreateDirectory(destDir);

            // Calculate total files and size
            var allFiles = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .ToList();

            state.TotalFiles = allFiles.Count;
            state.TotalSize = allFiles.Sum(f => f.Length);
            state.ProcessedFiles = 0;
            state.ProcessedSize = 0;
            state.UpdateState();

            // Copy all files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);

                var fileInfo = new FileInfo(file);
                state.ProcessedFiles++;
                state.ProcessedSize += fileInfo.Length;
                state.UpdateState();
            }

            // Copy all subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectoryRecursiveWithProgress(subDir, destSubDir, state);
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
