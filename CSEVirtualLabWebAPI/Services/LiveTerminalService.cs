using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;
using CSEVirtualLabWebAPI.Hubs;

namespace CSEVirtualLabWebAPI.Services
{
    public class LiveTerminalService
    {
        private readonly ExecutionSessionService _sessionService;
        private readonly IHubContext<TerminalHub> _hubContext;

        private const string GccPath = @"C:\mingw64\bin\gcc.exe";

        public LiveTerminalService(
            ExecutionSessionService sessionService,
            IHubContext<TerminalHub> hubContext)
        {
            _sessionService = sessionService;
            _hubContext = hubContext;
        }

        public async Task StartExecution(
            string connectionId,
            string sourceCode)
        {
            string? tempFolder = null;

            try
            {
                await _hubContext.Clients
                    .Client(connectionId)
                    .SendAsync("ExecutionStarted");

                tempFolder = Path.Combine(
                    Path.GetTempPath(),
                    Guid.NewGuid().ToString());

                Directory.CreateDirectory(tempFolder);

                string sourceFile = Path.Combine(tempFolder, "program.c");
                string exeFile = Path.Combine(tempFolder, "program.exe");

                string preparedSourceCode =
                    PrepareSourceForLiveExecution(sourceCode);

                await File.WriteAllTextAsync(sourceFile, preparedSourceCode);

                var compileInfo = new ProcessStartInfo
                {
                    FileName = GccPath,
                    Arguments = $"\"{sourceFile}\" -o \"{exeFile}\" -lm",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var compileProcess = new Process
                {
                    StartInfo = compileInfo
                };

                compileProcess.Start();

                string compileOutput =
                    await compileProcess.StandardOutput.ReadToEndAsync();

                string compileErrors =
                    await compileProcess.StandardError.ReadToEndAsync();

                await compileProcess.WaitForExitAsync();

                if (compileProcess.ExitCode != 0)
                {
                    string errorText =
                        string.IsNullOrWhiteSpace(compileErrors)
                            ? compileOutput
                            : compileErrors;

                    await _hubContext.Clients
                        .Client(connectionId)
                        .SendAsync("ReceiveOutput", errorText);

                    await _hubContext.Clients
                        .Client(connectionId)
                        .SendAsync("ExecutionError", "Compilation failed.");

                    CleanupTempFolder(tempFolder);
                    return;
                }

                var runInfo = new ProcessStartInfo
                {
                    FileName = exeFile,
                    WorkingDirectory = tempFolder,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process
                {
                    StartInfo = runInfo,
                    EnableRaisingEvents = true
                };

                process.Start();

                _sessionService.AddProcess(connectionId, process);

                _ = StreamReaderToClient(
                    connectionId,
                    process.StandardOutput);

                _ = StreamReaderToClient(
                    connectionId,
                    process.StandardError);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await process.WaitForExitAsync();

                        _sessionService.RemoveProcess(connectionId);

                        await _hubContext.Clients
                            .Client(connectionId)
                            .SendAsync("ExecutionCompleted", "Program completed.");

                        process.Dispose();

                        CleanupTempFolder(tempFolder);
                    }
                    catch (Exception ex)
                    {
                        await _hubContext.Clients
                            .Client(connectionId)
                            .SendAsync("ExecutionError", ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                if (tempFolder != null)
                {
                    CleanupTempFolder(tempFolder);
                }

                await _hubContext.Clients
                    .Client(connectionId)
                    .SendAsync("ExecutionError", ex.Message);
            }
        }

        public async Task SendInput(
            string connectionId,
            string input)
        {
            var process = _sessionService.GetProcess(connectionId);

            if (process == null || process.HasExited)
            {
                return;
            }

            try
            {
                await process.StandardInput.WriteLineAsync(input);
                await process.StandardInput.FlushAsync();
            }
            catch (Exception ex)
            {
                await _hubContext.Clients
                    .Client(connectionId)
                    .SendAsync("ExecutionError", ex.Message);
            }
        }

        public void EndSession(string connectionId)
        {
            var process = _sessionService.GetProcess(connectionId);

            if (process == null)
            {
                return;
            }

            try
            {
                if (!process.HasExited)
                {
                    process.Kill(true);
                }

                process.Dispose();

                _sessionService.RemoveProcess(connectionId);

                Console.WriteLine($"Session Stopped : {connectionId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task StreamReaderToClient(
            string connectionId,
            StreamReader reader)
        {
            char[] buffer = new char[256];

            while (true)
            {
                int count = await reader.ReadAsync(
                    buffer,
                    0,
                    buffer.Length);

                if (count <= 0)
                {
                    break;
                }

                string text = new string(buffer, 0, count);

                await _hubContext.Clients
                    .Client(connectionId)
                    .SendAsync("ReceiveOutput", text);
            }
        }

        private static string PrepareSourceForLiveExecution(string sourceCode)
        {
            if (sourceCode.Contains("setvbuf(stdout"))
            {
                return sourceCode;
            }

            const string setupCode =
                "\n    setvbuf(stdout, NULL, _IONBF, 0);" +
                "\n    setvbuf(stderr, NULL, _IONBF, 0);\n";

            return Regex.Replace(
                sourceCode,
                @"(int\s+main\s*\([^)]*\)\s*\{)",
                "$1" + setupCode,
                RegexOptions.Singleline);
        }

        private static void CleanupTempFolder(string tempFolder)
        {
            try
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
            catch
            {
                // Ignore temp cleanup errors.
            }
        }
    }
}