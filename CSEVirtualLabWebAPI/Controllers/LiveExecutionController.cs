using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LiveExecutionController : ControllerBase
    {
        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(new
            {
                success = true,
                message = "Live execution is handled by SignalR TerminalHub."
            });
        }
    }
}