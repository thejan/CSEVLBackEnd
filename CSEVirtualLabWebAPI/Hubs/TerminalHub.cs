using Microsoft.AspNetCore.SignalR;
using CSEVirtualLabWebAPI.Services;

namespace CSEVirtualLabWebAPI.Hubs
{
    public class TerminalHub : Hub
    {
        private readonly LiveTerminalService _liveTerminalService;

        public TerminalHub(LiveTerminalService liveTerminalService)
        {
            _liveTerminalService = liveTerminalService;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client Connected : {Context.ConnectionId}");

            await Clients.Caller.SendAsync(
                "ConnectionEstablished",
                Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client Disconnected : {Context.ConnectionId}");

            _liveTerminalService.EndSession(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task StartExecution(string sourceCode)
        {
            Console.WriteLine($"StartExecution called by {Context.ConnectionId}");

            await _liveTerminalService.StartExecution(
                Context.ConnectionId,
                sourceCode);
        }

        public async Task SendInput(string input)
        {
            await _liveTerminalService.SendInput(
                Context.ConnectionId,
                input);
        }

        public async Task StopExecution()
        {
            _liveTerminalService.EndSession(Context.ConnectionId);

            await Clients.Caller.SendAsync(
                "ExecutionStopped",
                "Program terminated.");
        }
    }
}