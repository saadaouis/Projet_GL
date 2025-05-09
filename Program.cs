// <copyright file="Program.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System;
using EasySave.Controllers;

namespace EasySave
{
    /// <summary>
    /// Main class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main method.
        /// </summary>
        private static void Main(string[] args)
        {
            Controller controller = new Controller();
            controller.Initialization();
        }
    }
}
