using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _missionRepo;

    public MissionService(IMissionRepository missionRepo)
    {
        _missionRepo = missionRepo;
    }

    public async Task<IEnumerable<MissionDto>> GetAllMissionsAsync()
    {
        var missions = await _missionRepo.GetAvailableMissionsAsync();
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
            m.GrammarTarget
        ));
    }

    public async Task<MissionDto?> GetMissionByIdAsync(int id)
    {
        var m = await _missionRepo.GetByIdAsync(id);
        if (m == null) return null;
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
            m.GrammarTarget
        );
    }
}
