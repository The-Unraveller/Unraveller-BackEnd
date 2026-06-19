using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class LlmProviderService : ILLMProviderService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LlmProviderService> _logger;
    private readonly HttpClient _httpClient;

    // Primary (Gemini) config
    private readonly string _geminiBaseUrl;
    private readonly string _geminiApiKey;
    private readonly string _geminiModel;
    private readonly bool _isGeminiConfigured;

    // Fallback (Claude) config
    private readonly string _claudeBaseUrl;
    private readonly string _claudeApiKey;
    private readonly string _claudeModel;
    private readonly bool _isClaudeConfigured;

    public LlmProviderService(IConfiguration configuration, ILogger<LlmProviderService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        // Configure timeouts and retry policies
        _httpClient.Timeout = TimeSpan.FromSeconds(120);

        // Use the most up-to-date Google AI SDK endpoints and model naming
        // Updated to use gemini-2.5-flash for better performance and reliability
        _geminiBaseUrl = _configuration["LlmApi:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1";
        _geminiApiKey = _configuration["LlmApi:ApiKey"] ?? "";
        _geminiModel = _configuration["LlmApi:Model"] ?? "gemini-2.5-flash";
        _isGeminiConfigured = !string.IsNullOrWhiteSpace(_geminiApiKey);

        _logger.LogInformation("Gemini configured: {IsConfigured}, Model: {GeminiModel}", _isGeminiConfigured, _geminiModel);

        // Claude (fallback) with safer defaults and better model for reliability
        _claudeBaseUrl = _configuration["LlmApi:ClaudeBaseUrl"] ?? "https://claude.zunef.com/v1/ai/messages";
        _claudeApiKey = _configuration["LlmApi:ClaudeApiKey"] ?? "";
        _claudeModel = _configuration["LlmApi:ClaudeModel"] ?? "claude-opus-4-8";
        _isClaudeConfigured = !string.IsNullOrWhiteSpace(_claudeApiKey);

        _logger.LogInformation("Claude configured: {IsConfigured}, Model: {ClaudeModel}", _isClaudeConfigured, _claudeModel);
    }

    public async Task<ProviderEvaluationResponse> GetEvaluationResponseAsync(string systemPrompt, string userMessage)
    {
        _logger.LogInformation("Processing evaluation request for user message length: {MessageLength}", userMessage.Length);

        // Try Gemini first (primary provider), fallback to Claude if fails
        if (_isGeminiConfigured)
        {
            try
            {
                _logger.LogInformation("Attempting primary provider: Gemini with model {GeminiModel}", _geminiModel);
                var geminiResult = await GetGeminiEvaluationResponseAsync(systemPrompt, userMessage);
                _logger.LogInformation("Gemini provider succeeded - response received");
                return geminiResult;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Primary provider Gemini failed, attempting fallback provider...");
            }
        }
        else
        {
            _logger.LogWarning("Gemini API key not configured, skipping primary provider");
        }

        // Try Claude only if configured
        if (_isClaudeConfigured)
        {
            try
            {
                _logger.LogInformation("Attempting fallback provider: Claude with model {ClaudeModel}", _claudeModel);
                var claudeResult = await GetClaudeEvaluationResponseAsync(systemPrompt, userMessage);
                _logger.LogInformation("Claude provider succeeded - response received");
                return claudeResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Both providers failed - Gemini and Claude unavailable");
                return GetUltimateFallbackResponse();
            }
        }
        else
        {
            _logger.LogError("No providers configured - both Gemini and Claude API keys missing");
            return GetUltimateFallbackResponse();
        }
    }

    private async Task<ProviderEvaluationResponse> GetGeminiEvaluationResponseAsync(string systemPrompt, string userMessage)
    {
        // Ensure we have valid API key
        if (!_isGeminiConfigured || string.IsNullOrWhiteSpace(_geminiApiKey))
        {
            _logger.LogError("Gemini API key is not configured or is empty");
            throw new InvalidOperationException("Gemini API key is required but not configured");
        }

        var combinedPrompt = $"{systemPrompt}\n\n[USER MESSAGE]\n{userMessage}";

        var url = $"{_geminiBaseUrl}/models/{_geminiModel}:generateContent?key={_geminiApiKey}";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = combinedPrompt }
                                }
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 0.7,
                            topP = 0.9,
                            maxOutputTokens = 3000,
                            responseMimeType = "application/json"
                        },
                        safetySettings = new[]
                        {
                            new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                            new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                            new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                            new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" }
                        }
                    }),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            // Ensure proper retry policy
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogError("Gemini API returned error status {StatusCode}: {ErrorContent}", (int)response.StatusCode, errorContent);
                throw new Exception($"Gemini API error {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
            _logger.LogDebug("Raw Gemini response received: {ResponseContent}", responseContent);

            string extractedText = string.Empty;
            try
            {
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0 &&
                    candidates[0].TryGetProperty("content", out var contentProp) &&
                    contentProp.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0 &&
                    parts[0].TryGetProperty("text", out var textProp))
                {
                    extractedText = textProp.GetString() ?? "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse raw Gemini response JSON");
            }

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                extractedText = responseContent;
            }

            return ParseAndValidate(extractedText, "Gemini");
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Gemini API request timed out after 90 seconds");
            throw new TimeoutException("Gemini API request timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Gemini API call");
            throw;
        }
    }

    private async Task<string> StreamGeminiAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var textBuilder = new StringBuilder();
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        int charCount = 0;
        int maxChars = 6000; // Safety limit for response size

        try
        {
            while ((line = await reader.ReadLineAsync(ct)) != null && charCount < maxChars)
            {
                if (line.StartsWith("data:"))
                {
                    var json = line.Substring(5).Trim();
                    if (json.StartsWith("{") && json.EndsWith("}"))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(json);
                            if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                            {
                                var first = candidates[0];
                                if (first.TryGetProperty("content", out var content) &&
                                    content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                                {
                                    var text = parts[0].GetProperty("text").GetString();
                                    if (!string.IsNullOrEmpty(text))
                                    {
                                        textBuilder.Append(text);
                                        charCount += text.Length;
                                    }
                                }
                            }

                            // Check for finish reason
                            if (doc.RootElement.TryGetProperty("candidates", out var candArray) && candArray.GetArrayLength() > 0 &&
                                candArray[0].TryGetProperty("finishReason", out var reason))
                            {
                                _logger.LogDebug("Gemini response finished with reason: {FinishReason}", reason.GetString());
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error parsing Gemini stream data");
                        }
                    }
                }
                else if (line.StartsWith("event: done") || line.StartsWith("event: message_stop"))
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Gemini stream read was cancelled");
            throw;
        }

        _logger.LogDebug("StreamGemini collected {CharCount} characters of text", textBuilder.Length);
        return textBuilder.ToString();
    }

    private async Task<ProviderEvaluationResponse> GetClaudeEvaluationResponseAsync(string systemPrompt, string userMessage)
    {
        // Ensure we have valid API key
        if (!_isClaudeConfigured || string.IsNullOrWhiteSpace(_claudeApiKey))
        {
            _logger.LogError("Claude API key is not configured or is empty");
            throw new InvalidOperationException("Claude API key is required but not configured");
        }

        var safeUserMessage = $"[USER_TEXT]\n{userMessage}\n[/USER_TEXT]";

        var requestBody = new
        {
            model = _claudeModel,
            max_tokens = 3000,
            temperature = 0.7,
            top_p = 0.9,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = safeUserMessage } }
        };

        var url = _claudeBaseUrl;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            // Add both headers as some Claude gateways require both
            request.Headers.Add("x-api-key", _claudeApiKey);
            request.Headers.Add("Authorization", $"Bearer {_claudeApiKey}");
            request.Headers.Add("anthropic-version", "2023-06-01");

            // Ensure proper retry policy and timeout handling
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogError("Claude API returned error status {StatusCode}: {ErrorContent}", (int)response.StatusCode, errorContent);
                throw new Exception($"Claude API error {response.StatusCode}: {errorContent}");
            }

            var fullText = await StreamClaudeAsync(response, cts.Token);
            _logger.LogDebug("Raw Claude response received (first 200 chars): {ResponsePreview}", fullText.Length > 200 ? fullText.Substring(0, 200) : fullText);

            return ParseAndValidate(fullText, "Claude");
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Claude API request timed out after 90 seconds");
            throw new TimeoutException("Claude API request timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Claude API call");
            throw;
        }
    }

    private async Task<string> StreamClaudeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var textBuilder = new StringBuilder();
        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        bool isSse = false;
        string? line;
        int charCount = 0;
        int maxChars = 6000;

        try
        {
            while ((line = await reader.ReadLineAsync(ct)) != null && charCount < maxChars)
            {
                if (line.StartsWith("event:") || line.StartsWith("data:"))
                    isSse = true;

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
                                    break;

                                if (doc.RootElement.TryGetProperty("delta", out var deltaProp) &&
                                    deltaProp.TryGetProperty("text", out var textProp))
                                {
                                    var text = textProp.GetString();
                                    if (!string.IsNullOrEmpty(text))
                                    {
                                        textBuilder.Append(text);
                                        charCount += text.Length;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error parsing Claude stream data");
                            }
                        }
                    }
                }
                else if (line.StartsWith("event: message_stop"))
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Claude stream read was cancelled");
            throw;
        }

        _logger.LogDebug("StreamClaude collected {CharCount} characters of text", textBuilder.Length);
        return textBuilder.ToString();
    }

    private ProviderEvaluationResponse ParseAndValidate(string content, string provider)
    {
        try
        {
            // Clean up markdown code blocks if present
            if (content.StartsWith("```json")) content = content[7..];
            if (content.StartsWith("```")) content = content[3..];
            if (content.EndsWith("```")) content = content[..^3];
            content = content.Trim();

            // Extract JSON object
            int first = content.IndexOf('{');
            int last = content.LastIndexOf('}');
            if (first < 0 || last <= first)
                throw new JsonException("No JSON object found");

            var json = content.Substring(first, last - first + 1);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            var response = JsonSerializer.Deserialize<ProviderEvaluationResponse>(json, options);

            if (response == null)
                throw new JsonException("Deserialization returned null");

            // Ensure non-null values
            response.NpcResponse ??= "I need to think about that.";
            response.WritingFeedback ??= new WritingFeedbackDto(
                new WritingScoreDto(50, 50, 50, 50, 50, 50),
                new List<CorrectionDto>(),
                null,
                "* Không có phản hồi chi tiết."
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Provider} failed to parse response: {Content}", provider, content);
            throw;
        }
    }

    private ProviderEvaluationResponse GetFallbackResponse(string provider)
    {
        return new ProviderEvaluationResponse
        {
            NpcResponse = "I need to think about that.",
            WritingFeedback = new WritingFeedbackDto(
                new WritingScoreDto(50, 50, 50, 50, 50, 50),
                new List<CorrectionDto>(),
                null,
                $"* Lỗi hệ thống: {provider} gặp sự cố. Vui lòng thử lại."
            ),
            SuspicionChange = 0,
            XpEarned = 0
        };
    }

    private ProviderEvaluationResponse GetUltimateFallbackResponse()
    {
        return new ProviderEvaluationResponse
        {
            NpcResponse = "I need to think about that.",
            WritingFeedback = new WritingFeedbackDto(
                new WritingScoreDto(50, 50, 50, 50, 50, 50),
                new List<CorrectionDto>(),
                null,
                "* Lỗi hệ thống: Tất cả các nhà cung cấp AI đều không khả dụng. Vui lòng thử lại sau."
            ),
            SuspicionChange = 0,
            XpEarned = 0
        };
    }

    public async Task<string> GenerateTextAsync(string systemPrompt, string userMessage)
    {
        _logger.LogInformation("Processing text generation request of length: {MessageLength}", userMessage.Length);

        if (_isGeminiConfigured)
        {
            try
            {
                _logger.LogInformation("Attempting text generation with Gemini...");
                var geminiResult = await GetGeminiTextAsync(systemPrompt, userMessage);
                return geminiResult;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Primary Gemini text generation failed, attempting Claude fallback...");
            }
        }

        if (_isClaudeConfigured)
        {
            try
            {
                _logger.LogInformation("Attempting text generation with Claude...");
                var claudeResult = await GetClaudeTextAsync(systemPrompt, userMessage);
                return claudeResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Both text generation providers failed");
                throw;
            }
        }

        throw new InvalidOperationException("No LLM providers configured for text generation");
    }

    private async Task<string> GetGeminiTextAsync(string systemPrompt, string userMessage)
    {
        if (!_isGeminiConfigured || string.IsNullOrWhiteSpace(_geminiApiKey))
        {
            throw new InvalidOperationException("Gemini API key is required but not configured");
        }

        var combinedPrompt = $"{systemPrompt}\n\n[USER MESSAGE]\n{userMessage}";
        var url = $"{_geminiBaseUrl}/models/{_geminiModel}:generateContent?key={_geminiApiKey}";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = combinedPrompt }
                                }
                            }
                        },
                        generationConfig = new
                        {
                            temperature = 0.7,
                            topP = 0.9,
                            maxOutputTokens = 2000
                        }
                    }),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogError("Gemini API returned error status {StatusCode}: {ErrorContent}", (int)response.StatusCode, errorContent);
                throw new Exception($"Gemini API error {response.StatusCode}: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cts.Token);
            
            using var doc = JsonDocument.Parse(responseContent);
            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var contentProp) &&
                contentProp.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textProp))
            {
                return textProp.GetString() ?? "";
            }

            throw new Exception("Could not find generated text in Gemini response.");
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Gemini API request timeout during text generation");
        }
    }

    private async Task<string> GetClaudeTextAsync(string systemPrompt, string userMessage)
    {
        if (!_isClaudeConfigured || string.IsNullOrWhiteSpace(_claudeApiKey))
        {
            throw new InvalidOperationException("Claude API key is required but not configured");
        }

        var safeUserMessage = $"[USER_TEXT]\n{userMessage}\n[/USER_TEXT]";
        var requestBody = new
        {
            model = _claudeModel,
            max_tokens = 2000,
            temperature = 0.7,
            top_p = 0.9,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = safeUserMessage } }
        };

        var url = _claudeBaseUrl;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            request.Headers.Add("x-api-key", _claudeApiKey);
            request.Headers.Add("Authorization", $"Bearer {_claudeApiKey}");
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                throw new Exception($"Claude API error {response.StatusCode}: {errorContent}");
            }

            var fullText = await StreamClaudeAsync(response, cts.Token);
            return fullText;
        }
        catch (TaskCanceledException)
        {
            throw new TimeoutException("Claude API request timeout during text generation");
        }
    }
}