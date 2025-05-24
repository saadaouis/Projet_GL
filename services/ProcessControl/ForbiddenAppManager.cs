using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EasySave.Services.ProcessControl
{
    // This class manages a list of forbidden applications and checks if any are currently running
    public class ForbiddenAppManager
    {
        // Internal list that holds the names of forbidden processes
        private readonly List<string> _forbiddenProcesses = new();

        // Adds a new process name to the list of forbidden processes, if it’s not already present
        public void AddForbiddenProcess(string processName)
        {
            if (!_forbiddenProcesses.Contains(processName))
                _forbiddenProcesses.Add(processName);
        }

        // Removes a process name from the list of forbidden processes
        public void RemoveForbiddenProcess(string processName)
        {
            _forbiddenProcesses.Remove(processName);
        }

        // Returns a read-only copy of the list of forbidden processes
        public IReadOnlyList<string> GetForbiddenProcesses()
        {
            return _forbiddenProcesses.AsReadOnly();
        }

        // Checks if any forbidden process is currently running
        // Returns true and sets the runningApp name if a forbidden process is found
        public bool IsAnyForbiddenAppRunning(out string runningApp)
        {
            foreach (var name in _forbiddenProcesses)
            {
                // Use Process.GetProcessesByName to find running processes with the forbidden name
                if (Process.GetProcessesByName(name).Any())
                {
                    runningApp = name;
                    return true;
                }
            }

            // No forbidden process found running
            runningApp = null;
            return false;
        }
    }
}
