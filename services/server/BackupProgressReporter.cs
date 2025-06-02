using System;

namespace EasySave.Services.Server
{
    public class BackupProgressReporter : IBackupProgressReporter
    {
        public void ReportProgress(string projectName, double progress)
        {
            Console.WriteLine($"Progress for {projectName}: {progress}%");
            // Add additional logic here, such as updating UI or logging to a file
        }

        public void ReportError(string projectName, string error)
        {
            Console.WriteLine($"Error for {projectName}: {error}");
            // Add additional logic here, such as logging to a file or notifying the user
        }

        public void ReportCompletion(string projectName)
        {
            Console.WriteLine($"Backup completed for {projectName}");
            // Add additional logic here, such as updating UI or logging to a file
        }
    }
} 