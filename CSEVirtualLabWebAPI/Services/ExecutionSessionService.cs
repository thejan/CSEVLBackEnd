using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSEVirtualLabWebAPI.Services
{
    public class ExecutionSessionService
    {
        private readonly ConcurrentDictionary<string, Process> _processes = new();

        public void AddProcess(
            string connectionId,
            Process process)
        {
            if (_processes.TryRemove(connectionId, out var oldProcess))
            {
                try
                {
                    if (!oldProcess.HasExited)
                    {
                        oldProcess.Kill(true);
                    }

                    oldProcess.Dispose();
                }
                catch
                {
                    // Ignore cleanup errors.
                }
            }

            _processes[connectionId] = process;
        }

        public Process? GetProcess(
            string connectionId)
        {
            _processes.TryGetValue(
                connectionId,
                out var process);

            return process;
        }

        public void RemoveProcess(
            string connectionId)
        {
            _processes.TryRemove(
                connectionId,
                out _);
        }
    }
}