using CSEVirtualLabDataAccessLayer;
using CSEVirtualLabWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LabsController : ControllerBase
    {
        private readonly VirtualLabRepository repository;

        public LabsController(VirtualLabRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterLab(
            [FromBody] RegisterLabRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool registered =
                    await repository.RegisterLabAsync(
                        request.UserId,
                        request.LabId);

                return Ok(new
                {
                    success = registered,
                    message = "Lab registered successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
