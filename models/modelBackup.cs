// <copyright file="modelBackup.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CryptoSoftService;
using EasySave.Services.Logging;
using EasySave.Services.ProcessControl;
using EasySave.Services.State;
using Microsoft.Extensions.DependencyInjection;

namespace EasySave.Models
{
    /// <summary>
    /// Model class for handling backup operations.
    /// </summary>
    public class ModelBackup
    {
        private const int MaxProjects = 999;
        private readonly Dictionary<string, CancellationTokenSource> autoSaveTasks = new();
        private readonly Dictionary<string, BackupState> backupStates = new();
        private readonly CryptosoftService cryptosoftService;
        private readonly BackupStateRecorder backupStateRecorder;
        private List<string> forbiddenProcesses = new();
        private readonly Dictionary<string, bool> pausedProjects = new();
        private readonly Dictionary<string, bool> stoppedProjects = new();
        private readonly Dictionary<string, string> currentBackupFolders = new();



        private readonly LoggingService logger;

        private float totalEncryptTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBackup"/> class.
        /// </summary>
        /// <param name="sourcePath">The source directory path.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        public ModelBackup(string sourcePath, string destinationPath)
        {
            this.SourcePath = sourcePath;
            this.DestinationPath = destinationPath;
            this.backupStateRecorder = new BackupStateRecorder();
            this.cryptosoftService = ServiceExtensions.GetService<CryptosoftService>();
            this.logger = App.ServiceProvider!.GetRequiredService<LoggingService>();

            // Load forbidden processes from config
            var config = App.ServiceProvider!.GetRequiredService<ModelConfig>().Load();
            if (!string.IsNullOrWhiteSpace(config._forbiddenProcesses))
            {
                forbiddenProcesses = config._forbiddenProcesses
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .ToList();
            }

            // Load forbidden processes from config
            var config = App.ServiceProvider!.GetRequiredService<ModelConfig>().Load();
            if (!string.IsNullOrWhiteSpace(config._forbiddenProcesses))
            {
                forbiddenProcesses = config._forbiddenProcesses
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .ToList();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBackup"/> class.
        /// </summary>
        public ModelBackup()
        {
            this.SourcePath = string.Empty;
            this.DestinationPath = string.Empty;
            this.backupStateRecorder = new BackupStateRecorder();
            this.cryptosoftService = ServiceExtensions.GetService<CryptosoftService>();
            this.logger = App.ServiceProvider!.GetRequiredService<LoggingService>();

            // Load forbidden processes from config
            var config = App.ServiceProvider!.GetRequiredService<ModelConfig>().Load();
            if (!string.IsNullOrWhiteSpace(config._forbiddenProcesses))
            {
                forbiddenProcesses = config._forbiddenProcesses
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .ToList();
            }

            // Load forbidden processes from config
            var config = App.ServiceProvider!.GetRequiredService<ModelConfig>().Load();
            if (!string.IsNullOrWhiteSpace(config._forbiddenProcesses))
            {
                forbiddenProcesses = config._forbiddenProcesses
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .ToList();
            }
        }

        ///<summary>Gets or sets the source directory path for backups.</summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>Gets or sets the destination directory path for backups.</summary>
        public string DestinationPath { get; set; } = string.Empty;

        /// <summary>
        /// Fetches the most recent projects from the filesystem.
        /// </summary>
        /// <param name="directory">The directory to fetch projects from.</param>
        /// <returns>A list of projects with their details.</returns>
        public async Task<List<Project>> FetchProjectsAsync(string directory = "source")
        {
            string path = string.Empty;

            if (directory == "destination")
            {
                path = this.DestinationPath;
            }
            else
            {
                path = this.SourcePath;
            }

            Console.WriteLine(path);

            var projects = new List<Project>();

            try
            {
                // Get all directories and order by last write time
                var directories = await Task.Run(() => Directory.GetDirectories(path)
                    .Select(dir => new DirectoryInfo(dir))
                    .OrderByDescending(dir => dir.LastWriteTime)
                    .Take(MaxProjects));

                foreach (var dir in directories)
                {
                    try
                    {
                        // Calculate directory size
                        double sizeInMB = await Task.Run(() => CalculateDirectorySize(dir) / (1024.0 * 1024.0));

                        // Create project object
                        var project = new Project
                        {
                            Name = dir.Name,
                            LastBackup = dir.LastWriteTime,                                                                                                                                            
                            Size = Math.Round(sizeInMB, 2),
                            Path = dir.FullName,
                        };

                        projects.Add(project);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"SourcePath: {this.SourcePath}");
                        Console.WriteLine($"DestinationPath: {this.DestinationPath}");
                        Console.WriteLine($"Error processing directory {dir.Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SourcePath: {this.SourcePath}");
                Console.WriteLine($"DestinationPath: {this.DestinationPath}");
                Console.WriteLine($"Error fetching projects: {ex.Message}");
            }

            Console.WriteLine($"FetchProjects() completed. Total projects fetched: {projects.Count}");
            return projects;
        }


        public void PauseProject(string projectName)
        {
            Console.WriteLine($"Pausing project: {projectName}"); // Debug log
            lock (pausedProjects)
            {
                pausedProjects[projectName] = true;
            }
        }

        public void ResumeProject(string projectName)
        {
            Console.WriteLine($"Resuming project: {projectName}"); // Debug log
            lock (pausedProjects)
            {
                pausedProjects[projectName] = false;
                Monitor.PulseAll(pausedProjects);
            }
        }

        public void StopProject(string projectName)
        {
            lock (stoppedProjects) 
            {        
                stoppedProjects[projectName] = true;

                if (currentBackupFolders.TryGetValue(projectName, out var folderToDelete) && Directory.Exists(folderToDelete))
                {
                    try
                    {
                        Console.WriteLine($"Removing incomplete backup: {folderToDelete}");
                        Directory.Delete(folderToDelete, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error cleaning up stopped backup: {ex.Message}");
                    }
                }
            }
            ResumeProject(projectName); // To break out of pause if needed
        }

        private bool IsProjectPaused(string projectName)
        {
            lock (pausedProjects)
            {
                return pausedProjects.TryGetValue(projectName, out var paused) && paused;
            }
        }

        private bool IsProjectStopped(string projectName)
        {
            lock (stoppedProjects) { return stoppedProjects.TryGetValue(projectName, out var s) && s; }
        }

        /// <summary>
        /// Gets the current backup state for a project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>The backup state for the project.</returns>
        public async Task<BackupState> GetBackupStateAsync(string projectName)
        {
            if (!this.backupStates.ContainsKey(projectName))
            {
                this.backupStates[projectName] = new BackupState { ProjectName = projectName };
            }

            return await Task.FromResult(this.backupStates[projectName]);
        }

         /// <summary>
        /// Saves a project with the specified version number.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="isDifferential">Whether this is a differential backup.</param>
        /// <param name="progressReporter">Callback for reporting progress updates (0-100).</param>
        /// <returns>True if the save was successful, false otherwise.</returns>
        public async Task<bool> SaveProjectAsync(string projectName, bool isDifferential = false, IProgress<double>? progressReporter = null)
        {
             // Initialize project state
            lock (pausedProjects)
            {
                pausedProjects[projectName] = false;
            }
            lock (stoppedProjects)
            {
                stoppedProjects[projectName] = false;
            }

            var stopwatch = Stopwatch.StartNew();
            var forbiddenAppManager = new ForbiddenAppManager();

            // V�rifie si un processus bloquant est en cours d'ex�cution
            if (forbiddenAppManager.IsAnyForbiddenAppRunning(out var runningApp))
            {
                // Console visible uniquement si ton projet est en mode Console Application
                Console.WriteLine($"[ALERTE] Le processus interdit '{runningApp}' est en cours d'ex�cution. Fermeture de l'application.");

                Environment.Exit(1);
                return false;
            }

            try
            {
                // Log the start of the backup
                var startLog = new Dictionary<string, string>
                {
                    { "Name", projectName },
                    { "FileSource", Path.Combine(this.SourcePath, projectName) },
                    { "FileTarget", Path.Combine(this.DestinationPath, projectName) },
                    { "FileSize", "0" },
                    { "FileTransferTime", "0" },
                    { "encryptTime", "0" },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                };
                this.logger.Log(startLog);

                var state = await this.GetBackupStateAsync(projectName);
                state.CurrentOperation = isDifferential ? "Differential backup" : "Full backup";
                state.IsComplete = false;
                state.ErrorMessage = null;
                state.ProcessedFiles = 0;
                state.TotalFiles = 0;
                state.ProcessedSize = 0;
                state.TotalSize = 0;
                await state.UpdateStateAsync(); // Initial state update
                progressReporter?.Report(0); // Report initial 0% progress

                // Record initial state
                this.backupStateRecorder.RecordStateAsync(
                    projectName,
                    this.SourcePath,
                    this.DestinationPath,
                    isDifferential,
                    0,
                    0,
                    0,
                    0);

                Console.WriteLine($"Starting {(isDifferential ? "differential" : "full")} backup for project: {projectName}");

                string projectDir = Path.Combine(this.DestinationPath, projectName);
                string saveTypeDir = isDifferential ? "updates" : "backups";
                string saveDir = Path.Combine(projectDir, saveTypeDir);
                string sourceDirPath = Path.Combine(this.SourcePath, projectName);

                // Create directories if they don't exist
                await Task.Run(() => Directory.CreateDirectory(saveDir));
                Console.WriteLine($"Created directory: {saveDir}");

                if (isDifferential)
                {
                    // For differential backup, find the latest backup directory
                    string backupsDir = Path.Combine(projectDir, "backups");
                    string lastBackupDir = string.Empty;

                    if (Directory.Exists(backupsDir))
                    {
                        var backupDirs = await Task.Run(() => Directory.GetDirectories(backupsDir)
                            .Select(dir => new DirectoryInfo(dir))
                            .OrderByDescending(dir => dir.LastWriteTime)
                            .ToList());

                        if (backupDirs.Any())
                        {
                            lastBackupDir = backupDirs.First().FullName;
                            Console.WriteLine($"Found last backup directory: {lastBackupDir}");
                        }
                    }

                    // Check if there are any changes before creating a new update
                    if (!string.IsNullOrEmpty(lastBackupDir) && !HasModifiedFiles(sourceDirPath, lastBackupDir))
                    {
                        Console.WriteLine("No changes detected, skipping differential backup");
                        state.IsComplete = true;
                        state.CurrentOperation = "No changes detected";
                        await state.UpdateStateAsync();
                        progressReporter?.Report(0); // Report 0% on no changes
                        return true;
                    }

                    // Get the next version number
                    var (major, minor) = await Task.Run(() => GetNextVersionNumber(projectDir, isDifferential));
                    string versionDir = Path.Combine(saveDir, $"V{major}.{minor}");
                    Console.WriteLine($"Creating update version: V{major}.{minor}");

                    // Copy only modified files
                    if (!string.IsNullOrEmpty(lastBackupDir))
                    {
                        await Task.Run(() => this.CopyModifiedFilesWithProgressAsync(sourceDirPath, versionDir, lastBackupDir, state, progressReporter, projectName, isDifferential, stopwatch));
                    }
                    else
                    {
                        // If no backup exists, copy everything
                        if (!await Task.Run(() => this.CopyDirectoryRecursiveAsync(sourceDirPath, versionDir, state, progressReporter, projectName, isDifferential, stopwatch)))
                        {
                            progressReporter?.Report(0); // Report 0% on failure if needed
                            return false;
                        }
                    }
                }
                else
                {
                    // For manual backups, copy everything
                    var (major, minor) = GetNextVersionNumber(projectDir, isDifferential);
                    string versionDir = Path.Combine(saveDir, $"V{major}");
                    currentBackupFolders[projectName] = versionDir;
                    if (!await this.CopyDirectoryRecursiveAsync(sourceDirPath, versionDir, state, progressReporter, projectName, isDifferential, stopwatch))
                    {
                        progressReporter?.Report(0); // Report 0% on failure if needed
                        return false;
                    }


                    // Wait a bit to ensure all files are written
                    await Task.Delay(1000);


                    Console.WriteLine($"Verifying files in {versionDir}");
                    if (!Directory.Exists(versionDir))
                    {
                        Console.WriteLine($"Directory {versionDir} does not exist");
                        return false;
                    }

                    var files = Directory.GetFiles(versionDir, "*.*", SearchOption.AllDirectories);
                    if (files.Length == 0)
                    {
                        Console.WriteLine("No files found to encrypt");
                        return true;
                    }

                    Console.WriteLine($"Found {files.Length} files to encrypt");
                    this.totalEncryptTime = 0; // Reset total encryption time
                    foreach (var file in files)
                    {

                        if (IsProjectStopped(projectName))
                        {
                            Console.WriteLine($"Project {projectName} was stopped.");
                            break;
                        }

                        // Pause logic
                        lock (pausedProjects)
                        {
                            while (IsProjectPaused(projectName))
                            {
                                Console.WriteLine($"Project {projectName} is paused, waiting...");
                                Monitor.Wait(pausedProjects);
                                if (IsProjectStopped(projectName)) // Check if stopped while paused
                                {
                                    break;
                                }
                            }
                        }

                        // Check for forbidden process and pause if needed
                        if (this.IsBlockedProcessRunning())
                        {
                            PauseProject(projectName);
                            while (this.IsBlockedProcessRunning())
                            {
                                Thread.Sleep(1000); // Check every second
                            }
                            ResumeProject(projectName);
                        }

                        // Process the file only if not stopped
                        if (!IsProjectStopped(projectName))
                        {
                            if (File.Exists(file))
                            {
                                try
                                {
                                    var encryptTime = Stopwatch.StartNew();
                                    var encrypted = await this.cryptosoftService.Encrypt(file);
                                    encryptTime.Stop();
                                    this.totalEncryptTime += (float)encryptTime.Elapsed.TotalSeconds;
                                    Console.WriteLine($"Encrypted {file}: {encrypted} in {encryptTime.Elapsed.TotalSeconds:F3} seconds");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Failed to encrypt {file}: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"File {file} does not exist");
                            }
                        }
                    }
                }

                Console.WriteLine($"Encryption complete. Total encryption time: {this.totalEncryptTime:F3} seconds");
                
                Console.WriteLine($"Encryption complete. Total encryption time: {this.totalEncryptTime:F3} seconds");
                

                state.IsComplete = true;
                state.CurrentOperation = "Complete";
                await state.UpdateStateAsync();
                progressReporter?.Report(100); // Report final 100% progress

                // Record final state
                this.backupStateRecorder.RecordStateAsync(
                    projectName,
                    this.SourcePath,
                    this.DestinationPath,
                    isDifferential,
                    state.TotalSize,
                    state.TotalFiles - state.ProcessedFiles,
                    (int)stopwatch.Elapsed.TotalSeconds,
                    (float)state.SizeProgressPercentage);

                // Log the completion of the backup
                var endLog = new Dictionary<string, string>
                {
                    { "Name", projectName },
                    { "FileSource", Path.Combine(this.SourcePath, projectName) },
                    { "FileTarget", Path.Combine(this.DestinationPath, projectName) },
                    { "FileSize", state.TotalSize.ToString() },
                    { "FileTransferTime", stopwatch.Elapsed.TotalSeconds.ToString("F3") },
                    { "encryptTime", this.totalEncryptTime.ToString("F3") },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                };
                this.logger.Log(endLog);

                Console.WriteLine($"{(isDifferential ? "Differential" : "Full")} backup completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                var state = await this.GetBackupStateAsync(projectName);
                state.IsComplete = true;
                state.ErrorMessage = ex.Message;
                state.CurrentOperation = "Error";
                await state.UpdateStateAsync();
                progressReporter?.Report(0); // Report 0% on error

                // Record error state
                this.backupStateRecorder.RecordStateAsync(
                    projectName,
                    this.SourcePath,
                    this.DestinationPath,
                    isDifferential,
                    0,
                    0,
                    0,
                    0);

                // Log the error
                var errorLog = new Dictionary<string, string>
                {
                    { "Name", projectName },
                    { "FileSource", Path.Combine(this.SourcePath, projectName) },
                    { "FileTarget", Path.Combine(this.DestinationPath, projectName) },
                    { "FileSize", "0" },
                    { "FileTransferTime", stopwatch.Elapsed.TotalSeconds.ToString("F3") },
                    { "encryptTime", "0" },
                    { "time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                    { "error", ex.Message },
                };
                this.logger.Log(errorLog);

                Console.WriteLine($"Error saving project: {ex.Message}");
                return false;
            }
        }

        /*         /// <summary>
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
                } */

       /*  /// <summary>
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
 */
/*          /// <summary>
        /// Gets all available versions for a project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <returns>A list of versions with their paths and types.</returns>
        public List<(string Path, string Version, bool IsUpdate)> FetchVersions(string projectName)
        {
            var versions = new List<(string Path, string Version, bool IsUpdate)>();
            string projectPath = Path.Combine(this.destinationPath, projectName);

            // Get major versions
            string backupsDir = Path.Combine(projectPath, "backups");
            if (Directory.Exists(backupsDir))
            {
                foreach (var dir in Directory.GetDirectories(backupsDir))
                {
                    string version = Path.GetFileName(dir);
                    if (version.StartsWith("V"))
                    {
                        versions.Add((dir, version, false));
                    }
                }
            }

            // Get updates
            string updatesDir = Path.Combine(projectPath, "updates");
            if (Directory.Exists(updatesDir))
            {
                foreach (var dir in Directory.GetDirectories(updatesDir))
                {
                    string version = Path.GetFileName(dir);
                    if (version.StartsWith("V"))
                    {
                        versions.Add((dir, version, true));
                    }
                }
            }

            // Sort versions
            versions.Sort((a, b) =>
            {
                var aParts = a.Version.Split('.');
                var bParts = b.Version.Split('.');

                int aMajor = int.Parse(aParts[0].Substring(1));
                int bMajor = int.Parse(bParts[0].Substring(1));

                if (aMajor != bMajor)
                {
                    return aMajor.CompareTo(bMajor);
                }

                if (a.IsUpdate && b.IsUpdate)
                {
                    int aMinor = int.Parse(aParts[1]);
                    int bMinor = int.Parse(bParts[1]);
                    return aMinor.CompareTo(bMinor);
                }

                return a.IsUpdate ? 1 : -1;
            });

            return versions;
        } */

        /* /// <summary>
        /// Downloads a specific version of a project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="versionPath">The path to the version to download.</param>
        /// <param name="isUpdate">Whether this is an update version.</param>
        /// <param name="state">The backup state to update progress.</param>
        /// <returns>True if the download was successful, false otherwise.</returns>
        public bool DownloadVersion(string projectName, string versionPath, bool isUpdate, BackupState state)
        {
            try
            {
                string targetPath = Path.Combine(this.sourcePath, projectName);

                if (isUpdate)
                {
                    string majorVersion = Path.GetFileName(versionPath).Split('.')[0];
                    string majorVersionPath = Path.Combine(this.destinationPath, projectName, "backups", majorVersion);

                    if (!Directory.Exists(majorVersionPath))
                    {
                        state.ErrorMessage = $"Major version {majorVersion} not found.";
                        return false;
                    }

                    state.CurrentOperation = $"Downloading major version {majorVersion}";
                    state.UpdateState();

                    if (!this.CopyDirectoryRecursive(majorVersionPath, targetPath, state))
                    {
                        return false;
                    }
                }

                state.CurrentOperation = $"Downloading version {Path.GetFileName(versionPath)}";
                state.UpdateState();

                if (!this.CopyDirectoryRecursive(versionPath, targetPath, state))
                {
                    return false;
                }

                state.IsComplete = true;
                state.CurrentOperation = "Download complete";
                state.UpdateState();
                return true;
            }
            catch (Exception ex)
            {
                state.IsComplete = true;
                state.ErrorMessage = ex.Message;
                state.CurrentOperation = "Error";
                state.UpdateState();
                return false;
            }
        } */

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
        /// Saves a project with the specified version number.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="isDifferential">Whether this is a differential backup.</param>
        /// <param name="progressReporter">Callback for reporting progress updates (0-100).</param>
        /// <returns>True if the save was successful, false otherwise.</returns>
        /// 
        private bool IsBlockedProcessRunning()
        {
            return Process.GetProcesses().Any(p =>
            {
                try 
                {
                    return forbiddenProcesses.Contains(p.ProcessName.ToLower()); 
                    return forbiddenProcesses.Contains(p.ProcessName.ToLower()); 
                }
                catch 
                {
                    return false; 
                }
            });
        }

        /// <summary>
        /// Copies only modified files with progress tracking.
        /// </summary>
        private async Task CopyModifiedFilesWithProgressAsync(string sourceDir, string destDir, string lastBackupDir, BackupState state, IProgress<double>? progressReporter = null, string? projectName = null, bool isDifferential = false, Stopwatch? stopwatch = null)
        {
            try
            {
                Directory.CreateDirectory(destDir);
                var sourceDirectoryInfo = new DirectoryInfo(sourceDir);
                var allSourceFiles = await Task.Run(() => 
                    sourceDirectoryInfo.GetFiles("*.*", SearchOption.AllDirectories).ToList());

                var modifiedFiles = new List<FileInfo>();
                foreach (var sourceFile in allSourceFiles)
                {
                    //Add checks for stop and pause functions
                    if (projectName != null && IsProjectStopped(projectName))
                        return;

                    if (projectName != null)
                    {
                        lock (pausedProjects)
                        {
                            while (IsProjectPaused(projectName))
                            {
                                Monitor.Wait(pausedProjects);
                                if (IsProjectStopped(projectName))
                                {
                                    return;
                                }
                            }
                        }
                    }

                    string relativePath = Path.GetRelativePath(sourceDir, sourceFile.FullName);
                    string lastBackupFilePath = Path.Combine(lastBackupDir, relativePath);
                    if (!File.Exists(lastBackupFilePath))
                    {
                        modifiedFiles.Add(sourceFile);
                    }
                    else
                    {
                        var lastBackupFileInfo = new FileInfo(lastBackupFilePath);
                        if (sourceFile.LastWriteTime > lastBackupFileInfo.LastWriteTime || sourceFile.Length != lastBackupFileInfo.Length)
                        {
                            modifiedFiles.Add(sourceFile);
                        }
                    }
                }

                state.TotalFiles = modifiedFiles.Count;
                state.TotalSize = modifiedFiles.Sum(f => f.Length);
                state.ProcessedFiles = 0;
                state.ProcessedSize = 0;
                await state.UpdateStateAsync();
                progressReporter?.Report(0); 
                if (state.TotalFiles == 0)
                {
                    progressReporter?.Report(100); // Report 100% as no work needed
                    return;
                }
                
                foreach (var sourceFile in modifiedFiles)
                {
                    string relativePath = Path.GetRelativePath(sourceDir, sourceFile.FullName);
                    string destFile = Path.Combine(destDir, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                    await Task.Run(() => sourceFile.CopyTo(destFile, true));
                    state.ProcessedFiles++;
                    state.ProcessedSize += sourceFile.Length;
                    progressReporter?.Report(state.SizeProgressPercentage);
                    await state.UpdateStateAsync();

                    // Record state after each file
                    if (projectName != null && stopwatch != null)
                    {
                        this.backupStateRecorder.RecordStateAsync(
                            projectName,
                            this.SourcePath,
                            this.DestinationPath,
                            isDifferential,
                            state.TotalSize,
                            state.TotalFiles - state.ProcessedFiles,
                            (int)stopwatch.Elapsed.TotalSeconds,
                            (float)state.SizeProgressPercentage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CopyModifiedFilesWithProgressAsync: {ex.Message}");
                state.ErrorMessage = $"Error during differential copy: {ex.Message}";
                progressReporter?.Report(state.TotalSize > 0 ? state.SizeProgressPercentage : 0);
                await state.UpdateStateAsync(); 
                throw; // Re-throw to be caught by SaveProjectAsync
            }
        }

        private async Task<bool> CopyDirectoryRecursiveAsync(string sourceDir, string targetDir, BackupState state, IProgress<double>? progressReporter = null, string? projectName = null, bool isDifferential = false, Stopwatch? stopwatch = null)
        {
            try
            {
                Directory.CreateDirectory(targetDir);
                var sourceDirectoryInfo = new DirectoryInfo(sourceDir);
                if (state.TotalFiles == 0 && state.TotalSize == 0)
                {
                    var allFilesForSizeCalc = sourceDirectoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
                    state.TotalFiles = allFilesForSizeCalc.Length;
                    state.TotalSize = allFilesForSizeCalc.Sum(f => f.Length);
                    state.ProcessedFiles = 0;
                    state.ProcessedSize = 0;
                    progressReporter?.Report(0); 
                    await state.UpdateStateAsync();
                }

                foreach (var fileInfo in sourceDirectoryInfo.GetFiles())
                {
                    //Add checks for pause and stop functions
                    if (projectName != null && IsProjectStopped(projectName))
                    {
                        return false;
                    }

                    if (projectName != null)
                    {
                        lock (pausedProjects)
                        {
                            while (IsProjectPaused(projectName))
                            {
                                Monitor.Wait(pausedProjects);
                                if (IsProjectStopped(projectName))
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    string targetFilePath = Path.Combine(targetDir, fileInfo.Name);
                    await Task.Run(() => fileInfo.CopyTo(targetFilePath, true));
                    state.ProcessedFiles++;
                    state.ProcessedSize += fileInfo.Length;
                    progressReporter?.Report(state.SizeProgressPercentage);
                    await state.UpdateStateAsync();

                    // Record state after each file
                    if (projectName != null && stopwatch != null)
                    {
                        this.backupStateRecorder.RecordStateAsync(
                            projectName,
                            this.SourcePath,
                            this.DestinationPath,
                            isDifferential,
                            state.TotalSize,
                            state.TotalFiles - state.ProcessedFiles,
                            (int)stopwatch.Elapsed.TotalSeconds,
                            (float)state.SizeProgressPercentage);
                    }
                }

                foreach (var subDir in sourceDirectoryInfo.GetDirectories())
                {
                    string dirName = Path.GetFileName(subDir.FullName);
                    string destSubDir = Path.Combine(targetDir, dirName);
                    if (!await this.CopyDirectoryRecursiveAsync(subDir.FullName, destSubDir, state, progressReporter, projectName, isDifferential, stopwatch))
                    {
                        return false;
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error copying directory: {ex.Message}");
                state.ErrorMessage = $"Error copying: {ex.Message}";
                state.IsComplete = true;
                progressReporter?.Report(state.SizeProgressPercentage);
                await state.UpdateStateAsync();
                return false;
            }
        }

        /// <summary>
        /// Represents a backup project with its properties.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="lastBackup">The last backup date of the project.</param>
        /// <param name="size">The size of the project.</param>
        /// <param name="path">The path of the project.</param>
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
            /// Gets or sets the path of the project.
            /// </summary>
            public string Path { get; set; } = string.Empty;

/* 
            /// <summary>
            /// Gets or sets a value indicating whether gets or sets whether auto-save is enabled for this project.
            /// </summary>
            public bool AutoSaveEnabled { get; set; } = false;
            /// <summary>
            /// Gets or sets the auto-save interval in seconds.
            /// </summary>
            public int AutoSaveInterval { get; set; } = 300; // Default 5 minutes */
        }
    }
}
