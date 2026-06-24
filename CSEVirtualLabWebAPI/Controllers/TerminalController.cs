using Microsoft.AspNetCore.Mvc;
using CSEVirtualLabWebAPI.Models;
using CSEVirtualLabWebAPI.Services;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TerminalController : ControllerBase
    {
        private readonly ExecutionSessionService
            _sessionService;

        public TerminalController(
            ExecutionSessionService sessionService)
        {
            _sessionService = sessionService;
        }

        [HttpPost("input")]
        public IActionResult SendInput(
            TerminalInputRequest request)
        {
            var process =
                _sessionService.GetProcess(
                    request.ConnectionId);

            if (process == null)
            {
                return NotFound(
                    "Process not found");
            }

            process.StandardInput.WriteLine(
                request.Input);

            process.StandardInput.Flush();

            return Ok();
        }

        [HttpPost("stop/{connectionId}")]
        public IActionResult StopExecution(
            string connectionId)
        {
            var process =
                _sessionService.GetProcess(
                    connectionId);

            if (process == null)
            {
                return NotFound();
            }

            process.Kill(true);

            _sessionService.RemoveProcess(
                connectionId);

            return Ok();
        }
    }
}