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
            .OrderByDescending(m => m.Id)
            .ToListAsync();

        return missions.Select(m => MapToManagementDto(m));
    }

    public async Task<IEnumerable<MissionManagementDto>> GetPendingMissionsAsync()
    {
        var missions = await _context.Missions
            .Include(m => m.Npc)
            .Where(m => m.ApprovalStatus == ApprovalStatus.Pending)
            .OrderByDescending(m => m.Id)
            .ToListAsync();

        return missions.Select(m => MapToManagementDto(m));
    }

    public async Task<bool> CreateMissionAsync(MissionCreateDto dto, int creatorId)
    {
        // 1. Domain Validation
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new DomainException("Mission title cannot be empty.");

        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new DomainException("Mission description cannot be empty.");

        var npc = await _context.Npcs.FindAsync(dto.NpcId);
        if (npc == null)
            throw new DomainException($"NPC with ID {dto.NpcId} does not exist.");

        // 2. Map Entity
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
            Locked = true, // Default to locked
            NpcId = dto.NpcId,
            ApprovalStatus = ApprovalStatus.Pending,
            RejectionReason = null,
            CreatedByUserId = creatorId
        };

        // 3. Save
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
        mission.Locked = false; // Unlock for gameplay

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
        mission.Locked = true; // Keep locked

        await _missionRepository.UpdateAsync(mission);
        await _missionRepository.SaveChangesAsync();
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
            CreatedByUserId = m.CreatedByUserId
        };
    }
}
