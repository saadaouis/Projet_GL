using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace EasySave.Services.ProcessControl
{
    // This class manages a list of forbidden applications and checks if any are currently running
    public class ForbiddenAppManager
    {
        // Internal list that holds the names of forbidden processes
        private readonly List<string> _forbiddenProcesses = new();
        private static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        // Adds a new process name to the list of forbidden processes, if its not already present
        public void AddForbiddenProcess(string processName)
        {
            if (!_forbiddenProcesses.Contains(processName))
            {
                // For macOS, ensure we have both .app and non-.app versions
                if (IsMacOS)
                {
                    if (!processName.EndsWith(".app"))
                    {
                        _forbiddenProcesses.Add(processName);
                        _forbiddenProcesses.Add(processName + ".app");
                    }
                    else
                    {
                        _forbiddenProcesses.Add(processName);
                        _forbiddenProcesses.Add(processName.Replace(".app", ""));
                    }
                }
                else
                {
                    _forbiddenProcesses.Add(processName);
                }
            }
        }

        // Removes a process name from the list of forbidden processes
        public void RemoveForbiddenProcess(string processName)
        {
            if (IsMacOS)
            {
                _forbiddenProcesses.Remove(processName);
                _forbiddenProcesses.Remove(processName + ".app");
                _forbiddenProcesses.Remove(processName.Replace(".app", ""));
            }
            else
            {
                _forbiddenProcesses.Remove(processName);
            }
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
                try
                {
                    var processes = Process.GetProcessesByName(name);
                    if (processes.Any())
                    {
                        runningApp = name;
                        return true;
                    }

                    // Additional check for macOS .app processes
                    if (IsMacOS)
                    {
                        var allProcesses = Process.GetProcesses();
                        foreach (var process in allProcesses)
                        {
                            try
                            {
                                if (process.ProcessName.Contains(name) || 
                                    (name.EndsWith(".app") && process.ProcessName.Contains(name.Replace(".app", ""))))
                                {
                                    runningApp = name;
                                    return true;
                                }
                            }
                            catch
                            {
                                // Ignore processes we can't access
                                continue;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore any errors in process enumeration
                    continue;
                }
            }

            // No forbidden process found running
            runningApp = string.Empty;
            return false;
        }
    }
}
