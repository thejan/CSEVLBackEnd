using Microsoft.AspNetCore.Mvc;
using CSEVirtualLabWebAPI.Models;
using CSEVirtualLabWebAPI.Services;

namespace CSEVirtualLabWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExecutionController : ControllerBase
    {
        private readonly CCompilerService _compiler;

        public ExecutionController(
            CCompilerService compiler)
        {
            _compiler = compiler;
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run(
            [FromBody] CodeExecutionRequest request)
        {
            try
            {
                Console.WriteLine("========== EXECUTION REQUEST ==========");
                Console.WriteLine("STEP 1 - Request Received");

                if (request == null ||
                    string.IsNullOrWhiteSpace(request.SourceCode))
                {
                    Console.WriteLine("STEP 1A - Invalid Request");

                    return BadRequest(new
                    {
                        success = false,
                        output = "",
                        error = "Source code is empty."
                    });
                }

                Console.WriteLine("STEP 2 - Calling Compiler Service");

                var result =
                  await _compiler.ExecuteCode(request.SourceCode, request.Input);

                Console.WriteLine("STEP 3 - Compiler Service Returned");

                Console.WriteLine("========== EXECUTION COMPLETED ==========");

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("========== EXECUTION ERROR ==========");
                Console.WriteLine(ex.ToString());

                return Ok(new
                {
                    success = false,
                    output = "",
                    error = ex.Message
                });
            }
        }
    }
}