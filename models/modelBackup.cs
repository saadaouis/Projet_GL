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

        /// <summary>
        /// Fetches the most recent projects from the filesystem.
        /// </summary>
        /// <param name="sourcePath">The source directory path.</param>
        /// <param name="destinationPath">The destination directory path.</param>
        /// <returns>A list of projects with their details.</returns>
        public List<Project> FetchProjects(string sourcePath, string destinationPath)
        {
            var projects = new List<Project>();

            try
            {
                // Get all directories and order by last write time
                var directories = Directory.GetDirectories(destinationPath)
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

        /// <summary>Download a backup version.</summary>
        /// <param name="project_number">The project number.</param>
        /// <param name="version_number">The version number.</param>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        public void DownloadVersion(int project_number, int version_number, string sourcePath, string destinationPath)
        {
            string backup_path = Path.Combine(sourcePath, $"Project{project_number}", $"Version{version_number}");
            string active_path = Path.Combine(destinationPath, $"Project{project_number}");

            if (Directory.Exists(backup_path)) // Ensure the source directory exists
            {
                Directory.CreateDirectory(active_path); // Create the target directory
                foreach (var file in Directory.GetFiles(backup_path))
                {
                    var file_name = Path.GetFileName(file);
                    File.Copy(file, Path.Combine(active_path, file_name), overwrite: true);
                }
            }
            else
            {
                throw new Exception("Backup version not found");
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
