// <copyright file="Program.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using System.Security.Cryptography.X509Certificates;
using Avalonia;

namespace EasySave
{
    /// <summary>
    /// Main class.
    /// </summary>
    internal class Program
    {
        public static string? LaunchArgument { get; private set; }
        public static string? ProjectArgument { get; private set; }


        /// <summary>
        /// Main method.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args[0].Length > 0)
            {
                if (args[1].Length > 0 && args[0].Contains("config"))
                {
                    Console.WriteLine("Config doesn't need arguments");
                    LaunchArgument = args[0];
                }
                else if (args[1].Length > 0 && !args[1].Contains("config"))
                {
                    Console.WriteLine("Your choice is : " + args[0]);
                    Console.WriteLine("Your project is : " + args[1]);
                    LaunchArgument = args[0];
                    ProjectArgument = args[1];
                }
            }

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        /// <summary>
        /// Builds the Avalonia application.
        /// </summary>
        /// <returns>The AppBuilder instance.</returns>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
