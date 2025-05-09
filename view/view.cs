// <copyright file="view.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

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
    }
}
