using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Exceptions;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class AIEvaluationService : IAIEvaluationService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _context;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly string _model;

    public AIEvaluationService(
        HttpClient httpClient,
        AppDbContext context,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _context = context;
        _apiKey = configuration["LlmApi:ApiKey"] ?? "dummy_key";
        _baseUrl = configuration["LlmApi:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
        _model = configuration["LlmApi:Model"] ?? "claude-sonnet-4-6";
    }

    public async Task<DialogueResponseDto> EvaluateMessageAsync(int userId, int missionId, string playerMessage)
    {
        // 1. Fetch User, Mission, NPC, and current progress
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new DomainException("User not found.");

        var mission = await _context.Missions
            .Include(m => m.Npc)
            .FirstOrDefaultAsync(m => m.Id == missionId);
        if (mission == null || mission.ApprovalStatus != ApprovalStatus.Approved) 
            throw new DomainException("Mission not found or not approved.");

        var npc = mission.Npc;
        if (npc == null) throw new DomainException("NPC details not found for this mission.");

        // Lazy Energy Recharge
        RechargeEnergyLazy(user);

        if (user.Energy < 5)
        {
            throw new Exception("Not enough energy. Each message requires 5 energy.");
        }

        // Get recent conversation history for context (last 10 turns)
        var historyList = await _context.Dialogues
            .Where(d => d.UserId == userId && d.MissionId == missionId)
            .OrderByDescending(d => d.Timestamp)
            .Take(10)
            .ToListAsync();

        // Sort ascending in memory for chronological dialogue flow
        var history = historyList.OrderBy(d => d.Timestamp).ToList();

        var historyBlock = new System.Text.StringBuilder();
        if (history.Count > 0)
        {
            historyBlock.AppendLine("\n--- CONVERSATION HISTORY (most recent turns) ---");
            foreach (var entry in history)
            {
                historyBlock.AppendLine($"Player: {entry.PlayerMessage}");
                historyBlock.AppendLine($"{npc.Name}: {entry.NpcResponse}");
            }
            historyBlock.AppendLine("--- END OF HISTORY ---");
        }

        // 2. Send request to LLM with CEFR dynamic constraint
        string cefrInstructions = user.EnglishLevel switch
        {
            "A1" or "A2" => "Use very simple English vocabulary (A1-A2 level). Write short, direct, simple sentences. Be extremely forgiving with grammar/vocabulary mistakes. If the user makes an error, gently correct it and explain simply in Vietnamese.",
            "B1" or "B2" => "Use intermediate English (B1-B2 level) with common phrasal verbs, idioms, and expressions. Write moderate-length sentences. Grade grammar strictly but focus on natural conversational flow. Point out unnatural phrasing in Vietnamese.",
            "C1" or "C2" => "Use advanced, nuanced, professional, and highly idiomatic English (C1-C2 level). Use complex sentence structures, passive voice, and subtext. Grade their grammar and vocabulary VERY strictly. If their sentence is correct but basic, suggest a more sophisticated/native-like alternative in Vietnamese.",
            _ => "Use intermediate English (B1 level)."
        };

        string systemPrompt = $@"You are {npc.Name}, a {npc.Role} in a cyberpunk English-learning roleplay game.
CHARACTER PROFILE:
- Name: {npc.Name}
- Role: {npc.Role}
- Personality: {npc.Personality}
- Description: {npc.Description}

MISSION GOAL: {mission.Goal}
MISSION SCENARIO: {mission.Description}
MISSION GRAMMAR TARGET (MỤC TIÊU NGỮ PHÁP): {mission.GrammarTarget}

CURRENT STATE:
- Turns played: {history.Count}

{historyBlock}

PLAYER ENGLISH LEVEL: {user.EnglishLevel}
YOUR LANGUAGE CONSTRAINT & CEFR RULES:
{cefrInstructions}

ROLEPLAY & EVALUATION RULES:
1. Stay in character as {npc.Name} at all times. Never break character in the 'npcResponse' field.
2. The complexity of your English in 'npcResponse' MUST match the player's {user.EnglishLevel} level constraint.
3. Perform a strict, word-by-word spelling, capitalization, and grammar evaluation of the PLAYER'S message (""{playerMessage}""):
   - Identify typos (e.g. ""cofffe"" -> ""coffee"", ""i"" -> ""I""). If there are typos, you MUST document them in the 'Sửa lỗi' section; do NOT say 'Không có lỗi'.
   - Grade the player's grammar and naturalness based on their level ({user.EnglishLevel}).
   - **CRITICAL GRAMMAR QUEST TARGET CHECK**: Verify if the player successfully used or attempted the MISSION GRAMMAR TARGET (""{mission.GrammarTarget}""):
     * If they SUCCESSFULY applied the target structure: Reduce suspicion significantly (suspicionChange = -15 to -5) and award higher XP (xpEarned = 15 to 20).
     * If they completely IGNORED or FAILED the target structure: Penalize them by increasing suspicion (suspicionChange = +5 to +20) and award minimal XP (xpEarned = 0 to 5).
     * Otherwise, for general spelling/grammar errors, Bad spelling/grammar should INCREASE suspicion (+10 to +30), while correct/natural phrasing should DECREASE suspicion (-10 to 0).
4. Provide a constructive English coaching tip for the PLAYER in the 'feedback' field:
   - CRITICAL: THIS FEEDBACK MUST BE FOR THE PLAYER'S MESSAGE (""{playerMessage}""). Do NOT review or mention your own NPC response (""npcResponse"") in this feedback field.
   - CRITICAL L10N RULE: THIS FEEDBACK MUST BE WRITTEN IN VIETNAMESE.
   - Format the feedback string strictly using this structure:
     * Sửa lỗi (nếu có): [Nếu người chơi viết sai chính tả, viết thường đầu câu, hay sai ngữ pháp, hãy ghi rõ lỗi và sửa lại ở đây. Nếu không có lỗi nào, ghi: ""Không phát hiện lỗi.""]
     * Diễn đạt tự nhiên hơn: [Cách viết trôi chảy, bản xứ hơn cho ý định của người chơi]
     * Giải thích ngắn gọn: [Giải thích quy tắc hoặc từ vựng bằng tiếng Việt. Bạn BẮT BUỘC nhận xét rõ người chơi đã đạt mục tiêu ngữ pháp ""{mission.GrammarTarget}"" hay chưa, giải thích cấu trúc đó một cách ngắn gọn.]
5. Output MUST be a single, valid JSON object with exactly the following structure (no markdown formatting, no other text):
{{
  ""npcResponse"": ""your dialogue response in character (in English, adapted to CEFR)"",
  ""feedback"": ""helpful out-of-character English coaching tip (IN VIETNAMESE, strictly formatted as specified above)"",
  ""suspicionChange"": integer (-20 to 30),
  ""xpEarned"": integer (0 to 20)
}}";

        var safeUserMessage = $"[USER_TEXT]\n{playerMessage}\n[/USER_TEXT]";
        var combinedPrompt = $"{systemPrompt}\n\nUser Message: {safeUserMessage}";

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

        GeminiResponse? geminiResponse = null;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            // Use ResponseHeadersRead to begin streaming immediately without buffering the entire body
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Do NOT throw — set fallback response so the game degrades gracefully
                geminiResponse = GetFallbackResponse($"Claude API returned status code {response.StatusCode}. Details: {errorContent}");
            }
            else
            {
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
                        geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(contentString);
                    }
                    catch (JsonException jsonEx)
                    {
                        geminiResponse = GetFallbackResponse($"JSON Deserialization failed: {jsonEx.Message}. Raw text: {contentString}");
                    }
                }
                else
                {
                    geminiResponse = GetFallbackResponse($"No JSON object found in response. Raw text: {contentString}");
                }
            }
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            // Network errors, timeouts, JSON parse failures → graceful fallback
            geminiResponse = GetFallbackResponse($"{ex.GetType().Name}: {ex.Message}");
        }

        if (geminiResponse == null)
        {
            geminiResponse = GetFallbackResponse("Unknown parse error (geminiResponse was null).");
        }

        // Clamp suspicionChange and xpEarned to target constraints
        geminiResponse.SuspicionChange = Math.Clamp(geminiResponse.SuspicionChange, -20, 30);
        geminiResponse.XpEarned = Math.Clamp(geminiResponse.XpEarned, 0, 20);

        // 3. Database Updates inside a Database Transaction for consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Deduct Energy
            user.Energy -= 5;

            // Fetch or Initialize User Progress
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.MissionId == missionId);

            if (progress == null)
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    MissionId = missionId,
                    CurrentSuspicion = mission.StartSuspicion,
                    Status = MissionStatus.InProgress,
                    TurnCount = 0,
                    XpEarned = 0
                };
                await _context.UserProgresses.AddAsync(progress);
            }
            else if (progress.Status != MissionStatus.InProgress)
            {
                // Reset progress state if replaying a completed/failed mission
                progress.CurrentSuspicion = mission.StartSuspicion;
                progress.Status = MissionStatus.InProgress;
                progress.TurnCount = 0;
                progress.XpEarned = 0;
            }

            // Update Progress values
            progress.TurnCount += 1;
            progress.CurrentSuspicion += geminiResponse.SuspicionChange;
            progress.CurrentSuspicion = Math.Clamp(progress.CurrentSuspicion, 0, mission.MaxSuspicion);
            progress.XpEarned += geminiResponse.XpEarned;
            progress.LastActivity = DateTime.UtcNow;

            // Check Win/Lose Conditions
            bool isLose = progress.CurrentSuspicion >= mission.MaxSuspicion;
            bool isWin = !isLose && progress.TurnCount >= 10 && progress.CurrentSuspicion < 50;

            if (isWin) progress.Status = MissionStatus.Completed;
            else if (isLose) progress.Status = MissionStatus.Failed;

            // Add XP to User Balance
            user.XpBalance += geminiResponse.XpEarned;

            // Create Dialogue record
            var dialogue = new Dialogue
            {
                UserId = userId,
                NpcId = npc.Id,
                MissionId = missionId,
                PlayerMessage = playerMessage,
                NpcResponse = geminiResponse.NpcResponse,
                Feedback = geminiResponse.Feedback,
                SuspicionChange = geminiResponse.SuspicionChange,
                Timestamp = DateTime.UtcNow
            };
            await _context.Dialogues.AddAsync(dialogue);

            // Persist all DB changes
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            return new DialogueResponseDto(
                geminiResponse.NpcResponse,
                geminiResponse.Feedback,
                progress.CurrentSuspicion,
                isWin,
                isLose,
                progress.TurnCount,
                geminiResponse.XpEarned
            );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> GenerateHintAsync(int userId, int missionId)
    {
        var mission = await _context.Missions
            .Include(m => m.Npc)
            .FirstOrDefaultAsync(m => m.Id == missionId);
        if (mission == null) throw new DomainException("Mission not found.");

        var npc = mission.Npc;
        if (npc == null) throw new DomainException("NPC details not found for this mission.");

        var historyList = await _context.Dialogues
            .Where(d => d.UserId == userId && d.MissionId == missionId)
            .OrderByDescending(d => d.Timestamp)
            .Take(10)
            .ToListAsync();

        var history = historyList.OrderBy(d => d.Timestamp).ToList();

        var historyBlock = new System.Text.StringBuilder();
        if (history.Count > 0)
        {
            historyBlock.AppendLine("\n--- CONVERSATION HISTORY ---");
            foreach (var entry in history)
            {
                historyBlock.AppendLine($"Player: {entry.PlayerMessage}");
                historyBlock.AppendLine($"{npc.Name}: {entry.NpcResponse}");
            }
            historyBlock.AppendLine("--- END OF HISTORY ---");
        }

        string systemPrompt = $@"You are the AI game master for a cyberpunk English-learning roleplay game.
The player is currently playing a mission:
- Mission Goal: {mission.Goal}
- Mission Scenario: {mission.Description}
- NPC: {npc.Name} (Role: {npc.Role}, Personality: {npc.Personality})

Here is the recent conversation history:
{historyBlock}

The player has used a 'Hint' item because they are stuck and don't know how to reply to the NPC {npc.Name} in English.
Task:
1. Suggest a short, polite, or natural English sentence that the player could use to reply to the NPC in this exact situation.
2. The entire response and explanation MUST be written in Vietnamese so that the player can easily understand the coaching hint.
3. Keep the hint short, highly actionable, and encouraging (maximum 3 sentences).
4. Do NOT output any JSON, markdown code block backticks (like ```), or formatting. Just output the plain Vietnamese text directly.";

        var messages = new[]
        {
            new { role = "user", content = "Hãy gợi ý một câu tiếng Anh để trả lời NPC." }
        };

        var requestBody = new
        {
            model = _model,
            max_tokens = 1024,
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Claude API returned {response.StatusCode}. Details: {errorContent}");
            }

            string contentString = string.Empty;
            var textBuilder = new System.Text.StringBuilder();

            try
            {
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
                                            break;
                                        }

                                        if (doc.RootElement.TryGetProperty("delta", out var deltaProp) &&
                                            deltaProp.TryGetProperty("text", out var textProp))
                                        {
                                            textBuilder.Append(textProp.GetString());
                                        }
                                    }
                                    catch
                                    {
                                        // Ignore malformed JSON lines
                                    }
                                }
                            }
                            else if (line.StartsWith("event: message_stop"))
                            {
                                break;
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
                // If we already have some text, ignore the network termination exception
                if (textBuilder.Length == 0)
                {
                    throw;
                }
            }

            contentString = textBuilder.ToString().Trim();
            return contentString;
        }
        catch (Exception ex)
        {
            return $"Không thể kết nối với AI gợi ý: {ex.Message}. Bạn hãy thử trả lời NPC một cách lịch sự và tự nhiên.";
        }
    }

    public async Task<GameSessionDto> GetActiveSessionAsync(int userId, int missionId)
    {
        var progress = await _context.UserProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.MissionId == missionId);

        if (progress == null || progress.Status != MissionStatus.InProgress)
        {
            return new GameSessionDto { HasActiveSession = false };
        }

        var history = await _context.Dialogues
            .Where(d => d.UserId == userId && d.MissionId == missionId)
            .OrderBy(d => d.Timestamp)
            .ToListAsync();

        return new GameSessionDto
        {
            HasActiveSession = true,
            CurrentSuspicion = progress.CurrentSuspicion,
            TurnCount = progress.TurnCount,
            XpEarned = progress.XpEarned,
            History = history.Select(h => new DialogueMessageHistoryDto
            {
                Role = h.PlayerMessage == null ? "npc" : "player", // If PlayerMessage exists, it was player turn. Let's make it robust: we can map or distinguish.
                PlayerMessage = h.PlayerMessage,
                NpcResponse = h.NpcResponse,
                Feedback = h.Feedback,
                SuspicionChange = h.SuspicionChange
            }).ToList()
        };
    }

    public async Task<bool> ResetSessionAsync(int userId, int missionId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.MissionId == missionId);

            var mission = await _context.Missions.FirstOrDefaultAsync(m => m.Id == missionId);
            var startSuspicion = mission?.StartSuspicion ?? 10;

            if (progress != null)
            {
                progress.CurrentSuspicion = startSuspicion;
                progress.Status = MissionStatus.InProgress;
                progress.TurnCount = 0;
                progress.XpEarned = 0;
                progress.LastActivity = DateTime.UtcNow;
                _context.UserProgresses.Update(progress);
            }
            else
            {
                progress = new UserProgress
                {
                    UserId = userId,
                    MissionId = missionId,
                    CurrentSuspicion = startSuspicion,
                    Status = MissionStatus.InProgress,
                    TurnCount = 0,
                    XpEarned = 0,
                    LastActivity = DateTime.UtcNow
                };
                await _context.UserProgresses.AddAsync(progress);
            }

            // Remove all dialogues
            var dialogues = await _context.Dialogues
                .Where(d => d.UserId == userId && d.MissionId == missionId)
                .ToListAsync();

            if (dialogues.Any())
            {
                _context.Dialogues.RemoveRange(dialogues);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private void RechargeEnergyLazy(User user)
    {
        var now = DateTime.UtcNow;
        var timeElapsed = now - user.LastEnergyRechargedAt;

        if (timeElapsed.TotalMinutes >= 30)
        {
            int intervals = (int)(timeElapsed.TotalMinutes / 30);
            int energyToRecharge = intervals * 10;

            user.Energy = Math.Min(user.MaxEnergy, user.Energy + energyToRecharge);
            user.LastEnergyRechargedAt = user.LastEnergyRechargedAt.AddMinutes(intervals * 30);
        }
    }

    private GeminiResponse GetFallbackResponse(string technicalDetails)
    {
        return new GeminiResponse
        {
            NpcResponse = "I didn't quite catch that. Can you repeat it?",
            Feedback = $"* Sửa lỗi (nếu có): Không phát hiện lỗi.\n* Diễn đạt tự nhiên hơn:\n* Giải thích ngắn gọn: Hệ thống AI đang tạm thời quá tải hoặc gặp lỗi kết nối. Vui lòng gửi lại câu trả lời sau giây lát.\nChi tiết kỹ thuật: {technicalDetails}",
            SuspicionChange = 0,
            XpEarned = 0
        };
    }

    private class GeminiResponse
    {
        [JsonPropertyName("npcResponse")]
        public string NpcResponse { get; set; } = string.Empty;

        [JsonPropertyName("feedback")]
        public string Feedback { get; set; } = string.Empty;

        [JsonPropertyName("suspicionChange")]
        public int SuspicionChange { get; set; }

        [JsonPropertyName("xpEarned")]
        public int XpEarned { get; set; }
    }
}
