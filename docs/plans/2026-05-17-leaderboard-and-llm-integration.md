# Leaderboard and Configurable LLM Integration Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a dynamic SQLite-backed Leaderboard API, configuration options for OpenAI & Google Gemini API, and wire the React frontend to display live ranking data with high-fidelity micro-interactions.

**Architecture:**
* **Database Schema**: Add `XpEarned` to `UserProgress` to record player progress per mission. Seed mock players (`Minh Khôi`, `Lan Anh`, `Tuấn Khoa`) with realistic progress to populate the leaderboard.
* **LLM Provider Service**: Update `LlmProviderService` to use configurable `BaseUrl` and `Model` parameters, allowing the app to run on OpenAI or Gemini's OpenAI-compatible endpoint.
* **Leaderboard API**: Add a REST endpoint `/api/Leaderboard` querying all user progresses, summing up the XP, and ranking them with dynamic badge assignments.
* **Frontend Connection**: Wire `Result.tsx` to dynamically load the leaderboard from `/api/Leaderboard` and add custom CSS/Tailwind animations.

**Tech Stack:**
* **Backend**: .NET 9 Web API, Entity Framework Core, SQLite
* **Frontend**: React, TypeScript, Vite, Axios, Tailwind CSS, Lucide Icons

---

### Task 1: Update Database Entity and Seed Data

**Files:**
* Modify: `d:\CODE\EXE\BackEnd\TheUnraveller.Core\Entities\UserProgress.cs`
* Modify: `d:\CODE\EXE\BackEnd\TheUnraveller.Infrastructure\Data\AppDbContext.cs`

**Step 1.1: Add XpEarned to UserProgress**
Update `UserProgress.cs` to include a persistent column for tracking XP earned in each mission session:
```csharp
public int XpEarned { get; set; } = 0;
```

**Step 1.2: Seed Leaderboard Users and Progress in AppDbContext**
Modify `OnModelCreating` in `AppDbContext.cs` to:
1. Seed three additional users: `Minh Khôi`, `Lan Anh`, `Tuấn Khoa`.
2. Seed completed `UserProgress` values for all users (including `KHOA_PRO`) so that the total sum of `XpEarned` corresponds precisely to their default mock values (`4800`, `3950`, `3200`, `1250`).

```csharp
// Inside AppDbContext.cs OnModelCreating
// Seed default users
modelBuilder.Entity<User>().HasData(
    new User { Id = 1, Username = "KHOA_PRO", Email = "khoapro@gmail.com", PasswordHash = "HASH1", CreatedAt = DateTime.UtcNow },
    new User { Id = 2, Username = "Minh Khôi", Email = "minhkhoi@gmail.com", PasswordHash = "HASH2", CreatedAt = DateTime.UtcNow },
    new User { Id = 3, Username = "Lan Anh", Email = "lananh@gmail.com", PasswordHash = "HASH3", CreatedAt = DateTime.UtcNow },
    new User { Id = 4, Username = "Tuấn Khoa", Email = "tuankhoa@gmail.com", PasswordHash = "HASH4", CreatedAt = DateTime.UtcNow }
);

// Seed user progresses with earned XP to populate the leaderboard dynamically
modelBuilder.Entity<UserProgress>().HasData(
    // Minh Khôi (User 2) - 4800 XP total
    new UserProgress { Id = 10, UserId = 2, MissionId = 1, CurrentSuspicion = 15, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000 },
    new UserProgress { Id = 11, UserId = 2, MissionId = 2, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1200 },
    new UserProgress { Id = 12, UserId = 2, MissionId = 3, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1300 },
    new UserProgress { Id = 13, UserId = 2, MissionId = 4, CurrentSuspicion = 30, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1300 },

    // Lan Anh (User 3) - 3950 XP total
    new UserProgress { Id = 20, UserId = 3, MissionId = 1, CurrentSuspicion = 10, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 950 },
    new UserProgress { Id = 21, UserId = 3, MissionId = 2, CurrentSuspicion = 15, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000 },
    new UserProgress { Id = 22, UserId = 3, MissionId = 3, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000 },
    new UserProgress { Id = 23, UserId = 3, MissionId = 4, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 1000 },

    // Tuấn Khoa (User 4) - 3200 XP total
    new UserProgress { Id = 30, UserId = 4, MissionId = 1, CurrentSuspicion = 20, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800 },
    new UserProgress { Id = 31, UserId = 4, MissionId = 2, CurrentSuspicion = 22, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800 },
    new UserProgress { Id = 32, UserId = 4, MissionId = 3, CurrentSuspicion = 25, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800 },
    new UserProgress { Id = 33, UserId = 4, MissionId = 4, CurrentSuspicion = 28, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 800 },

    // KHOA_PRO (User 1) - 1250 XP starter progress
    new UserProgress { Id = 40, UserId = 1, MissionId = 1, CurrentSuspicion = 30, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 600 },
    new UserProgress { Id = 41, UserId = 1, MissionId = 2, CurrentSuspicion = 35, Status = MissionStatus.Completed, TurnCount = 5, XpEarned = 650 }
);
```

---

### Task 2: Update Game Engine XP Accumulation

**Files:**
* Modify: `d:\CODE\EXE\BackEnd\TheUnraveller.Service\Implementations\GameEngineService.cs`

**Step 2.1: Accumulate XP and Reset Replay State**
1. In `ProcessPlayerMessageAsync`, reset `XpEarned = 0` when replaying a mission.
2. Add the newly earned turn XP to `progress.XpEarned` on each turn:
```csharp
// Inside GameEngineService.cs -> ProcessPlayerMessageAsync
else if (progress.Status != MissionStatus.InProgress)
{
    progress.CurrentSuspicion = mission.StartSuspicion;
    progress.Status = MissionStatus.InProgress;
    progress.TurnCount = 0;
    progress.XpEarned = 0; // Reset XP for this playthrough
}

// ...
int xpEarned = suspicionChange <= 0 ? (mission.XpReward / 5) : 5;
progress.XpEarned += xpEarned;
```

---

### Task 3: Make LLM Service URL and Model Configurable

**Files:**
* Modify: `d:\CODE\EXE\BackEnd\TheUnraveller.API\appsettings.json`
* Modify: `d:\CODE\EXE\BackEnd\TheUnraveller.API\appsettings.Development.json`
* Modify: `d:\CODE\EXE\BackEnd\TheUnraveller.Service\Implementations\LlmProviderService.cs`

**Step 3.1: Add Configuration Settings**
Add default configurations to `appsettings.json`:
```json
"LlmApi": {
  "BaseUrl": "https://api.openai.com/v1/",
  "Model": "gpt-4o",
  "ApiKey": "dummy_key"
}
```

**Step 3.2: Use Configuration Settings in LlmProviderService**
Update `LlmProviderService.cs` constructor and endpoint request construction to read `BaseUrl` and `Model` dynamically:
```csharp
private readonly string _baseUrl;
private readonly string _model;

public LlmProviderService(HttpClient httpClient, IConfiguration configuration)
{
    _httpClient = httpClient;
    _apiKey = configuration["LlmApi:ApiKey"] ?? "dummy_key";
    _baseUrl = configuration["LlmApi:BaseUrl"] ?? "https://api.openai.com/v1/";
    _model = configuration["LlmApi:Model"] ?? "gpt-4o";
}

// In GetNpcResponseAsync:
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
```

---

### Task 4: Implement Leaderboard API Endpoint

**Files:**
* Create: `d:\CODE\EXE\BackEnd\TheUnraveller.Service\Interfaces\ILeaderboardService.cs`
* Create: `d:\CODE\EXE\BackEnd\TheUnraveller.Service\Implementations\LeaderboardService.cs`
* Create: `d:\CODE\EXE\BackEnd\TheUnraveller.API\Controllers\LeaderboardController.cs`
* Modify: `d:\CODE\EXE\BackEnd\TheUnraveller.Service\DTOs\GameDtos.cs`
* Modify: `d:\CODE\EXE\BackEnd\TheUnraveller.API\Program.cs`

**Step 4.1: Add DTO to GameDtos.cs**
```csharp
public record LeaderboardEntryDto(int Rank, string Name, int Xp, string Badge, bool IsYou);
```

**Step 4.2: Add Leaderboard Interface and Service**
Define `ILeaderboardService.cs`:
```csharp
using TheUnraveller.Service.DTOs;

namespace TheUnraveller.Service.Interfaces;

public interface ILeaderboardService
{
    Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(int currentUserId);
}
```

Implement `LeaderboardService.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class LeaderboardService : ILeaderboardService
{
    private readonly AppDbContext _context;

    public LeaderboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(int currentUserId)
    {
        var users = await _context.Users
            .Include(u => u.Progresses)
            .ToListAsync();

        var rankedUsers = users
            .Select(u => new
            {
                u.Id,
                Name = u.Username,
                TotalXp = u.Progresses.Sum(p => p.XpEarned)
            })
            .OrderByDescending(x => x.TotalXp)
            .ToList();

        var leaderboard = new List<LeaderboardEntryDto>();
        for (int i = 0; i < rankedUsers.Count; i++)
        {
            var user = rankedUsers[i];
            int rank = i + 1;
            string badge = rank switch
            {
                1 => "👑",
                2 => "🥈",
                3 => "🥉",
                _ => "⚡"
            };

            leaderboard.Add(new LeaderboardEntryDto(
                rank,
                user.Name,
                user.TotalXp,
                badge,
                user.Id == currentUserId
            ));
        }

        return leaderboard;
    }
}
```

**Step 4.3: Create LeaderboardController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboardService;

    public LeaderboardController(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetLeaderboard([FromQuery] int userId = 1)
    {
        var entries = await _leaderboardService.GetLeaderboardAsync(userId);
        return Ok(entries);
    }
}
```

**Step 4.4: Register Leaderboard Service in Program.cs**
```csharp
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
```

---

### Task 5: Database Schema Reset and Compilation

**Step 5.1: Terminate Backend Server, Wipe DB, Rebuild**
Run command to stop the backend, delete old database files, and build:
```powershell
Stop-Process -Id 3900 -Force -ErrorAction SilentlyContinue
Remove-Item d:\CODE\EXE\BackEnd\TheUnraveller.API\unraveller.db* -Force
dotnet build d:\CODE\EXE\BackEnd\TheUnraveller.sln
```

---

### Task 6: Connect React Frontend to Dynamic Leaderboard

**Files:**
* Modify: `d:\CODE\EXE\FrontEnd\src\services\api.ts`
* Modify: `d:\CODE\EXE\FrontEnd\src\pages\Result\Result.tsx`

**Step 6.1: Update api.ts with Leaderboard fetch**
Add a method `getLeaderboard` to Axios service:
```typescript
export interface LeaderboardEntry {
  rank: number;
  name: string;
  xp: number;
  badge: string;
  isYou: boolean;
}

export const getLeaderboard = async (userId: number = 1): Promise<LeaderboardEntry[]> => {
  try {
    const res = await axiosInstance.get<LeaderboardEntry[]>(`/api/Leaderboard?userId=${userId}`);
    return res.data;
  } catch (err) {
    console.error("Failed to fetch leaderboard from API, returning mock data", err);
    return [
      { rank: 1, name: 'Minh Khôi', xp: 4800, badge: '👑', isYou: false },
      { rank: 2, name: 'Lan Anh',   xp: 3950, badge: '🥈', isYou: false },
      { rank: 3, name: 'Tuấn Khoa', xp: 3200, badge: '🥉', isYou: false },
      { rank: 4, name: 'You',       xp: 1250, badge: '⚡', isYou: true }
    ];
  }
};
```

**Step 6.2: Integrate Leaderboard Endpoint in Result.tsx**
1. Read dynamic leaderboard array from `getLeaderboard()` inside `useEffect`.
2. Ensure it handles UI loading and fallback seamlessly.

---

### Task 7: UI Aesthetic Shake Polish in Game Console

**Files:**
* Modify: `d:\CODE\EXE\FrontEnd\src\pages\Game\Game.tsx`

**Step 7.1: Add Shake Animation to suspicion bar**
In `Game.tsx`, add a dynamic state `shouldShake` triggered on suspicion increase, and map it to a shake animation class on the suspicion card wrapper:
```typescript
const [shouldShake, setShouldShake] = useState(false);

// inside processMessage response handler
const newSus = res.newSuspicionLevel;
if (newSus > suspicion) {
  setShouldShake(true);
  setTimeout(() => setShouldShake(false), 500);
}
```
Add Tailwind/CSS animation:
```css
@keyframes ur-shake {
  0%, 100% { transform: translateX(0); }
  20%, 60% { transform: translateX(-4px); }
  40%, 80% { transform: translateX(4px); }
}
.animate-shake {
  animation: ur-shake 0.3s ease-in-out;
}
```
