using Moq;
using System.Security.Claims;
using System.Threading.Tasks;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Exceptions;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Service.DTOs;
using TheUnraveller.Service.Implementations;
using TheUnraveller.Service.Interfaces;
using Xunit;

namespace TheUnraveller.Tests;

public class GameEngineGameplayTests
{
    private readonly Mock<IDialogueRepository> _dialogueRepoMock;
    private readonly Mock<IUserProgressRepository> _progressRepoMock;
    private readonly Mock<IMissionRepository> _missionRepoMock;
    private readonly Mock<ILLMProviderService> _llmServiceMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly GameEngineService _gameEngine;

    public GameEngineGameplayTests()
    {
        _dialogueRepoMock = new Mock<IDialogueRepository>();
        _progressRepoMock = new Mock<IUserProgressRepository>();
        _missionRepoMock = new Mock<IMissionRepository>();
        _llmServiceMock = new Mock<ILLMProviderService>();
        _userRepoMock = new Mock<IUserRepository>();

        _gameEngine = new GameEngineService(
            _dialogueRepoMock.Object,
            _progressRepoMock.Object,
            _missionRepoMock.Object,
            _llmServiceMock.Object,
            _userRepoMock.Object
        );
    }

    [Fact]
    public async Task ProcessPlayerMessageAsync_PendingMission_ShouldThrowDomainException()
    {
        // Arrange
        var request = new DialogueRequestDto(1, 42, "Hello Barista!");
        
        var mockUser = new User 
        { 
            Id = 1, 
            Username = "PlayerOne", 
            Energy = 100 
        };
        
        var mockPendingMission = new Mission 
        { 
            Id = 42, 
            Title = "Unapproved Cafe Scenario", 
            ApprovalStatus = ApprovalStatus.Pending 
        };

        _userRepoMock
            .Setup(u => u.GetByIdAsync(1))
            .ReturnsAsync(mockUser);

        _missionRepoMock
            .Setup(m => m.GetByIdAsync(42))
            .ReturnsAsync(mockPendingMission);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => 
            _gameEngine.ProcessPlayerMessageAsync(request)
        );

        Assert.Equal("Mission not found or not approved.", exception.Message);
    }

    [Fact]
    public async Task ProcessPlayerMessageAsync_RejectedMission_ShouldThrowDomainException()
    {
        // Arrange
        var request = new DialogueRequestDto(1, 43, "Hello Detective!");
        
        var mockUser = new User 
        { 
            Id = 1, 
            Username = "PlayerOne", 
            Energy = 100 
        };
        
        var mockRejectedMission = new Mission 
        { 
            Id = 43, 
            Title = "Rejected Detective Case", 
            ApprovalStatus = ApprovalStatus.Rejected 
        };

        _userRepoMock
            .Setup(u => u.GetByIdAsync(1))
            .ReturnsAsync(mockUser);

        _missionRepoMock
            .Setup(m => m.GetByIdAsync(43))
            .ReturnsAsync(mockRejectedMission);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => 
            _gameEngine.ProcessPlayerMessageAsync(request)
        );

        Assert.Equal("Mission not found or not approved.", exception.Message);
    }
}
