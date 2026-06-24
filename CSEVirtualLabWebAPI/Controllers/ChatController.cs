using CSEVirtualLabWebAPI.Models;
using CSEVirtualLabWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [EnableRateLimiting("ChatPolicy")]
    public class ChatController : ControllerBase
    {
        private readonly GeminiCProgrammingService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            GeminiCProgrammingService chatService,
            ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpPost("c-programming")]
        public async Task<IActionResult> AskCProgrammingQuestion(
            [FromBody] CProgrammingChatRequest request,
            CancellationToken cancellationToken)
        {
            request.SourceCode ??= string.Empty;
            request.ExperimentTitle ??= string.Empty;
            request.History ??= [];

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please enter a C programming question."
                });
            }

            if (request.Message.Length > 4000)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "The question is too long."
                });
            }

            request.SourceCode =
                request.SourceCode.Length > 12000
                    ? request.SourceCode[..12000]
                    : request.SourceCode;

            request.History =
                request.History
                    .Where(item =>
                        item.Content.Length <= 4000)
                    .TakeLast(12)
                    .ToList();

            try
            {
                var answer =
                    await _chatService.AskAsync(
                        request,
                        cancellationToken);

                return Ok(new
                {
                    success = true,
                    answer
                });
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogWarning(
                    exception,
                    "AI chatbot configuration or response error.");

                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        success = false,
                        message =
                            "Gemini is not configured or returned an invalid response."
                    });
            }
            catch (HttpRequestException exception)
            {
                _logger.LogError(
                    exception,
                    "Gemini chatbot request failed.");

                if (exception.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    return StatusCode(
                        StatusCodes.Status429TooManyRequests,
                        new
                        {
                            success = false,
                            message =
                                "The chatbot usage limit has been reached. Please try again later."
                        });
                }

                if (
                    exception.StatusCode is
                        HttpStatusCode.Unauthorized or
                        HttpStatusCode.Forbidden
                )
                {
                    return StatusCode(
                        StatusCodes.Status503ServiceUnavailable,
                        new
                        {
                            success = false,
                            message =
                                "The Gemini API key is invalid or does not have access."
                        });
                }

                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new
                    {
                        success = false,
                        message =
                            "Unable to contact Gemini. Please try again."
                    });
            }
        }
    }
}
