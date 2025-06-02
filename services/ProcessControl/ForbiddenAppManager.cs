using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System;

namespace EasySave.Services.ProcessControl
{
    // This class manages a list of forbidden applications and checks if any are currently running
    public class ForbiddenAppManager : IDisposable
    {
        // Internal list that holds the names of forbidden processes
        private readonly List<string> _forbiddenProcesses = new();
        private static readonly bool IsMacOS = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        private bool _disposed;

        public void InitializeFromConfig(string processesString)
        {
            if (string.IsNullOrWhiteSpace(processesString))
                return;

            _forbiddenProcesses.Clear();
            
            var processes = processesString.Split(',')
                                        .Select(p => p.Trim())
                                        .Where(p => !string.IsNullOrWhiteSpace(p));
                                        
            foreach (var process in processes)
            {
                AddForbiddenProcess(process);
            }
            
            Console.WriteLine($"Initialized forbidden processes: {string.Join(", ", _forbiddenProcesses)}");
        }

        // Adds a new process name to the list of forbidden processes, if its not already present
        public void AddForbiddenProcess(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                throw new ArgumentNullException(nameof(processName));

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
            if (string.IsNullOrWhiteSpace(processName))
                throw new ArgumentNullException(nameof(processName));

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
            runningApp = string.Empty;
            List<Process> processesToDispose = new();

            try
            {
                // Debug: Log forbidden processes being checked
                Console.WriteLine($"Checking forbidden processes: {string.Join(", ", _forbiddenProcesses)}");
                
                // Get all running processes once
                var allProcesses = Process.GetProcesses();
                processesToDispose.AddRange(allProcesses);
                
                foreach (var name in _forbiddenProcesses)
                {
                    try
                    {
                        // Check for exact process name match
                        var matchingProcesses = allProcesses.Where(p => 
                            p.ProcessName.Equals(name, StringComparison.OrdinalIgnoreCase));
                        
                        if (matchingProcesses.Any())
                        {
                            Console.WriteLine($"Found forbidden process: {name}");
                            runningApp = name;
                            return true;
                        }

                        // Check for partial matches (especially useful for Linux/macOS)
                        var partialMatches = allProcesses.Where(p => 
                            p.ProcessName.Contains(name, StringComparison.OrdinalIgnoreCase));
                        
                        if (partialMatches.Any())
                        {
                            var matchedProcess = partialMatches.First();
                            Console.WriteLine($"Found forbidden process (partial match): {name} as {matchedProcess.ProcessName}");
                            runningApp = name;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking process {name}: {ex.Message}");
                        continue;
                    }
                }
                
                Console.WriteLine("No forbidden processes found running.");
            }
            finally
            {
                foreach (var process in processesToDispose)
                {
                    try { process.Dispose(); } catch { /* Ignore disposal errors */ }
                }
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _forbiddenProcesses.Clear();
                }
                _disposed = true;
            }
        }

        ~ForbiddenAppManager()
        {
            Dispose(false);
        }
    }
}
