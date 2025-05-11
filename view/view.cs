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
        public static void ShowMessage(string message, string severity = "text")
        {
            var color = ConsoleColor.White;
            switch (severity)
            {
                case "info":
                    color = ConsoleColor.Blue;
                    break;
                case "warning":
                    color = ConsoleColor.Yellow;
                    break;
                case "error":
                    color = ConsoleColor.Red;
                    break;
                case "text":
                    color = ConsoleColor.White;
                    break;
            }

            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Validates if a path exists and is a directory.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <returns>True if the path is valid, false otherwise.</returns>
        private static bool IsValidDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                ShowMessage("Path cannot be empty", "error");
                return false;
            }

            if (!Directory.Exists(path))
            {
                ShowMessage($"Directory does not exist: {path}", "error");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes the configuration form.
        /// </summary>
        /// <returns>A dictionary containing the configuration values.</returns>
        public Dictionary<string, string> InitializeForm()
        {
            ShowMessage("Config initialization", "info");

            // Source folder selection
            string? source;
            do
            {
                ShowMessage("Enter Source Folder path:", "text");
                source = Console.ReadLine();
            } while (!IsValidDirectory(source));

            // Destination folder selection
            string? destination;
            do
            {
                ShowMessage("Enter Destination Folder path:", "text");
                destination = Console.ReadLine();
            } while (!IsValidDirectory(destination));

            // Language selection
            ShowMessage("\nSelect language / Choisir la langue:", "info");
            ShowMessage("1) English", "text");
            ShowMessage("2) Français", "text");

            string? languageChoice = Console.ReadLine();
            string language = languageChoice == "2" ? "Fr" : "En";

            return new Dictionary<string, string>
            {
                { "source", source },
                { "destination", destination },
                { "language", language },
            };
        }
    }
}
