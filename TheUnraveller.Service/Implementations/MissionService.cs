using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepo;
    private readonly AppDbContext _context;

    public MissionService(IMissionRepository missionRepo, AppDbContext context)
    {
        _missionRepo = missionRepo;
        _context = context;
    }

    public async Task<IEnumerable<MissionDto>> GetAllMissionsAsync(int? userId = null)
    {
        var missions = await _missionRepo.GetAvailableMissionsAsync();
        
        var completedSubTaskIds = userId.HasValue
            ? await _context.UserSubTaskProgresses.Where(p => p.UserId == userId.Value).Select(p => p.SubTaskId).ToListAsync()
            : new List<int>();

        return missions.Select(m => new MissionDto(
            m.Id,
            m.Title,
            m.Goal,
            m.Description,
            m.StartSuspicion,
            m.Stage,
            m.Difficulty,
            m.XpReward,
            m.ImageUrl,
            m.Npc?.Name ?? string.Empty,
            m.Npc?.NpcEmoji ?? string.Empty,
            m.Locked,
            m.GrammarTarget,
            (int)m.Domain,
            m.InitialChoices,
            m.SyntaxPuzzlesJson,
            MapSubTasks(m, completedSubTaskIds)
        ));
    }

    public async Task<MissionDto?> GetMissionByIdAsync(int id, int? userId = null)
    {
        var m = await _missionRepo.GetByIdAsync(id);
        if (m == null) return null;

        var completedSubTaskIds = userId.HasValue
            ? await _context.UserSubTaskProgresses.Where(p => p.UserId == userId.Value && p.MissionId == id).Select(p => p.SubTaskId).ToListAsync()
            : new List<int>();

        return new MissionDto(
            m.Id,
            m.Title,
            m.Goal,
            m.Description,
            m.StartSuspicion,
            m.Stage,
            m.Difficulty,
            m.XpReward,
            m.ImageUrl,
            m.Npc?.Name ?? string.Empty,
            m.Npc?.NpcEmoji ?? string.Empty,
            m.Locked,
            m.GrammarTarget,
            (int)m.Domain,
            m.InitialChoices,
            m.SyntaxPuzzlesJson,
            MapSubTasks(m, completedSubTaskIds)
        );
    }

    private static List<MissionSubTaskDto> MapSubTasks(Mission m, List<int> completedSubTaskIds)
    {
        return m.SubTasks?
            .OrderBy(s => s.OrderIndex)
            .Select(s => new MissionSubTaskDto(
                s.Id,
                s.MissionId,
                s.OrderIndex,
                s.Label,
                s.LabelEn,
                s.HintPhrase,
                s.IsOptional,
                s.XpBonus,
                completedSubTaskIds.Contains(s.Id)
            )).ToList() ?? new List<MissionSubTaskDto>();
    }
}
