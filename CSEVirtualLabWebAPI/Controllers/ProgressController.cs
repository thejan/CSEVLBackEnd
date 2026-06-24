using CSEVirtualLabDataAccessLayer;
using CSEVirtualLabWebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase
    {
        private readonly VirtualLabRepository repository;

        public ProgressController(VirtualLabRepository repository)
        {
            this.repository = repository;
        }

        [HttpPut("execution")]
        public async Task<IActionResult> UpdateExecutionStatus(
            [FromBody] UpdateExecutionStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool updated =
                await repository.UpdateExecutionStatusAsync(
                    request.UserId,
                    request.LabId,
                    request.ExperimentId);

            if (!updated)
            {
                return NotFound("Lab status row was not found.");
            }

            return Ok(new
            {
                success = true,
                message = "Execution status updated successfully."
            });
        }

        [HttpPut("quiz")]
        public async Task<IActionResult> UpdateQuizStatus(
            [FromBody] UpdateQuizStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool updated =
                await repository.UpdateQuizStatusAsync(
                    request.UserId,
                    request.LabId,
                    request.ExperimentId,
                    request.QuizScore,
                    request.QuizMaxMarks);

            if (!updated)
            {
                QuizAttemptDto? attempt =
                    await repository.GetQuizAttemptAsync(
                        request.UserId,
                        request.LabId,
                        request.ExperimentId);

                if (attempt == null)
                {
                    return NotFound("Lab status row was not found.");
                }

                return Conflict(new
                {
                    success = false,
                    message =
                        "Only one quiz attempt is allowed for each experiment.",
                    attempt
                });
            }

            return Ok(new
            {
                success = true,
                message = "Quiz status updated successfully."
            });
        }

        [HttpGet("quiz-attempt")]
        public async Task<IActionResult> GetQuizAttempt(
            [FromQuery] int userId,
            [FromQuery] int labId,
            [FromQuery] int experimentId)
        {
            QuizAttemptDto? attempt =
                await repository.GetQuizAttemptAsync(
                    userId,
                    labId,
                    experimentId);

            if (attempt == null)
            {
                return NotFound("Lab status row was not found.");
            }

            return Ok(attempt);
        }

        [HttpPut("assignment")]
        public async Task<IActionResult> UpdateAssignmentStatus(
            [FromBody] UpdateAssignmentStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                bool updated =
                    await repository.UpdateAssignmentStatusAsync(
                        request.UserId,
                        request.LabId,
                        request.ExperimentId,
                        request.AssignmentId);

                if (!updated)
                {
                    return NotFound("Lab status row was not found.");
                }

                return Ok(new
                {
                    success = true,
                    message =
                        "Assignment status updated successfully."
                });
            }
            catch (ArgumentException ex)
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
