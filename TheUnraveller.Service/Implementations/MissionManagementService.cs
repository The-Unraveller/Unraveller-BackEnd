using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Exceptions;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheUnraveller.Service.Implementations;

public class MissionManagementService : IMissionManagementService
{
    private readonly IMissionRepository _missionRepository;
    private readonly AppDbContext _context;

    public MissionManagementService(IMissionRepository missionRepository, AppDbContext context)
    {
        _missionRepository = missionRepository;
        _context = context;
    }

    public async Task<IEnumerable<MissionManagementDto>> GetAllMissionsForManagementAsync()
    {
        var missions = await _context.Missions
            .Include(m => m.Npc)
            .Include(m => m.SubTasks)
            .OrderByDescending(m => m.Id)
            .ToListAsync();

        return missions.Select(m => MapToManagementDto(m));
    }

    public async Task<IEnumerable<MissionManagementDto>> GetPendingMissionsAsync()
    {
        var missions = await _context.Missions
            .Include(m => m.Npc)
            .Include(m => m.SubTasks)
            .Where(m => m.ApprovalStatus == ApprovalStatus.Pending)
            .OrderByDescending(m => m.Id)
            .ToListAsync();

        return missions.Select(m => MapToManagementDto(m));
    }

    public async Task<bool> CreateMissionAsync(MissionCreateDto dto, int creatorId)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new DomainException("Mission title cannot be empty.");

        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new DomainException("Mission description cannot be empty.");

        var npc = await _context.Npcs.FindAsync(dto.NpcId);
        if (npc == null)
            throw new DomainException($"NPC with ID {dto.NpcId} does not exist.");

        var newMission = new Mission
        {
            Title = dto.Title,
            Goal = dto.Goal,
            Description = dto.Description,
            StartSuspicion = dto.StartSuspicion,
            MaxSuspicion = dto.MaxSuspicion,
            Stage = string.IsNullOrEmpty(dto.Stage) ? "Stage X" : dto.Stage,
            Difficulty = string.IsNullOrEmpty(dto.Difficulty) ? "Beginner" : dto.Difficulty,
            XpReward = dto.XpReward,
            ImageUrl = dto.ImageUrl,
            Locked = true,
            NpcId = dto.NpcId,
            ApprovalStatus = ApprovalStatus.Pending,
            RejectionReason = null,
            CreatedByUserId = creatorId,
            GrammarTarget = dto.GrammarTarget
        };

        if (dto.SubTasks != null && dto.SubTasks.Count > 0)
        {
            int order = 1;
            foreach (var subDto in dto.SubTasks)
            {
                newMission.SubTasks.Add(new MissionSubTask
                {
                    Label = subDto.Label,
                    LabelEn = subDto.LabelEn,
                    HintPhrase = subDto.HintPhrase,
                    TriggerKeywords = subDto.TriggerKeywords ?? new List<string>(),
                    IsOptional = subDto.IsOptional,
                    XpBonus = subDto.XpBonus,
                    OrderIndex = order++
                });
            }
        }

        await _missionRepository.AddAsync(newMission);
        await _missionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMissionAsync(int id, MissionUpdateDto dto)
    {
        var mission = await _missionRepository.GetByIdAsync(id);
        if (mission == null)
            throw new DomainException("Mission not found.");

        if (!string.IsNullOrEmpty(dto.Title)) mission.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Goal)) mission.Goal = dto.Goal;
        if (!string.IsNullOrEmpty(dto.Description)) mission.Description = dto.Description;
        if (dto.XpReward.HasValue) mission.XpReward = dto.XpReward.Value;
        if (!string.IsNullOrEmpty(dto.Stage)) mission.Stage = dto.Stage;
        if (!string.IsNullOrEmpty(dto.Difficulty)) mission.Difficulty = dto.Difficulty;
        if (dto.NpcId.HasValue) mission.NpcId = dto.NpcId.Value;
        if (!string.IsNullOrEmpty(dto.ImageUrl)) mission.ImageUrl = dto.ImageUrl;

        await _missionRepository.UpdateAsync(mission);
        await _missionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveMissionAsync(int id)
    {
        var mission = await _missionRepository.GetByIdAsync(id);
        if (mission == null)
            throw new DomainException("Mission not found.");

        mission.ApprovalStatus = ApprovalStatus.Approved;
        mission.RejectionReason = null;
        mission.Locked = false;

        await _missionRepository.UpdateAsync(mission);
        await _missionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectMissionAsync(int id, string reason)
    {
        var mission = await _missionRepository.GetByIdAsync(id);
        if (mission == null)
            throw new DomainException("Mission not found.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("A rejection reason must be provided.");

        mission.ApprovalStatus = ApprovalStatus.Rejected;
        mission.RejectionReason = reason;
        mission.Locked = true;

        await _missionRepository.UpdateAsync(mission);
        await _missionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<NpcDto>> GetAllNpcsAsync()
    {
        var npcs = await _context.Npcs.ToListAsync();
        return npcs.Select(n => new NpcDto
        {
            Id = n.Id,
            Name = n.Name,
            Role = n.Role,
            NpcEmoji = n.NpcEmoji,
            Description = n.Description,
            Personality = n.Personality
        });
    }

    public async Task<NpcDto> CreateNpcAsync(NpcCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new DomainException("NPC name cannot be empty.");
        if (string.IsNullOrWhiteSpace(dto.Role))
            throw new DomainException("NPC role cannot be empty.");

        var npc = new Npc
        {
            Name = dto.Name,
            Role = dto.Role,
            Description = dto.Description ?? string.Empty,
            Personality = dto.Personality ?? string.Empty,
            NpcEmoji = string.IsNullOrWhiteSpace(dto.NpcEmoji) ? "👤" : dto.NpcEmoji
        };

        _context.Npcs.Add(npc);
        await _context.SaveChangesAsync();

        return new NpcDto
        {
            Id = npc.Id,
            Name = npc.Name,
            Role = npc.Role,
            NpcEmoji = npc.NpcEmoji,
            Description = npc.Description,
            Personality = npc.Personality
        };
    }

    public async Task<bool> UpdateNpcAsync(int id, NpcCreateDto dto)
    {
        var npc = await _context.Npcs.FindAsync(id);
        if (npc == null)
            throw new DomainException("NPC not found.");

        if (!string.IsNullOrWhiteSpace(dto.Name)) npc.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Role)) npc.Role = dto.Role;
        if (dto.Description != null) npc.Description = dto.Description;
        if (dto.Personality != null) npc.Personality = dto.Personality;
        if (!string.IsNullOrWhiteSpace(dto.NpcEmoji)) npc.NpcEmoji = dto.NpcEmoji;

        _context.Npcs.Update(npc);
        await _context.SaveChangesAsync();
        return true;
    }

    private static MissionManagementDto MapToManagementDto(Mission m)
    {
        return new MissionManagementDto
        {
            Id = m.Id,
            Title = m.Title,
            Goal = m.Goal,
            Description = m.Description,
            StartSuspicion = m.StartSuspicion,
            MaxSuspicion = m.MaxSuspicion,
            Stage = m.Stage,
            Difficulty = m.Difficulty,
            XpReward = m.XpReward,
            ImageUrl = m.ImageUrl,
            Locked = m.Locked,
            NpcId = m.NpcId,
            NpcName = m.Npc?.Name ?? string.Empty,
            NpcEmoji = m.Npc?.NpcEmoji ?? string.Empty,
            ApprovalStatus = (int)m.ApprovalStatus,
            RejectionReason = m.RejectionReason,
            CreatedByUserId = m.CreatedByUserId,
            GrammarTarget = m.GrammarTarget,
            SubTasks = m.SubTasks.Select(s => new SubTaskManagementDto
            {
                Id = s.Id,
                MissionId = s.MissionId,
                OrderIndex = s.OrderIndex,
                Label = s.Label,
                LabelEn = s.LabelEn,
                HintPhrase = s.HintPhrase,
                TriggerKeywords = s.TriggerKeywords,
                IsOptional = s.IsOptional,
                XpBonus = s.XpBonus
            }).OrderBy(s => s.OrderIndex).ToList()
        };
    }
}
