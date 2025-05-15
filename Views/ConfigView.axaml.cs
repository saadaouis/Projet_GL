// <copyright file="ConfigView.axaml.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using Avalonia.Controls;
using EasySave.ViewModels;

namespace EasySave.Views
{
    /// <summary>
    /// The configuration view.
    /// </summary>
    public partial class ConfigView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigView"/> class.
        /// </summary>
        private string language = "En";

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigView"/> class.
        /// </summary>
        public ConfigView()
        {
            this.InitializeComponent();
        }

        private void LanguageComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is ConfigViewModel viewModel && sender is ComboBox comboBox)
            {
                viewModel.CurrentConfig.Language = comboBox.SelectedItem as string ?? "En";
            }
        }
    }
} 