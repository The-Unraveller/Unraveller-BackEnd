using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Core.Interfaces;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.Implementations;
using Xunit;

namespace TheUnraveller.Tests;

public class MissionServiceTests
{
    private readonly Mock<IMissionRepository> _missionRepoMock;
    private readonly AppDbContext _context;
    private readonly MissionService _missionService;

    public MissionServiceTests()
    {
        _missionRepoMock = new Mock<IMissionRepository>();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _missionService = new MissionService(_missionRepoMock.Object, _context);
    }

    [Fact]
    public async Task GetAllMissionsAsync_ShouldOnlyReturnMissionsReturnedByGetAvailableMissionsAsync()
    {
        // Arrange
        // (Note: SpecificRepositories.cs has been updated to filter this at the database/repository boundary.
        // We verify here that the service layer correctly retrieves from that filtered source and maps them.)
        var mockNpc = new Npc { Id = 1, Name = "Barista", NpcEmoji = "☕" };
        var mockAvailableMissions = new List<Mission>
        {
            new Mission 
            { 
                Id = 1, 
                Title = "Coffee Shop", 
                ApprovalStatus = ApprovalStatus.Approved, 
                Npc = mockNpc,
                Locked = false
            },
            new Mission 
            { 
                Id = 2, 
                Title = "Classroom instructions", 
                ApprovalStatus = ApprovalStatus.Approved, 
                Npc = mockNpc,
                Locked = false
            }
        };

        _missionRepoMock
            .Setup(r => r.GetAvailableMissionsAsync())
            .ReturnsAsync(mockAvailableMissions);

        // Act
        var result = await _missionService.GetAllMissionsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, m => Assert.False(m.Locked));
        _missionRepoMock.Verify(r => r.GetAvailableMissionsAsync(), Times.Once);
    }
}
