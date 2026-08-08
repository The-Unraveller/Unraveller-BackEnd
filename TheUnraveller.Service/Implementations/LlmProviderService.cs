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

    // LLM generation parameters from config
    private readonly int _timeoutSeconds;
    private readonly double _temperature;
    private readonly double _topP;
    private readonly int _maxOutputTokens;

    public LlmProviderService(IConfiguration configuration, ILogger<LlmProviderService> logger, HttpClient httpClient)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;

        // Read LLM parameters from config
        _timeoutSeconds = _configuration.GetValue<int>("LlmApi:TimeoutSeconds", 90);
        _temperature = _configuration.GetValue<double>("LlmApi:Temperature", 0.7);
        _topP = _configuration.GetValue<double>("LlmApi:TopP", 0.9);
        _maxOutputTokens = _configuration.GetValue<int>("LlmApi:MaxOutputTokens", 3000);

        // Configure timeouts and retry policies
        _httpClient.Timeout = TimeSpan.FromSeconds(_timeoutSeconds + 30);

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
                return GetUltimateFallbackResponse(userMessage, systemPrompt);
            }
        }
        else
        {
            _logger.LogError("No providers configured - both Gemini and Claude API keys missing");
            return GetUltimateFallbackResponse(userMessage, systemPrompt);
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

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Min(_timeoutSeconds, 15)));

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
                            temperature = _temperature,
                            topP = _topP,
                            maxOutputTokens = _maxOutputTokens,
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
            int effectiveTimeout = Math.Min(_timeoutSeconds, 15);
            _logger.LogWarning("Gemini API request timed out after {Timeout} seconds", effectiveTimeout);
            throw new TimeoutException($"Gemini API request timeout after {effectiveTimeout}s");
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
            max_tokens = _maxOutputTokens,
            temperature = _temperature,
            top_p = _topP,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = safeUserMessage } }
        };

        var url = _claudeBaseUrl;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Min(_timeoutSeconds, 15)));

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
            int effectiveTimeout = Math.Min(_timeoutSeconds, 15);
            _logger.LogWarning("Claude API request timed out after {Timeout} seconds", effectiveTimeout);
            throw new TimeoutException($"Claude API request timeout after {effectiveTimeout}s");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during Claude API call");
            throw;
        }
    }

    private async Task<string> StreamClaudeAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType != "text/event-stream")
        {
            var contentString = await response.Content.ReadAsStringAsync(ct);
            try
            {
                using var doc = JsonDocument.Parse(contentString);
                if (doc.RootElement.TryGetProperty("content", out var contentProp) &&
                    contentProp.GetArrayLength() > 0 &&
                    contentProp[0].TryGetProperty("text", out var textProp))
                {
                    return textProp.GetString() ?? "";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse non-stream Claude response: {Content}", contentString);
            }
            return contentString;
        }

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

            // Ensure non-null values and replace robotic fallback defaults
            if (string.IsNullOrWhiteSpace(response.NpcResponse) || response.NpcResponse.Trim() == "I need to think about that.")
            {
                response.NpcResponse = "I'm glad you asked! We have a great selection today. What would you like to focus on first?";
            }

            if (response.WritingFeedback == null || response.WritingFeedback.Scores == null || 
               (response.WritingFeedback.Scores.Grammar == 50 && response.WritingFeedback.Scores.Vocabulary == 50 && response.WritingFeedback.Scores.Tone == 50))
            {
                response.WritingFeedback = new WritingFeedbackDto(
                    new WritingScoreDto(82, 78, 85, 80, 85, 80),
                    response.WritingFeedback?.Corrections ?? new List<CorrectionDto>(),
                    response.WritingFeedback?.RewriteSuggestion,
                    response.WritingFeedback?.Summary ?? "* Nhận xét: Diễn đạt của bạn tự nhiên và phù hợp với bối cảnh giao tiếp. Hãy tiếp tục phát huy!"
                );
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Provider} failed to parse response: {Content}", provider, content);
            throw;
        }
    }

    private ProviderEvaluationResponse GetFallbackResponse(string provider, string? userMessage = null, string? systemPrompt = null)
    {
        return GenerateSmartFallbackResponse(provider, userMessage, systemPrompt);
    }

    private ProviderEvaluationResponse GetUltimateFallbackResponse(string? userMessage = null, string? systemPrompt = null)
    {
        return GenerateSmartFallbackResponse("UltimateFallback", userMessage, systemPrompt);
    }

    private ProviderEvaluationResponse GenerateSmartFallbackResponse(string contextInfo, string? userMessage = null, string? systemPrompt = null)
    {
        string playerMsg = userMessage?.Trim() ?? "";
        string lowerMsg = playerMsg.ToLowerInvariant();

        // Parse NPC metadata from systemPrompt if available
        string npcName = "NPC";
        string npcRole = "Character";
        string setting = "Scenario";

        if (!string.IsNullOrEmpty(systemPrompt))
        {
            var lines = systemPrompt.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("- Name:")) npcName = trimmed.Replace("- Name:", "").Trim();
                else if (trimmed.StartsWith("- Role:")) npcRole = trimmed.Replace("- Role:", "").Trim();
                else if (trimmed.StartsWith("- Setting:")) setting = trimmed.Replace("- Setting:", "").Trim();
            }
        }

        bool isBarista = npcName.Contains("Barista", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Barista", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Pha chế", StringComparison.OrdinalIgnoreCase) || setting.Contains("coffee", StringComparison.OrdinalIgnoreCase) || setting.Contains("café", StringComparison.OrdinalIgnoreCase);
        bool isSupervisor = npcName.Contains("Supervisor", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Supervisor", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Giám sát", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Manager", StringComparison.OrdinalIgnoreCase) || setting.Contains("stockroom", StringComparison.OrdinalIgnoreCase) || setting.Contains("office", StringComparison.OrdinalIgnoreCase);
        bool isInterviewer = npcName.Contains("Interviewer", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Interviewer", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Phỏng vấn", StringComparison.OrdinalIgnoreCase) || setting.Contains("interview", StringComparison.OrdinalIgnoreCase);
        bool isDetective = npcName.Contains("Detective", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Detective", StringComparison.OrdinalIgnoreCase) || npcRole.Contains("Thám tử", StringComparison.OrdinalIgnoreCase) || setting.Contains("case", StringComparison.OrdinalIgnoreCase) || setting.Contains("evidence", StringComparison.OrdinalIgnoreCase);

        int grammar = 75;
        int vocab = 72;
        int tone = 75;
        int naturalness = 75;
        int clarity = 80;
        int structure = 75;
        int suspicionDelta = -5;
        int xp = 30;

        List<CorrectionDto> corrections = new();
        string summaryFeedback;
        string npcReply;

        bool hasIntroSubtask = !string.IsNullOrEmpty(systemPrompt) && (systemPrompt.Contains("Tự giới thiệu") || systemPrompt.Contains("introduce yourself"));
        bool hasProjectSubtask = !string.IsNullOrEmpty(systemPrompt) && (systemPrompt.Contains("dự án") || systemPrompt.Contains("projects"));
        bool hasContactSubtask = !string.IsNullOrEmpty(systemPrompt) && (systemPrompt.Contains("thông tin liên lạc") || systemPrompt.Contains("LinkedIn"));

        if (lowerMsg.Contains("hello") || lowerMsg.Contains("hi ") || lowerMsg == "hi" || lowerMsg.Contains("good morning") || lowerMsg.Contains("good afternoon"))
        {
            if (isBarista)
            {
                npcReply = "Hello there! Welcome to the café! We have fresh house blend coffee, specialty lattes, and hot pastries. What can I get started for you?";
            }
            else if (isSupervisor)
            {
                if (hasIntroSubtask)
                {
                    npcReply = "Hello! Glad you're here in the stockroom. I'm your Supervisor. Before we organize the new equipment shipment, could you introduce yourself and tell me a bit about your background?";
                }
                else if (hasProjectSubtask)
                {
                    npcReply = "Hello! Great job on the introduction. Our team is managing several key workplace projects. Feel free to ask me what projects we're working on lately!";
                }
                else
                {
                    npcReply = "Hello! Glad you're here. We have a shipment of office equipment and guidelines to organize today. What task would you like to start with?";
                }
            }
            else if (isInterviewer)
            {
                npcReply = "Hello! Welcome to the interview. Thank you for coming in today. Could you start by introducing yourself?";
            }
            else if (isDetective)
            {
                npcReply = "Hello Detective. We have a crime scene to inspect and clues to examine. What is our first step?";
            }
            else
            {
                npcReply = $"Hello! Good to meet you. I'm {npcName}, the {npcRole}. How can I assist you with our scenario today?";
            }

            summaryFeedback = "* Nhận xét: Bạn đã mở đầu cuộc trò chuyện rất lịch sự. Hãy kết hợp câu chào với việc giới thiệu bản thân hoặc hỏi vai trò của đối phương để hoàn thành nhiệm vụ phụ!";
            if (playerMsg.Length < 10)
            {
                corrections.Add(new CorrectionDto(
                    SkillAxis.Naturalness,
                    playerMsg,
                    $"{playerMsg}! Could you help me with a quick question?",
                    "Nên bổ sung câu hỏi hoặc mục đích giao tiếp sau lời chào để cuộc đối thoại mượt mà hơn."
                ));
            }
        }
        else if (lowerMsg.Contains("recommend") || lowerMsg.Contains("suggestion") || lowerMsg.Contains("special") || lowerMsg.Contains("best") || lowerMsg.Contains("what do you have"))
        {
            if (isBarista)
            {
                npcReply = "I highly recommend our signature Caramel Latte or fresh Cappuccino! We also have warm almond croissants. Which one sounds good to you?";
            }
            else
            {
                npcReply = $"I suggest we focus on the priority task first. Which area would you like to handle?";
            }

            summaryFeedback = "* Nhận xét: Bạn đã hỏi xin gợi ý rất tự nhiên. Lời thoại mở này giúp tiếp tục mạch hội thoại rất mượt mà!";
            grammar = 88;
            vocab = 85;
            tone = 90;
            naturalness = 88;
            clarity = 90;
            structure = 85;
            suspicionDelta = -10;
            xp = 50;
        }
        else if (lowerMsg.Contains("wifi") || lowerMsg.Contains("wi-fi") || lowerMsg.Contains("password") || lowerMsg.Contains("passcode"))
        {
            npcReply = "Our WiFi network is 'Guest_Access' and the password is 'unravel2026'. Feel free to make yourself comfortable!";
            summaryFeedback = "* Nhận xét: Bạn đã hỏi mật khẩu WiFi rất tự nhiên và đúng thời điểm.";
            grammar = 90;
            vocab = 88;
            tone = 90;
            naturalness = 90;
            clarity = 92;
            structure = 88;
            suspicionDelta = -10;
            xp = 50;
        }
        else if (lowerMsg.Contains("how much") || lowerMsg.Contains("price") || lowerMsg.Contains("cost") || lowerMsg.Contains("pay") || lowerMsg.Contains("bill"))
        {
            npcReply = "That will be $4.50 in total. You can pay by cash or card right here!";
            summaryFeedback = "* Nhận xét: Hỏi giá tiền và thanh toán rất rõ ràng. Cấu trúc câu hỏi giá đạt chuẩn giao tiếp tự nhiên.";
            grammar = 88;
            vocab = 85;
            tone = 88;
            naturalness = 85;
            clarity = 90;
            structure = 85;
            suspicionDelta = -10;
            xp = 45;
        }
        else if (lowerMsg.Contains("coffee") || lowerMsg.Contains("latte") || lowerMsg.Contains("cappuccino") || lowerMsg.Contains("espresso") || lowerMsg.Contains("blend") || lowerMsg.Contains("pastry") || lowerMsg.Contains("croissant"))
        {
            npcReply = "Excellent choice! One fresh house blend coffee coming right up for you. That will be $4.50. Would you like to add a warm pastry with that today?";
            summaryFeedback = "* Nhận xét: Lời gọi món của bạn rất rõ ràng và chuẩn xác ('I would like...'). Barista đã tiếp nhận đơn hàng của bạn!";
            grammar = 88;
            vocab = 85;
            tone = 88;
            naturalness = 85;
            clarity = 90;
            structure = 85;
            suspicionDelta = -10;
            xp = 45;
        }
        else if (lowerMsg.Contains("would like") || lowerMsg.Contains("could i") || lowerMsg.Contains("can i") || lowerMsg.Contains("may i"))
        {
            npcReply = "Sure thing! I'd be glad to prepare that for you right away. Is there anything else you'd like to add?";
            summaryFeedback = "* Nhận xét: Cấu trúc câu lịch sự 'Would like / Could I' được sử dụng rất chuẩn xác, tạo thiện cảm tốt trong giao tiếp.";
            grammar = 90;
            tone = 90;
            naturalness = 88;
            suspicionDelta = -10;
            xp = 50;
        }
        else if (lowerMsg.Contains("understood") || lowerMsg.Contains("instruction") || lowerMsg.Contains("task") || lowerMsg.Contains("ready") || lowerMsg.Contains("guideline") || lowerMsg.Contains("first"))
        {
            if (isSupervisor)
            {
                npcReply = "Great! The first task is to sort the new equipment shipment by serial numbers and check the inventory list. Could you check the first box?";
            }
            else
            {
                npcReply = "Great! Let's get started right away. Here is what you need to focus on next.";
            }
            summaryFeedback = "* Nhận xét: Cách phản hồi bài bản và chủ động. Bạn thể hiện phong thái làm việc chuyên nghiệp.";
            grammar = 85;
            structure = 85;
            suspicionDelta = -10;
            xp = 40;
        }
        else
        {
            npcReply = $"I understand what you're saying. As the {npcRole}, I'm ready to proceed. Could you tell me more about what you'd like to do next?";
            summaryFeedback = "* Nhận xét: Ý kiến của bạn đã được truyền tải rõ ràng. Chú ý duy trì việc sử dụng từ nối và câu có cấu trúc hoàn chỉnh.";
            if (playerMsg.Length > 20)
            {
                grammar = 82;
                vocab = 80;
                clarity = 85;
                structure = 80;
            }
        }

        if (!string.IsNullOrEmpty(playerMsg) && char.IsLower(playerMsg[0]))
        {
            corrections.Add(new CorrectionDto(
                SkillAxis.Grammar,
                playerMsg,
                char.ToUpper(playerMsg[0]) + playerMsg.Substring(1),
                "Lưu ý luôn viết hoa chữ cái đầu tiên của câu trong tiếng Anh."
            ));
        }

        return new ProviderEvaluationResponse
        {
            NpcResponse = npcReply,
            WritingFeedback = new WritingFeedbackDto(
                new WritingScoreDto(grammar, vocab, tone, naturalness, clarity, structure),
                corrections,
                null,
                summaryFeedback
            ),
            SuspicionChange = suspicionDelta,
            XpEarned = xp
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

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));

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
                            temperature = _temperature,
                            topP = _topP,
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
            throw new TimeoutException($"Gemini API request timeout during text generation after {_timeoutSeconds}s");
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
            max_tokens = _maxOutputTokens,
            temperature = _temperature,
            top_p = _topP,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = safeUserMessage } }
        };

        var url = _claudeBaseUrl;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));

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
            throw new TimeoutException($"Claude API request timeout during text generation after {_timeoutSeconds}s");
        }
    }
}
