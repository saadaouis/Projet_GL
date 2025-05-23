using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EasySave.Services.ProcessControl
{
    public class ForbiddenAppManager
    {
        private readonly List<string> _forbiddenProcesses = new();

        public void AddForbiddenProcess(string processName)
        {
            if (!_forbiddenProcesses.Contains(processName))
                _forbiddenProcesses.Add(processName);
        }

        public void RemoveForbiddenProcess(string processName)
        {
            _forbiddenProcesses.Remove(processName);
        }

        public IReadOnlyList<string> GetForbiddenProcesses()
        {
            return _forbiddenProcesses.AsReadOnly();
        }

        public bool IsAnyForbiddenAppRunning(out string runningApp)
        {
            foreach (var name in _forbiddenProcesses)
            {
                if (Process.GetProcessesByName(name).Any())
                {
                    runningApp = name;
                    return true;
                }
            }

            runningApp = null;
            return false;
        }
    }
}
