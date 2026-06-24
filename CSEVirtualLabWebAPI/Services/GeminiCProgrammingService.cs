using System.Text;
using System.Text.Json;
using CSEVirtualLabWebAPI.Models;

namespace CSEVirtualLabWebAPI.Services
{
    public class GeminiCProgrammingService
    {
        private const string DefaultBaseUrl =
            "https://generativelanguage.googleapis.com/v1beta";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiCProgrammingService> _logger;

        public GeminiCProgrammingService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<GeminiCProgrammingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> AskAsync(
            CProgrammingChatRequest request,
            CancellationToken cancellationToken)
        {
            var apiKey =
                _configuration["Gemini:ApiKey"] ??
                Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key is not configured.");
            }

            var baseUrl =
                _configuration["Gemini:BaseUrl"] ??
                DefaultBaseUrl;

            var model =
                _configuration["Gemini:Model"] ??
                "gemini-3.1-flash-lite";

            var contents = request.History
                .Where(message =>
                    message.Role is "user" or "assistant" &&
                    !string.IsNullOrWhiteSpace(message.Content))
                .TakeLast(12)
                .Select(message => new
                {
                    role =
                        message.Role == "assistant"
                            ? "model"
                            : "user",
                    parts = new[]
                    {
                        new
                        {
                            text = message.Content.Trim()
                        }
                    }
                })
                .ToList();

            contents.Add(new
            {
                role = "user",
                parts = new[]
                {
                    new
                    {
                        text = request.Message.Trim()
                    }
                }
            });

            var payload = new
            {
                system_instruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = BuildInstructions(request)
                        }
                    }
                },
                contents,
                generationConfig = new
                {
                    maxOutputTokens = 800
                }
            };

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{baseUrl.TrimEnd('/')}/models/{model}:generateContent");

            httpRequest.Headers.Add(
                "x-goog-api-key",
                apiKey);

            httpRequest.Content =
                new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

            var client =
                _httpClientFactory.CreateClient();

            client.Timeout =
                TimeSpan.FromMinutes(2);

            using var response =
                await client.SendAsync(
                    httpRequest,
                    cancellationToken);

            var responseBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Gemini request failed with status {StatusCode}: {ResponseBody}",
                    response.StatusCode,
                    responseBody);

                throw new HttpRequestException(
                    "Gemini could not process the request.",
                    null,
                    response.StatusCode);
            }

            using var document =
                JsonDocument.Parse(responseBody);

            var answer =
                ExtractText(document.RootElement);

            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty or blocked response.");
            }

            return answer.Trim();
        }

        private static string BuildInstructions(
            CProgrammingChatRequest request)
        {
            var context = string.IsNullOrWhiteSpace(request.SourceCode)
                ? "No current source code was supplied."
                : $"Current student source code:\n```c\n{request.SourceCode}\n```";

            return
                "You are the C Programming Help Bot inside an educational virtual lab. " +
                "Answer questions genuinely related to the C programming language, " +
                "including syntax, logic, algorithms, compilation errors, debugging, " +
                "input/output, functions, arrays, strings, pointers, structures, files, " +
                "memory, and data structures. If a question is unrelated to C programming, " +
                "politely request a C programming question. Be accurate, encouraging, concise, " +
                "and suitable for a college student. Use short C examples when useful. " +
                "Return clean plain text suitable for a small chat window. Use short paragraphs " +
                "and simple numbered or bulleted lists. Do not use Markdown headings, bold markers, " +
                "italics markers, or fenced code blocks. " +
                "Do not claim code was executed unless execution output was supplied. " +
                $"Current experiment: {request.ExperimentId} - {request.ExperimentTitle}. " +
                context;
        }

        private static string ExtractText(
            JsonElement root)
        {
            if (
                !root.TryGetProperty(
                    "candidates",
                    out var candidates) ||
                candidates.ValueKind != JsonValueKind.Array
            )
            {
                return string.Empty;
            }

            var textParts =
                new List<string>();

            foreach (var candidate in candidates.EnumerateArray())
            {
                if (
                    !candidate.TryGetProperty(
                        "content",
                        out var content) ||
                    !content.TryGetProperty(
                        "parts",
                        out var parts) ||
                    parts.ValueKind != JsonValueKind.Array
                )
                {
                    continue;
                }

                foreach (var part in parts.EnumerateArray())
                {
                    if (
                        part.TryGetProperty(
                            "text",
                            out var text) &&
                        text.ValueKind == JsonValueKind.String
                    )
                    {
                        textParts.Add(
                            text.GetString() ?? string.Empty);
                    }
                }
            }

            return string.Join(
                Environment.NewLine,
                textParts);
        }
    }
}
