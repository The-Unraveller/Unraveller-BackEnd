using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheUnraveller.Core.Entities;
using TheUnraveller.Infrastructure.Data;
using TheUnraveller.Service.Implementations;
using TheUnraveller.Service.DTOs;
using Xunit;

namespace TheUnraveller.Tests;

public class BadgeServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;

    public BadgeServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    private void SeedDatabase()
    {
        // Clear tables that could contain custom test states
        _context.UserBadges.RemoveRange(_context.UserBadges);
        _context.UserProgresses.RemoveRange(_context.UserProgresses);
        _context.Users.RemoveRange(_context.Users);
        _context.SaveChanges();

        // Seed a default test user
        var user = new User
        {
            Id = 1,
            Username = "TestUser",
            Email = "test@example.com",
            PasswordHash = "hashed",
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            Energy = 100,
            MaxEnergy = 100,
            LastEnergyRechargedAt = DateTime.UtcNow,
            XpBalance = 0,
            StreakCount = 0
        };
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task AwardBadgesForMissionAsync_AwardsFirstSteps_ForFirstCompletion()
    {
        // Arrange
        SeedDatabase();
        
        var progress = new UserProgress
        {
            UserId = 1,
            MissionId = 1,
            Status = MissionStatus.Completed,
            CompletedAt = DateTime.UtcNow
        };
        _context.UserProgresses.Add(progress);
        await _context.SaveChangesAsync();

        var badgeService = new BadgeService(_context);

        // Act
        await badgeService.AwardBadgesForMissionAsync(1, 1, 50);
        await _context.SaveChangesAsync();

        // Assert
        var awarded = await _context.UserBadges.ToListAsync();
        Assert.Single(awarded);
        Assert.Equal(1, awarded[0].BadgeId); // Id = 1 is 'First Steps'
    }

    [Fact]
    public async Task AwardBadgesForMissionAsync_AwardsSkillful_WhenAverageScoreGreaterOrEqualTo70()
    {
        // Arrange
        SeedDatabase();
        
        var progress = new UserProgress
        {
            UserId = 1,
            MissionId = 1,
            Status = MissionStatus.Completed,
            CompletedAt = DateTime.UtcNow
        };
        _context.UserProgresses.Add(progress);
        await _context.SaveChangesAsync();

        var badgeService = new BadgeService(_context);

        // Act
        await badgeService.AwardBadgesForMissionAsync(1, 1, 75);
        await _context.SaveChangesAsync();

        // Assert
        var awarded = await _context.UserBadges.Select(ub => ub.BadgeId).ToListAsync();
        Assert.Contains(1, awarded); // Also gets First Steps
        Assert.Contains(2, awarded); // Id = 2 is 'Skillful'
    }

    [Fact]
    public async Task AwardBadgesForMissionAsync_AwardsPerfectionist_WhenAverageScoreGreaterOrEqualTo90()
    {
        // Arrange
        SeedDatabase();
        
        var progress = new UserProgress
        {
            UserId = 1,
            MissionId = 1,
            Status = MissionStatus.Completed,
            CompletedAt = DateTime.UtcNow
        };
        _context.UserProgresses.Add(progress);
        await _context.SaveChangesAsync();

        var badgeService = new BadgeService(_context);

        // Act
        await badgeService.AwardBadgesForMissionAsync(1, 1, 95);
        await _context.SaveChangesAsync();

        // Assert
        var awarded = await _context.UserBadges.Select(ub => ub.BadgeId).ToListAsync();
        Assert.Contains(1, awarded); // First Steps
        Assert.Contains(2, awarded); // Skillful
        Assert.Contains(3, awarded); // Perfectionist
    }

    [Fact]
    public async Task AwardBadgesForMissionAsync_AwardsWritingCoach_WhenUserXpBalanceGreaterThanOrEqualTo1000()
    {
        // Arrange
        SeedDatabase();
        
        var user = await _context.Users.FindAsync(1);
        user!.XpBalance = 1000;
        _context.Users.Update(user);

        var progress = new UserProgress
        {
            UserId = 1,
            MissionId = 1,
            Status = MissionStatus.Completed,
            CompletedAt = DateTime.UtcNow
        };
        _context.UserProgresses.Add(progress);
        await _context.SaveChangesAsync();

        var badgeService = new BadgeService(_context);

        // Act
        await badgeService.AwardBadgesForMissionAsync(1, 1, 50);
        await _context.SaveChangesAsync();

        // Assert
        var awarded = await _context.UserBadges.Select(ub => ub.BadgeId).ToListAsync();
        Assert.Contains(5, awarded); // Id = 5 is 'Writing Coach'
    }

    [Fact]
    public async Task AwardBadgesForMissionAsync_AwardsLifetimeLearner_WhenTotalCompletionsGreaterThanOrEqualTo10()
    {
        // Arrange
        SeedDatabase();
        
        // Add 10 completed missions
        for (int mId = 1; mId <= 10; mId++)
        {
            var progress = new UserProgress
            {
                UserId = 1,
                MissionId = mId,
                Status = MissionStatus.Completed,
                CompletedAt = DateTime.UtcNow
            };
            _context.UserProgresses.Add(progress);
        }
        await _context.SaveChangesAsync();

        var badgeService = new BadgeService(_context);

        // Act
        await badgeService.AwardBadgesForMissionAsync(1, 10, 50);
        await _context.SaveChangesAsync();

        // Assert
        var awarded = await _context.UserBadges.Select(ub => ub.BadgeId).ToListAsync();
        Assert.Contains(8, awarded); // Id = 8 is 'Lifetime Learner'
    }

    [Fact]
    public async Task AwardBadgesForMissionAsync_AwardsLinguist_WhenCompletionsInAll3Domains()
    {
        // Arrange
        SeedDatabase();
        
        // Completed missions in:
        // Mission 1 (Professional)
        // Mission 6 (Social)
        // Mission 7 (Academic)
        var progresses = new[]
        {
            new UserProgress { UserId = 1, MissionId = 1, Status = MissionStatus.Completed, CompletedAt = DateTime.UtcNow },
            new UserProgress { UserId = 1, MissionId = 6, Status = MissionStatus.Completed, CompletedAt = DateTime.UtcNow },
            new UserProgress { UserId = 1, MissionId = 7, Status = MissionStatus.Completed, CompletedAt = DateTime.UtcNow }
        };
        _context.UserProgresses.AddRange(progresses);
        await _context.SaveChangesAsync();

        var badgeService = new BadgeService(_context);

        // Act
        await badgeService.AwardBadgesForMissionAsync(1, 7, 50);
        await _context.SaveChangesAsync();

        // Assert
        var awarded = await _context.UserBadges.Select(ub => ub.BadgeId).ToListAsync();
        Assert.Contains(7, awarded); // Id = 7 is 'Linguist'
    }

    [Fact]
    public async Task AwardBadgesForMissionAsync_DoesNotAwardDuplicateBadges()
    {
        // Arrange
        SeedDatabase();
        
        var firstBadge = new UserBadge
        {
            UserId = 1,
            BadgeId = 1,
            EarnedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.UserBadges.Add(firstBadge);

        var progress = new UserProgress
        {
            UserId = 1,
            MissionId = 1,
            Status = MissionStatus.Completed,
            CompletedAt = DateTime.UtcNow
        };
        _context.UserProgresses.Add(progress);
        await _context.SaveChangesAsync();

        var badgeService = new BadgeService(_context);

        // Act
        await badgeService.AwardBadgesForMissionAsync(1, 1, 50);
        await _context.SaveChangesAsync();

        // Assert
        var awarded = await _context.UserBadges.Where(ub => ub.BadgeId == 1).ToListAsync();
        Assert.Single(awarded); // Only one First Steps badge should exist
    }

    [Fact]
    public async Task GetUserBadgesAsync_ReturnsCorrectDetails()
    {
        // Arrange
        SeedDatabase();
        
        var earnedAt = DateTime.UtcNow;
        var userBadge = new UserBadge
        {
            UserId = 1,
            BadgeId = 1, // 'First Steps'
            EarnedAt = earnedAt
        };
        _context.UserBadges.Add(userBadge);
        await _context.SaveChangesAsync();

        var badgeService = new BadgeService(_context);

        // Act
        var result = await badgeService.GetUserBadgesAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result[0].BadgeId);
        Assert.Equal("First Steps", result[0].Name);
        Assert.Equal("Complete your first mission", result[0].Description);
        Assert.Equal("👣", result[0].Icon);
        Assert.Equal(earnedAt, result[0].EarnedAt);
    }
}
