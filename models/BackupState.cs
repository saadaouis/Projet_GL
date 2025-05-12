// <copyright file="BackupState.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

namespace EasySave.Models
{
    /// <summary>
    /// Represents the state of a backup operation.
    /// </summary>
    public class BackupState
    {
        /// <summary>
        /// Event that is raised when the backup state changes.
        /// </summary>
        public event EventHandler<BackupState>? StateChanged;

        /// <summary>
        /// Gets or sets the name of the project being backed up.
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current operation being performed.
        /// </summary>
        public string CurrentOperation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of files to process.
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Gets or sets the number of files processed.
        /// </summary>
        public int ProcessedFiles { get; set; }

        /// <summary>
        /// Gets or sets the total size of files to process in bytes.
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Gets or sets the size of processed files in bytes.
        /// </summary>
        public long ProcessedSize { get; set; }

        /// <summary>
        /// Gets the progress percentage of the backup operation.
        /// </summary>
        public double ProgressPercentage => this.TotalFiles > 0 ? (double)this.ProcessedFiles / this.TotalFiles * 100 : 0;

        /// <summary>
        /// Gets the progress percentage of the backup operation by size.
        /// </summary>
        public double SizeProgressPercentage => this.TotalSize > 0 ? (double)this.ProcessedSize / this.TotalSize * 100 : 0;

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets a boolean indicating if the backup operation is complete.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets any error message that occurred during the backup.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Updates the backup state and raises the StateChanged event.
        /// </summary>
        public void UpdateState()
        {
            this.StateChanged?.Invoke(this, this);
        }
    }
}