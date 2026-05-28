using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheUnraveller.Core.Entities;
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
        _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
    }

    public async Task<DialogueResponseDto> EvaluateMessageAsync(int userId, int missionId, string playerMessage)
    {
        // 1. Fetch User, Mission, NPC, and current progress
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception("User not found.");

        var mission = await _context.Missions
            .Include(m => m.Npc)
            .FirstOrDefaultAsync(m => m.Id == missionId);
        if (mission == null) throw new Exception("Mission not found.");

        var npc = mission.Npc;
        if (npc == null) throw new Exception("NPC details not found for this mission.");

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

        // 2. Send request to LLM (Gemini 1.5 Flash)
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

ROLEPLAY & EVALUATION RULES:
1. Stay in character as {npc.Name} at all times. Never break character in the 'npcResponse' field.
2. Evaluate the player's English message:
   - If their English has grammatical errors, spelling mistakes, or is unnatural/inappropriate for the context, increase 'suspicionChange' (between +5 and +30).
   - If their English is highly fluent, natural, grammatically correct, and polite/appropriate, decrease 'suspicionChange' (between -10 and -1).
   - For neutral responses, 'suspicionChange' can be 0.
3. Award 'xpEarned' based on their effort and correctness (between 0 and 20).
4. Provide a constructive, helpful out-of-character English coaching tip in the 'feedback' field (e.g. correct grammar, offer a better alternative phrasing, or praise their vocabulary).
5. Output MUST be a single, valid JSON object with exactly the following structure (no markdown formatting, no other text):
{{
  ""npcResponse"": ""your dialogue response in character"",
  ""feedback"": ""helpful out-of-character English coaching tip"",
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
                response_mime_type = "application/json"
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
                throw new Exception($"Gemini API returned status code {response.StatusCode}");
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
            // Fallback response in case of API timeout or failure
            geminiResponse = new GeminiResponse
            {
                NpcResponse = "I didn't quite catch that. Can you repeat it?",
                Feedback = $"System Error: Failed to process AI response ({ex.Message}).",
                SuspicionChange = 0,
                XpEarned = 0
            };
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
            bool isWin = !isLose && progress.TurnCount >= 5 && progress.CurrentSuspicion < 50;

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
