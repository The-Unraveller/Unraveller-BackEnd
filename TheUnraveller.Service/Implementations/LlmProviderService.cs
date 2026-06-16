using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
    private readonly string _provider;

    public LlmProviderService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _provider = configuration["LlmApi:Provider"]?.ToLower() ?? "claude";

        var apiKeyConfig = configuration["LlmApi:ApiKey"];
        _apiKey = string.IsNullOrEmpty(apiKeyConfig) || apiKeyConfig.Contains("PLACEHOLDER")
            ? "dummy_key"
            : apiKeyConfig;

        var baseUrlConfig = configuration["LlmApi:BaseUrl"];
        _baseUrl = string.IsNullOrEmpty(baseUrlConfig) || baseUrlConfig.Contains("PLACEHOLDER") || !baseUrlConfig.StartsWith("http")
            ? _provider == "gemini"
                ? "https://generativelanguage.googleapis.com/v1beta"
                : "https://api.openai.com/v1/"
            : baseUrlConfig;

        _model = configuration["LlmApi:Model"] ?? (_provider == "gemini" ? "gemini-2.0-flash" : "claude-haiku-4-5");
    }

    public async Task<LlmResponseDto> GetNpcResponseAsync(string systemPrompt, string userMessage)
    {
        return _provider switch
        {
            "gemini" => await GetGeminiResponseAsync(systemPrompt, userMessage),
            _ => await GetClaudeResponseAsync(systemPrompt, userMessage)
        };
    }

    private async Task<LlmResponseDto> GetClaudeResponseAsync(string systemPrompt, string userMessage)
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

    private async Task<LlmResponseDto> GetGeminiResponseAsync(string systemPrompt, string userMessage)
    {
        // Combine system and user prompts for Gemini (no separate system message)
        var combinedPrompt = $"{systemPrompt}\n\n[USER MESSAGE]\n{userMessage}";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = combinedPrompt } } }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 4096
            }
        };

        var apiKey = _apiKey;
        var model = _model;
        var url = $"{_baseUrl}/models/{model}:generateContent?key={apiKey}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return GetFallbackResponse($"System Error: Gemini API returned {response.StatusCode}. Details: {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseJson);
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                if (firstCandidate.TryGetProperty("content", out var content) &&
                    content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString() ?? "";

                    // Try to extract JSON from the response
                    var jsonMatch = Regex.Match(text, @"\{.*\s*""NpcResponse""\s*:.*\}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (jsonMatch.Success)
                    {
                        try
                        {
                            var result = JsonSerializer.Deserialize<LlmResponseDto>(jsonMatch.Value);
                            if (result != null) return result;
                        }
                        catch { }
                    }

                    // Fallback: return plain text as NpcResponse
                    return new LlmResponseDto
                    {
                        NpcResponse = text,
                        Feedback = "AI did not provide structured feedback in expected JSON format.",
                        SuspicionDelta = 0
                    };
                }
            }

            return GetFallbackResponse("System Error: Gemini response structure invalid (missing candidates/content).");
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
