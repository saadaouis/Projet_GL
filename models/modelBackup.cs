// <copyright file="modelBackup.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasySave.Models
{
    /// <summary>
    /// Model class for handling backup operations.
    /// </summary>
    public class ModelBackup
    {
        private const int MaxProjects = 5;
        private readonly string sourcePath;
        private readonly string destinationPath;

        /// <summary>
        /// Fetches the most recent projects from the filesystem.
        /// </summary>
        /// <param name="sourcePath">The source directory path.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        /// <returns>A list of projects with their details.</returns>
        public ModelBackup(string sourcePath, string destinationPath)
        {
            this.sourcePath = sourcePath;
            this.destinationPath = destinationPath;
        }

        public ModelBackup()
        {
            this.sourcePath = string.Empty;
            this.destinationPath = string.Empty;
        }

        /// <summary>
        /// Fetches the most recent projects from the filesystem.
        /// </summary>
        /// <returns>A list of projects with their details.</returns>
        public List<ModelBackup.Project> FetchProjects()
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
        }
    }
}
