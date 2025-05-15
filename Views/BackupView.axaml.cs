// <copyright file="BackupView.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System; // Added for EventArgs
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave.ViewModels;

namespace EasySave.Views
{
    /// <summary>
    /// The backup view.
    /// </summary>
    public partial class BackupView : UserControl
    {
        private bool projectsLoaded = false; // Flag to ensure projects are loaded only once

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupView"/> class.
        /// </summary>
        public BackupView()
        {
            this.InitializeComponent();
            this.AttachedToVisualTree += this.BackupView_AttachedToVisualTree;
        }

        private async void BackupView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            // Ensure DataContext is a BackupViewModel and projects haven't been loaded yet
            if (this.DataContext is BackupViewModel viewModel && !this.projectsLoaded)
            {
                Console.WriteLine("BackupView attached to visual tree. Loading projects...");
                await viewModel.LoadProjectsAsync();
                await viewModel.LoadProjectsAsync("destination");
                this.projectsLoaded = true; // Set flag to true after loading
                Console.WriteLine("Projects loaded by BackupView_AttachedToVisualTree.");
            }
        }

        private void ProjectsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is BackupViewModel viewModel && sender is ListBox listBox)
            {
                viewModel.UpdateSelectedProjects(listBox.SelectedItems?.Cast<object>() ?? Enumerable.Empty<object>());
            }
        }
    }
} 