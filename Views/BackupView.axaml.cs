// <copyright file="BackupView.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System; // Added for EventArgs
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave.ViewModels;
using EasySave.Models;
using Avalonia.VisualTree;

namespace EasySave.Views
{
    /// <summary>
    /// The backup view.
    /// </summary>
    public partial class BackupView : UserControl
    {
        private bool projectsLoaded = false; // Flag to ensure projects are loaded only once
        private BackupViewModel? ViewModel => DataContext as BackupViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupView"/> class.
        /// </summary>
        public BackupView()
        {
            this.InitializeComponent();
            this.AttachedToVisualTree += this.BackupView_AttachedToVisualTree;
            this.DataContextChanged += OnDataContextChanged;
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

        private void OnDataContextChanged(object? sender, EventArgs e)
        {
            if (ViewModel != null)
            {
                // Subscribe to property changes to update individual progress bars
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != null && e.PropertyName.StartsWith("ProjectProgress["))
            {
                // Extract project name from property name
                var projectName = e.PropertyName.Substring(16, e.PropertyName.Length - 17); // Remove "ProjectProgress[" and "]"
                UpdateProjectProgressBar(projectName);
            }
        }

        private void UpdateProjectProgressBar(string projectName)
        {
            if (ViewModel == null) return;

            // Find the ListBoxItem that contains the project with the given name
            var listBox = this.FindControl<ListBox>("ProjectsListBox");
            if (listBox != null)
            {
                foreach (var item in listBox.GetVisualDescendants())
                {
                    if (item is ListBoxItem listBoxItem && 
                        listBoxItem.DataContext is ModelBackup.Project project && 
                        project.Name == projectName)
                    {
                        // Find the ProgressBar within this ListBoxItem
                        var progressBar = listBoxItem.FindDescendantOfType<ProgressBar>();
                        var statusText = listBoxItem.FindDescendantOfType<TextBlock>("StatusText");
                        
                        if (progressBar != null)
                        {
                            var progress = ViewModel.GetProjectProgress(projectName);
                            progressBar.Value = progress;
                            
                            // Update status text
                            if (statusText != null)
                            {
                                if (progress == 0)
                                    statusText.Text = "Waiting...";
                                else if (progress == 100)
                                    statusText.Text = "✅ Completed";
                                else if (progress > 0)
                                    statusText.Text = "🔄 In Progress";
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void ProjectsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && ViewModel != null)
            {
                ViewModel.UpdateSelectedProjects(listBox.SelectedItems?.Cast<object>() ?? []);
            }
        }
    }

    // Extension method to help find controls by name
    public static class ControlExtensions
    {
        public static T? FindDescendantOfType<T>(this Control control, string? name = null) where T : Control
        {
            foreach (var child in control.GetVisualDescendants())
            {
                if (child is T directMatch && (name == null || directMatch.Name == name))
                {
                    return directMatch;
                }

                if (child is Control childControl)
                {
                    var result = childControl.FindDescendantOfType<T>(name);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
    }
} 