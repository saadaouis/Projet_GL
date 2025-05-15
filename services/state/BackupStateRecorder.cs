// <copyright file="BackupStateRecorder.cs" company="EasySave">
// Copyright (c) EasySave. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace EasySave.Services.State
{
    /// <summary>
    /// This class is used to record the state of the backup.
    /// </summary>
    public class BackupStateRecorder
    {
        private const string FilePath = "json/state.json";
        private static readonly SemaphoreSlim WriteLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Record the state of the backup.
        /// </summary>
        /// <param name="name">The name of the backup.</param>
        /// <param name="source">The source of the backup.</param>
        /// <param name="destination">The destination of the backup.</param>
        /// <param name="isDifferential">Whether the backup is a differential backup.</param>
        /// <param name="totalFileSize">The total size of the files to backup.</param>
        /// <param name="numberOfFilesLeft">The number of files left to backup.</param>
        /// <param name="timeElapsed">The time elapsed since the backup started.</param>
        /// <param name="progress">The progress of the backup.</param>
        public async void RecordStateAsync(string name, string source, string destination, bool isDifferential, float totalFileSize, int numberOfFilesLeft, int timeElapsed, float progress)
        {
            var state = new Dictionary<string, object>
            {
                { "name", name },
                { "source", source },
                { "destination", destination },
                { "isDifferential", isDifferential },
                { "totalFileSize (MB)", totalFileSize },
                { "numberOfFilesLeft", numberOfFilesLeft },
                { "timeElapsed (s)", timeElapsed },
                { "progress", progress },
            };

            await WriteLock.WaitAsync();
            try
            {
                Directory.CreateDirectory("json"); // Ensure directory exists
                string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });

                // Append to existing or create new file
                await File.AppendAllTextAsync(FilePath, json + "\n");
            }
            finally
            {
                WriteLock.Release();
            }
        }
    }
}
