// <copyright file="view.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using EasySave.Models;

namespace EasySave.Views
{
    /// <summary>
    /// View class.
    /// </summary>
    public class View
    {
        private const int MinMenuChoice = 1;
        private const int MaxMenuChoice = 6;
        private const string SeverityInfo = "info";
        private const string SeverityWarning = "warning";
        private const string SeverityError = "error";
        private const string SeverityText = "text";
        private const string LanguageFrench = "Fr";
        private const string LanguageEnglish = "En";
        private string language = "En";

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        /// <summary>
        /// Displays a message.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="severity">The severity of the message. "info", "warning", "error" "text".</param>
        public static void ShowMessage(string message, string severity = SeverityText)
        {
            var color = ConsoleColor.White;
            switch (severity)
            {
                case SeverityInfo:
                    color = ConsoleColor.Blue;
                    break;
                case SeverityWarning:
                    color = ConsoleColor.Yellow;
                    break;
                case SeverityError:
                    color = ConsoleColor.Red;
                    break;
                case SeverityText:
                    color = ConsoleColor.White;
                    break;
            }

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Displays a list of projects and allows the user to select one.
        /// </summary>
        /// <param name="projects">The list of projects to display.</param>
        /// <returns>The index of the selected project.</returns>
        public static int ShowProjectList(List<ModelBackup.Project> projects)
        {
            Console.WriteLine();

            for (int i = 0; i < projects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {projects[i].Name}");
            }

            int selected;
            while (!int.TryParse(Console.ReadLine(), out selected) || selected < 1 || selected > projects.Count)
            {
                Console.WriteLine("Invalid input. Please enter a number between 1 and 5.");
            }

            return selected - 1;
        }

        /// <summary>
        /// Displays a list of projects and allows the user to select multiple projects.
        /// </summary>
        /// <param name="projects">The list of projects to display.</param>
        /// <returns>A list of indices of the selected projects.</returns>
        public static List<int> ShowMultipleProjectList(List<ModelBackup.Project> projects)
        {
            Console.WriteLine("Select projects (comma-separated numbers, e.g., 1,3,5) :");

            for (int i = 0; i < projects.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {projects[i].Name}");
            }

            List<int> selectedIndices = new List<int>();
            bool validInput = false;

            while (!validInput)
            {
                string input = Console.ReadLine() ?? string.Empty;
                selectedIndices.Clear();
                validInput = true;

                string[] selections = input.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (string selection in selections)
                {
                    if (int.TryParse(selection.Trim(), out int index) && index >= 1 && index <= projects.Count)
                    {
                        selectedIndices.Add(index - 1);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid input: {selection}. Please enter numbers between 1 and {projects.Count}.");
                        validInput = false;
                        break;
                    }
                }

                if (validInput && selectedIndices.Count == 0)
                {
                    Console.WriteLine("You must select at least one project.");
                    validInput = false;
                }
            }

            return selectedIndices;
        }

        /// <summary> Console.WriteLine with color. </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="severity">The severity of the message. "info", "warning", "error" "text".</param>
        /// <returns>The input of the user.</returns>
        public static string ConsoleReadLine(string message, string severity = SeverityText)
        {
            ShowMessage(message, severity);
            while (true)
            {
                var input = Console.ReadLine();
                if (input != null)
                {
                    return input;
                }
                else
                {
                    ShowMessage("Invalid input", SeverityError);
                }
            }
        }

        /// <summary>
        /// Clears the console.
        /// </summary>
        public static void ClearConsole()
        {
            Console.Clear();
        }

        /// <summary>
        /// Displays the backup progress for a project.
        /// </summary>
        /// <param name="state">The backup state to display.</param>
        public static void ShowBackupProgress(BackupState state)
        {
            Console.Clear();
            Console.WriteLine($"Project: {state.ProjectName}");
            Console.WriteLine($"Status: {state.CurrentOperation}");

            if (state.ErrorMessage != null)
            {
                Console.WriteLine($"Error: {state.ErrorMessage}");
                return;
            }

            // File progress bar
            Console.Write("Files: [");
            int fileProgress = (int)(state.ProgressPercentage / 2);
            Console.Write(new string('#', fileProgress));
            Console.Write(new string('-', 50 - fileProgress));
            Console.WriteLine($"] {state.ProcessedFiles}/{state.TotalFiles} files");

            // Size progress bar
            Console.Write("Size:  [");
            int sizeProgress = (int)(state.SizeProgressPercentage / 2);
            Console.Write(new string('#', sizeProgress));
            Console.Write(new string('-', 50 - sizeProgress));
            Console.WriteLine($"] {FormatFileSize(state.ProcessedSize)}/{FormatFileSize(state.TotalSize)}");

            if (state.IsComplete)
            {
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Displays a list of versions and allows the user to select one.
        /// </summary>
        /// <param name="versions">The list of versions to display.</param>
        /// <returns>The index of the selected version.</returns>
        public static int ShowVersionList(List<(string Path, string Version, bool IsUpdate)> versions)
        {
            View.ShowMessage("\nAvailable versions:", SeverityInfo);
            for (int i = 0; i < versions.Count; i++)
            {
                string type = versions[i].IsUpdate ? "Update" : "Backup";
                View.ShowMessage($"{i + 1}. {type} {versions[i].Version}", SeverityText);
            }

            int selected;
            while (!int.TryParse(Console.ReadLine(), out selected) || selected < 1 || selected > versions.Count)
            {
                View.ShowMessage("Invalid selection. Please try again.", SeverityError);
            }

            return selected - 1;
        }

                /// <summary>
        /// Change the language of the application.
        /// </summary>
        /// <param name="language">The language to change to.</param>
        public void ChangeLanguage(string language)
        {
            this.language = language;
        }

                /// <summary> Display the main menu. </summary>
        /// <returns>The choice of the user.</returns>
        public int ShowMenu()
        {
            ShowMessage(this.language == LanguageEnglish ? "Main menu" : "Menu principal", SeverityInfo);
            ShowMessage(this.language == LanguageEnglish ? "1) Download backup" : "1) Sauvegarder une sauvegarde", SeverityText);
            ShowMessage(this.language == LanguageEnglish ? "2) Save backup" : "2) Sauvegarder une sauvegarde", SeverityText);
            ShowMessage(this.language == LanguageEnglish ? "3) Toogle AutoSave" : "3) Activer/Désactiver l'enregistrement automatique", SeverityText);
            ShowMessage(this.language == LanguageEnglish ? "4) Modify config" : "4) Modifier la configuration", SeverityText);
            ShowMessage(this.language == LanguageEnglish ? "5) Toggle console logging" : "5) Activer/Désactiver le journalisation console", SeverityText);
            ShowMessage(this.language == LanguageEnglish ? "6) Exit" : "6) Quitter", SeverityText);

            while (true)
            {
                string? choice = Console.ReadLine();
                if (choice != null && int.TryParse(choice, out int choiceInt) &&
                    choiceInt >= MinMenuChoice && choiceInt <= MaxMenuChoice)
                {
                    return choiceInt;
                }
                else
                {
                    ShowMessage("Invalid choice", SeverityError);
                }
            }
        }

        /// <summary>
        /// Initializes the configuration form.
        /// </summary>
        /// <returns>A dictionary containing the configuration values.</returns>
        public Dictionary<string, string> InitializeForm()
        {
            ShowMessage("Config initialization", SeverityInfo);

            // Source folder selection
            string? source;
            do
            {
                ShowMessage("Enter Source Folder path:", SeverityText);
                source = Console.ReadLine();
            }
            while (!IsValidDirectory(source));

            // Destination folder selection
            string? destination;
            do
            {
                ShowMessage("Enter Destination Folder path:", SeverityText);
                destination = Console.ReadLine();
            }
            while (!IsValidDirectory(destination));

            // Language selection
            ShowMessage("\nSelect language / Choisir la langue:", SeverityInfo);
            ShowMessage("1) English", SeverityText);
            ShowMessage("2) Français", SeverityText);

            string? languageChoice = Console.ReadLine();
            string language = languageChoice == "2" ? LanguageFrench : LanguageEnglish;

            return new Dictionary<string, string>
            {
                { "Source", source ?? string.Empty },
                { "Destination", destination ?? string.Empty },
                { "Language", language },
            };
        }

        /// <summary>
        /// Validates if a path exists and is a directory.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <returns>True if the path is valid, false otherwise.</returns>
        private static bool IsValidDirectory(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                ShowMessage("Path cannot be empty", SeverityError);
                return false;
            }

            if (!Directory.Exists(path))
            {
                ShowMessage($"Directory does not exist: {path}", SeverityError);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Formats a file size in bytes to a human-readable string.
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}