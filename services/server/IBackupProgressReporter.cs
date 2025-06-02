using System;

namespace EasySave.Services.Server
{
    public interface IBackupProgressReporter
    {
        void ReportProgress(string projectName, double progress);
        void ReportError(string projectName, string error);
        void ReportCompletion(string projectName);
    }
} 