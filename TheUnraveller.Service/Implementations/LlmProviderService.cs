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

        // Gemini expects a combined prompt or separate parts. We'll combine system and user for simplicity in Gemini 1.5.
        var combinedPrompt = $"{systemPrompt}\n\nUser Message: {safeUserMessage}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = combinedPrompt } } }
            },
            generationConfig = new
            {
                temperature = 0.7,
                responseMimeType = "application/json"
            }
        };

        var targetUrl = $"{_baseUrl.TrimEnd('/')}?key={_apiKey}";
        var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return GetFallbackResponse($"System Error: Gemini API returned {response.StatusCode}. Details: {errorContent}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);
            using var document = JsonDocument.Parse(jsonResponse);

            var contentString = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            contentString = contentString?.Trim() ?? string.Empty;
            if (contentString.StartsWith("```json")) contentString = contentString.Substring(7);
            if (contentString.EndsWith("```")) contentString = contentString.Substring(0, contentString.Length - 3);

            return JsonSerializer.Deserialize<LlmResponseDto>(contentString) ?? GetFallbackResponse("System Error: Failed to parse NPC response.");
        }
        catch (TaskCanceledException)
        {
            return GetFallbackResponse("System Error: The NPC is taking too long to think. Timeout reached.");
        }
        catch (Exception ex)
        {
            return GetFallbackResponse($"System Error: Unexpected error occurred: {ex.Message}");
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
