// <copyright file="MainWindow.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using Avalonia.Controls;
using EasySave.ViewModels;

namespace EasySave.Views
{
    /// <summary>
    /// The main window.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class with a specific view model.
        /// </summary>
        /// <param name="viewModel">The view model to set.</param>
        public MainWindow(MainViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
