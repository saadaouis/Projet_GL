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


        private readonly string destinationDirectory;
        private readonly string sourceDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBackup"/> class.
        /// </summary>
        /// <param name="destinationPath">The path where projects are stored. If null, uses default AppData location.</param>
        /// <param name="sourcePath">The path where projects are stored.</param>
        public ModelBackup(string? destinationPath = null, string? sourcePath = null)
        {
            // Use provided path or default to AppData location
            this.destinationDirectory = destinationPath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "Projects");

            this.sourceDirectory = sourcePath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EasySave",
                "Projects");

            // Ensure the projects directory exists
            if (!Directory.Exists(this.destinationDirectory))
            {
                Directory.CreateDirectory(this.destinationDirectory);
            }
        }

        /// <summary>
        /// Fetches the most recent projects from the filesystem.
        /// </summary>
        /// <returns>A list of projects with their details.</returns>
        public List<Project> FetchProjects()
        {
            var projects = new List<Project>();

            try
            {
                // Get all directories and order by last write time
                var directories = Directory.GetDirectories(this.destinationDirectory)
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
                        var project = new Project(
                            name: dir.Name,
                            lastBackup: dir.LastWriteTime,
                            size: Math.Round(sizeInMB, 2));

                        projects.Add(project);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with other directories
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
        /// Calculates the total size of a directory in bytes.
        /// </summary>
        /// <param name="directory">The directory to calculate size for.</param>
        /// <returns>The total size in bytes.</returns>
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
                        // Skip files we can't access
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
                        // Skip directories we can't access
                        continue;
                    }
                }
            }
            catch (Exception)
            {
                // Return 0 if we can't access the directory
                return 0;
            }

            return size;
        }

        /// <summary>
        /// Represents a backup project with its properties.
        /// </summary>
        ///
        public class Project
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Project"/> class.
            /// </summary>
            /// <param name="name">The name of the project.</param>
            /// <param name="lastBackup">The last backup date of the project.</param>
            /// <param name="size">The size of the project in megabytes.</param>
            public Project(string name, DateTime lastBackup, double size)
            {
                this.Name = name;
                this.LastBackup = lastBackup;
                this.Size = size;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Project"/> class with default values.
            /// </summary>
            public Project()
            {
                this.Name = string.Empty;
                this.LastBackup = DateTime.Now;
                this.Size = 0;
            }

            /// <summary>
            /// Gets or sets the name of the project.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the last backup date of the project.
            /// </summary>
            public DateTime LastBackup { get; set; }

            /// <summary>
            /// Gets or sets the size of the project in megabytes.
            /// </summary>
            public double Size { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Project"/> class.
            /// </summary>
            /// <param name="name">The name of the project.</param>
            /// <param name="lastBackup">The last backup date of the project.</param>
            /// <param name="size">The size of the project in megabytes.</param>
            ///
            /// <summary>
        }
    }
}
