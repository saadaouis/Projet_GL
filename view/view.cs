// <copyright file="view.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace EasySave.Views
{
    /// <summary>
    /// View class.
    /// </summary>
    public class View
    {
        private const int MinMenuChoice = 1;
        private const int MaxMenuChoice = 5;
        private const string SeverityInfo = "info";
        private const string SeverityWarning = "warning";
        private const string SeverityError = "error";
        private const string SeverityText = "text";
        private const string LanguageFrench = "Fr";
        private const string LanguageEnglish = "En";

        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        public View()
        {
        }

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

        /// <summary> Display the main menu. </summary>
        /// <returns>The choice of the user.</returns>
        public static int ShowMenu()
        {
            ShowMessage("Main menu", SeverityInfo);
            ShowMessage("1) Download backup", SeverityText);
            ShowMessage("2) Save backup", SeverityText);
            ShowMessage("3) Toogle AutoSave", SeverityText);
            ShowMessage("4) Modify config", SeverityText);
            ShowMessage("5) Exit", SeverityText);

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
        /// Clears the console.
        /// </summary>
        public static void ClearConsole()
        {
            Console.Clear();
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
    }
}