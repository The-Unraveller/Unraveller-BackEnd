using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Interfaces;

namespace TheUnraveller.Service.Implementations;

public class GameEngineService : IGameEngineService
{
    private readonly IDialogueRepository _dialogueRepo;
    private readonly IUserProgressRepository _progressRepo;
    private readonly IMissionRepository _missionRepo;

    public GameEngineService(
        IDialogueRepository dialogueRepo, 
        IUserProgressRepository progressRepo,
        IMissionRepository missionRepo)
    {
        _dialogueRepo = dialogueRepo;
        _progressRepo = progressRepo;
        _missionRepo = missionRepo;
    }

    public async Task<DialogueResponseDto> ProcessPlayerMessageAsync(DialogueRequestDto request)
    {
        var progress = await _progressRepo.GetUserProgressAsync(request.UserId, request.MissionId);
        var mission = await _missionRepo.GetByIdAsync(request.MissionId);
        
        if (mission == null) throw new Exception("Mission not found.");
        
        if (progress == null)
        {
            progress = new UserProgress
            {
                UserId = request.UserId,
                MissionId = request.MissionId,
                CurrentSuspicion = mission.StartSuspicion,
                Status = MissionStatus.InProgress
            };
            await _progressRepo.AddAsync(progress);
        }

        // --- Mocking AI Logic ---
        // In reality, you'd call an AI service like OpenAI/Gemini here.
        string npcResponse = $"I hear you, but why should I trust you regarding '{request.Message.Substring(0, Math.Min(10, request.Message.Length))}...'?";
        string feedback = "Try to use more formal language next time.";
        int suspicionChange = request.Message.ToLower().Contains("please") ? -5 : 10;
        // -------------------------

        progress.CurrentSuspicion += suspicionChange;
        if (progress.CurrentSuspicion < 0) progress.CurrentSuspicion = 0;
        
        bool isLose = progress.CurrentSuspicion >= mission.MaxSuspicion;
        bool isWin = progress.CurrentSuspicion <= 10 && request.Message.Length > 20; // Example win condition

        if (isWin) progress.Status = MissionStatus.Completed;
        if (isLose) progress.Status = MissionStatus.Failed;

        var dialogue = new Dialogue
        {
            UserId = request.UserId,
            NpcId = mission.NpcId,
            MissionId = request.MissionId,
            PlayerMessage = request.Message,
            NpcResponse = npcResponse,
            Feedback = feedback,
            SuspicionChange = suspicionChange
        };

        await _dialogueRepo.AddAsync(dialogue);
        await _progressRepo.SaveChangesAsync();

        return new DialogueResponseDto(
            npcResponse, 
            feedback, 
            progress.CurrentSuspicion, 
            isWin, 
            isLose
        );
    }
}
