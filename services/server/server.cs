using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySave.Models;
using EasySave.Services.Logging;

namespace EasySave.Services.Server
{
    public class BackupServer
    {
        private readonly TcpListener _server;
        private readonly ModelBackup _modelBackup;
        private readonly LoggingService _logger;
        private readonly Dictionary<string, BackupState> _activeBackups;
        private bool _isRunning;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public BackupServer(int port, ModelBackup modelBackup, LoggingService logger)
        {
            _server = new TcpListener(IPAddress.Any, port);
            _modelBackup = modelBackup;
            _logger = logger;
            _activeBackups = new Dictionary<string, BackupState>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            try
            {
                _server.Start();
                _isRunning = true;
                Console.WriteLine($"Backup server started on port {((IPEndPoint)_server.LocalEndpoint).Port}");

                while (_isRunning)
                {
                    try
                    {
                        var client = await _server.AcceptTcpClientAsync();
                        _ = HandleClientAsync(client);
                    }
                    catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidOperationException)
                    {
                        // Server was stopped
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error accepting client: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
                throw;
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _cancellationTokenSource.Cancel();
            _server.Stop();
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    var buffer = new byte[1024];
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    var command = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    var response = await ProcessCommandAsync(command);
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
        }

        private async Task<string> ProcessCommandAsync(string command)
        {
            try
            {
                var parts = command.Split('_');
                var action = parts[0].ToLower();

                if (action == "list")
                    return ListActiveBackups();

                if (parts.Length != 2)
                    return "Invalid command format. Use: start_1, stop_2, cancel_3, or list";

                var projectId = parts[1];

                switch (action)
                {
                    case "start":
                        return await StartBackupAsync(projectId);
                    case "stop":
                        return StopBackup(projectId);
                    case "resume":
                        return ResumeBackup(projectId);
                    case "cancel":
                        return CancelBackup(projectId);
                    default:
                        return "Unknown command";
                }
            }
            catch (Exception ex)
            {
                return $"Error processing command: {ex.Message}";
            }
        }

        private async Task<string> StartBackupAsync(string projectId)
        {
            try
            {
                // Get project name from ID (you'll need to implement this mapping)
                var projectName = GetProjectNameFromId(projectId);
                if (string.IsNullOrEmpty(projectName))
                {
                    return "Project not found";
                }

                var progress = new Progress<double>(p => UpdateBackupProgress(projectName, p));
                var state = await _modelBackup.GetBackupStateAsync(projectName);
                _activeBackups[projectName] = state;

                // Start backup in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _modelBackup.SaveProjectAsync(projectName, false, progress);
                        _activeBackups.Remove(projectName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during backup: {ex.Message}");
                        _activeBackups.Remove(projectName);
                    }
                });

                return $"Started backup for project {projectName}";
            }
            catch (Exception ex)
            {
                return $"Error starting backup: {ex.Message}";
            }
        }

        private string StopBackup(string projectId)
        {
            try
            {
                var projectName = GetProjectNameFromId(projectId);
                if (string.IsNullOrEmpty(projectName))
                {
                    return "Project not found";
                }

                _modelBackup.PauseProject(projectName);
                return $"Paused backup for project {projectName}";
            }
            catch (Exception ex)
            {
                return $"Error pausing backup: {ex.Message}";
            }
        }

        private string CancelBackup(string projectId)
        {
            try
            {
                var projectName = GetProjectNameFromId(projectId);
                if (string.IsNullOrEmpty(projectName))
                {
                    return "Project not found";
                }

                _modelBackup.StopProject(projectName);
                _activeBackups.Remove(projectName); // Remove from active backups
                return $"Cancelled backup for project {projectName}";
            }
            catch (Exception ex)
            {
                return $"Error cancelling backup: {ex.Message}";
            }
        }

        public string ResumeBackup(string projectId)
        {
            var projectName = GetProjectNameFromId(projectId);
            if (string.IsNullOrEmpty(projectName))
            {
                return "Project not found";
            }
            _modelBackup.ResumeProject(projectName);
            return $"Resumed backup for project {projectName}";
        }

        public string ListActiveBackups()
        {
            var activeBackups = new List<string>();
            foreach (var backup in _activeBackups)
            {
                activeBackups.Add($"Project: {backup.Key}, Progress: {backup.Value.SizeProgressPercentage:F2}%");
            }
            return string.Join("\n", activeBackups);
        }

        private void UpdateBackupProgress(string projectName, double progress)
        {
            if (_activeBackups.TryGetValue(projectName, out var state))
            {
                // Calculate the new processed size based on the progress percentage
                if (state.TotalSize > 0)
                {
                    state.ProcessedSize = (long)(state.TotalSize * (progress / 100.0));
                }
                
                // Calculate the new processed files based on the progress percentage
                if (state.TotalFiles > 0)
                {
                    state.ProcessedFiles = (int)(state.TotalFiles * (progress / 100.0));
                }
                
                state.UpdateStateAsync().Wait(); // Update the state asynchronously
            }
        }

        private string GetProjectNameFromId(string projectId)
        {
            // TODO: Implement mapping from project ID to project name
            // This could be done by maintaining a dictionary of project IDs to names
            // or by querying your project storage system
            return projectId; // For now, just return the ID as the name
        }

        public Dictionary<string, BackupState> GetActiveBackups()
        {
            return _activeBackups;
        }
    }
}