using CSEVirtualLabDataAccessLayer;
using CSEVirtualLabDataAccessLayer.Models;
using CSEVirtualLabWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/user-requests")]
    public class UserRequestsController : ControllerBase
    {
        private readonly VirtualLabRepository repository;

        public UserRequestsController(
            VirtualLabRepository repository)
        {
            this.repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateUserRequestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                UserRequest created =
                    await repository.CreateUserRequestAsync(
                        request.UserId,
                        request.RequestType,
                        request.Description);

                return Ok(new
                {
                    success = true,
                    message = "Your request was submitted successfully.",
                    requestId = created.RequestId
                });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new
                {
                    success = false,
                    message = exception.Message
                });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new
                {
                    success = false,
                    message = exception.Message
                });
            }
        }
    }
}
