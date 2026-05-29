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

    public AIEvaluationService(
        HttpClient httpClient,
        AppDbContext context,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _context = context;
        _apiKey = configuration["LlmApi:ApiKey"] ?? "dummy_key";
        _baseUrl = configuration["LlmApi:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
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

CURRENT STATE:
- Turns played: {history.Count}

{historyBlock}

PLAYER ENGLISH LEVEL: {user.EnglishLevel}
YOUR LANGUAGE CONSTRAINT & CEFR RULES:
{cefrInstructions}

ROLEPLAY & EVALUATION RULES:
1. Stay in character as {npc.Name} at all times. Never break character in the 'npcResponse' field.
2. The complexity of your English in 'npcResponse' MUST perfectly match the player's {user.EnglishLevel} level constraint defined above.
3. Evaluate the player's English message:
   - For A1-A2: Be very forgiving. If there are major errors, correct them simply.
   - For B1-B2: Grade strictly on natural conversational flow.
   - For C1-C2: Grade extremely strictly. Even if correct, suggest sophisticated vocabulary/idioms.
   - Adjust 'suspicionChange' (between -10 and 30) and 'xpEarned' (between 0 and 20) based on their accuracy relative to their {user.EnglishLevel} CEFR level.
4. Provide a constructive, helpful out-of-character English coaching tip in the 'feedback' field.
   - CRITICAL L10N RULE: THIS FEEDBACK MUST BE WRITTEN IN VIETNAMESE. Explain their errors or suggest native-like phrasings according to their level.
5. Output MUST be a single, valid JSON object with exactly the following structure (no markdown formatting, no other text):
{{
  ""npcResponse"": ""your dialogue response in character (in English, adapted to CEFR)"",
  ""feedback"": ""helpful out-of-character English coaching tip (IN VIETNAMESE)"",
  ""suspicionChange"": integer (-10 to 30),
  ""xpEarned"": integer (0 to 20)
}}";

        var safeUserMessage = $"[USER_TEXT]\n{playerMessage}\n[/USER_TEXT]";
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

        var targetUrl = $"{_baseUrl}?key={_apiKey}";
        var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

        string rawLlmResponse = string.Empty;
        GeminiResponse? geminiResponse = null;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Gemini API returned status code {response.StatusCode}. Details: {errorContent}");
            }

            rawLlmResponse = await response.Content.ReadAsStringAsync(cts.Token);
            using var document = JsonDocument.Parse(rawLlmResponse);

            var contentString = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            contentString = contentString?.Trim() ?? string.Empty;
            if (contentString.StartsWith("```json")) contentString = contentString.Substring(7);
            if (contentString.EndsWith("```")) contentString = contentString.Substring(0, contentString.Length - 3);
            contentString = contentString.Trim();

            geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(contentString);
        }
        catch (Exception ex)
        {
            // Throw exception to prevent database transaction (energy deduction, turn count, and dirty dialogues) on system failures
            throw new Exception($"Gemini API error: {ex.Message}");
        }

        if (geminiResponse == null)
        {
            geminiResponse = new GeminiResponse
            {
                NpcResponse = "I didn't quite catch that. Can you repeat it?",
                Feedback = "System Error: Failed to parse NPC response.",
                SuspicionChange = 0,
                XpEarned = 0
            };
        }

        // Clamp suspicionChange and xpEarned to target constraints
        geminiResponse.SuspicionChange = Math.Clamp(geminiResponse.SuspicionChange, -10, 30);
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

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = systemPrompt } } }
            },
            generationConfig = new
            {
                temperature = 0.7
            }
        };

        var targetUrl = $"{_baseUrl}?key={_apiKey}";
        var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var response = await _httpClient.SendAsync(request, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API returned status code {response.StatusCode}");
            }

            var rawLlmResponse = await response.Content.ReadAsStringAsync(cts.Token);
            using var document = JsonDocument.Parse(rawLlmResponse);

            var contentString = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return contentString?.Trim() ?? "Hãy thử chào hỏi NPC và hỏi thêm thông tin một cách lịch sự.";
        }
        catch (Exception ex)
        {
            return $"Không thể kết nối với AI gợi ý: {ex.Message}. Bạn hãy thử trả lời NPC một cách lịch sự và tự nhiên.";
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
