using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class LlmProviderService : ILLMProviderService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;

    public LlmProviderService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["LlmApi:ApiKey"] ?? "dummy_key";
        _baseUrl = configuration["LlmApi:BaseUrl"] ?? "https://api.openai.com/v1/";
        _model = configuration["LlmApi:Model"] ?? "gpt-4o";
    }

    public async Task<LlmResponseDto> GetNpcResponseAsync(string systemPrompt, string userMessage)
    {
        // Safe-guard against Prompt Injection
        var safeUserMessage = $"[USER_TEXT]\n{userMessage}\n[/USER_TEXT]";

        var requestBody = new
        {
            model = _model,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = safeUserMessage }
            },
            temperature = 0.7
        };

        var targetUrl = $"{_baseUrl.TrimEnd('/')}/chat/completions";
        var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

        try
        {
            // Adding a cancellation token to enforce a 5 second timeout in reality, 
            // HttpClient timeout would be set in Program.cs configuration.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                return GetFallbackResponse("System Error: The NPC is currently unavailable.");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);
            using var document = JsonDocument.Parse(jsonResponse);
            var contentString = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // Sanitize Markdown JSON tags if present
            contentString = contentString?.Trim() ?? string.Empty;
            if (contentString.StartsWith("```json")) contentString = contentString.Substring(7);
            if (contentString.EndsWith("```")) contentString = contentString.Substring(0, contentString.Length - 3);

            return JsonSerializer.Deserialize<LlmResponseDto>(contentString) ?? GetFallbackResponse("System Error: Failed to parse NPC response.");
        }
        catch (TaskCanceledException)
        {
            return GetFallbackResponse("System Error: The NPC is taking too long to think. Timeout reached.");
        }
        catch (Exception)
        {
            return GetFallbackResponse("System Error: Unexpected error occurred while contacting NPC.");
        }
    }

    private LlmResponseDto GetFallbackResponse(string errorFeedback)
    {
        return new LlmResponseDto
        {
            NpcResponse = "I didn't quite catch that. Can you say it again?",
            Feedback = errorFeedback,
            SuspicionDelta = 0
        };
    }
}
