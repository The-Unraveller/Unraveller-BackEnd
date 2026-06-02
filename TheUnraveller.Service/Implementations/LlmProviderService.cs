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
        _model = configuration["LlmApi:Model"] ?? "claude-haiku-4-5";
    }

    public async Task<LlmResponseDto> GetNpcResponseAsync(string systemPrompt, string userMessage)
    {
        // Safe-guard against Prompt Injection
        var safeUserMessage = $"[USER_TEXT]\n{userMessage}\n[/USER_TEXT]";

        var messages = new[]
        {
            new { role = "user", content = safeUserMessage }
        };

        var requestBody = new
        {
            model = _model,
            max_tokens = 4096,
            system = systemPrompt,
            messages = messages,
            temperature = 0.7
        };

        var targetUrl = _baseUrl;
        var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
        request.Headers.Add("x-api-key", _apiKey);
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            // Use ResponseHeadersRead to begin streaming immediately without buffering the entire body
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return GetFallbackResponse($"System Error: Claude API returned {response.StatusCode}. Details: {errorContent}");
            }

            string contentString = string.Empty;
            var textBuilder = new System.Text.StringBuilder();

            try
            {
                // Stream line by line to break immediately when message_stop is encountered, avoiding 19s keep-alive socket delays
                using (var stream = await response.Content.ReadAsStreamAsync(cts.Token))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    bool isSse = false;
                    string? line;

                    while ((line = await reader.ReadLineAsync(cts.Token)) != null)
                    {
                        if (line.StartsWith("event:") || line.StartsWith("data:"))
                        {
                            isSse = true;
                        }

                        if (isSse)
                        {
                            if (line.StartsWith("data:"))
                            {
                                var json = line.Substring(line.IndexOf(':') + 1).Trim();
                                if (json.StartsWith("{") && json.EndsWith("}"))
                                {
                                    try
                                    {
                                        using var doc = JsonDocument.Parse(json);
                                        if (doc.RootElement.TryGetProperty("type", out var typeProp) && typeProp.GetString() == "message_stop")
                                        {
                                            break; // Stream finished, exit immediately!
                                        }

                                        if (doc.RootElement.TryGetProperty("delta", out var deltaProp) &&
                                            deltaProp.TryGetProperty("text", out var textProp))
                                        {
                                            textBuilder.Append(textProp.GetString());
                                        }
                                    }
                                    catch
                                    {
                                        // Ignore malformed JSON lines in the stream
                                    }
                                }
                            }
                            else if (line.StartsWith("event: message_stop"))
                            {
                                break; // Stream finished, exit immediately!
                            }
                        }
                        else
                        {
                            textBuilder.AppendLine(line);
                        }
                    }
                }
            }
            catch
            {
                // If we already have a potential JSON block in our builder, ignore the network termination exception
                var tempString = textBuilder.ToString().Trim();
                int first = tempString.IndexOf('{');
                int last = tempString.LastIndexOf('}');
                if (first < 0 || last <= first)
                {
                    // No valid JSON accumulated, rethrow the exception to let the outer fallback handle it
                    throw;
                }
            }

            contentString = textBuilder.ToString().Trim();

            contentString = contentString.Trim();
            if (contentString.StartsWith("```json")) contentString = contentString.Substring(7);
            if (contentString.EndsWith("```")) contentString = contentString.Substring(0, contentString.Length - 3);
            contentString = contentString.Trim();

            // Robust JSON extraction to prevent issues with reasoning token wrapper prefixes
            int firstBrace = contentString.IndexOf('{');
            int lastBrace = contentString.LastIndexOf('}');
            if (firstBrace >= 0 && lastBrace > firstBrace)
            {
                contentString = contentString.Substring(firstBrace, lastBrace - firstBrace + 1);
                try
                {
                    return JsonSerializer.Deserialize<LlmResponseDto>(contentString) ?? GetFallbackResponse("System Error: Failed to parse NPC response (null deserialization result).");
                }
                catch (JsonException jsonEx)
                {
                    return GetFallbackResponse($"System Error: JSON parsing failed: {jsonEx.Message}. Raw text: {contentString}");
                }
            }
            else
            {
                return GetFallbackResponse($"System Error: No JSON object found in response. Raw text: {contentString}");
            }
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
