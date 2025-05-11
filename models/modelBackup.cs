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

        public ModelBackup(string sourcePath, string destinationPath)
        {
            this.sourcePath = sourcePath;
            this.destinationPath = destinationPath;
        }

        /// <summary>
        /// Fetches the most recent projects from the filesystem.
        /// </summary>
        /// <param name="sourcePath">The source directory path.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        /// <returns>A list of projects with their details.</returns>
        public List<Project> FetchProjects()
        {
            var projects = new List<Project>();
            Console.WriteLine("Starting FetchProjects()...");

            try
            {
                Console.WriteLine($"Fetching directories from: {destinationPath}");

                // Get all directories and order by last write time
                var directories = Directory.GetDirectories(destinationPath)
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
        /// Saves a project with the specified version number.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="sourcePath">The source directory path.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        /// <param name="isAutoSave">Whether this is an auto-save operation.</param>
        /// <returns>True if the save was successful, false otherwise.</returns>
        public bool SaveProject(string projectName, string sourcePath, string destinationPath, bool isAutoSave = false)
        {
            try
            {
                string projectDir = Path.Combine(destinationPath, projectName);
                string saveTypeDir = isAutoSave ? "updates" : "backups";
                string saveDir = Path.Combine(projectDir, saveTypeDir);

                // Create directories if they don't exist
                Directory.CreateDirectory(saveDir);

                // Get the next version number
                int nextVersion = GetNextVersionNumber(saveDir, isAutoSave);
                string versionDir = Path.Combine(saveDir, $"V{nextVersion}");

                // Copy the project
                CopyDirectoryRecursive(sourcePath, versionDir);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving project: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Starts auto-save for a project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        /// <param name="intervalSeconds">The auto-save interval in seconds.</param>
        public void StartAutoSave(string projectName, int intervalSeconds)
        {
            if (autoSaveTasks.ContainsKey(projectName))
            {
                StopAutoSave(projectName);
            }

            var cts = new CancellationTokenSource();
            autoSaveTasks[projectName] = cts;

            Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(intervalSeconds * 1000, cts.Token);
                    if (!cts.Token.IsCancellationRequested)
                    {
                        SaveProject(projectName, sourcePath, destinationPath, true);
                    }
                }
            }, cts.Token);
        }

        /// <summary>
        /// Stops auto-save for a project.
        /// </summary>
        /// <param name="projectName">The name of the project.</param>
        public void StopAutoSave(string projectName)
        {
            if (autoSaveTasks.TryGetValue(projectName, out var cts))
            {
                cts.Cancel();
                autoSaveTasks.Remove(projectName);
            }
        }

        /// <summary>
        /// Gets the next version number for a project.
        /// </summary>
        private static int GetNextVersionNumber(string saveDir, bool isAutoSave)
        {
            if (!Directory.Exists(saveDir))
            {
                return 1;
            }

            var existingVersions = Directory.GetDirectories(saveDir)
                .Select(dir => Path.GetFileName(dir))
                .Where(name => name.StartsWith("V"))
                .Select(name => int.TryParse(name[1..], out int num) ? num : 0)
                .ToList();

            if (!existingVersions.Any())
            {
                return 1;
            }

            return existingVersions.Max() + 1;
        }

        /// <summary>
        /// Copies a directory recursively.
        /// </summary>
        private static void CopyDirectoryRecursive(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            // Copy all files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);
                File.Copy(file, destFile, true);
            }

            // Copy all subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(destDir, dirName);
                CopyDirectoryRecursive(subDir, destSubDir);
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
            /// Gets or sets whether auto-save is enabled for this project.
            /// </summary>
            public bool AutoSaveEnabled { get; set; } = false;

            /// <summary>
            /// Gets or sets the auto-save interval in seconds.
            /// </summary>
            public int AutoSaveInterval { get; set; } = 300; // Default 5 minutes
        }
    }
}
