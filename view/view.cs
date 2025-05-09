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
        /// Initializes the configuration form.
        /// </summary>
        /// <returns>A dictionary containing the configuration values.</returns>
        public Dictionary<string, string> InitializeForm()
        {
            ShowMessage("Config initialization", "info");

            // Source folder selection
            string? source = ShowFolderDialog("Select Source Folder");
            if (string.IsNullOrEmpty(source))
            {
                ShowMessage("Source folder selection cancelled", "error");
                return new Dictionary<string, string>();
            }

            // Destination folder selection
            string? destination = ShowFolderDialog("Select Destination Folder");
            if (string.IsNullOrEmpty(destination))
            {
                ShowMessage("Destination folder selection cancelled", "error");
                return new Dictionary<string, string>();
            }

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

        /// <summary>
        /// Shows a folder selection dialog.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <returns>The selected folder path or null if cancelled.</returns>
        private static string? ShowFolderDialog(string title)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows implementation using PowerShell
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-command \"Add-Type -AssemblyName System.Windows.Forms; $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog; $folderBrowser.Description = '{title}'; $folderBrowser.ShowNewFolderButton = $true; if($folderBrowser.ShowDialog() -eq 'OK') {{ $folderBrowser.SelectedPath }}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string result = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return string.IsNullOrEmpty(result) ? null : result;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS implementation using osascript
                var startInfo = new ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = $"-e 'tell application \"System Events\" to set folderPath to choose folder with prompt \"{title}\"'",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string result = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return string.IsNullOrEmpty(result) ? null : result;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux implementation using zenity
                var startInfo = new ProcessStartInfo
                {
                    FileName = "zenity",
                    Arguments = $"--file-selection --directory --title=\"{title}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    string result = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                    return string.IsNullOrEmpty(result) ? null : result;
                }
            }

            // Fallback to console input if dialog fails
            ShowMessage($"Please enter the {title.ToLower()} path:", "info");
            return Console.ReadLine();
        }
    }
}
